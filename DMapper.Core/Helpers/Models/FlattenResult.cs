using System.Collections;
using System.Reflection;

namespace DMapper.Helpers.Models;

/// <summary>
/// Contains the result of a flatten operation:
/// - FlattenedType: the original type that was flattened.
/// - Properties: a dictionary mapping flattened property paths to FlattenedProperty objects.
/// 
/// It also provides a Rehydrate() method that creates an instance of FlattenedType
/// with all properties (including collections) instantiated.
/// </summary>
public class FlattenResult
{
    public Type FlattenedType { get; set; }
    public Dictionary<string, FlattenedProperty> Properties { get; set; }

    public FlattenResult(Type flattenedType, Dictionary<string, FlattenedProperty> properties)
    {
        FlattenedType = flattenedType;
        Properties = properties;
        LinkFlattenedProperties();
    }

    /// <summary>
    /// Rehydrates an instance of FlattenedType from the flattened dictionary.
    /// All intermediate objects and collections are instantiated.
    /// </summary>
    public object Rehydrate(string separator = ".")
    {
        if (FlattenedType == null) return null;
        object instance = Activator.CreateInstance(FlattenedType);
        // Process keys in order from shallowest to deepest.
        var sortedKeys =
            Properties.Keys.OrderBy(k => k.Split(new string[] { separator }, StringSplitOptions.None).Length);
        foreach (var key in sortedKeys)
        {
            var fp = Properties[key];
            SetNestedValue(instance, key, fp.Value, fp.PropertyType, separator);
        }

        FinalizeArrays(instance);
        return instance;
    }
   
    /// <summary>
    /// Sets the value of a nested property in an object.
    /// </summary>
    private static void SetNestedValue(object instance, string path, object value, Type propertyType, string separator)
       {
           string[] parts = path.Split(new string[] { separator }, StringSplitOptions.None);
           object current = instance;
           for (int i = 0; i < parts.Length; i++)
           {
               bool isLast = i == parts.Length - 1;
               string part = parts[i];
   
               if (part.Contains("["))
               {
                   current = ProcessCollectionSegment(current, part, isLast, ref value);
                   if (current == null)
                       return;
               }
               else
               {
                   current = ProcessPropertySegment(current, part, isLast, ref value);
                   if (current == null)
                       return;
               }
           }
       } 
    
    /// <summary>
    /// Link the flattened properties in sorted order (by key) using the Next and Previous pointers.
    /// </summary>
    private void LinkFlattenedProperties()
    {
        var sortedKeys = Properties.Keys.OrderBy(k => k).ToList();
        for (int i = 0; i < sortedKeys.Count; i++)
        {
            var fp = Properties[sortedKeys[i]];
            fp.Previous = i > 0 ? Properties[sortedKeys[i - 1]] : null;
            fp.Next = i < sortedKeys.Count - 1 ? Properties[sortedKeys[i + 1]] : null;
        }
    }

    /// <summary>
    /// Processes a segment that represents a collection (e.g. "Tags[0]").
    /// </summary>
    private static object ProcessCollectionSegment(object current, string segment, bool isLast, ref object value)
    {
        int bracketIndex = segment.IndexOf('[');
        string propName = segment.Substring(0, bracketIndex);
        int endBracket = segment.IndexOf(']', bracketIndex);
        if (endBracket < 0)
            return null;

        string indexStr = segment.Substring(bracketIndex + 1, endBracket - bracketIndex - 1);
        if (!int.TryParse(indexStr, out int index))
            return null;

        PropertyInfo prop = current.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null)
            return null;

        // Retrieve or create the collection.
        object coll = prop.GetValue(current);
        if (coll == null)
        {
            Type collType = prop.PropertyType;
            Type elementType = GetElementType(collType);
            Type listType = typeof(List<>).MakeGenericType(elementType);
            coll = Activator.CreateInstance(listType);
            prop.SetValue(current, coll);
        }

        var ilist = coll as IList;
        if (ilist == null)
            return null;

        // Ensure the list is large enough.
        while (ilist.Count <= index)
        {
            Type elementType = GetElementType(ilist.GetType());
            ilist.Add(Activator.CreateInstance(elementType));
        }

        if (isLast)
        {
            // At the final segment, convert and assign the value.
            object finalValue = MapCollectionValue(prop, value);
            ilist[index] = finalValue;
            return current;
        }
        else
        {
            return ilist[index];
        }
    }

    /// <summary>
    /// Processes a normal (non-collection) property segment.
    /// </summary>
    private static object ProcessPropertySegment(object current, string segment, bool isLast, ref object value)
    {
        PropertyInfo prop = current.GetType().GetProperty(segment, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null)
            return null;

        if (isLast)
        {
            // If the property itself is a collection and the incoming value is an IList,
            // map each element individually.
            if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                prop.PropertyType != typeof(string) &&
                value is IList)
            {
                value = MapCollectionValue(prop, value);
            }
            else
            {
                // Handle enum conversion if necessary.
                Type targetType = prop.PropertyType;
                if (value != null)
                {
                    if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        Type underlying = Nullable.GetUnderlyingType(targetType);
                        if (underlying.IsEnum)
                        {
                            value = Enum.ToObject(underlying, value);
                        }
                    }
                    else if (targetType.IsEnum)
                    {
                        value = Enum.ToObject(targetType, value);
                    }
                }

                // If value is null and the property is an array or a complex type, initialize it.
                if (prop.PropertyType.IsArray && value == null)
                {
                    Type elementType = prop.PropertyType.GetElementType();
                    value = Array.CreateInstance(elementType, 0);
                }
                else if (value == null && !ObjectFlattener.IsSimpleType(prop.PropertyType))
                {
                    value = Activator.CreateInstance(prop.PropertyType);
                }
            }

            prop.SetValue(current, value);
            return current;
        }
        else
        {
            object next = prop.GetValue(current);
            if (next == null)
            {
                // For non-final segments, create an intermediate object.
                if (prop.PropertyType.IsArray)
                {
                    Type elementType = prop.PropertyType.GetElementType();
                    Type listType = typeof(List<>).MakeGenericType(elementType);
                    next = Activator.CreateInstance(listType);
                }
                else
                {
                    next = Activator.CreateInstance(prop.PropertyType);
                }

                prop.SetValue(current, next);
            }

            return next;
        }
    }

    /// <summary>
    /// Converts the incoming value to the proper collection type for the destination property.
    /// Handles arrays as well as generic IList types.
    /// </summary>
    private static object MapCollectionValue(PropertyInfo prop, object value)
    {
        if (prop.PropertyType.IsArray && value != null && value is IList srcList)
        {
            Type elementType = prop.PropertyType.GetElementType();
            Array array = Array.CreateInstance(elementType, srcList.Count);
            for (int j = 0; j < srcList.Count; j++)
            {
                var srcElem = srcList[j];
                object destElem;
                if (srcElem != null && !ObjectFlattener.IsSimpleType(elementType))
                {
                    destElem = Activator.CreateInstance(elementType);
                    destElem = ReflectionHelper.ReplacePropertiesRecursive_V5((dynamic)destElem, srcElem);
                }
                else
                {
                    destElem = srcElem;
                }

                array.SetValue(destElem, j);
            }

            return array;
        }
        else if (prop.PropertyType.IsArray && value == null)
        {
            Type elementType = prop.PropertyType.GetElementType();
            return Array.CreateInstance(elementType, 0);
        }
        else if (!prop.PropertyType.IsArray &&
                 typeof(IList).IsAssignableFrom(prop.PropertyType) &&
                 value is IList srcList2)
        {
            Type elementType = prop.PropertyType.IsGenericType
                ? prop.PropertyType.GetGenericArguments()[0]
                : typeof(object);
            var destListType = typeof(List<>).MakeGenericType(elementType);
            var destList = Activator.CreateInstance(destListType) as IList;
            for (int j = 0; j < srcList2.Count; j++)
            {
                var srcElem = srcList2[j];
                object destElem;
                if (srcElem != null && !ObjectFlattener.IsSimpleType(elementType))
                {
                    destElem = Activator.CreateInstance(elementType);
                    destElem = ReflectionHelper.ReplacePropertiesRecursive_V5((dynamic)destElem, srcElem);
                }
                else
                {
                    destElem = srcElem;
                }

                destList.Add(destElem);
            }

            return destList;
        }

        return value;
    }

    /// <summary>
    /// Returns the element type of an array or a generic collection; otherwise, returns object.
    /// </summary>
    private static Type GetElementType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType();
        if (type.IsGenericType)
            return type.GetGenericArguments()[0];
        return typeof(object);
    }
    
    /// <summary> Converts all IList properties to arrays.
    /// </summary>
    private static void FinalizeArrays(object instance)
    {
        if (instance == null) return;
        Type type = instance.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.GetIndexParameters().Length > 0) continue;
            object value = prop.GetValue(instance);
            if (value == null) continue;
            if (prop.PropertyType.IsArray && value is IList list)
            {
                Type elementType = prop.PropertyType.GetElementType();
                Array array = Array.CreateInstance(elementType, list.Count);
                list.CopyTo(array, 0);
                prop.SetValue(instance, array);
            }
            else if (!ObjectFlattener.IsSimpleType(prop.PropertyType))
            {
                FinalizeArrays(value);
            }
        }
    }

}