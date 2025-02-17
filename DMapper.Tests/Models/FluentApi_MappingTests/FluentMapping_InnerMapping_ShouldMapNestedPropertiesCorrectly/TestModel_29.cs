using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_InnerMapping_ShouldMapNestedPropertiesCorrectly;

public class SourceFluentTest2
{
    public string Outer { get; set; } = "OuterValue";
    public InnerSourceFluent Inner { get; set; } = new InnerSourceFluent();
}

public class InnerSourceFluent
{
    public string InnerProp { get; set; } = "InnerValue";
}

public class DestinationFluentTest2 : IDMapperConfiguration
{
    public string Outer { get; set; }
    public InnerDestFluent Inner { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Outer, "Outer")
            .Map(x => Inner.MyInner, "Inner.InnerProp");
    }
}

public class InnerDestFluent : IDMapperConfiguration
{
    public string MyInner { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => MyInner, "InnerProp");
    }
}