using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DMapper.Attributes;
using DMapper.Constants;
using DMapper.Converters;
using DMapper.Helpers.FluentConfigurations;
using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Helpers
{
    /// <summary>
    /// DMapper V8 (experimental): expression-compiled mappers.
    ///
    /// Goals:
    ///  - Build a mapping plan once per (TSource, TDestination) and compile it.
    ///  - Use compiled getters per source path; delegate final assignment to the robust V6 setter.
    ///  - Keep attribute surface and Fluent API identical to v6.
    ///
    /// Notes:
    ///  - ComplexBind is still applied after direct property assignment (like v6).
    ///  - Collections are handled by the V6 setter (which already supports lists/arrays).
    /// </summary>
    public static partial class ReflectionHelper
    {
        public static TDestination ReplacePropertiesRecursive_V8<TDestination, TSource>(
            TDestination destination, TSource source)
        {
            if (destination is null || source is null) return destination;

            var key = (typeof(TSource), typeof(TDestination));
            var mapper = _compiledMapCache.GetOrAdd(key, k => BuildCompiledMapper(k.Item1, k.Item2));
            mapper(source!, destination!);

            // Keep ComplexBind behavior identical to v6 for now.
            ProcessComplexBindAttributes_V6(source!, destination!);
            return destination;
        }

        private static readonly ConcurrentDictionary<(Type Src, Type Dest), Action<object, object>> _compiledMapCache = new();
        private static readonly ConcurrentDictionary<(Type Type, string Path), Func<object, object>> _getterCache = new();
        private static readonly ConcurrentDictionary<(Type Type, string Path), Action<object, object>> _setterCache = new();
        private static readonly ConcurrentDictionary<Type, Dictionary<string, List<string>>> _mapCacheV8 = new();
        private static readonly ConcurrentDictionary<Type, Dictionary<string, IDMapperPropertyConverter>> _convCacheV8 = new();

        private static Action<object, object> BuildCompiledMapper(Type srcType, Type destType)
        {
            // Build/resolve mapping and converter caches once per destination type
            var mapping = _mapCacheV8.GetOrAdd(destType, BuildMappingDictionary_V8);
            var converters = _convCacheV8.GetOrAdd(destType, BuildConverterCache_V8);

            // parameters: (object src, object dest)
            var srcObj = Expression.Parameter(typeof(object), "src");
            var destObj = Expression.Parameter(typeof(object), "dest");
            var srcCast = Expression.Variable(srcType, "s");
            var destCast = Expression.Variable(destType, "d");

            var assignSrc = Expression.Assign(srcCast, Expression.Convert(srcObj, srcType));
            var assignDest = Expression.Assign(destCast, Expression.Convert(destObj, destType));

            var block = new List<Expression> { assignSrc, assignDest };

            // For each destination key, build candidate checks in order.
            foreach (var kvp in mapping)
            {
                var destKey = kvp.Key; // e.g. "Address.Street"
                var candidates = kvp.Value; // e.g. ["Address.Line1", "Street"]

                // Build: var val = Getter(s, candidate);
                // if (val != null) { if(conv) val = conv.Convert(val); Setter(d, destKey, val); goto next; }
                LabelTarget next = Expression.Label("NEXT_" + destKey);
                var valVar = Expression.Variable(typeof(object), "val");
                var stmts = new List<Expression>();

                foreach (var candidate in candidates)
                {
                    var getDel = _getterCache.GetOrAdd((srcType, candidate), BuildGetter);
                    var getterConst = Expression.Constant(getDel);
                    var invokeGet = Expression.Invoke(getterConst, Expression.Convert(srcCast, typeof(object)));
                    var assignVal = Expression.Assign(valVar, invokeGet);
                    var notNull = Expression.NotEqual(valVar, Expression.Constant(null));

                    // Apply converter if available for this destKey
                    Expression valueExpr = valVar;
                    if (converters.TryGetValue(destKey, out var conv))
                    {
                        var convConst = Expression.Constant(conv);
                        var convertCall = Expression.Call(
                            Expression.Convert(convConst, typeof(IDMapperPropertyConverter)),
                            typeof(IDMapperPropertyConverter).GetMethod(nameof(IDMapperPropertyConverter.Convert))!,
                            valVar);
                        valueExpr = Expression.Convert(convertCall, typeof(object));
                    }

                    var setDel = _setterCache.GetOrAdd((destType, destKey), BuildSetter);
                    var setterConst = Expression.Constant(setDel);
                    var callSetter = Expression.Invoke(setterConst,
                        Expression.Convert(destCast, typeof(object)),
                        valueExpr);

                    // if ((val = Getter(...)) != null) { callSetter; goto NEXT; }
                    stmts.Add(assignVal);
                    stmts.Add(Expression.IfThen(notNull, Expression.Block(callSetter, Expression.Goto(next))));
                }

                stmts.Add(Expression.Label(next));
                block.Add(Expression.Block(new[] { valVar }, stmts));
            }

            var body = Expression.Block(new[] { srcCast, destCast }, block);
            var lambda = Expression.Lambda<Action<object, object>>(body, srcObj, destObj);
            return lambda.Compile();
        }

        private static Func<object, object> BuildGetter((Type Type, string Path) key)
        {
            var (_, path) = key;
            return (obj) => GetValueByPath(obj, path);
        }

        private static Action<object, object> BuildSetter((Type Type, string Path) key)
        {
            var (_, path) = key;
            return (target, value) => SetNestedValueDirect_V6(
                target,
                path,
                value,
                GlobalConstants.DefaultDotSeparator);
        }


        private static object GetValueByPath(object src, string path)
        {
            if (src == null || string.IsNullOrWhiteSpace(path)) return null;
            object current = src;
            var parts = path.Split(new[] { GlobalConstants.DefaultDotSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                if (current == null) return null;
                var t = current.GetType();
                var prop = t.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null) return null;
                try
                {
                    current = prop.GetValue(current);
                }
                catch
                {
                    return null;
                }
            }
            return current;
        }

        private static Dictionary<string, List<string>> BuildMappingDictionary_V8(Type destinationType)
        {
            var mapping = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            BuildRec(destinationType, "", mapping, null, new HashSet<Type>());

            // Merge fluent configuration if implemented
            if (typeof(IDMapperConfiguration).IsAssignableFrom(destinationType))
            {
                var instance = Activator.CreateInstance(destinationType) as IDMapperConfiguration;
                if (instance != null)
                {
                    var builder = new DMapperConfigure();
                    instance.ConfigureMapping(builder);
                    var fluentMappings = builder.GetMappings();
                    foreach (var kvp in fluentMappings)
                        mapping[kvp.Key] = kvp.Value;
                }
            }

            return mapping;

            static void BuildRec(
                Type type,
                string destPrefix,
                Dictionary<string, List<string>> mapping,
                string effectiveSourcePrefix,
                HashSet<Type> visited)
            {
                if (type == null || type == typeof(string) || visited.Contains(type)) return;
                visited.Add(type);

                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.GetCustomAttribute<CopyIgnoreAttribute>() != null) continue;

                    string currentDestKey = string.IsNullOrEmpty(destPrefix) ? prop.Name : destPrefix + GlobalConstants.DefaultDotSeparator + prop.Name;

                    string newEffectiveSourcePrefix;
                    var bindAttr = prop.GetCustomAttribute<BindToAttribute>();
                    if (bindAttr != null && bindAttr.PropNames.Any())
                    {
                        var candidates = new List<string>();
                        foreach (var candidate in bindAttr.PropNames)
                        {
                            if (candidate.Contains(GlobalConstants.DefaultDotSeparator) || bindAttr.UseLiteralName)
                                candidates.Add(candidate);
                            else
                                candidates.Add(effectiveSourcePrefix != null
                                    ? effectiveSourcePrefix + GlobalConstants.DefaultDotSeparator + candidate
                                    : candidate);
                        }

                        string fallbackCandidate = effectiveSourcePrefix != null
                            ? effectiveSourcePrefix + GlobalConstants.DefaultDotSeparator + prop.Name
                            : prop.Name;
                        if (!candidates.Any(c => c.Equals(fallbackCandidate, StringComparison.OrdinalIgnoreCase)))
                            candidates.Add(fallbackCandidate);

                        newEffectiveSourcePrefix = candidates.First();
                        mapping[currentDestKey] = candidates;
                    }
                    else
                    {
                        string candidateSource = effectiveSourcePrefix != null
                            ? effectiveSourcePrefix + GlobalConstants.DefaultDotSeparator + prop.Name
                            : currentDestKey;
                        newEffectiveSourcePrefix = candidateSource;
                        mapping[currentDestKey] = new List<string> { candidateSource };
                    }

                    // Recurse only into non-collection complex types
                    if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                        BuildRec(prop.PropertyType, currentDestKey, mapping, newEffectiveSourcePrefix, new HashSet<Type>(visited));
                }
            }
        }

        private static Dictionary<string, IDMapperPropertyConverter> BuildConverterCache_V8(
            Type destinationType)
        {
            string separator = GlobalConstants.DefaultDotSeparator;
            var cache = new Dictionary<string, IDMapperPropertyConverter>(StringComparer.OrdinalIgnoreCase);
            BuildRec(destinationType, "", new HashSet<Type>());
            return cache;

            void BuildRec(Type type, string prefix, HashSet<Type> visited)
            {
                if (type == null || type == typeof(string) || visited.Contains(type)) return;
                visited.Add(type);

                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    string destKey = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}{separator}{prop.Name}";
                    if (prop.GetCustomAttribute<DMapperConverterAttribute>() is { } vcAttr)
                        cache[destKey] = vcAttr.Instantiate();

                    if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                        BuildRec(prop.PropertyType, destKey, visited);
                }
            }
        }

        private static bool TryConvertForAssignment(object value, Type targetType, out object result)
        {
            result = null;
            if (value is null)
            {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                {
                    result = null; return true;
                }
                return false;
            }

            var nonNullTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (nonNullTarget.IsEnum)
            {
                if (value is string s && string.IsNullOrWhiteSpace(s)) { result = null; return true; }
                if (Enum.TryParse(nonNullTarget, value.ToString(), true, out var e)) { result = e!; return true; }
                return false;
            }

            if (TrySpecialConvert(value, targetType, out var special)) { result = special; return true; }

            if (nonNullTarget.IsInstanceOfType(value)) { result = value; return true; }

            try
            {
                result = Convert.ChangeType(value, nonNullTarget, System.Globalization.CultureInfo.InvariantCulture);
                return true;
            }
            catch { return false; }
        }
    }
}
