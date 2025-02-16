using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_ComplexBindMapping_ShouldMapAbsoluteKeyCorrectly;

// Models for Test 3
public class SourceTest3
{
    public string Data { get; set; } = "DataFromSource";
    public NestedSource Nested { get; set; } = new NestedSource();
}

public class NestedSource
{
    public string Info { get; set; } = "NestedInfo";
}

public class DestinationTest3
{
    public string Data { get; set; }

    [ComplexBind("NestedDestination.Info", "Nested.Info")]
    public NestedDest NestedDestination { get; set; }
}

public class NestedDest
{
    public string Info { get; set; }
}