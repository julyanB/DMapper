using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_AbsoluteBindTo_ShouldMapFullPathPropertyCorrectly;

// Models for Test 8
public class SourceTest8
{
    public string M { get; set; } = "MValue";
    public NestedSource8 N { get; set; } = new NestedSource8();
}

public class NestedSource8
{
    public string O { get; set; } = "OValue";
}

public class DestinationTest8
{
    // Using an absolute BindTo key "N.O"
    [BindTo("N.O")]
    public string X { get; set; }
}