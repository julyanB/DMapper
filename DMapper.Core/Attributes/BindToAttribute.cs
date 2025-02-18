namespace DMapper.Attributes;

/// <summary>
/// Decorate destination properties with [BindTo("SourcePropName1,SourcePropName2")] to specify one or more source property names.
/// You can use dot-notation to traverse nested properties.
/// Example: [BindTo("Test22.Test33")]
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class BindToAttribute : Attribute
{
    public List<string> PropNames { get; set; }
    public bool UseLiteralName { get; set; }

    public BindToAttribute(string dest, bool useLiteralName = false)
    {
        PropNames = dest.Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
        
        UseLiteralName = useLiteralName;
    }
}