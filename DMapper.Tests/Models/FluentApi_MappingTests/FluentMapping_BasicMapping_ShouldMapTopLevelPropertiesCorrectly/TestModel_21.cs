using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_BasicMapping_ShouldMapTopLevelPropertiesCorrectly;

public class SourceFluentTest1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }
    public Source2_Fluent Source2 { get; set; } = new Source2_Fluent();
}

public class Source2_Fluent
{
    public string SourceName2 { get; set; } = "SourceName2";
}

public class DestinationFluentTest1 : IDMapperConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Destination { get; set; }
    public Destination2_Fluent Source2 { get; set; }
    public string DontChange { get; set; } = "DontChange";
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Destination, "Source");
        // Map nested Source2 property using a dot‑notation key.
        builder.Map(x => Source2.Destination3_Fluent.DestinationName3, "Source2.SourceName2");
    }
}

public class Destination2_Fluent
{
    public Destination3_Fluent Destination3_Fluent { get; set; }
}

public class Destination3_Fluent
{
    public string DestinationName3 { get; set; }
}