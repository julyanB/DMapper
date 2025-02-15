using System.Reflection;

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
}