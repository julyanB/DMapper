using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_FallbackMapping_ShouldUseFallbackBindToCandidate;

public class SourceFluentTest6
{
    public string X { get; set; } = "FallbackValue";
    public string B { get; set; } = "Beta";
}

public class DestinationFluentTest6 : IDMapperConfiguration
{
    public string B { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => B, "X");
    }
}