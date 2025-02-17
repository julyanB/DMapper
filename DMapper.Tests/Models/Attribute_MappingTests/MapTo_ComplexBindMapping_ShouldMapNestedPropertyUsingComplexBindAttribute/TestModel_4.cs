using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_ComplexBindMapping_ShouldMapNestedPropertyUsingComplexBindAttribute;

// Models for Test 4
public class SourceTest4
{
    public string Extra { get; set; } = "ExtraValue";
}

public class DestinationTest4
{
    [ComplexBind("Sub.Info", "Extra")]
    public SubDest Sub { get; set; }
}

public class SubDest
{
    public string Info { get; set; }
}