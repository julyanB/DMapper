using DMapper.Helpers.FluentConfigurations.Contracts;

namespace DMapper.Tests.Models.FluentApi_MappingTests.FluentMapping_NestedCollectionMapping_ShouldMapCollectionsCorrectly;

public class CollectionSourceFluent1
{
    public System.Collections.Generic.List<CollectionItemSourceFluent> Items { get; set; } = new System.Collections.Generic.List<CollectionItemSourceFluent>
    {
        new CollectionItemSourceFluent { Value = "A", Number = 1 },
        new CollectionItemSourceFluent { Value = "B", Number = 2 }
    };

    public CollectionItemSourceFluent[] ArrayItems { get; set; } = new CollectionItemSourceFluent[]
    {
        new CollectionItemSourceFluent { Value = "C", Number = 3 },
        new CollectionItemSourceFluent { Value = "D", Number = 4 }
    };
}

public class CollectionItemSourceFluent
{
    public string Value { get; set; }
    public int Number { get; set; }
}

public class CollectionDestinationFluent1 : IDMapperConfiguration
{
    public System.Collections.Generic.List<CollectionItemDestFluent> Items { get; set; }
    public CollectionItemDestFluent[] ArrayItems { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Items, "Items")
            .Map(x => ArrayItems, "ArrayItems");
    }
}

public class CollectionItemDestFluent : IDMapperConfiguration
{
    public string Value { get; set; }
    public int Number { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Value, "Value")
            .Map(x => Number, "Number");
    }
}
