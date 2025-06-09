using System.Collections;
using System.ComponentModel;
using System.Reflection;
using DMapper.Attributes;
using DMapper.Comparers;
using DMapper.Helpers.Models;

namespace DMapper.Helpers;

public static partial class ReflectionHelper
{

    /// <summary>
    /// Recursively replaces properties from the source object into the destination object.
    /// Supports:
    ///   - [BindTo] on destination properties (with dot‑notation and fallback to the destination name)
    ///   - [RootBindTo] on destination properties to map a nested destination property from a source property.
    /// </summary>
    public static TDestination ReplacePropertiesRecursive_V4<TDestination, TSource>(TDestination destination, TSource source)
    {
        // Here, rootSource and currentSource start as the same object.
        var visited = new HashSet<object>(new ReferenceComparer());
        ReplacePropertiesRecursiveInternal_V4(source, source, destination, visited);
        ProcessComplexBindAttributes_V4(source, destination);
        return destination;
    }
    
    #region V4 Helper Methods

    /// <summary>
    /// Internal recursive implementation for property mapping.
    /// </summary>
    private static object ReplacePropertiesRecursiveInternal_V4(object rootSource, object currentSource, object destination, HashSet<object> visited)
    {
        if (currentSource == null || destination == null)
            return destination;

        // Avoid cycles.
        if (!IsSimpleType(currentSource.GetType()) && !visited.Add(currentSource))
            return destination;

        Type currentSourceType = currentSource.GetType();
        Type destinationType = destination.GetType();

        // For nested mappings (currentSource != rootSource), use the current source type for building property chains.
        Type effectiveSourceType = (currentSourceType == rootSource.GetType())
            ? rootSource.GetType()
            : currentSourceType;

        // Use the effective source type for caching.
        var mappings = GetPropertyMappings_V4(effectiveSourceType, effectiveSourceType, destinationType);

        foreach (var mapping in mappings)
        {
            var bindToAttr = mapping.DestinationProperty.GetCustomAttribute<Attributes.BindToAttribute>();
            // When a BindTo attribute is present and we are at the top level, evaluate against rootSource.
            object srcValue = (bindToAttr != null && currentSourceType == rootSource.GetType())
                ? GetValueFromChain_V4(rootSource, mapping.SourcePropertyChain)
                : GetValueFromChain_V4(currentSource, mapping.SourcePropertyChain);

            if (srcValue == null)
                continue;

            Type srcPropType = mapping.SourcePropertyChain.Last().PropertyType;
            Type destPropType = mapping.DestinationProperty.PropertyType;

            if (destPropType.IsEnum)
            {
                string stringValue = srcValue.ToString();
                if (Enum.GetNames(destPropType).Contains(stringValue))
                {
                    var enumValue = Enum.Parse(destPropType, stringValue);
                    mapping.DestinationProperty.SetValue(destination, enumValue);
                }

                continue;
            }

            if (typeof(IEnumerable).IsAssignableFrom(destPropType) && destPropType != typeof(string))
            {
                HandleCollection_V4(rootSource, mapping.DestinationProperty, destination, srcValue, visited);
                continue;
            }

            if (srcPropType.IsClass && srcPropType != typeof(string))
            {
                if (destPropType.IsAbstract || destPropType.IsInterface)
                    continue;

                var destValue = mapping.DestinationProperty.GetValue(destination);
                if (destValue == null)
                {
                    var ctor = MapperHelperCaches_V4.GetParameterlessConstructor(destPropType);
                    if (ctor == null)
                        continue;
                    destValue = ctor.Invoke(null);
                    mapping.DestinationProperty.SetValue(destination, destValue);
                }

                ReplacePropertiesRecursiveInternal_V4(rootSource, srcValue, destValue, visited);
            }
            else
            {
                if (destPropType.IsAssignableFrom(srcValue.GetType()))
                {
                    mapping.DestinationProperty.SetValue(destination, srcValue);
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(srcValue);
                    if (converter != null && converter.CanConvertTo(destPropType))
                    {
                        var convertedValue = converter.ConvertTo(srcValue, destPropType);
                        mapping.DestinationProperty.SetValue(destination, convertedValue);
                    }
                    else if (srcValue is IConvertible && typeof(IConvertible).IsAssignableFrom(destPropType))
                    {
                        var convertedValue = Convert.ChangeType(srcValue, destPropType);
                        mapping.DestinationProperty.SetValue(destination, convertedValue);
                    }
                }
            }
        }

        return destination;
    }
    

    /// <summary>
    /// Retrieves (or builds) the mapping between destination properties and source property chains.
    /// Supports dot‑notation in the [BindTo] attribute and always adds the destination property name as a fallback.
    /// </summary>
    private static List<PropertyMapping> GetPropertyMappings_V4(Type currentSourceType, Type dummy, Type destinationType)
    {
        // We ignore the second parameter and use currentSourceType for building chains.
        return MapperHelperCaches_V4.MappingCache.GetOrAdd(
            (currentSourceType, currentSourceType, destinationType),
            key =>
            {
                var mappings = new List<PropertyMapping>();
                var destProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var destProperty in destProperties)
                {
                    if (!destProperty.CanWrite)
                        continue;
                    if (destProperty.GetCustomAttribute<Attributes.CopyIgnoreAttribute>() != null)
                        continue;

                    List<string> sourceNames = new List<string>();
                    var bindToAttr = destProperty.GetCustomAttribute<BindToAttribute>();
                    if (bindToAttr?.PropNames is { Count: > 0 })
                        sourceNames.AddRange(bindToAttr.PropNames);
                    // Always add the destination property name as fallback.
                    if (!sourceNames.Contains(destProperty.Name))
                        sourceNames.Add(destProperty.Name);

                    PropertyInfo[] sourceChain = null;
                    // Always build the chain on the current source type.
                    Type typeToUse = currentSourceType;

                    foreach (var name in sourceNames)
                    {
                        var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
                        List<PropertyInfo> chain = new List<PropertyInfo>();
                        Type current = typeToUse;
                        bool valid = true;
                        foreach (var part in parts)
                        {
                            var prop = current.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                            if (prop == null)
                            {
                                valid = false;
                                break;
                            }

                            chain.Add(prop);
                            current = prop.PropertyType;
                        }

                        if (valid && chain.Count > 0)
                        {
                            sourceChain = chain.ToArray();
                            break; // Use the first valid chain.
                        }
                    }

                    if (sourceChain != null)
                    {
                        mappings.Add(new PropertyMapping
                        {
                            DestinationProperty = destProperty,
                            SourcePropertyChain = sourceChain
                        });
                    }
                }

                return mappings;
            });
    }


    /// <summary>
    /// Retrieves a nested property value by traversing the provided property chain.
    /// </summary>
    private static object GetValueFromChain_V4(object source, PropertyInfo[] chain)
    {
        object current = source;
        foreach (var prop in chain)
        {
            if (current == null)
                return null;
            current = prop.GetValue(current);
        }

        return current;
    }

    /// <summary>
    /// Handles collections (arrays and IList types).
    /// </summary>
    private static void HandleCollection_V4(object rootSource, PropertyInfo destinationProperty, object destination, object sourceValue, HashSet<object> visited)
    {
        var destPropType = destinationProperty.PropertyType;

        if (destPropType.IsArray)
        {
            if (!(sourceValue is Array sourceArray))
                return;

            var elementType = destPropType.GetElementType();
            var destArray = Array.CreateInstance(elementType, sourceArray.Length);

            for (int i = 0; i < sourceArray.Length; i++)
            {
                var sourceElement = sourceArray.GetValue(i);
                object destElement;

                if (sourceElement != null && elementType.IsClass && elementType != typeof(string))
                {
                    var ctor = MapperHelperCaches_V4.GetParameterlessConstructor(elementType);
                    if (ctor == null)
                    {
                        destElement = null;
                    }
                    else
                    {
                        destElement = ctor.Invoke(null);
                        // Pass rootSource, sourceElement as current, destElement, and visited.
                        ReplacePropertiesRecursiveInternal_V4(rootSource, sourceElement, destElement, visited);
                    }
                }
                else
                {
                    destElement = ConvertElement_V4(sourceElement, elementType);
                }

                destArray.SetValue(destElement, i);
            }

            destinationProperty.SetValue(destination, destArray);
        }
        else if (typeof(IList).IsAssignableFrom(destPropType))
        {
            var destList = destinationProperty.GetValue(destination) as IList;
            if (destList == null)
            {
                var ctor = MapperHelperCaches_V4.GetParameterlessConstructor(destPropType);
                if (ctor == null)
                    return;
                destList = ctor.Invoke(null) as IList;
                destinationProperty.SetValue(destination, destList);
            }

            destList.Clear();

            if (sourceValue is IEnumerable sourceList)
            {
                var genericArguments = destPropType.IsGenericType ? destPropType.GetGenericArguments() : null;
                var elementType = genericArguments?.FirstOrDefault() ?? typeof(object);

                foreach (var sourceElement in sourceList)
                {
                    object destElement;
                    if (sourceElement != null && elementType.IsClass && elementType != typeof(string))
                    {
                        var ctor = MapperHelperCaches_V4.GetParameterlessConstructor(elementType);
                        if (ctor == null)
                        {
                            destElement = null;
                        }
                        else
                        {
                            destElement = ctor.Invoke(null);
                            // Again, pass the rootSource to the recursive mapping call.
                            ReplacePropertiesRecursiveInternal_V4(rootSource, sourceElement, destElement, visited);
                        }
                    }
                    else
                    {
                        destElement = ConvertElement_V4(sourceElement, elementType);
                    }

                    destList.Add(destElement);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to convert a source element to the target type.
    /// </summary>
    private static object ConvertElement_V4(object sourceElement, Type targetType)
    {
        if (sourceElement == null)
            return null;
        if (targetType.IsAssignableFrom(sourceElement.GetType()))
            return sourceElement;

        var converter = TypeDescriptor.GetConverter(sourceElement);
        if (converter != null && converter.CanConvertTo(targetType))
            return converter.ConvertTo(sourceElement, targetType);

        if (sourceElement is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
            return Convert.ChangeType(sourceElement, targetType);

        return null;
    }

    /// <summary>
    /// Returns the property chain (an array of PropertyInfo) for a given dot‑separated path on the provided type.
    /// Returns null if any part of the chain is not found.
    /// </summary>
    private static PropertyInfo[] GetPropertyChainForPath_V4(Type type, string path)
    {
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        List<PropertyInfo> chain = new List<PropertyInfo>();
        Type current = type;
        foreach (var part in parts)
        {
            var prop = current.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return null;
            chain.Add(prop);
            current = prop.PropertyType;
        }

        return chain.ToArray();
    }

    /// <summary>
    /// Navigates an object using a dot‑separated path and sets the final property to the provided value.
    /// Creates intermediate objects if necessary.
    /// </summary>
    private static void SetValueForNestedPath_V4(object obj, string path, object value)
    {
        if (obj == null || string.IsNullOrEmpty(path))
            return;
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        object current = obj;
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            var prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return;

            if (i == parts.Length - 1)
            {
                // Set the value on the final property.
                if (prop.CanWrite)
                {
                    if (prop.PropertyType.IsAssignableFrom(value.GetType()))
                    {
                        prop.SetValue(current, value);
                    }
                    else
                    {
                        var converter = TypeDescriptor.GetConverter(value);
                        if (converter != null && converter.CanConvertTo(prop.PropertyType))
                        {
                            var converted = converter.ConvertTo(value, prop.PropertyType);
                            prop.SetValue(current, converted);
                        }
                        else if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(prop.PropertyType))
                        {
                            var converted = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(current, converted);
                        }
                    }
                }
            }
            else
            {
                // Navigate to the next property; create it if null.
                object next = prop.GetValue(current);
                if (next == null)
                {
                    var ctor = MapperHelperCaches_V4.GetParameterlessConstructor(prop.PropertyType);
                    if (ctor == null)
                        return;
                    next = ctor.Invoke(null);
                    prop.SetValue(current, next);
                }

                current = next;
            }
        }
    }

    /// <summary>
    /// Processes all properties in the destination (including nested objects)
    /// that are decorated with [ComplexBind] and sets the values from the source accordingly.
    /// </summary>
    /// <param name="source">The source object to retrieve values from.</param>
    /// <param name="destination">The root destination object.</param>
    private static void ProcessComplexBindAttributes_V4(object source, object destination)
    {
        // Create a visited hash set to track processed destination objects.
        var visited = new HashSet<object>(new ReferenceComparer());
        ProcessComplexBindAttributesRecursive(source, destination, destination, visited);
    }

    /// <summary>
    /// Recursively traverses the destination object's property graph, using a visited set
    /// to avoid infinite recursion in case of cycles.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="currentDest">The current destination object (may be nested).</param>
    /// <param name="rootDest">The root destination object (used for absolute paths).</param>
    /// <param name="visited">A set of already visited destination objects.</param>
    private static void ProcessComplexBindAttributesRecursive(object source, object currentDest, object rootDest, HashSet<object> visited)
    {
        if (currentDest == null || visited.Contains(currentDest))
            return;

        // Mark this object as visited.
        visited.Add(currentDest);

        // Get all public instance properties for the current destination object.
        var properties = currentDest.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            // Check if this property has the ComplexBind attribute.
            var complexBindAttr = prop.GetCustomAttribute<ComplexBindAttribute>();
            if (complexBindAttr != null)
            {
                // Process each candidate pair from the attribute.
                for (int i = 0; i < Math.Max(complexBindAttr.PropNames.Count, complexBindAttr.Froms.Count); i++)
                {
                    string destPath = i < complexBindAttr.PropNames.Count ? complexBindAttr.PropNames[i] : complexBindAttr.PropNames[0];
                    string sourcePath = i < complexBindAttr.Froms.Count ? complexBindAttr.Froms[i] : complexBindAttr.Froms[0];

                    object targetObj;
                    string actualDestPath = destPath;

                    // If the destination path starts with the property name + ".", treat it as relative.
                    if (destPath.StartsWith(prop.Name + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        targetObj = prop.GetValue(currentDest);
                        if (targetObj == null)
                        {
                            // Try to create an instance if needed.
                            var ctor = MapperHelperCaches_V4.GetParameterlessConstructor(prop.PropertyType);
                            if (ctor != null)
                            {
                                targetObj = ctor.Invoke(null);
                                prop.SetValue(currentDest, targetObj);
                            }
                            else
                            {
                                continue;
                            }
                        }

                        // Remove the decorated property name from the path.
                        actualDestPath = destPath.Substring(prop.Name.Length + 1);
                    }
                    else
                    {
                        // Otherwise, assume the destPath is absolute and resolve it from the root destination.
                        targetObj = rootDest;
                    }

                    // Get the source value based on the provided source path.
                    var sourceChain = GetPropertyChainForPath_V4(source.GetType(), sourcePath);
                    if (sourceChain == null)
                        continue;
                    object srcValue = GetValueFromChain_V4(source, sourceChain);
                    if (srcValue == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(actualDestPath))
                    {
                        // If the destination path is empty, set the value directly on the property.
                        if (prop.CanWrite)
                            prop.SetValue(currentDest, srcValue);
                    }
                    else
                    {
                        // Navigate the explicit nested destination path and set the value.
                        SetValueForNestedPath_V4(targetObj, actualDestPath, srcValue);
                    }

                    // Use the first successful candidate.
                    break;
                }
            }

            // If the property is a class (and not a string), then traverse deeper.
            if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
            {
                object childObj = prop.GetValue(currentDest);
                if (childObj != null)
                {
                    ProcessComplexBindAttributesRecursive(source, childObj, rootDest, visited);
                }
            }
        }
    }

    #endregion
}