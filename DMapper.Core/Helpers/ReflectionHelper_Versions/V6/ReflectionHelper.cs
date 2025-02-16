using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DMapper.Attributes;
using DMapper.Constants;
using DMapper.Helpers.Models;

namespace DMapper.Helpers
{
    public static partial class ReflectionHelper
    {
        /// <summary>
        /// v6 Mapping (Direct Assignment with Collection Support and Revised Mapping Dictionary):
        /// - Flattens the source (trimming leading separators).
        /// - Builds a mapping dictionary from the destination type by iterating over public properties.
        /// - Uses the mapping dictionary to directly assign destination properties.
        /// - For collection properties, maps each element (using a recursive mapping helper).
        /// - Also processes [ComplexBind] attributes.
        /// </summary>
        public static TDestination ReplacePropertiesRecursive_V6<TDestination, TSource>(TDestination destination, TSource source)
        {
            // 1. Flatten the source and trim keys.
            FlattenResult srcFlatten = ObjectFlattener.Flatten(source, GlobalConstants.DefaultDotSeparator);
            var fixedSrc = new Dictionary<string, FlattenedProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in srcFlatten.Properties)
            {
                string trimmedKey = kvp.Key.TrimStart(GlobalConstants.DefaultDotSeparator.ToCharArray());
                fixedSrc[trimmedKey] = kvp.Value;
            }

            // 2. Build the mapping dictionary from the destination type.
            Dictionary<string, List<string>> mappingDict = BuildMappingDictionary(typeof(TDestination));

            // 3. For each mapping entry, try each candidate source key.
            foreach (var mapping in mappingDict)
            {
                string destKey = mapping.Key;
                foreach (var candidate in mapping.Value)
                {
                    if (fixedSrc.TryGetValue(candidate, out FlattenedProperty srcProp) &&
                        srcProp.Value != null)
                    {
                        SetNestedValueDirect(destination, destKey, srcProp.Value, GlobalConstants.DefaultDotSeparator);
                        break; // Use the first candidate that yields a value.
                    }
                }
            }

            // 4. Process any ComplexBind attributes.
            ProcessComplexBindAttributes_V6(source, destination);

            return destination;
        }

        #region Direct Nested-Assignment Helper

        /// <summary>
        /// Splits the flattened key by the separator and traverses (creating intermediate objects if needed)
        /// to set the final property on the destination instance.
        /// If the destination property is a collection and the source value is enumerable, maps each element.
        /// </summary>
        private static void SetNestedValueDirect(object destination, string flattenedKey, object value, string separator)
        {
            string[] parts = flattenedKey.Split(new string[] { separator }, StringSplitOptions.None);
            object current = destination;
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                PropertyInfo prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                {
                    return;
                }

                bool isLast = i == parts.Length - 1;
                if (isLast)
                {
                    // If the destination property is a collection (IList) and the source value is an IEnumerable (but not string)
                    if (typeof(IList).IsAssignableFrom(prop.PropertyType) && value is IEnumerable srcEnumerable && !(value is string))
                    {
                        // Determine the element type of the destination collection.
                        Type destElementType = prop.PropertyType.IsArray
                            ? prop.PropertyType.GetElementType()
                            : (prop.PropertyType.IsGenericType
                                ? prop.PropertyType.GetGenericArguments().FirstOrDefault()
                                : typeof(object));

                        // Create and map the collection elements.
                        IList destList = CreateAndMapCollection(srcEnumerable, destElementType);

                        // If the destination property is an array, convert the list to an array.
                        if (prop.PropertyType.IsArray)
                        {
                            Array array = Array.CreateInstance(destElementType, destList.Count);
                            destList.CopyTo(array, 0);
                            prop.SetValue(current, array);
                        }
                        else
                        {
                            prop.SetValue(current, destList);
                        }
                    }
                    else
                    {
                        // For non-collection properties, perform the usual assignment.
                        try
                        {
                            Type targetType = prop.PropertyType;
                            // Handle enum conversion.
                            if (targetType.IsEnum)
                            {
                                object enumVal = Enum.Parse(targetType, value.ToString());
                                prop.SetValue(current, enumVal);
                            }
                            else if (targetType == value.GetType() || targetType.IsAssignableFrom(value.GetType()))
                            {
                                prop.SetValue(current, value);
                            }
                            else
                            {
                                // Handle nullable types.
                                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    targetType = Nullable.GetUnderlyingType(targetType);
                                }

                                object converted = Convert.ChangeType(value, targetType);
                                prop.SetValue(current, converted);
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
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
                        catch (Exception)
                        {
                            return;
                        }
                    }

                    current = next;
                }
            }
        }

        #endregion

        #region Mapping Dictionary Builder

        /// <summary>
        /// Builds a dictionary mapping each destination flattened key to a list of candidate source keys.
        /// This method iterates over public properties if the type is a class (except for string)
        /// and uses a per-branch visited set to break cycles.
        /// </summary>
        private static Dictionary<string, List<string>> BuildMappingDictionary(Type destinationType)
        {
            var mapping = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            BuildMappingDictionaryRecursive(destinationType, "", mapping, null, new HashSet<Type>());
            return mapping;
        }

        private static void BuildMappingDictionaryRecursive(
            Type type,
            string destPrefix,
            Dictionary<string, List<string>> mapping,
            string effectiveSourcePrefix,
            HashSet<Type> visited)
        {
            if (type == null || !(type.IsClass && type != typeof(string)))
                return;

            if (visited.Contains(type))
                return;

            visited.Add(type);

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                string currentDestKey = string.IsNullOrEmpty(destPrefix)
                    ? prop.Name
                    : destPrefix + GlobalConstants.DefaultDotSeparator + prop.Name;

                string newEffectiveSourcePrefix;
                var bindAttr = prop.GetCustomAttribute<BindToAttribute>();
                if (bindAttr != null && bindAttr.PropNames.Any())
                {
                    var candidates = new List<string>();
                    foreach (var candidate in bindAttr.PropNames)
                    {
                        if (candidate.Contains(GlobalConstants.DefaultDotSeparator))
                        {
                            candidates.Add(candidate);
                        }
                        else
                        {
                            candidates.Add(effectiveSourcePrefix != null
                                ? effectiveSourcePrefix + GlobalConstants.DefaultDotSeparator + candidate
                                : candidate);
                        }
                    }

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

                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    BuildMappingDictionaryRecursive(prop.PropertyType, currentDestKey, mapping, newEffectiveSourcePrefix, new HashSet<Type>(visited));
                }
            }
        }

        #endregion

        #region CreateAndMapCollection Helper

        /// <summary>
        /// Creates a new IList of the appropriate type and iterates through the source enumerable,
        /// mapping each element using a helper that maps the source element to the destination type.
        /// </summary>
        private static IList CreateAndMapCollection(IEnumerable srcEnumerable, Type destElementType)
        {
            var listType = typeof(List<>).MakeGenericType(destElementType);
            var destList = (IList)Activator.CreateInstance(listType);
            foreach (var srcElem in srcEnumerable)
            {
                object destElem;
                if (srcElem != null && destElementType.IsClass && destElementType != typeof(string))
                {
                    destElem = MapToObject(srcElem, destElementType);
                }
                else
                {
                    destElem = srcElem;
                }

                destList.Add(destElem);
            }

            return destList;
        }

        /// <summary>
        /// Maps the given source object to a new instance of the destination type using V6 mapping.
        /// This helper uses reflection to invoke ReplacePropertiesRecursive_V6.
        /// </summary>
        private static object MapToObject(object source, Type destinationType)
        {
            object dest = Activator.CreateInstance(destinationType);
            MethodInfo method = typeof(ReflectionHelper)
                .GetMethod(nameof(ReplacePropertiesRecursive_V6), BindingFlags.Public | BindingFlags.Static);
            MethodInfo genericMethod = method.MakeGenericMethod(destinationType, source.GetType());
            return genericMethod.Invoke(null, new object[] { dest, source });
        }

        #endregion

        #region ComplexBind Processing

        /// <summary>
        /// Processes all properties decorated with [ComplexBind] on the destination instance.
        /// For each such attribute, if a source candidate value is found, it is set onto the destination.
        /// </summary>
        private static void ProcessComplexBindAttributes_V6(object source, object destination)
        {
            var destType = destination.GetType();
            FlattenResult srcFlatten = ObjectFlattener.Flatten(source, GlobalConstants.DefaultDotSeparator);
            var fixedSrc = new Dictionary<string, FlattenedProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in srcFlatten.Properties)
            {
                string trimmedKey = kvp.Key.TrimStart(GlobalConstants.DefaultDotSeparator.ToCharArray());
                fixedSrc[trimmedKey] = kvp.Value;
            }

            foreach (var prop in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var complexBindAttrs = prop.GetCustomAttributes(typeof(ComplexBindAttribute), false)
                    .Cast<ComplexBindAttribute>();
                foreach (var attr in complexBindAttrs)
                {
                    for (int i = 0; i < Math.Max(attr.PropNames.Count, attr.Froms.Count); i++)
                    {
                        string destPath = i < attr.PropNames.Count ? attr.PropNames[i] : attr.PropNames[0];
                        string srcPath = i < attr.Froms.Count ? attr.Froms[i] : attr.Froms[0];

                        if (fixedSrc.TryGetValue(srcPath, out FlattenedProperty srcProp) &&
                            srcProp.Value != null)
                        {
                            if (string.IsNullOrWhiteSpace(destPath))
                            {
                                prop.SetValue(destination, srcProp.Value);
                            }
                            else
                            {
                                SetNestedValueDirect(destination, destPath, srcProp.Value, GlobalConstants.DefaultDotSeparator);
                            }

                            break;
                        }
                    }
                }
            }
        }

        #endregion
    }
}