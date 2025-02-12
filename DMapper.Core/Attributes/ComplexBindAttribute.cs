namespace DMapper.Attributes;

/// <summary>
/// Decorate a destination property with [RootBindTo("DestPropChain", "SourcePropChain")]
/// to map a nested destination property from a source property.
/// For example:
/// [RootBindTo("Dest2.Name", "Name")]
/// will set dest.Dest2.Name = src.Name.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ComplexBindAttribute : Attribute
{
    public List<string> PropNames { get; set; }
    public List<string> Froms { get; set; }

    public ComplexBindAttribute(string dest, string from)
    {
        PropNames = dest.Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();

        Froms = from.Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }
}
