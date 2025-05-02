namespace DMapper.Converters;

public interface IDMapperPropertyConverter
{
    /// <summary>Returns a value that can be assigned to the destination property.</summary>
    object Convert(object sourceValue);
}