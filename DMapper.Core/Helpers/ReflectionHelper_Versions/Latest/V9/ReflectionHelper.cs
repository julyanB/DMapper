using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using DMapper.Constants;
using DMapper.Converters;

namespace DMapper.Helpers
{
    /// <summary>
    /// DMapper V9: precomputed mapping plan with cached delegates and minimal reflection.
    /// </summary>
    public static partial class ReflectionHelper
    {
        public static TDestination ReplacePropertiesRecursive_V9<TDestination, TSource>(
            TDestination destination, TSource source)
        {
            if (destination is null || source is null) return destination;

                        var srcRuntimeType = source!.GetType();
            var plan = _compiledPlanCacheV9.GetOrAdd((srcRuntimeType, typeof(TDestination)), BuildMappingPlanV9);

                        var srcObj = (object)source!;
            var destObj = (object)destination!;

            foreach (var entry in plan)
            {
                foreach (var getter in entry.Getters)
                {
                    var candidateValue = getter(srcObj);
                    if (candidateValue == null) continue;

                    if (entry.Converter != null)
                    {
                        candidateValue = entry.Converter.Convert(candidateValue);
                    }

                    entry.Setter(destObj, candidateValue);
                    break;
                }
            }

            ProcessComplexBindAttributes_V6(srcObj, destObj);
            return destination;
        }

        private static readonly ConcurrentDictionary<(Type Src, Type Dest), MappingPlanEntryV9[]> _compiledPlanCacheV9 = new();
        private static readonly ConcurrentDictionary<(Type Type, string Path), Func<object, object>> _getterCacheV9 = new();

        private static MappingPlanEntryV9[] BuildMappingPlanV9((Type Src, Type Dest) key)
        {
            var (srcType, destType) = key;
            var mapping = _mapCacheV8.GetOrAdd(destType, BuildMappingDictionary_V8);
            var converters = _convCacheV8.GetOrAdd(destType, BuildConverterCache_V8);

            var entries = new List<MappingPlanEntryV9>(mapping.Count);
            foreach (var kvp in mapping)
            {
                var destKey = kvp.Key;
                var candidates = kvp.Value;
                if (candidates == null || candidates.Count == 0) continue;

                var getters = new Func<object, object>[candidates.Count];
                for (int i = 0; i < candidates.Count; i++)
                {
                    getters[i] = _getterCacheV9.GetOrAdd((srcType, candidates[i]), BuildGetterOptimized);
                }

                if (getters.Length == 0) continue;

                var setter = _setterCache.GetOrAdd((destType, destKey), BuildSetter);
                converters.TryGetValue(destKey, out var converter);

                entries.Add(new MappingPlanEntryV9(destKey, setter, getters, converter));
            }

            return entries.ToArray();
        }

        private static Func<object, object> BuildGetterOptimized((Type Type, string Path) key)
        {
            var (_, path) = key;
            if (string.IsNullOrWhiteSpace(path))
            {
                return _ => null;
            }

            var parts = path.Split(new[] { GlobalConstants.DefaultDotSeparator }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return _ => null;
            }

            var chain = new PropertyInfo[parts.Length];
            Type currentType = key.Type;
            for (int i = 0; i < parts.Length; i++)
            {
                var prop = currentType.GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                {
                    return _ => null;
                }

                chain[i] = prop;
                currentType = prop.PropertyType;
            }

            return src =>
            {
                if (src == null) return null;
                object current = src;
                for (int i = 0; i < chain.Length; i++)
                {
                    if (current == null) return null;
                    current = chain[i].GetValue(current);
                }

                return current;
            };
        }

        private sealed class MappingPlanEntryV9
        {
            public MappingPlanEntryV9(string destinationKey, Action<object, object> setter, Func<object, object>[] getters, IDMapperPropertyConverter? converter)
            {
                DestinationKey = destinationKey;
                Setter = setter;
                Getters = getters;
                Converter = converter;
            }

            public string DestinationKey { get; }
            public Action<object, object> Setter { get; }
            public Func<object, object>[] Getters { get; }
            public IDMapperPropertyConverter? Converter { get; }
        }
    }
}
