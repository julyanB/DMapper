using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_NestedRelativeBindTo_ShouldMapRelativePropertyKeyCorrectly;

// Models for Test 7
public class SourceTest7
{
    public NestedSource7 A { get; set; } = new NestedSource7();
}

public class NestedSource7
{
    public NestedSource7Inner B { get; set; } = new NestedSource7Inner();
}

public class NestedSource7Inner
{
    public string C { get; set; } = "Value7";
}

public class DestinationTest7
{
    public DestinationTest7A A { get; set; } = new DestinationTest7A();
}

public class DestinationTest7A
{
    // The relative BindTo candidate "A.B.C" will merge "B.C" into "A.B.C".
    [BindTo("A.B.C")]
    public string X { get; set; }
}