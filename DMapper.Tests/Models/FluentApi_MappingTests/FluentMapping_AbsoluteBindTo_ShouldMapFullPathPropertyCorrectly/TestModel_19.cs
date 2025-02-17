using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_AbsoluteBindTo_ShouldMapFullPathPropertyCorrectly;

public class SourceFluentTest8
{
    public string M { get; set; } = "MValue";
    public FluentNestedSource8 N { get; set; } = new FluentNestedSource8();
}

public class FluentNestedSource8
{
    public string O { get; set; } = "OValue";
}

public class DestinationFluentTest8 : IDMapperConfiguration
{
    public string X { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        // Map property X from the absolute source path "N.O"
        builder.Map(x => X, "N.O");
    }
}