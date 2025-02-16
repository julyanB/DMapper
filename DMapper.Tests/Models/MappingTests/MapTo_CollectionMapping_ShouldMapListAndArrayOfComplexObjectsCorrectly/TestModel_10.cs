namespace DMapper.Tests.Models.MappingTests.MapTo_CollectionMapping_ShouldMapListAndArrayOfComplexObjectsCorrectly;

// Models for Test 10
public class Src1
{
    public string Name { get; set; } = "Pesho";
    public List<Src2> Src2List { get; set; } = new List<Src2> { new Src2(), new Src2() };
}

public class Src2
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}

public class Dest1
{
    public string Name { get; set; }
    public List<Dest2> Src2List { get; set; }
}

public class Dest2
{
    public int? Age { get; set; }
    public string? Name { get; set; }
}