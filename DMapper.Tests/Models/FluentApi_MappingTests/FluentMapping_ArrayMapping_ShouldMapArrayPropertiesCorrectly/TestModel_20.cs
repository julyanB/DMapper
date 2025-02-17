using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ArrayMapping_ShouldMapArrayPropertiesCorrectly;

// Fluent version of Test 5 – Array Mapping
public class SourceFluentTest5
{
    public string[] Items { get; set; } = new string[] { "Item1", "Item2", "Item3" };
}

public class DestinationFluentTest5 : IDMapperConfiguration
{
    public string[] Items { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Items, "Items");
    }
}