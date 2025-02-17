using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_ListMappingWithPropertyBindTo_ShouldMapPropertiesOnListItemsCorrectly;

public class Src1_14_Fluent
{
    public string Name { get; set; } = "Pesho";
    public List<Src2_14_Fluent> Src2List { get; set; } = new List<Src2_14_Fluent> { new Src2_14_Fluent(), new Src2_14_Fluent() };
}

public class Src2_14_Fluent
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}

public class Dest1_14_Fluent : IDMapperConfiguration
{
    public string Name { get; set; }
    public List<Dest2_14_Fluent> Src2List_13 { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Name, "Name")
            .Map(x => Src2List_13, "Src2List");
    }
}

public class Dest2_14_Fluent : IDMapperConfiguration
{
    public int? Age2 { get; set; }
    public string Name { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Age2, "Age")
            .Map(x => Name, "Name");
    }
}