using DMapper.Converters;

namespace DMapper.Attributes;

/// <summary>
/// Decorate a destination property to tell DMapper which converter to use.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DMapperConverterAttribute : Attribute
{
    public Type ConverterType { get; }

    public DMapperConverterAttribute(Type converterType)
    {
        if (!typeof(IDMapperPropertyConverter).IsAssignableFrom(converterType))
            throw new ArgumentException($"{converterType.FullName} must implement IPropertyValueConverter");

        ConverterType = converterType;
    }

    /// <summary>Creates the converter; called once and cached.</summary>
    internal IDMapperPropertyConverter Instantiate() =>
        (IDMapperPropertyConverter)Activator.CreateInstance(ConverterType);
}