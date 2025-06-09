using DMapper.Core.Helpers;

namespace DMapper.Helpers;

public static partial class ReflectionHelper
{
    /// <summary>
    /// Replace Placeholders in a template(Document)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="template"></param>
    /// <param name="currentPath"></param>
    /// <returns></returns>
    public static TDest ReplacePropertiesRecursive_V1<TSrc, TDest>(TSrc source, TDest destination)
    {
        var type = source.GetType();
        var properties = type.GetProperties();
        var destinationType = destination.GetType();

        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(source);
            // if the property is a collection we skip it, because we cannot iterate through it with recursion
            if (propertyValue is (IEnumerable<object>))
                continue;


            ErrorHelper.HandleError(() =>
            {
                if (propertyValue is not null && property.PropertyType.IsClass && propertyValue is not string)
                {
                    // If property is another class (but not a string), continue exploring its properties
                    ReplacePropertiesRecursive_V1(propertyValue, destination);
                }
                else if (propertyValue is not null)
                {
                    var destProperty = destinationType.GetProperty(property.Name);
                    destProperty.SetValue(destination, propertyValue);
                }
            });
        }

        return destination;
    }
}