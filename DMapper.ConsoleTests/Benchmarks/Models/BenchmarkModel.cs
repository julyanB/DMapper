using DMapper.Attributes;

namespace TestCases.Benchmarks.Models;

// Source models
public class ComplexSource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public NestedSource Nested { get; set; }
    public List<NestedSource> NestedList { get; set; }
    public ComplexSource SelfReference { get; set; } // Circular reference
}

public class NestedSource
{
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public List<DeepNestedSource> DeepNested { get; set; }
}

public class DeepNestedSource
{
    public string Info { get; set; }
    public int Number { get; set; }
}

public class ComplexDestination
{
    public int Id { get; set; }
    [BindTo("Name")]
    public string FullName { get; set; }
    
    public NestedDestination Nested { get; set; }
    
    [BindTo("NestedList")]
    public List<NestedDestination> NestedList { get; set; }
    
    // Intentionally not mapping the circular reference directly.
    public ComplexDestination SelfReference { get; set; }
}

public class NestedDestination
{
    [BindTo("Description")]
    public string Desc { get; set; }
    
    public DateTime Date { get; set; }
    
    [ComplexBind("DeepNested.Info", "DeepNestedInfo")]
    public string DeepInfo { get; set; }
    
    public List<DeepNestedDestination> DeepNested { get; set; }
}

public class DeepNestedDestination
{
    public string Info { get; set; }
    public int Number { get; set; }
}