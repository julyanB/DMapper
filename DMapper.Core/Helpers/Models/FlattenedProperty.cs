namespace DMapper.Helpers.Models;

/// <summary>
/// Represents a flattened property with both its value and its type.
/// </summary>
public class FlattenedProperty
{
    public object Value { get; set; }
    public Type PropertyType { get; set; }

    public FlattenedProperty(object value, Type propertyType)
    {
        Value = value;
        PropertyType = propertyType;
    }

    public override string ToString()
    {
        return $"{PropertyType.Name} -> {Value}";
    }
}