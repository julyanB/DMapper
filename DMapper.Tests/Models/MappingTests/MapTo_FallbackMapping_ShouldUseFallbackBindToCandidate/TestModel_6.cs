using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_FallbackMapping_ShouldUseFallbackBindToCandidate;

// Models for Test 6
public class SourceTest6
{
    public string X { get; set; } = "FallbackValue";
    public string B { get; set; } = "Beta";
}

public class DestinationTest6
{
    [BindTo("X")]
    public string B { get; set; }
}