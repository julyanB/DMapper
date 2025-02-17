using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_MultiComplexBindingMapping_ShouldMapMultipleComplexSourcesCorrectly;

public class SourceFluentTest1_17
{
    public string Name1 { get; set; } = "Source1";
}

public class SourceFluentTest2_17
{
    public string Name2 { get; set; } = "Source2";
}

public class DestinationFluentTest1_17 : IDMapperConfiguration
{
    public DestinationTest2_17_Fluent DestinationTest2_17 { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        // Provide candidate keys (first non-null is used)
        builder.Map(x => DestinationTest2_17.Name, "Name2" , "Name1");
    }
}

public class DestinationTest2_17_Fluent
{
    public string Name { get; set; }
}