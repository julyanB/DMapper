using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_MultiComplexBindingMapping_ShouldMapMultipleComplexSourcesCorrectly;

// Models for Test 17
public class SourceTest1_17
{
    public string Name1 { get; set; } = "Source1";
}

public class SourceTest2_17
{
    public string Name2 { get; set; } = "Source2";
}

public class DestinationTest1_17
{
    [ComplexBind("DestinationTest2_17.Name", "Name2")]
    [ComplexBind("DestinationTest2_17.Name", "Name1")]
    public DestinationTest2_17 DestinationTest2_17 { get; set; }
}

public class DestinationTest2_17
{
    public string Name { get; set; }
}