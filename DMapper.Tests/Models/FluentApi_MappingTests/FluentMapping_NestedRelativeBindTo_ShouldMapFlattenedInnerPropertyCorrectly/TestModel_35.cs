using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_NestedRelativeBindTo_ShouldMapFlattenedInnerPropertyCorrectly;

public class SourceFluentTest9
{
    public NestedSource9_Fluent A { get; set; } = new NestedSource9_Fluent();
}

public class NestedSource9_Fluent
{
    public string Y { get; set; } = "Value9";
}

public class DestinationFluentTest9 : IDMapperConfiguration
{
    public DestinationTest9A_Fluent A { get; set; } = new DestinationTest9A_Fluent();
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        // Map A.Z from the source candidate "Y" (as if flattened)
        builder.Map(x => A.Z, "A.Y");
    }
}

public class DestinationTest9A_Fluent
{
    public string Z { get; set; }
}