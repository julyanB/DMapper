using System.Collections;
using System.Reflection;
using DMapper.Constants;
using DMapper.Helpers.Models;

namespace DMapper.Helpers;

public static class ObjectFlattener
{
    #region Public Flatten Overloads

    public static FlattenResult Flatten(object obj, string prefix = "", string separator = GlobalConstants.DefaultDotSeparator)
    {
        var visited = new HashSet<object>(new Comparers.ReferenceComparer());
        var dict = FlattenObject(obj, prefix, separator, visited);
        Type flattenedType = obj?.GetType();
        return new FlattenResult(flattenedType, dict);
    }

    public static FlattenResult Flatten(Type type, string prefix = "", string separator = GlobalConstants.DefaultDotSeparator)
    {
        // Start with a new visited set for type flattening.
        var visited = new HashSet<Type>(new Comparers.ReferenceComparer());
        var dict = FlattenStructure(type, prefix, separator, visited);
        return new FlattenResult(type, dict);
    }

    public static FlattenResult Flatten<T>(string prefix = "", string separator = GlobalConstants.DefaultDotSeparator)
    {
        return Flatten(typeof(T), prefix, separator);
    }

    #endregion

    #region Internal Flattening Methods

    private static Dictionary<string, FlattenedProperty> FlattenObject(object obj, string prefix, string separator, HashSet<object> visited)
    {
        var dict = new Dictionary<string, FlattenedProperty>(StringComparer.OrdinalIgnoreCase);
        if (obj == null)
            return dict;

        Type type = obj.GetType();
        // For non-simple types, check for cycles.
        if (!ReflectionHelper.IsSimpleType(type))
        {
            if (!visited.Add(obj))
            {
                // Cycle detected; return empty dictionary to break recursion.
                return dict;
            }
        }

        if (ReflectionHelper.IsSimpleType(type))
        {
            string key = string.IsNullOrEmpty(prefix) ? "Value" : prefix.Trim(separator.ToCharArray());
            dict[key] = new FlattenedProperty(obj, type);
            return dict;
        }

        if (obj is IEnumerable enumerable && !(obj is string))
        {
            string collKey = string.IsNullOrEmpty(prefix) ? "Value" : prefix.Trim(separator.ToCharArray());
            dict[collKey] = new FlattenedProperty(obj, obj.GetType());
            int index = 0;
            foreach (var item in enumerable)
            {
                string key = string.IsNullOrEmpty(prefix) ? $"[{index}]" : $"{prefix}[{index}]";
                var subDict = FlattenObject(item, key, separator, visited);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
                index++;
            }

            return dict;
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead)
                continue;
            object value;
            try
            {
                value = prop.GetValue(obj);
            }
            catch
            {
                continue;
            }

            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}{separator}{prop.Name}";
            if (value == null || ReflectionHelper.IsSimpleType(prop.PropertyType))
            {
                dict[key] = new FlattenedProperty(value, prop.PropertyType);
            }
            else
            {
                var subDict = FlattenObject(value, key, separator, visited);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
            }
        }

        return dict;
    }

    private static Dictionary<string, FlattenedProperty> FlattenStructure(Type type, string prefix, string separator, HashSet<Type> visited)
    {
        var dict = new Dictionary<string, FlattenedProperty>(StringComparer.OrdinalIgnoreCase);
        if (type == null)
            return dict;

        // If this type was already processed, break the recursion.
        if (visited.Contains(type))
            return dict;
        visited.Add(type);

        if (ReflectionHelper.IsSimpleType(type))
        {
            string key = string.IsNullOrEmpty(prefix) ? "Value" : prefix;
            dict[key] = new FlattenedProperty(null, type);
            return dict;
        }

        if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
        {
            string collKey = string.IsNullOrEmpty(prefix) ? "Value" : prefix;
            dict[collKey] = new FlattenedProperty(null, type);
            Type elementType = null;
            if (type.IsArray)
                elementType = type.GetElementType();
            else if (type.IsGenericType)
                elementType = type.GetGenericArguments().FirstOrDefault();
            if (elementType != null)
            {
                string key = string.IsNullOrEmpty(prefix) ? "[*]" : $"{prefix}[*]";
                var subDict = FlattenStructure(elementType, key, separator, visited);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
            }

            return dict;
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}{separator}{prop.Name}";
            if (ReflectionHelper.IsSimpleType(prop.PropertyType))
            {
                dict[key] = new FlattenedProperty(null, prop.PropertyType);
            }
            else
            {
                var subDict = FlattenStructure(prop.PropertyType, key, separator, visited);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
            }
        }

        return dict;
    }

    private static string CleanKey(string key, string separator)
    {
        return key.Trim(separator.ToCharArray());
    }
    #endregion
}