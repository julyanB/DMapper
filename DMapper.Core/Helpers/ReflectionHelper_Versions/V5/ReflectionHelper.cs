using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DMapper.Attributes;
using DMapper.Comparers;
using DMapper.Constants;
using DMapper.Helpers.Models;

namespace DMapper.Helpers
{
    /// <summary>
    /// v5 Mapping: uses the flattened representation of source and destination to perform mapping.
    /// Supports [BindTo] and [ComplexBind] attributes.
    /// </summary>
    public static partial class ReflectionHelper
    {

        public static TDestination ReplacePropertiesRecursive_V5<TDestination, TSource>(TDestination destination, TSource source)
        {
            // 1. Flatten the source object (actual instance, with values).
            FlattenResult srcFlatten = ObjectFlattener.Flatten(source, separator: GlobalConstants.DefaultDotSeparator);
            // 2. Flatten the destination structure from its type (values are initially null).
            FlattenResult destStructure = ObjectFlattener.Flatten(typeof(TDestination), separator: GlobalConstants.DefaultDotSeparator);
            // 3. Update destStructure with existing non-null values from the actual destination instance.
            FlattenResult destActual = ObjectFlattener.Flatten(destination, separator: GlobalConstants.DefaultDotSeparator);
            foreach (var key in destActual.Properties.Keys)
            {
                if (destActual.Properties[key].Value != null)
                {
                    destStructure.Properties[key].Value = destActual.Properties[key].Value;
                }
            }

            // 4. Merge direct key matches: if the source value is not null, override the destination.
            foreach (var key in destStructure.Properties.Keys.ToList())
            {
                if (srcFlatten.Properties.TryGetValue(key, out FlattenedProperty srcProp) && srcProp.Value != null)
                {
                    destStructure.Properties[key].Value = srcProp.Value;
                }
            }

            // 5. Process [BindTo] attributes on inner properties.
            foreach (var key in destStructure.Properties.Keys.ToList())
            {
                PropertyInfo leafProp = GetLeafPropertyInfo(typeof(TDestination), key, GlobalConstants.DefaultDotSeparator);
                if (leafProp == null)
                    continue;
                var bindAttr = leafProp.GetCustomAttribute<BindToAttribute>();
                if (bindAttr != null && bindAttr.PropNames.Any())
                {
                    // Determine parent prefix (if any) from the flattened key.
                    string parentPrefix = "";
                    int lastSep = key.LastIndexOf(GlobalConstants.DefaultDotSeparator);
                    if (lastSep > 0)
                        parentPrefix = key.Substring(0, lastSep);
                    
                    foreach (var candidate in bindAttr.PropNames)
                    {
                        // If candidate does not contain the separator and this is an inner property, prepend parent's prefix.
                        string fullCandidate = string.IsNullOrEmpty(parentPrefix)
                            ? candidate
                            : (!candidate.Contains(GlobalConstants.DefaultDotSeparator) ? parentPrefix + GlobalConstants.DefaultDotSeparator + candidate : candidate);
                        if (srcFlatten.Properties.TryGetValue(fullCandidate, out FlattenedProperty srcProp) && srcProp.Value != null)
                        {
                            destStructure.Properties[key].Value = srcProp.Value;
                            break;
                        }
                    }
                }
            }

            // 6. Process [ComplexBind] attributes (using the attribute’s destination key exactly).
            foreach (var prop in typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var complexBindAttrs = prop.GetCustomAttributes<ComplexBindAttribute>();
                foreach (var attr in complexBindAttrs)
                {
                    for (int i = 0; i < Math.Max(attr.PropNames.Count, attr.Froms.Count); i++)
                    {
                        string destKeyCandidate = i < attr.PropNames.Count ? attr.PropNames[i] : attr.PropNames[0];
                        string srcKeyCandidate = i < attr.Froms.Count ? attr.Froms[i] : attr.Froms[0];
                        if (srcFlatten.Properties.TryGetValue(srcKeyCandidate, out FlattenedProperty srcProp) && srcProp.Value != null)
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

            // 7. Rehydrate the destination from the merged flatten result.
            object rehydrated = destStructure.Rehydrate(separator: GlobalConstants.DefaultDotSeparator);
            return (TDestination)rehydrated;
        }

        /// <summary>
        /// Returns the leaf PropertyInfo from the flattened key.
        /// For example, if flattenedKey is ""Source2.SourceName2"" and rootType is Source1,
        /// this method returns the PropertyInfo for Source2.SourceName2.
        /// </summary>
        private static PropertyInfo GetLeafPropertyInfo(Type rootType, string flattenedKey, string separator)
        {
            string[] parts = flattenedKey.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            Type currentType = rootType;
            PropertyInfo leafProp = null;
            foreach (var part in parts)
            {
                leafProp = currentType.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (leafProp == null)
                    return null;
                currentType = leafProp.PropertyType;
            }
            return leafProp;
        }
    }
}
