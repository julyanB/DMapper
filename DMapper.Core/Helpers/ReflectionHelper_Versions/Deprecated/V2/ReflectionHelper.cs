using DMapper.Core.Helpers;

namespace DMapper.Helpers;

public static partial class ReflectionHelper
{

    public static TDest ReplacePropertiesRecursive_V2<TDest, TSrc>(TDest destination, TSrc source)
    {
        if (source == null || destination == null)
            return destination;

        var sourceType = source.GetType();
        var destinationType = destination.GetType();

        var sourceProperties = sourceType.GetProperties();
        var destinationProperties = destinationType.GetProperties();

        foreach (var sourceProperty in sourceProperties)
        {
            var sourceValue = sourceProperty.GetValue(source);
            var destinationProperty = destinationProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);

            if (sourceValue is null)
            {
                continue;
            }

            if (destinationProperty != null && destinationProperty.CanWrite)
            {
                if (sourceValue == null)
                {
                    destinationProperty.SetValue(destination, null);
                    continue;
                }

                if (sourceProperty.PropertyType.IsClass && sourceProperty.PropertyType != typeof(string))
                {
                    var destinationValue = destinationProperty.GetValue(destination);
                    if (destinationValue == null)
                    {
                        try
                        {
                            destinationValue = Activator.CreateInstance(destinationProperty.PropertyType);
                        }
                        catch (MissingMethodException)
                        {
                            // If there's no parameterless constructor, skip instantiation
                            continue;
                        }

                        destinationProperty.SetValue(destination, destinationValue);
                    }

                    ReplacePropertiesRecursive_V1(sourceValue, destinationValue);
                }
                else
                {
                    ErrorHelper.HandleError(() =>
                    {
                        if (destinationProperty.PropertyType.IsEnum)
                        {
                            var enumValue = Enum.Parse(destinationProperty.PropertyType, sourceValue.ToString());
                            destinationProperty.SetValue(destination, enumValue);
                        }
                        else if (destinationProperty.PropertyType == sourceProperty.PropertyType)
                        {
                            destinationProperty.SetValue(destination, sourceValue);
                        }
                        else
                        {
                            var convertedValue = Convert.ChangeType(sourceValue, destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, convertedValue);
                        }
                    });
                }
            }
        }

        return destination;
    }
}