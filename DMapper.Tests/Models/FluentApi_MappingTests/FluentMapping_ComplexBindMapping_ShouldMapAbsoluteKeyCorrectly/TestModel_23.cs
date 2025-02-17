using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexBindMapping_ShouldMapAbsoluteKeyCorrectly;

public class SourceFluentTest3
{
    public string Data { get; set; } = "DataFromSource";
    public NestedSource_Fluent Nested { get; set; } = new NestedSource_Fluent();
}

public class NestedSource_Fluent
{
    public string Info { get; set; } = "NestedInfo";
}

public class DestinationFluentTest3 : IDMapperConfiguration
{
    public string Data { get; set; }
    public NestedDest_Fluent NestedDestination { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Data, "Data")
            .Map(x => NestedDestination.Info, "Nested.Info");
    }
}

public class NestedDest_Fluent
{
    public string Info { get; set; }
}