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

    private static void SetNestedValue(object instance, string path, object value, Type propertyType, string separator)
    {
        string[] parts = path.Split(new string[] { separator }, StringSplitOptions.None);
        object current = instance;
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            int bracketIndex = part.IndexOf('[');
            if (bracketIndex >= 0)
            {
                // Handle collection segment, e.g., "Tags[0]"
                string propName = part.Substring(0, bracketIndex);
                int endBracket = part.IndexOf(']', bracketIndex);
                if (endBracket < 0)
                    return;
                string indexStr = part.Substring(bracketIndex + 1, endBracket - bracketIndex - 1);
                if (!int.TryParse(indexStr, out int index))
                    return;
                PropertyInfo prop = current.GetType().GetProperty(propName);
                if (prop == null)
                    return;
                object coll = prop.GetValue(current);
                if (coll == null)
                {
                    // For array properties, create a List<T> intermediate.
                    Type elementType = prop.PropertyType.IsArray
                        ? prop.PropertyType.GetElementType()
                        : (prop.PropertyType.IsGenericType
                            ? prop.PropertyType.GetGenericArguments()[0]
                            : typeof(object));
                    Type listType = typeof(List<>).MakeGenericType(elementType);
                    coll = Activator.CreateInstance(listType);
                    prop.SetValue(current, coll);
                }

                var ilist = coll as IList;
                if (ilist == null)
                    return;
                while (ilist.Count <= index)
                {
                    Type elementType = ilist.GetType().GetGenericArguments()[0];
                    ilist.Add(Activator.CreateInstance(elementType));
                }

                if (i == parts.Length - 1)
                {
                    if (prop.PropertyType.IsArray && value != null && value is IList list)
                    {
                        Type elementType = prop.PropertyType.GetElementType();
                        Array array = Array.CreateInstance(elementType, list.Count);
                        list.CopyTo(array, 0);
                        value = array;
                    }
                    else if (prop.PropertyType.IsArray && value == null)
                    {
                        Type elementType = prop.PropertyType.GetElementType();
                        value = Array.CreateInstance(elementType, 0);
                    }

                    ilist[index] = value;
                    return;
                }
                else
                {
                    current = ilist[index];
                }
            }
            else
            {
                PropertyInfo prop = current.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                    return;
                if (i == parts.Length - 1)
                {
                    // Handle enum conversion
                    Type targetType = prop.PropertyType;
                    if (value != null)
                    {
                        // If destination is a nullable enum, get its underlying type.
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

                    if (prop.PropertyType.IsArray && value == null)
                    {
                        Type elementType = prop.PropertyType.GetElementType();
                        value = Array.CreateInstance(elementType, 0);
                    }
                    else if (value == null && !ObjectFlattener.IsSimpleType(prop.PropertyType))
                    {
                        value = Activator.CreateInstance(prop.PropertyType);
                    }

                    prop.SetValue(current, value);
                }
                else
                {
                    object next = prop.GetValue(current);
                    if (next == null)
                    {
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

                    current = next;
                }
            }
        }
    }

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
            else if (!IsSimpleType(prop.PropertyType))
            {
                FinalizeArrays(value);
            }
        }
    }

    private static bool IsSimpleType(Type type)
    {
        if (type.IsEnum) return true;
        if (type.IsPrimitive) return true;
        var simpleTypes = new HashSet<Type>
        {
            typeof(string), typeof(decimal), typeof(DateTime),
            typeof(DateTimeOffset), typeof(TimeSpan), typeof(Guid),
            typeof(Uri), typeof(Version)
        };
        Type dateOnly = Type.GetType("System.DateOnly");
        if (dateOnly != null) simpleTypes.Add(dateOnly);
        Type timeOnly = Type.GetType("System.TimeOnly");
        if (timeOnly != null) simpleTypes.Add(timeOnly);
        if (simpleTypes.Contains(type)) return true;
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null) return IsSimpleType(underlying);
        return false;
    }
}