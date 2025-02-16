using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_InnerMapping_ShouldMapNestedPropertiesCorrectly;

// Models for Test 2
public class SourceTest2
{
    public string Outer { get; set; } = "OuterValue";
    public InnerSource Inner { get; set; } = new InnerSource();
}

public class InnerSource
{
    public string InnerProp { get; set; } = "InnerValue";
}

public class DestinationTest2
{
    public string Outer { get; set; }
    public InnerDest Inner { get; set; }
}

public class InnerDest
{
    [BindTo("InnerProp")]
    public string MyInner { get; set; }
}