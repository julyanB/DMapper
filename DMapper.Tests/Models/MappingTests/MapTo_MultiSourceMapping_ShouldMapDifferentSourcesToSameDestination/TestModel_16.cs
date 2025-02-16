using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_MultiSourceMapping_ShouldMapDifferentSourcesToSameDestination;

// Models for Test 16
public class SourceTest1_16
{
    public string Name1 { get; set; } = "Source1";
}

public class SourceTest2_16
{
    public string Name2 { get; set; } = "Source2";
}

public class DestinationTest16
{
    [BindTo("Name1, Name2")]
    public string Name { get; set; }
}