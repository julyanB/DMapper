using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ComplexBindMapping_ShouldMapNestedPropertyUsingFluentConfig;

public class SourceFluentTest4
{
    public string Extra { get; set; } = "ExtraValue";
}

public class DestinationFluentTest4 : IDMapperConfiguration
{
    public SubDest_Fluent Sub { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Sub.Info, "Extra");
    }
}

public class SubDest_Fluent
{
    public string Info { get; set; }
}