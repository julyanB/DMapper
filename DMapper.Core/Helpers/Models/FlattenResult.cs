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
    }

    public override string ToString()
    {
        return $"{FlattenedType.Name} -> {Properties.Count} properties";
    }

    /// <summary>
    /// Rehydrates a new instance of FlattenedType by setting its properties based on the flattened dictionary.
    /// Intermediate objects are instantiated as needed.
    /// After setting values, FinalizeArrays() is called to convert any intermediate List&lt;T&gt; used for array properties into actual arrays.
    /// </summary>
    /// <param name="separator">The separator used in the flattened keys (default is ":").</param>
    /// <returns>An instance of FlattenedType with properties populated.</returns>
    public object Rehydrate(string separator = ":")
    {
        if (FlattenedType == null)
            return null;

        object instance = Activator.CreateInstance(FlattenedType);

        // Process keys in order from shallowest to deepest.
        var sortedKeys = Properties.Keys.OrderBy(k => k.Split(new string[] { separator }, StringSplitOptions.None).Length);
        foreach (var key in sortedKeys)
        {
            var fp = Properties[key];
            SetNestedValue(instance, key, fp.Value, fp.PropertyType, separator);
        }

        // Finalize arrays: convert intermediate List<T> values into arrays.
        FinalizeArrays(instance);
        return instance;
    }

    /// <summary>
    /// Recursively navigates an object using a flattened key (which may include index notation)
    /// and sets the final property to the given value.
    /// For collection segments (e.g., "Tags[0]"), if the property is declared as an array,
    /// an intermediate List&lt;T&gt; is used and then later converted to an array.
    /// </summary>
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
                // Extract property name and index.
                string propName = part.Substring(0, bracketIndex);
                int endBracket = part.IndexOf(']', bracketIndex);
                if (endBracket < 0)
                    return; // Malformed key

                string indexStr = part.Substring(bracketIndex + 1, endBracket - bracketIndex - 1);
                if (!int.TryParse(indexStr, out int index))
                    return;

                PropertyInfo prop = current.GetType().GetProperty(propName);
                if (prop == null)
                    return;

                object coll = prop.GetValue(current);
                if (coll == null)
                {
                    // If property is declared as an array, instantiate a List<T> as intermediate.
                    Type elementType = null;
                    if (prop.PropertyType.IsArray)
                    {
                        elementType = prop.PropertyType.GetElementType();
                        Type listType = typeof(List<>).MakeGenericType(elementType);
                        coll = Activator.CreateInstance(listType);
                    }
                    else if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                    {
                        elementType = prop.PropertyType.IsGenericType
                            ? prop.PropertyType.GetGenericArguments()[0]
                            : typeof(object);
                        Type listType = typeof(List<>).MakeGenericType(elementType);
                        coll = Activator.CreateInstance(listType);
                    }
                    else
                    {
                        return;
                    }

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
                    // If the target property is an array and value is null, create an empty array.
                    if (prop.PropertyType.IsArray && value == null)
                    {
                        Type elementType = prop.PropertyType.GetElementType();
                        value = Array.CreateInstance(elementType, 0);
                    }
                    else if (value != null && prop.PropertyType.IsArray && value is IList list)
                    {
                        // Convert intermediate list to an array.
                        Type elementType = prop.PropertyType.GetElementType();
                        Array array = Array.CreateInstance(elementType, list.Count);
                        list.CopyTo(array, 0);
                        value = array;
                    }
                    else
                    {
                        ilist[index] = value;
                        return;
                    }
                }
                else
                {
                    current = ilist[index];
                }
            }
            else
            {
                PropertyInfo prop = current.GetType().GetProperty(part);
                if (prop == null)
                    return;
                if (i == parts.Length - 1)
                {
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
                        // For array properties, use List<T> as intermediate.
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

    /// <summary>
    /// Recursively traverses the object and converts any intermediate List&lt;T&gt; 
    /// (used for array properties) into actual arrays.
    /// </summary>
    private static void FinalizeArrays(object instance)
    {
        if (instance == null)
            return;

        Type type = instance.GetType();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip indexer properties.
            if (prop.GetIndexParameters().Length > 0)
                continue;
            if (!prop.CanRead || !prop.CanWrite)
                continue;

            object value;
            try
            {
                value = prop.GetValue(instance);
            }
            catch
            {
                continue;
            }

            if (value == null)
                continue;

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