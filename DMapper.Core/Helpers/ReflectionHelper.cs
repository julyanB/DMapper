using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using DMapper.Attributes;
using DMapper.Comparers;
using DMapper.Core.Helpers;
using DMapper.Helpers.Models;

namespace DMapper.Helpers;

public static class ReflectionHelper
{
    public static void SetProperty(object instance, string propertyName, object newValue)
    {
        Type type = instance.GetType();
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var propertyInfo = type
            .GetProperty(propertyName, flags)
            .DeclaringType
            .GetProperty(propertyName, flags);

        propertyInfo.SetValue(instance, newValue, null);
    }
    public static T DeepCopy<T>(T original)
    {
        if (original == null)
        {
            return default;
        }

        Type type = original.GetType();
        if (type.IsPrimitive || typeof(string).Equals(type))
        {
            return original;
        }

        T copy = (T)type.GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(original, null);

        foreach (FieldInfo field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
        {
            object fieldValue = field.GetValue(original);
            if (fieldValue is object)
            {
                field.SetValue(copy, DeepCopy(fieldValue));
            }
        }

        return copy;
    }

    public static TDest DeepCopy<TSrc, TDest>(TSrc src)
    {
        if (src is null)
        {
            return default!;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var srcJson = System.Text.Json.JsonSerializer.Serialize(src);
        var dest = System.Text.Json.JsonSerializer.Deserialize<TDest>(srcJson, options);

        return dest;
    }

    /// <summary>
    /// Replace Placeholders in a template(Document)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="template"></param>
    /// <param name="currentPath"></param>
    /// <returns></returns>
    public static TDest ReplacePropertiesRecursive<TSrc, TDest>(TSrc source, TDest destination)
    {
        var type = source.GetType();
        var properties = type.GetProperties();
        var destinationType = destination.GetType();

        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(source);
            // if the property is a collection we skip it, because we cannot iterate through it with recursion
            if (propertyValue is (IEnumerable<object>))
                continue;


            ErrorHelper.HandleError(() =>
            {
                if (propertyValue is not null && property.PropertyType.IsClass && propertyValue is not string)
                {
                    // If property is another class (but not a string), continue exploring its properties
                    ReplacePropertiesRecursive(propertyValue, destination);
                }
                else if (propertyValue is not null)
                {
                    var destProperty = destinationType.GetProperty(property.Name);
                    destProperty.SetValue(destination, propertyValue);
                }
            });
        }

        return destination;
    }

    public static TDest ReplacePropertiesRecursive_V2<TDest, TSrc>(TDest destination, TSrc source)
    {
        if (source == null || destination == null)
            return destination;

        var sourceType = source.GetType();
        var destinationType = destination.GetType();

        var sourceProperties = sourceType.GetProperties();
        var destinationProperties = destinationType.GetProperties();

        foreach (var sourceProperty in sourceProperties)
        {
            var sourceValue = sourceProperty.GetValue(source);
            var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);

            if (sourceValue is null)
            {
                continue;
            }

            if (destinationProperty != null && destinationProperty.CanWrite)
            {
                if (sourceValue == null)
                {
                    destinationProperty.SetValue(destination, null);
                    continue;
                }

                if (sourceProperty.PropertyType.IsClass && sourceProperty.PropertyType != typeof(string))
                {
                    var destinationValue = destinationProperty.GetValue(destination);
                    if (destinationValue == null)
                    {
                        try
                        {
                            destinationValue = Activator.CreateInstance(destinationProperty.PropertyType);
                        }
                        catch (MissingMethodException)
                        {
                            // If there's no parameterless constructor, skip instantiation
                            continue;
                        }

                        destinationProperty.SetValue(destination, destinationValue);
                    }

                    ReplacePropertiesRecursive(sourceValue, destinationValue);
                }
                else
                {
                    ErrorHelper.HandleError(() =>
                    {
                        if (destinationProperty.PropertyType.IsEnum)
                        {
                            var enumValue = Enum.Parse(destinationProperty.PropertyType, sourceValue.ToString());
                            destinationProperty.SetValue(destination, enumValue);
                        }
                        else if (destinationProperty.PropertyType == sourceProperty.PropertyType)
                        {
                            destinationProperty.SetValue(destination, sourceValue);
                        }
                        else
                        {
                            var convertedValue = Convert.ChangeType(sourceValue, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, convertedValue);
                        }
                    });
                }
            }
        }

        return destination;
    }

    public static TDest ReplacePropertiesRecursive_V3<TDest, TSrc>(TDest destination, TSrc source)
    {
        var visited = new HashSet<object>(new ReferenceComparer());
        return (TDest)ReplacePropertiesRecursiveInternal_V3(source, destination, visited);
    }

    /// <summary>
    /// Recursively replaces properties from the source object into the destination object.
    /// Supports:
    ///   - [BindTo] on destination properties (with dot‑notation and fallback to the destination name)
    ///   - [RootBindTo] on destination properties to map a nested destination property from a source property.
    /// </summary>
    public static TDest ReplacePropertiesRecursive_V4<TDest, TSrc>(TDest destination, TSrc source)
    {
        var visited = new HashSet<object>(new ReferenceComparer());
        ReplacePropertiesRecursiveInternal_V4(source, destination, visited);
        ProcessRootBindToAttributes_V4(source, destination);
        return destination;
    }

    #region V3 Helper Methods

    private static object ReplacePropertiesRecursiveInternal_V3(object source, object destination, HashSet<object> visited)
    {
        if (source == null || destination == null)
            return destination;

        // If we've already seen this source object, return the destination to avoid cyclical loops.
        if (!IsSimpleType(source.GetType()) && !visited.Add(source))
            return destination;

        var sourceType = source.GetType();
        var destinationType = destination.GetType();

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var destinationProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var sourceProperty in sourceProperties)
        {
            // Skip properties marked with [CopyIgnore]
            if (sourceProperty.GetCustomAttribute<CopyIgnoreAttribute>() != null)
                continue;

            var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);
            if (destinationProperty == null || !destinationProperty.CanWrite)
                continue;

            var sourceValue = sourceProperty.GetValue(source);
            if (sourceValue == null)
                continue; // No need to overwrite with null

            var sourcePropType = sourceProperty.PropertyType;
            var destPropType = destinationProperty.PropertyType;

            // Handle enums
            if (destPropType.IsEnum && sourceValue != null)
            {
                ErrorHelper.HandleError(() =>
                {
                    var enumValue = Enum.Parse(destPropType, sourceValue.ToString());
                    destinationProperty.SetValue(destination, enumValue);
                });
                continue;
            }

            // If it's a collection, handle specially
            if (typeof(IEnumerable).IsAssignableFrom(destPropType) && destPropType != typeof(string))
            {
                HandleCollection_V3(destinationProperty, destination, sourceValue, visited);
                continue;
            }

            // If it's a complex class (not a simple type and not a string), recurse
            if (sourcePropType.IsClass && sourcePropType != typeof(string))
            {
                // If abstract or interface, skip creating a new instance
                if (destPropType.IsAbstract || destPropType.IsInterface)
                {
                    // Without a factory or known mapping, we can't handle this scenario.
                    continue;
                }

                var destinationValue = destinationProperty.GetValue(destination);
                if (destinationValue == null)
                {
                    try
                    {
                        destinationValue = Activator.CreateInstance(destPropType);
                        destinationProperty.SetValue(destination, destinationValue);
                    }
                    catch (MissingMethodException)
                    {
                        // If there's no parameterless constructor, skip
                        continue;
                    }
                }

                // Recurse
                ReplacePropertiesRecursiveInternal_V3(sourceValue, destinationValue, visited);
            }
            else
            {
                // Simple, directly assignable or convertible type
                ErrorHelper.HandleError(() =>
                {
                    if (destPropType == sourcePropType)
                    {
                        // Same type assignment
                        destinationProperty.SetValue(destination, sourceValue);
                    }
                    else
                    {
                        // Attempt type conversion
                        var convertedValue = Convert.ChangeType(sourceValue, destPropType);
                        destinationProperty.SetValue(destination, convertedValue);
                    }
                });
            }
        }

        return destination;
    }

    private static void HandleCollection_V3(PropertyInfo destinationProperty, object destination, object sourceValue, HashSet<object> visited)
    {
        var destPropType = destinationProperty.PropertyType;

        // If it's an array
        if (destPropType.IsArray)
        {
            var sourceArray = sourceValue as Array;
            if (sourceArray == null) return;

            var elementType = destPropType.GetElementType();
            var destArray = Array.CreateInstance(elementType, sourceArray.Length);

            for (int i = 0; i < sourceArray.Length; i++)
            {
                var sourceElement = sourceArray.GetValue(i);
                object destElement;

                if (sourceElement != null && elementType.IsClass && elementType != typeof(string))
                {
                    // Complex type in array element
                    destElement = Activator.CreateInstance(elementType);
                    ReplacePropertiesRecursiveInternal_V3(sourceElement, destElement, visited);
                }
                else
                {
                    // Simple type or null
                    destElement = ConvertElement_V3(sourceElement, elementType);
                }

                destArray.SetValue(destElement, i);
            }

            destinationProperty.SetValue(destination, destArray);
        }
        else if (typeof(IList).IsAssignableFrom(destPropType))
        {
            // Handle IList collections (e.g., List<T>)
            var destList = destinationProperty.GetValue(destination) as IList;
            if (destList == null)
            {
                try
                {
                    destList = Activator.CreateInstance(destPropType) as IList;
                    destinationProperty.SetValue(destination, destList);
                }
                catch
                {
                    // Can't create the list
                    return;
                }
            }

            destList.Clear();

            var sourceList = sourceValue as IEnumerable;
            if (sourceList != null)
            {
                var genericArguments = destPropType.IsGenericType ? destPropType.GetGenericArguments() : null;
                var elementType = genericArguments?.FirstOrDefault() ?? typeof(object);

                foreach (var sourceElement in sourceList)
                {
                    object destElement;
                    if (sourceElement != null && elementType.IsClass && elementType != typeof(string))
                    {
                        destElement = Activator.CreateInstance(elementType);
                        ReplacePropertiesRecursiveInternal_V3(sourceElement, destElement, visited);
                    }
                    else
                    {
                        destElement = ConvertElement_V3(sourceElement, elementType);
                    }

                    destList.Add(destElement);
                }
            }
        }
        else if (typeof(IDictionary).IsAssignableFrom(destPropType))
        {
            // Handle IDictionary (e.g., Dictionary<TKey,TValue>)
            var destDict = destinationProperty.GetValue(destination) as IDictionary;
            if (destDict == null)
            {
                try
                {
                    destDict = Activator.CreateInstance(destPropType) as IDictionary;
                    destinationProperty.SetValue(destination, destDict);
                }
                catch
                {
                    return;
                }
            }

            destDict.Clear();

            var sourceDict = sourceValue as IDictionary;
            if (sourceDict != null)
            {
                var genericArguments = destPropType.IsGenericType ? destPropType.GetGenericArguments() : null;
                var keyType = genericArguments != null && genericArguments.Length > 0 ? genericArguments[0] : typeof(object);
                var valueType = genericArguments != null && genericArguments.Length > 1 ? genericArguments[1] : typeof(object);

                foreach (DictionaryEntry entry in sourceDict)
                {
                    var sourceKey = entry.Key;
                    var sourceVal = entry.Value;

                    var destKey = ConvertElement_V3(sourceKey, keyType);
                    object destVal;
                    if (sourceVal != null && valueType.IsClass && valueType != typeof(string))
                    {
                        destVal = Activator.CreateInstance(valueType);
                        ReplacePropertiesRecursiveInternal_V3(sourceVal, destVal, visited);
                    }
                    else
                    {
                        destVal = ConvertElement_V3(sourceVal, valueType);
                    }

                    destDict[destKey] = destVal;
                }
            }
        }
        else
        {
            // Unknown collection type, skip for simplicity
        }
    }

    private static object ConvertElement_V3(object sourceElement, Type targetType)
    {
        if (sourceElement == null) return null;
        if (targetType == sourceElement.GetType()) return sourceElement;

        return Convert.ChangeType(sourceElement, targetType);
    }

    #endregion

    #region V4 Helper Methods

    /// <summary>
    /// Internal recursive implementation for property mapping.
    /// </summary>
    private static object ReplacePropertiesRecursiveInternal_V4(object source, object destination, HashSet<object> visited)
    {
        if (source == null || destination == null)
            return destination;

        // Avoid cyclical loops.
        if (!IsSimpleType(source.GetType()) && !visited.Add(source))
            return destination;

        Type sourceType = source.GetType();
        Type destinationType = destination.GetType();

        // Get (or build) cached mappings between destination properties and matching source property chains.
        var mappings = GetPropertyMappings_V4(sourceType, destinationType);
        foreach (var mapping in mappings)
        {
            // Retrieve the nested source value from the chain.
            var srcValue = GetValueFromChain_V4(source, mapping.SourcePropertyChain);
            if (srcValue == null)
                continue;

            // Determine the type of the final property in the chain.
            Type srcPropType = mapping.SourcePropertyChain.Last().PropertyType;
            Type destPropType = mapping.DestinationProperty.PropertyType;

            // Handle enum conversion (assumes the source value’s ToString() exactly matches an enum name).
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

            // Handle collections (arrays and IList) except strings.
            if (typeof(IEnumerable).IsAssignableFrom(destPropType) && destPropType != typeof(string))
            {
                HandleCollection_V4(mapping.DestinationProperty, destination, srcValue, visited);
                continue;
            }

            // Handle complex (non-string) types recursively.
            if (srcPropType.IsClass && srcPropType != typeof(string))
            {
                if (destPropType.IsAbstract || destPropType.IsInterface)
                    continue;

                var destValue = mapping.DestinationProperty.GetValue(destination);
                if (destValue == null)
                {
                    var ctor = MapperHelperCaches.GetParameterlessConstructor(destPropType);
                    if (ctor == null)
                        continue;
                    destValue = ctor.Invoke(null);
                    mapping.DestinationProperty.SetValue(destination, destValue);
                }

                ReplacePropertiesRecursiveInternal_V4(srcValue, destValue, visited);
            }
            else
            {
                // Handle simple types.
                if (destPropType.IsAssignableFrom(srcValue.GetType()))
                {
                    mapping.DestinationProperty.SetValue(destination, srcValue);
                }
                else
                {
                    // Try using a TypeConverter first.
                    var converter = TypeDescriptor.GetConverter(srcValue);
                    if (converter != null && converter.CanConvertTo(destPropType))
                    {
                        var convertedValue = converter.ConvertTo(srcValue, destPropType);
                        mapping.DestinationProperty.SetValue(destination, convertedValue);
                    }
                    else if (srcValue is IConvertible && typeof(IConvertible).IsAssignableFrom(destPropType))
                    {
                        // Only attempt conversion if both source and destination are convertible.
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
    private static List<PropertyMapping> GetPropertyMappings_V4(Type sourceType, Type destinationType)
    {
        return MapperHelperCaches.MappingCache.GetOrAdd((sourceType, destinationType), key =>
        {
            var mappings = new List<PropertyMapping>();
            var destProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var destProperty in destProperties)
            {
                if (!destProperty.CanWrite)
                    continue;
                if (destProperty.GetCustomAttribute<CopyIgnoreAttribute>() != null)
                    continue;

                // Build candidate source names.
                List<string> sourceNames = new List<string>();
                var bindToAttr = destProperty.GetCustomAttribute<BindToAttribute>();
                if (bindToAttr?.PropNames is { Count: > 0 })
                    sourceNames.AddRange(bindToAttr.PropNames);
                // Always add the destination property name as a fallback.
                if (!sourceNames.Contains(destProperty.Name))
                    sourceNames.Add(destProperty.Name);

                PropertyInfo[] sourceChain = null;
                foreach (var name in sourceNames)
                {
                    // Allow dot-notation for nested properties.
                    var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    List<PropertyInfo> chain = new List<PropertyInfo>();
                    Type currentType = sourceType;
                    bool valid = true;
                    foreach (var part in parts)
                    {
                        var prop = currentType.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                        if (prop == null)
                        {
                            valid = false;
                            break;
                        }

                        chain.Add(prop);
                        currentType = prop.PropertyType;
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
    private static void HandleCollection_V4(PropertyInfo destinationProperty, object destination, object sourceValue, HashSet<object> visited)
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
                    var ctor = MapperHelperCaches.GetParameterlessConstructor(elementType);
                    if (ctor == null)
                    {
                        destElement = null;
                    }
                    else
                    {
                        destElement = ctor.Invoke(null);
                        ReplacePropertiesRecursiveInternal_V4(sourceElement, destElement, visited);
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
                var ctor = MapperHelperCaches.GetParameterlessConstructor(destPropType);
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
                        var ctor = MapperHelperCaches.GetParameterlessConstructor(elementType);
                        if (ctor == null)
                        {
                            destElement = null;
                        }
                        else
                        {
                            destElement = ctor.Invoke(null);
                            ReplacePropertiesRecursiveInternal_V4(sourceElement, destElement, visited);
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
    /// Processes any properties decorated with [RootBindTo] on the destination.
    /// For each such property, it retrieves the candidate destination path and source path,
    /// then sets the nested destination property with the source value.
    /// </summary>
    private static void ProcessRootBindToAttributes_V4(object source, object destination)
    {
        if (source == null || destination == null)
            return;

        var destType = destination.GetType();
        var props = destType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            var rootBindAttrs = prop.GetCustomAttributes<ComplexBindAttribute>().ToArray();
            if (rootBindAttrs.Length == 0)
                continue;

            foreach (var rootBindAttr in rootBindAttrs)
            {
                // Process each attribute separately.
                for (int i = 0; i < Math.Max(rootBindAttr.PropNames.Count, rootBindAttr.Froms.Count); i++)
                {
                    string destPath = i < rootBindAttr.PropNames.Count ? rootBindAttr.PropNames[i] : rootBindAttr.PropNames[0];
                    string sourcePath = i < rootBindAttr.Froms.Count ? rootBindAttr.Froms[i] : rootBindAttr.Froms[0];

                    // If the destination path starts with the decorated property name, remove it.
                    string relativeDestPath = destPath;
                    if (destPath.StartsWith(prop.Name + ".", StringComparison.OrdinalIgnoreCase))
                        relativeDestPath = destPath.Substring(prop.Name.Length + 1);

                    var sourceChain = GetPropertyChainForPath_V4(source.GetType(), sourcePath);
                    if (sourceChain == null)
                        continue;
                    object srcValue = GetValueFromChain_V4(source, sourceChain);
                    if (srcValue == null)
                        continue;

                    // Get (or create) the object held by the property.
                    object destObj = prop.GetValue(destination);
                    if (destObj == null)
                    {
                        var ctor = MapperHelperCaches.GetParameterlessConstructor(prop.PropertyType);
                        if (ctor == null)
                            continue;
                        destObj = ctor.Invoke(null);
                        prop.SetValue(destination, destObj);
                    }

                    // Set the nested destination property using the relative path.
                    SetValueForNestedPath_V4(destObj, relativeDestPath, srcValue);
                    break; // Optionally, break after the first successful bind for this attribute.
                }
            }

        }
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
                    var ctor = MapperHelperCaches.GetParameterlessConstructor(prop.PropertyType);
                    if (ctor == null)
                        return;
                    next = ctor.Invoke(null);
                    prop.SetValue(current, next);
                }

                current = next;
            }
        }
    }

    #endregion

    #region Common Methods

    private static bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive) return true;
        if (type == typeof(string)) return true;
        if (type == typeof(decimal)) return true;
        if (type == typeof(DateTime)) return true;
        if (type == typeof(DateTimeOffset)) return true;
        if (type == typeof(TimeSpan)) return true;
        if (type == typeof(Guid)) return true;

        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
            return IsSimpleType(underlyingType);

        return false;
    }

    #endregion
}