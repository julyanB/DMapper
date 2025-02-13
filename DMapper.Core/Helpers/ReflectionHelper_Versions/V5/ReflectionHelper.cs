using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DMapper.Attributes;
using DMapper.Comparers;
using DMapper.Helpers.Models;

namespace DMapper.Helpers;

/// <summary>
/// v5 Mapping: uses the flattened representation of source and destination to perform mapping.
/// Supports [BindTo] and [ComplexBind] attributes.
/// </summary>
public static partial class ReflectionHelper
{
    private const string DefaultSeparator = ".";

    /// <summary>
    /// v5 Mapping: uses the flattened representation of source and destination to perform mapping.
    /// Supports [BindTo] and [ComplexBind] attributes.
    /// </summary>
    public static TDestination ReplacePropertiesRecursive_V5<TDestination, TSource>(TDestination destination, TSource source)
    {
        // 1. Flatten the source object (with values)
        FlattenResult srcFlatten = ObjectFlattener.Flatten(source, separator: DefaultSeparator);

        // 2. Flatten the structure of the destination type (values are null)
        FlattenResult destStructure = ObjectFlattener.Flatten(typeof(TDestination), separator: DefaultSeparator);

        // 3. Merge the two dictionaries:
        // Copy matching keys (same name)
        foreach (var key in destStructure.Properties.Keys.ToList())
        {
            if (srcFlatten.Properties.TryGetValue(key, out FlattenedProperty srcProp))
            {
                destStructure.Properties[key].Value = srcProp.Value;
            }
        }

        // Next, check for [BindTo] attributes.
        Type destType = typeof(TDestination);
        foreach (var prop in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var bindAttr = prop.GetCustomAttribute<BindToAttribute>();
            if (bindAttr != null && bindAttr.PropNames.Any())
            {
                foreach (var candidate in bindAttr.PropNames)
                {
                    if (srcFlatten.Properties.TryGetValue(candidate, out FlattenedProperty srcProp))
                    {
                        destStructure.Properties[prop.Name].Value = srcProp.Value;
                        break;
                    }
                }
            }
        }

        // Then, process ComplexBind attributes.
        // Here, we use the destination key exactly as provided in the attribute.
        foreach (var prop in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var complexBindAttrs = prop.GetCustomAttributes<ComplexBindAttribute>();
            foreach (var attr in complexBindAttrs)
            {
                for (int i = 0; i < Math.Max(attr.PropNames.Count, attr.Froms.Count); i++)
                {
                    string destKeyCandidate = i < attr.PropNames.Count ? attr.PropNames[i] : attr.PropNames[0];
                    string srcKeyCandidate = i < attr.Froms.Count ? attr.Froms[i] : attr.Froms[0];

                    // Use the candidate as an absolute key.
                    if (srcFlatten.Properties.TryGetValue(srcKeyCandidate, out FlattenedProperty srcProp))
                    {
                        if (destStructure.Properties.ContainsKey(destKeyCandidate))
                        {
                            destStructure.Properties[destKeyCandidate].Value = srcProp.Value;
                        }

                        break;
                    }
                }
            }
        }

        // 4. Rehydrate the destination from the merged flatten result.
        object rehydrated = destStructure.Rehydrate(separator: DefaultSeparator);
        // Finally, process ComplexBind attributes recursively on the rehydrated object.
        ProcessComplexBindAttributes_V4(source, rehydrated);
        return (TDestination)rehydrated;
    }
}