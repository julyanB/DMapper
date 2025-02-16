namespace DMapper.Tests.Models.MappingTests.MapTo_CycleDependencyMapping_ShouldHandleCircularReferencesGracefully;

// Models for Test 15
public class SourceTest15
{
    public string Name { get; set; } = "Parent";
    public SourceTest15 Child { get; set; }
}

public class DestinationTest15
{
    public string Name { get; set; }
    public DestinationTest15 Child { get; set; }
}