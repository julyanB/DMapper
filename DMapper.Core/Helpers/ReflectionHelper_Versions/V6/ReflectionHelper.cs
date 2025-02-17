using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DMapper.Attributes;
using DMapper.Constants;
using DMapper.Extensions;
using DMapper.Helpers.FluentConfigurations;
using DMapper.Helpers.FluentConfigurations.Contracts;
using DMapper.Helpers.Models;

namespace DMapper.Helpers
{
    public static partial class ReflectionHelper
    {
        /// <summary>
        /// Performs version 6 mapping between a source and a destination object.
        /// This method:
        /// <list type="bullet">
        ///   <item>
        ///     <description>Flattens the source object into a dictionary of property paths and values.</description>
        ///   </item>
        ///   <item>
        ///     <description>Builds a mapping dictionary from the destination type by iterating over its public properties.
        ///     The dictionary maps flattened destination keys to candidate source property keys (including any custom logic from [BindTo] attributes).</description>
        ///   </item>
        ///   <item>
        ///     <description>Directly assigns source values to the destination properties by matching the candidate keys.</description>
        ///   </item>
        ///   <item>
        ///     <description>Handles collection properties by mapping each element.</description>
        ///   </item>
        ///   <item>
        ///     <description>Processes any [ComplexBind] attributes on the destination to support advanced nested mappings.</description>
        ///   </item>
        /// </list>
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <param name="destination">An instance of the destination object to populate.</param>
        /// <param name="source">The source object from which property values are read.</param>
        /// <returns>The destination object with properties mapped from the source.</returns>
        /// <example>
        /// <code>
        /// // Given a source object (e.g., PersonDto) and a destination object (e.g., Person),
        /// // where Person has properties that may be decorated with [BindTo] or [ComplexBind] attributes:
        /// Person dest = new Person();
        /// PersonDto src = GetPersonDto();
        /// dest = ReflectionHelper.ReplacePropertiesRecursive_V6(dest, src);
        /// </code>
        /// </example>
        public static TDestination ReplacePropertiesRecursive_V6<TDestination, TSource>(TDestination destination, TSource source)
        {
            // 1. Flatten the source and trim keys.
            FlattenResult srcFlatten = ObjectFlattener.Flatten(source, GlobalConstants.DefaultDotSeparator);
            var fixedSrc = srcFlatten.Properties;

            // 2. Build the mapping dictionary from the destination type.
            Dictionary<string, List<string>> mappingDict = BuildMappingDictionary_V6(typeof(TDestination));

            // 3. For each mapping entry, try each candidate source key.
            foreach (var mapping in mappingDict)
            {
                string destKey = mapping.Key;
                foreach (var candidate in mapping.Value)
                {
                    if (fixedSrc.TryGetValue(candidate, out FlattenedProperty srcProp) &&
                        srcProp.Value != null)
                    {
                        SetNestedValueDirect_V6(destination, destKey, srcProp.Value, GlobalConstants.DefaultDotSeparator);
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
        /// Sets a nested property value on the destination object by traversing its flattened key.
        /// Creates intermediate objects if necessary.
        /// <para>
        /// If the destination property is a collection (implements IList) and the source value is an IEnumerable (but not a string),
        /// each element of the collection is mapped to the destination collection type.
        /// </para>
        /// </summary>
        /// <param name="destination">The destination object whose property is to be set.</param>
        /// <param name="flattenedKey">The dot-separated property path (e.g., "Address.Street").</param>
        /// <param name="value">The value to set on the final property.</param>
        /// <param name="separator">The separator used in the flattened key, typically a dot (".").</param>
        /// <example>
        /// For a flattened key "Address.Street", this method will:
        /// <list type="number">
        ///   <item>Retrieve the "Address" property from the destination object.</item>
        ///   <item>If "Address" is null, instantiate a new Address object.</item>
        ///   <item>Set the "Street" property of the Address object to the provided value.</item>
        /// </list>
        /// </example>
        private static void SetNestedValueDirect_V6(object destination, string flattenedKey, object value, string separator)
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
                        IList destList = CreateAndMapCollection_V6(srcEnumerable, destElementType);

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
                            // Swallow conversion exceptions to allow mapping to continue.
                        }
                    }
                }
                else
                {
                    // For intermediate properties, retrieve or create the object instance.
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
        /// Builds a mapping dictionary for the destination type.
        /// The dictionary maps each flattened destination property path to a list of candidate source property paths.
        /// Candidate source keys may come from the property name itself or from a [BindTo] attribute.
        /// </summary>
        /// <param name="destinationType">The destination type whose properties will be mapped.</param>
        /// <returns>
        /// A dictionary where each key is a flattened property path from the destination type and
        /// each value is a list of candidate source property paths.
        /// </returns>
        /// <example>
        /// For a destination type with a property "Address" decorated with [BindTo("Street,City")],
        /// this method might produce an entry similar to: 
        /// "Address" -> { "Address.Street", "Address.City" }
        /// </example>
        private static Dictionary<string, List<string>> BuildMappingDictionary_V6(Type destinationType)
        {
            // (1) Build the dictionary from reflection/attributes.
            var mapping = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            BuildMappingDictionaryRecursive_V6(destinationType, "", mapping, null, new HashSet<Type>());

            // (2) Check if the destination type implements the fluent configuration interface.
            if (typeof(IDMapperConfiguration).IsAssignableFrom(destinationType))
            {
                // Create an instance to access its configuration.
                var instance = Activator.CreateInstance(destinationType) as IDMapperConfiguration;
                if (instance != null)
                {
                    var builder = new DMapperConfigure();
                    instance.ConfigureMapping(builder);
                    var fluentMappings = builder.GetMappings();

                    foreach (var kvp in fluentMappings)
                    {
                        mapping[kvp.Key] = kvp.Value;
                    }
                }
            }

            return mapping;
        }


        /// <summary>
        /// Recursively builds the mapping dictionary by traversing the destination type's property hierarchy.
        /// </summary>
        /// <param name="type">The current type being traversed.</param>
        /// <param name="destPrefix">
        /// The current flattened property prefix (e.g., "Address" when processing Address.Street).
        /// </param>
        /// <param name="mapping">
        /// The mapping dictionary being built. Keys are flattened property paths, and values are lists of candidate source keys.
        /// </param>
        /// <param name="effectiveSourcePrefix">
        /// The current prefix used for constructing source keys, which may be adjusted based on [BindTo] attributes.
        /// </param>
        /// <param name="visited">
        /// A set of visited types to prevent cyclical recursion.
        /// </param>
        private static void BuildMappingDictionaryRecursive_V6(
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
                // Build the current flattened destination key.
                string currentDestKey = string.IsNullOrEmpty(destPrefix)
                    ? prop.Name
                    : destPrefix + GlobalConstants.DefaultDotSeparator + prop.Name;

                string newEffectiveSourcePrefix;
                var bindAttr = prop.GetCustomAttribute<BindToAttribute>();
                if (bindAttr != null && bindAttr.PropNames.Any())
                {
                    // If the property has a [BindTo] attribute, use its candidate names.
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
                    // If no [BindTo] attribute, use the property name (with proper prefixing) as the candidate.
                    string candidateSource = effectiveSourcePrefix != null
                        ? effectiveSourcePrefix + GlobalConstants.DefaultDotSeparator + prop.Name
                        : currentDestKey;
                    newEffectiveSourcePrefix = candidateSource;
                    mapping[currentDestKey] = new List<string> { candidateSource };
                }

                // Recursively build the mapping dictionary for nested class properties.
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                {
                    BuildMappingDictionaryRecursive_V6(prop.PropertyType, currentDestKey, mapping, newEffectiveSourcePrefix, new HashSet<Type>(visited));
                }
            }
        }

        #endregion

        #region CreateAndMapCollection Helper

        /// <summary>
        /// Creates a new IList of the appropriate type and maps each element from the source enumerable
        /// to the destination element type.
        /// </summary>
        /// <param name="srcEnumerable">The source collection to map from.</param>
        /// <param name="destElementType">The type of elements expected in the destination collection.</param>
        /// <returns>An IList containing the mapped elements.</returns>
        /// <example>
        /// If <c>srcEnumerable</c> is an IEnumerable of objects representing addresses and
        /// <c>destElementType</c> is <c>Address</c>, this method will map each object to an instance of <c>Address</c>.
        /// </example>
        private static IList CreateAndMapCollection_V6(IEnumerable srcEnumerable, Type destElementType)
        {
            var listType = typeof(List<>).MakeGenericType(destElementType);
            var destList = (IList)Activator.CreateInstance(listType);
            foreach (var srcElem in srcEnumerable)
            {
                object destElem;
                if (srcElem != null && destElementType.IsClass && destElementType != typeof(string))
                {
                    destElem = MapToObject_V6(srcElem, destElementType);
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
        /// Maps the given source object to a new instance of the destination type using version 6 mapping.
        /// This helper uses reflection to invoke <see cref="ReplacePropertiesRecursive_V6{TDestination, TSource}"/>.
        /// </summary>
        /// <param name="source">The source object to map from.</param>
        /// <param name="destinationType">The type to map the source object to.</param>
        /// <returns>
        /// A new instance of <paramref name="destinationType"/> with properties mapped from the source.
        /// </returns>
        /// <example>
        /// If <c>source</c> is an instance of <c>PersonDto</c> and <c>destinationType</c> is <c>Person</c>,
        /// this method creates a new <c>Person</c> and maps properties from <c>PersonDto</c> to <c>Person</c>.
        /// </example>
        private static object MapToObject_V6(object source, Type destinationType)
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
        /// Processes all destination properties decorated with the [ComplexBind] attribute.
        /// For each such attribute, if a matching source candidate value is found,
        /// the value is set on the destination property (or its nested property if a dot-notated path is specified).
        /// </summary>
        /// <param name="source">The source object from which to retrieve values.</param>
        /// <param name="destination">The destination object where values will be assigned.</param>
        /// <example>
        /// Suppose a destination property is decorated with:
        /// <code>
        /// [ComplexBind("Address.Street", "StreetName")]
        /// </code>
        /// If the source object contains a property named <c>StreetName</c> with a non-null value,
        /// this method will assign that value to the nested property <c>Address.Street</c> of the destination.
        /// </example>
        private static void ProcessComplexBindAttributes_V6(object source, object destination)
        {
            var destType = destination.GetType();
            FlattenResult srcFlatten = ObjectFlattener.Flatten(source, GlobalConstants.DefaultDotSeparator);
            var fixedSrc = new Dictionary<string, FlattenedProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in srcFlatten.Properties)
            {
                // Trim any leading separators from the source key.
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
                                // If destPath is empty, assign directly to the property.
                                prop.SetValue(destination, srcProp.Value);
                            }
                            else
                            {
                                // Otherwise, set the nested property value.
                                SetNestedValueDirect_V6(destination, destPath, srcProp.Value, GlobalConstants.DefaultDotSeparator);
                            }

                            break; // Use the first matching candidate.
                        }
                    }
                }
            }
        }

        #endregion
    }
}
