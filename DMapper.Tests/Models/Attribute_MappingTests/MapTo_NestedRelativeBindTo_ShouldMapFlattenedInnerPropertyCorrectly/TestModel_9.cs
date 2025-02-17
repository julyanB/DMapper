using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_NestedRelativeBindTo_ShouldMapFlattenedInnerPropertyCorrectly;

// Models for Test 9
public class SourceTest9
{
    public NestedSource9 A { get; set; } = new NestedSource9();
}

public class NestedSource9
{
    public string Y { get; set; } = "Value9";
}

public class DestinationTest9
{
    public DestinationTest9A A { get; set; } = new DestinationTest9A();
}

public class DestinationTest9A
{
    // The candidate "Y" is applied as "A.Y" in the flattened path.
    [BindTo("Y")]
    public string Z { get; set; }
}