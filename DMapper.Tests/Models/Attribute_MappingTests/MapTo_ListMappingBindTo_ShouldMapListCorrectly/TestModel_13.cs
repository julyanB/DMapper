using DMapper.Attributes;

namespace DMapper.Tests.Models.MappingTests.MapTo_ListMappingBindTo_ShouldMapListCorrectly;

// Models for Test 13
public class Src1_13
{
    public string Name { get; set; } = "Pesho";
    public List<Src2_13> Src2List { get; set; } = new List<Src2_13> { new Src2_13(), new Src2_13() };
}

public class Src2_13
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}

public class Dest1_13
{
    public string Name { get; set; }
    [BindTo("Src2List")]
    public List<Dest2_13> Src2List_13 { get; set; }
}

public class Dest2_13
{
    public int? Age { get; set; }
    public string? Name { get; set; }
}