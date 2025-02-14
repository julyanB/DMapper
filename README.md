# DMapper

DMapper is a lightweight and efficient .NET object mapping library designed to simplify object transformation, deep copying, and recursive property binding using advanced reflection techniques. The latest version (v5) uses a flattened-object approach that leverages a flattening engine with Next/Previous pointers for each flattened property. This enables precise control over both relative and absolute property mappings via custom attributes.

## Features

- **Flattening Engine with Next/Previous Pointers:**
  Objects are flattened into a dictionary of properties, where each flattened property includes Next and Previous pointers. These pointers can be used as a fallback when an exact key isn’t found during mapping.
- **Advanced Recursive Mapping (v5):**
  The v5 engine maps source objects to destination objects solely by merging their flattened representations. It supports both [BindTo] and [ComplexBind] attributes, enabling you to specify relative or absolute property paths (e.g. 'x.y.u').
- **Attribute-Based Mapping:**
  - **[BindTo]:** Use this attribute to map a property from a source key. If the candidate key is relative (i.e. does not contain the separator), the engine prepends the parent path to form a fully qualified key.
  - **[ComplexBind]:** Use this attribute for mapping complex or nested properties by specifying an exact (absolute) flattened key.
- **Array and Collection Support:**
  Intermediate collections are created as List<T> and converted into arrays during rehydration, ensuring that array-typed properties are properly instantiated.
- **Deep Copying and Fluent Extensions:**
  In addition to mapping, DMapper provides methods for deep copying objects and extension methods for a fluent API.
- **Preserving Unmapped Properties:**
  If a destination property already has a non-null value and is not remapped, its value remains unchanged.

## Installation

Simply add the DMapper source files to your .NET project. No external dependencies are required.

## Usage

### 1. Mapping with v5

The v5 mapping engine relies solely on the flattened representation of source and destination objects. It performs the following steps:
1. **Flatten the Source:**
   The source object is converted into a dictionary of flattened properties with actual values.
2. **Flatten the Destination Structure:**
   The destination type’s structure is flattened (values are initially null). Then the actual destination instance is flattened and its non-null values are preserved.
3. **Merge:**
   The engine merges direct key matches, processes [BindTo] attributes (adding parent paths to relative candidate keys), and applies [ComplexBind] attributes using absolute keys.
4. **Rehydrate:**
   A new destination instance is created by rehydrating the merged flatten dictionary, with all intermediate objects and collections instantiated.

### Test Cases

#### Test 1: Basic Mapping with [BindTo] on Top-Level Properties
```csharp
public class Source1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }
    public Source2 Source2 { get; set; } = new Source2();
}
public class Source2
{
    public string SourceName2 { get; set; } = "SourceName2";
}
public class Destination1
{
    public int Id { get; set; }
    public string Name { get; set; }
    [BindTo("Source")]
    public string Destination { get; set; }
    public Destination2 Source2 { get; set; }
    public string DontChange { get; set; } = "DontChange";
}
public class Destination2
{
    [BindTo("SourceName2")]
    public string DestinationName3 { get; set; }
}
```

#### Test 2: Inner Mapping with [BindTo] on Nested Property
```csharp
public class SourceTest2
{
    public string Outer { get; set; } = "OuterValue";
    public InnerSource Inner { get; set; } = new InnerSource();
}
public class InnerSource
{
    public string InnerProp { get; set; } = "InnerValue";
}
public class DestinationTest2
{
    public string Outer { get; set; }
    public InnerDest Inner { get; set; }
}
public class InnerDest
{
    [BindTo("InnerProp")]
    public string MyInner { get; set; }
}
```

#### Test 3: ComplexBind Mapping Using an Absolute Destination Key
```csharp
public class SourceTest3
{
    public string Data { get; set; } = "DataFromSource";
    public NestedSource Nested { get; set; } = new NestedSource();
}
public class NestedSource
{
    public string Info { get; set; } = "NestedInfo";
}
public class DestinationTest3
{
    public string Data { get; set; }
    [ComplexBind("NestedDestination.Info", "Nested.Info")]
    public NestedDest NestedDestination { get; set; }
}
public class NestedDest
{
    public string Info { get; set; }
}
```

#### Test 4: ComplexBind on Inner Property with an Absolute Key
```csharp
public class SourceTest4
{
    public string Extra { get; set; } = "ExtraValue";
}
public class DestinationTest4
{
    [ComplexBind("Sub.Info", "Extra")]
    public SubDest Sub { get; set; }
}
public class SubDest
{
    public string Info { get; set; }
}
```

#### Test 5: Array Mapping with [BindTo]
```csharp
public class SourceTest5
{
    public string[] Items { get; set; } = new string[] { "Item1", "Item2", "Item3" };
}
public class DestinationTest5
{
    [BindTo("Items")]
    public string[] Items { get; set; }
}
```

#### Test 6: Fallback Mapping Using [BindTo] Candidate
```csharp
public class SourceTest6
{
    public string X { get; set; } = "FallbackValue";
    public string B { get; set; } = "Beta";
}
public class DestinationTest6
{
    [BindTo("X")]
    public string B { get; set; }
}
```

#### Test 7: Relative BindTo with Nested Path "B.C"
```csharp
public class SourceTest7
{
    public NestedSource7 A { get; set; } = new NestedSource7();
}
public class NestedSource7
{
    public NestedSource7Inner B { get; set; } = new NestedSource7Inner();
}
public class NestedSource7Inner
{
    public string C { get; set; } = "Value7";
}
public class DestinationTest7
{
    public DestinationTest7A A { get; set; } = new DestinationTest7A();
}
public class DestinationTest7A
{
    [BindTo("A.B.C")]
    public string X { get; set; }
}
```

#### Test 8: Absolute BindTo with Full Path
```csharp
public class SourceTest8
{
    public string M { get; set; } = "MValue";
    public NestedSource8 N { get; set; } = new NestedSource8();
}
public class NestedSource8
{
    public string O { get; set; } = "OValue";
}
public class DestinationTest8
{
    [BindTo("N.O")]
    public string X { get; set; }
}
```

#### Test 9: Nested Relative BindTo on Inner Property
```csharp
public class SourceTest9
{
    public NestedSource9 A { get; set; } = new NestedSource9();
}
public class NestedSource9
{
    public string Y { get; set; } = "Value9";
}
public class DestinationTest9
{
    public DestinationTest9A A { get; set; } = new DestinationTest9A();
}
public class DestinationTest9A
{
    [BindTo("Y")]
    public string Z { get; set; }
}
```

#### Test 10: Mapping a Collection Property (List of Complex Objects)
```csharp
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
    public string? Name { get; set; }
    public List<Dest2> Src2List { get; set; }
}
public class Dest2
{
    public int? Age { get; set; }
    public string? Name { get; set; }
}
```

## Test 11: Mapping a Collection (List & Array) of Complex Objects
```csharp
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
```

## Test 12: Mapping Nested Collections (List of Lists)
```csharp
public class NestedCollectionSource
{
    public List<List<CollectionItemSource>> NestedItems { get; set; } = new List<List<CollectionItemSource>>
    {
        new List<CollectionItemSource>
        {
            new CollectionItemSource { Value = "E", Number = 5 },
            new CollectionItemSource { Value = "F", Number = 6 }
        },
        new List<CollectionItemSource>
        {
            new CollectionItemSource { Value = "G", Number = 7 }
        }
    };
}
public class NestedCollectionDestination
{
    public List<List<CollectionItemDest>> NestedItems { get; set; }
}
```

## Test 13: Mapping a List with [BindTo]
```csharp
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
    public string? Name { get; set; }
    
    [BindTo("Src2List")]
    public List<Dest2_13> Src2List_13 { get; set; }
}
public class Dest2_13
{
    public int? Age { get; set; }
    public string? Name { get; set; }
}
```

## Test 14: Mapping a List with [BindTo] on Their Properties as Well
```csharp
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
    public string? Name { get; set; }
    
    [BindTo("Src2List")]
    public List<Dest2_14> Src2List_13 { get; set; }
}
public class Dest2_14
{
    [BindTo("Age")]
    public int? Age2 { get; set; }
    public string? Name { get; set; }
}
```


---

## License

DMapper is licensed under the MIT License.

Happy coding with DMapper! 🚀

