using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_ComplexObjectBinding_ShouldBindComplexObjectPropertyCorrectly;

// Models for Test 18
public class SourceTest1_18
{
    public SourceTest2_18 SourceTest2_18 { get; set; } = new SourceTest2_18();
}

public class SourceTest2_18
{
    public string Name { get; set; } = "Source";
}

public class DestinationTest1_18
{
    public DestinationTest2_18 DestinationTest2_18 { get; set; }
}

public class DestinationTest2_18
{
    [BindTo("SourceTest2_18.Name")]
    public string Name { get; set; }
}