using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_NestedCollectionMapping_ShouldMapNestedListsCorrectly;

// Models for Test 11
public class CollectionSource1
{
    public List<CollectionItemSource> Items { get; set; } = new List<CollectionItemSource>
    {
        new CollectionItemSource { Value = "A", Number = 1 },
        new CollectionItemSource { Value = "B", Number = 2 }
    };

    public CollectionItemSource[] ArrayItems { get; set; } = new CollectionItemSource[]
    {
        new CollectionItemSource { Value = "C", Number = 3 },
        new CollectionItemSource { Value = "D", Number = 4 }
    };
}

public class CollectionItemSource
{
    public string Value { get; set; }
    public int Number { get; set; }
}

public class CollectionDestination1
{
    [BindTo("Items")]
    public List<CollectionItemDest> Items { get; set; }

    [BindTo("ArrayItems")]
    public CollectionItemDest[] ArrayItems { get; set; }
}

public class CollectionItemDest
{
    public string Value { get; set; }
    public int Number { get; set; }
}