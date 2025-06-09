using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DMapper.Constants;
using DMapper.Converters;
using DMapper.Helpers.Models;

namespace DMapper.Helpers;

public static partial class ReflectionHelper
{
    // Cache mapping delegates per destination type
    private static readonly ConcurrentDictionary<Type, Action<object, Dictionary<string, FlattenedProperty>>> _delegateCache_V7 = new();

    /// <summary>
    /// Performs version 7 mapping using cached delegates built from the version 6 mapping information.
    /// </summary>
    public static TDestination ReplacePropertiesRecursive_V7<TDestination, TSource>(TDestination destination, TSource source)
    {
        FlattenResult srcFlatten = ObjectFlattener.Flatten(source, GlobalConstants.DefaultDotSeparator);
        var fixedSrc = srcFlatten.Properties;

        var action = _delegateCache_V7.GetOrAdd(typeof(TDestination), BuildDelegate_V7<TDestination>);
        action(destination, fixedSrc);

        // Reuse existing ComplexBind logic
        ProcessComplexBindAttributes_V6(source, destination);

        return destination;
    }

    private static Action<object, Dictionary<string, FlattenedProperty>> BuildDelegate_V7<TDestination>(Type destType)
    {
        var mappingDict = _mappingCache_V6.GetOrAdd(destType, t => BuildMappingDictionary_V6(t));
        var converterCache = _converterCache_V6.GetOrAdd(destType, t => BuildConverterCache_V6(t));

        // Cache property chains for faster access
        var chainCache = new Dictionary<string, PropertyInfo[]>();
        PropertyInfo[] GetChain(string destKey)
        {
            if (chainCache.TryGetValue(destKey, out var chain))
                return chain;

            var parts = destKey.Split(GlobalConstants.DefaultDotSeparator);
            var list = new List<PropertyInfo>();
            Type current = destType;
            foreach (var part in parts)
            {
                var prop = current.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                {
                    list.Clear();
                    break;
                }
                list.Add(prop);
                current = prop.PropertyType;
            }
            chain = list.ToArray();
            chainCache[destKey] = chain;
            return chain;
        }

        void Setter(object dest, string destKey, object value)
        {
            var chain = GetChain(destKey);
            if (chain.Length == 0) return;

            object current = dest;
            for (int i = 0; i < chain.Length; i++)
            {
                var prop = chain[i];
                bool last = i == chain.Length - 1;
                if (last)
                {
                    AssignValue(prop, current, value);
                }
                else
                {
                    object next = prop.GetValue(current);
                    if (next == null)
                    {
                        try
                        {
                            next = Activator.CreateInstance(prop.PropertyType);
                            prop.SetValue(current, next);
                        }
                        catch
                        {
                            return;
                        }
                    }

                    current = next;
                }
            }
        }

        return (destObj, src) =>
        {
            foreach (var mapping in mappingDict)
            {
                string destKey = mapping.Key;
                foreach (string candidate in mapping.Value)
                {
                    if (!src.TryGetValue(candidate, out var srcProp) || srcProp.Value is null)
                        continue;

                    object valueToAssign = srcProp.Value;
                    if (converterCache.TryGetValue(destKey, out var conv) && conv != null)
                    {
                        valueToAssign = conv.Convert(valueToAssign);
                    }

                    Setter(destObj, destKey, valueToAssign);
                    break;
                }
            }
        };

        // Local helper mirrors logic from SetNestedValueDirect_V6 but uses PropertyInfo chain
        void AssignValue(PropertyInfo prop, object instance, object value)
        {
            try
            {
                if (typeof(IList).IsAssignableFrom(prop.PropertyType) && value is IEnumerable srcEnum && value is not string)
                {
                    Type destElementType = prop.PropertyType.IsArray
                        ? prop.PropertyType.GetElementType()!
                        : (prop.PropertyType.IsGenericType
                            ? prop.PropertyType.GetGenericArguments().First()
                            : typeof(object));

                    IList destList = CreateAndMapCollection_V6(srcEnum, destElementType);
                    if (prop.PropertyType.IsArray)
                    {
                        Array arr = Array.CreateInstance(destElementType, destList.Count);
                        destList.CopyTo(arr, 0);
                        prop.SetValue(instance, arr);
                    }
                    else
                    {
                        prop.SetValue(instance, destList);
                    }
                    return;
                }

                Type targetType = prop.PropertyType;
                Type enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                if (enumType.IsEnum)
                {
                    if (Enum.TryParse(enumType, value.ToString(), true, out var enumVal))
                        prop.SetValue(instance, enumVal);
                    return;
                }

                if (targetType == value?.GetType() || targetType.IsAssignableFrom(value?.GetType()))
                {
                    prop.SetValue(instance, value);
                    return;
                }

                if (TrySpecialConvert(value, targetType, out var special))
                {
                    prop.SetValue(instance, special);
                    return;
                }

                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    targetType = Nullable.GetUnderlyingType(targetType)!;

                object converted = Convert.ChangeType(value, targetType);
                prop.SetValue(instance, converted);
            }
            catch
            {
                // Ignore failed assignments
            }
        }
    }
}

