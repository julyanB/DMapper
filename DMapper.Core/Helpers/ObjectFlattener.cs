using System.Collections;
using System.Reflection;
using DMapper.Helpers.Models;

namespace DMapper.Helpers;

public static class ObjectFlattener
{
    private const string DefaultSeparator = ".";

    #region Public Flatten Overloads

    public static FlattenResult Flatten(object obj, string prefix = "", string separator = DefaultSeparator)
    {
        var dict = FlattenObject(obj, prefix, separator);
        Type flattenedType = obj?.GetType();
        return new FlattenResult(flattenedType, dict);
    }

    public static FlattenResult Flatten(Type type, string prefix = "", string separator = DefaultSeparator)
    {
        var dict = FlattenStructure(type, prefix, separator);
        return new FlattenResult(type, dict);
    }

    public static FlattenResult Flatten<T>(string prefix = "", string separator = DefaultSeparator)
    {
        return Flatten(typeof(T), prefix, separator);
    }

    #endregion

    #region Internal Flattening Methods

    private static Dictionary<string, FlattenedProperty> FlattenObject(object obj, string prefix, string separator)
    {
        var dict = new Dictionary<string, FlattenedProperty>(StringComparer.OrdinalIgnoreCase);
        if (obj == null) return dict;
        Type type = obj.GetType();
        if (IsSimpleType(type))
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
                var subDict = FlattenObject(item, key, separator);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
                index++;
            }

            return dict;
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
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
            if (value == null || IsSimpleType(prop.PropertyType))
            {
                dict[key] = new FlattenedProperty(value, prop.PropertyType);
            }
            else
            {
                var subDict = FlattenObject(value, key, separator);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
            }
        }

        return dict;
    }

    private static Dictionary<string, FlattenedProperty> FlattenStructure(Type type, string prefix, string separator)
    {
        var dict = new Dictionary<string, FlattenedProperty>(StringComparer.OrdinalIgnoreCase);
        if (type == null) return dict;
        if (IsSimpleType(type))
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
                var subDict = FlattenStructure(elementType, key, separator);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
            }

            return dict;
        }

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}{separator}{prop.Name}";
            if (IsSimpleType(prop.PropertyType))
            {
                dict[key] = new FlattenedProperty(null, prop.PropertyType);
            }
            else
            {
                var subDict = FlattenStructure(prop.PropertyType, key, separator);
                foreach (var kvp in subDict)
                    dict[kvp.Key] = kvp.Value;
            }
        }

        return dict;
    }

    public static bool IsSimpleType(Type type)
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

    #endregion
}