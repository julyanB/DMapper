using System.Reflection;
using DMapper.Attributes;
using DMapper.Helpers.Models;

namespace DMapper.Helpers
{
    /// <summary>
    /// v5 Mapping: uses the flattened representation of source and destination to perform mapping.
    /// Supports [BindTo] and [ComplexBind] attributes.
    /// </summary>
    public static partial class ReflectionHelper
    {
        private const string DefaultSeparator = ".";
        
        public static TDestination ReplacePropertiesRecursive_V5<TDestination, TSource>(TDestination destination, TSource source)
        {
            // 1. Flatten the source object (with values)
            FlattenResult srcFlatten = ObjectFlattener.Flatten(source, separator: DefaultSeparator);

            // 2. Flatten the structure of the destination type (values are null)
            FlattenResult destStructure = ObjectFlattener.Flatten(typeof(TDestination), separator: DefaultSeparator);

            // 3. Merge the two dictionaries:
            // First, copy matching keys (same name)
            foreach (var key in destStructure.Properties.Keys.ToList())
            {
                if (srcFlatten.Properties.TryGetValue(key, out FlattenedProperty srcProp))
                {
                    destStructure.Properties[key].Value = srcProp.Value;
                }
            }
            // Next, for each destination property, check for [BindTo] attributes
            Type destType = typeof(TDestination);
            foreach (var prop in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var bindAttr = prop.GetCustomAttribute<BindToAttribute>();
                if (bindAttr != null && bindAttr.PropNames.Any())
                {
                    foreach (var candidate in bindAttr.PropNames)
                    {
                        // Look up the candidate key in the source flatten dictionary.
                        if (srcFlatten.Properties.TryGetValue(candidate, out FlattenedProperty srcProp))
                        {
                            // Use the destination property name as the key.
                            destStructure.Properties[prop.Name].Value = srcProp.Value;
                            break;
                        }
                    }
                }
            }
            // Then, process ComplexBind attributes:
            foreach (var prop in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var complexBindAttrs = prop.GetCustomAttributes<ComplexBindAttribute>();
                foreach (var attr in complexBindAttrs)
                {
                    // For each candidate pair, try to find a matching source flattened key.
                    for (int i = 0; i < Math.Max(attr.PropNames.Count, attr.Froms.Count); i++)
                    {
                        string destKeyCandidate = i < attr.PropNames.Count ? attr.PropNames[i] : attr.PropNames[0];
                        string srcKeyCandidate = i < attr.Froms.Count ? attr.Froms[i] : attr.Froms[0];

                        // If the destination candidate starts with the property name, remove that prefix.
                        if (destKeyCandidate.StartsWith(prop.Name + DefaultSeparator, StringComparison.OrdinalIgnoreCase))
                        {
                            destKeyCandidate = destKeyCandidate.Substring(prop.Name.Length + 1);
                        }
                        // Construct the full destination key by combining the property name and the candidate.
                        string fullDestKey = prop.Name + DefaultSeparator + destKeyCandidate;
                        if (srcFlatten.Properties.TryGetValue(srcKeyCandidate, out FlattenedProperty srcProp))
                        {
                            // Update the destination flattened property.
                            destStructure.Properties[fullDestKey].Value = srcProp.Value;
                            break;
                        }
                    }
                }
            }

            // 4. Rehydrate the destination from the merged flatten result.
            object rehydrated = destStructure.Rehydrate(separator: DefaultSeparator);

            return (TDestination)rehydrated;
        }
    }
}
