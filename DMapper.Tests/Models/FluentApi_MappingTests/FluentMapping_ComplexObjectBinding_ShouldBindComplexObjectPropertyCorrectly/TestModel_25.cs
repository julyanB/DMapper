using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexObjectBinding_ShouldBindComplexObjectPropertyCorrectly;

public class SourceFluentTest1_18
{
    public SourceTest2_18_Fluent SourceTest2_18 { get; set; } = new SourceTest2_18_Fluent();
}

public class SourceTest2_18_Fluent
{
    public string Name { get; set; } = "Source";
}

public class DestinationFluentTest1_18 : IDMapperConfiguration
{
    public DestinationTest2_18_Fluent DestinationTest2_18 { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => DestinationTest2_18.Name, "SourceTest2_18.Name");
    }
}

public class DestinationTest2_18_Fluent
{
    public string Name { get; set; }
}