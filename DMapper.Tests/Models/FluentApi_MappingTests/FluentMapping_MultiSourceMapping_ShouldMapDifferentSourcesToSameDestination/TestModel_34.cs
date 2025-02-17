using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_MultiSourceMapping_ShouldMapDifferentSourcesToSameDestination;

public class SourceFluentTest1_16
{
    public string Name1 { get; set; } = "Source1";
}

public class SourceFluentTest2_16
{
    public string Name2 { get; set; } = "Source2";
}

public class DestinationFluentTest16 : IDMapperConfiguration
{
    public string Name { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Name, "Name1","Name2");
    }
}