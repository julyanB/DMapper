using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexObjectWithBindTo_ShouldBindComplexObjectWithFluentOverrideCorrectly;

public class SourceFluentTest1_19
{
    public SourceTest2_19_Fluent SourceTest2_19 { get; set; } = new SourceTest2_19_Fluent();
}

public class SourceTest2_19_Fluent
{
    public string Name { get; set; } = "Source";
    public int Age { get; set; } = 25;
}

public class DestinationFluentTest1_19 : IDMapperConfiguration
{
    public DestinationTest2_19_Fluent DestinationTest2_19 { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => DestinationTest2_19.Name2, "SourceTest2_19.Name")
            .Map(x => DestinationTest2_19.Age, "SourceTest2_19.Age");
    }
}

public class DestinationTest2_19_Fluent
{
    public string Name2 { get; set; }
    public int Age { get; set; }
}