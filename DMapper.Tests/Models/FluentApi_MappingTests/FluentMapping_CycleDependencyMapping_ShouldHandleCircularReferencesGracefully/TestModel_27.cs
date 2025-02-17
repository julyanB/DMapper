using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_CycleDependencyMapping_ShouldHandleCircularReferencesGracefully;

// Fluent version of Test 15 – Cycle Dependency Mapping
public class SourceFluentTest15
{
    public string Name { get; set; } = "Parent";
    public SourceFluentTest15 Child { get; set; }
}

public class DestinationFluentTest15 : IDMapperConfiguration
{
    public string Name { get; set; }
    public DestinationFluentTest15 Child { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        // For simplicity, map only the top-level property to avoid infinite recursion.
        builder.Map(x => Name, "Name");
    }
}