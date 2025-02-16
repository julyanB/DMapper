using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_ArrayMapping_ShouldMapArrayPropertiesCorrectly;

// Models for Test 5
public class SourceTest5
{
    public string[] Items { get; set; } = new string[] { "Item1", "Item2", "Item3" };
}

public class DestinationTest5
{
    [BindTo("Items")]
    public string[] Items { get; set; }
}