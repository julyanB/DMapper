using System.Reflection;

namespace DMapper.Helpers.Models;

// A mapping between a destination property and the source property chain (for nested properties).
public class PropertyMapping
{
    public PropertyInfo DestinationProperty { get; set; }
    public PropertyInfo[] SourcePropertyChain { get; set; }
}