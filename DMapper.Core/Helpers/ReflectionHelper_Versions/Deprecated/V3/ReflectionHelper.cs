using System.Collections;
using System.Reflection;
using DMapper.Attributes;
using DMapper.Comparers;
using DMapper.Core.Helpers;

namespace DMapper.Helpers;

public static partial class ReflectionHelper
{
    public static TDest ReplacePropertiesRecursive_V3<TDest, TSrc>(TDest destination, TSrc source)
    {
        var visited = new HashSet<object>(new ReferenceComparer());
        return (TDest)ReplacePropertiesRecursiveInternal_V3(source, destination, visited);
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
}