namespace DMapper.Attributes;

/// <summary>
/// When applied to a property, tells the mapper to skip copying this property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CopyIgnoreAttribute : Attribute
{
}