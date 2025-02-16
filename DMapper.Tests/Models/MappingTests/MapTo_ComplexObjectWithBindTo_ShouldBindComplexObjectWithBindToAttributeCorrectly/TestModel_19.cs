using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_ComplexObjectWithBindTo_ShouldBindComplexObjectWithBindToAttributeCorrectly;

// Models for Test 19
public class SourceTest1_19
{
    public SourceTest2_19 SourceTest2_19 { get; set; } = new SourceTest2_19();
}

public class SourceTest2_19
{
    public string Name { get; set; } = "Source";
    public int Age { get; set; } = 25;
}

public class DestinationTest1_19
{
    [BindTo("SourceTest2_19")]
    public DestinationTest2_19 DestinationTest2_19 { get; set; }
}

public class DestinationTest2_19
{
    [BindTo("Name")]
    public string Name2 { get; set; }
    public int Age { get; set; }
}