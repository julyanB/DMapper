using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_ListMappingWithPropertyBindTo_ShouldMapPropertiesOnListItemsCorrectly;

// Models for Test 14
public class Src1_14
{
    public string Name { get; set; } = "Pesho";
    public List<Src2_14> Src2List { get; set; } = new List<Src2_14> { new Src2_14(), new Src2_14() };
}

public class Src2_14
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}

public class Dest1_14
{
    public string Name { get; set; }
    [BindTo("Src2List")]
    public List<Dest2_14> Src2List_13 { get; set; }
}

public class Dest2_14
{
    [BindTo("Age")]
    public int? Age2 { get; set; }
    public string? Name { get; set; }
}