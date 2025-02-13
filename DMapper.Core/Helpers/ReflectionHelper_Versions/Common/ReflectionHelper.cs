using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMapper.Helpers;

public static partial class ReflectionHelper
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
}