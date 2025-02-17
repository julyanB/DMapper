using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_RelativeBindTo_ShouldMapNestedRelativePropertyCorrectly;

public class SourceFluentTest7
{
    public NestedSource7_Fluent A { get; set; } = new NestedSource7_Fluent();
}

public class NestedSource7_Fluent
{
    public NestedSource7Inner_Fluent B { get; set; } = new NestedSource7Inner_Fluent();
}

public class NestedSource7Inner_Fluent
{
    public string C { get; set; } = "Value7";
}

public class DestinationFluentTest7 : IDMapperConfiguration
{
    public DestinationTest7A_Fluent A { get; set; } = new DestinationTest7A_Fluent();
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        // Map A.X from the relative source candidate "A.B.C"
        builder.Map(x => A.X, "A.B.C");
    }
}

public class DestinationTest7A_Fluent
{
    public string X { get; set; }
}