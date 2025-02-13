# DMapper

DMapper is a lightweight and efficient .NET object mapping library designed to simplify object transformation, deep copying, and recursive property binding using advanced reflection techniques. The latest version (v5) uses a flattened‐object approach that leverages a flattening engine with Next/Previous pointers for each flattened property. This enables precise control over both relative and absolute property mappings via custom attributes.

## Features

- **Flattening Engine with Next/Previous Pointers:**  
  Objects are flattened into a dictionary of properties, where each flattened property includes Next and Previous pointers. These pointers can be used as a fallback when an exact key isn’t found during mapping.
- **Advanced Recursive Mapping (v5):**  
  The v5 engine maps source objects to destination objects solely by merging their flattened representations. It supports both [BindTo] and [ComplexBind] attributes, enabling you to specify relative or absolute property paths (e.g. 'x.y.u').
- **Attribute-Based Mapping:**  
  - **[BindTo]:** Use this attribute to map a property from a source key. If the candidate key is relative (i.e. does not contain the separator), the engine prepends the parent path to form a fully qualified key. For example, if the destination flattened key is 'A.B.C' and you specify [BindTo("B.C")], it will look up 'A.B.C'.  
  - **[ComplexBind]:** Use this attribute for mapping complex or nested properties by specifying an exact (absolute) flattened key. The attribute’s destination key is used exactly as provided.
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

#### Example: Basic Mapping
```csharp
var source = new Source1
{
    Id = 1,
    Name = "Alice",
    Source = "SourceValue",
    Source2 = new Source2 { SourceName2 = "SourceName2" }
};

Destination1 dest = new Destination1
{
    DontChange = "DontChange"
};
dest = ReflectionHelper.ReplacePropertiesRecursive_V5<Destination1, Source1>(dest, source);

Console.WriteLine($"Id: {dest.Id}");
Console.WriteLine($"Name: {dest.Name}");
Console.WriteLine($"Destination (from [BindTo("Source")]): {dest.Destination}");
Console.WriteLine($"Destination2.DestinationName3 (from [BindTo("SourceName2")]): {dest.Source2?.DestinationName3}");
Console.WriteLine($"DontChange: {dest.DontChange}");
```

### 2. Using Nested [BindTo] Attributes with Dot‑Separated Paths

You can specify relative or absolute keys for inner properties.

**Relative BindTo:**
```csharp
public class DestinationTest7A
{
    // Candidate "B.C" is relative. For a flattened key "A.B.C", the candidate is transformed to "A.B.C".
    [BindTo("B.C")]
    public string X { get; set; }
}
```

**Absolute BindTo:**
```csharp
public class DestinationTest8
{
    // Candidate "N.O" is used exactly as is.
    [BindTo("N.O")]
    public string X { get; set; }
}
```

**Nested Relative BindTo on Inner Property:**
```csharp
public class DestinationTest9A
{
    // Candidate "Y" becomes "A.Y" when mapped (if the flattened key is "A.Y").
    [BindTo("Y")]
    public string Z { get; set; }
}
```

### 3. Using [ComplexBind] Attributes

[ComplexBind] lets you map nested properties using an absolute flattened key. For example:
```csharp
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

### 4. Array Mapping

Array properties are supported. Intermediate lists are created and later converted to arrays:
```csharp
public class DestinationTest5
{
    [BindTo("Items")]
    public string[] Items { get; set; }
}
```

### 5. Fluent API

DMapper provides extension methods for a fluent API:
```csharp
Destination1 destination = source.MapTo<Destination1>();
```

## Test Cases
Below are several test cases demonstrating different mapping scenarios:

Test 1: Basic Mapping with [BindTo] on Top-Level Properties
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
    public string SourceName2 { get; set; } = ""SourceName2"";
}
public class Destination1
{
    public int Id { get; set; }
    public string Name { get; set; }
    [BindTo(""Source"")]
    public string Destination { get; set; }
    public Destination2 Source2 { get; set; }
    public string DontChange { get; set; } = ""DontChange"";
}
public class Destination2
{
    [BindTo(""SourceName2"")]
    public string DestinationName3 { get; set; }
}
```

Test 2: Inner Mapping with [BindTo] on Nested Property

```csharp

public class SourceTest2
{
    public string Outer { get; set; } = ""OuterValue"";
    public InnerSource Inner { get; set; } = new InnerSource();
}
public class InnerSource
{
    public string InnerProp { get; set; } = ""InnerValue"";
}
public class DestinationTest2
{
    public string Outer { get; set; }
    public InnerDest Inner { get; set; }
}
public class InnerDest
{
    [BindTo(""InnerProp"")]
    public string MyInner { get; set; }
}


Test 3: ComplexBind Mapping Using an Absolute Destination Key

public class SourceTest3
{
    public string Data { get; set; } = ""DataFromSource"";
    public NestedSource Nested { get; set; } = new NestedSource();
}
public class NestedSource
{
    public string Info { get; set; } = ""NestedInfo"";
}
public class DestinationTest3
{
    public string Data { get; set; }
    [ComplexBind(""NestedDestination.Info"", ""Nested.Info"")]
    public NestedDest NestedDestination { get; set; }
}
public class NestedDest
{
    public string Info { get; set; }
}
Test 4: ComplexBind on Inner Property with an Absolute Key

public class SourceTest4
{
    public string Extra { get; set; } = ""ExtraValue"";
}
public class DestinationTest4
{
    [ComplexBind(""Sub.Info"", ""Extra"")]
    public SubDest Sub { get; set; }
}
public class SubDest
{
    public string Info { get; set; }
}
Test 5: Array Mapping with [BindTo]
```

```csharp

public class SourceTest5
{
    public string[] Items { get; set; } = new string[] { ""Item1"", ""Item2"", ""Item3"" };
}
public class DestinationTest5
{
    [BindTo(""Items"")]
    public string[] Items { get; set; }
}
Test 6: Fallback Mapping Using [BindTo] Candidate
```

```csharp
public class SourceTest6
{
    public string X { get; set; } = ""FallbackValue"";
    public string B { get; set; } = ""Beta"";
}
public class DestinationTest6
{
    [BindTo(""X"")]
    public string B { get; set; }
}
Test 7: Nested Relative [BindTo] with a Candidate like ""B.C""
```

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
    public string C { get; set; } = ""Value7"";
}
public class DestinationTest7
{
    public DestinationTest7A A { get; set; } = new DestinationTest7A();
}
public class DestinationTest7A
{
    [BindTo(""B.C"")]
    public string X { get; set; }
}
Test 8: Absolute [BindTo] with a Full Path
```

```csharp
public class SourceTest8
{
    public string M { get; set; } = ""MValue"";
    public NestedSource8 N { get; set; } = new NestedSource8();
}
public class NestedSource8
{
    public string O { get; set; } = ""OValue"";
}
public class DestinationTest8
{
    [BindTo(""N.O"")]
    public string X { get; set; }
}
Test 9: Nested Relative [BindTo] on an Inner Property
```

```csharp
public class SourceTest9
{
    public NestedSource9 A { get; set; } = new NestedSource9();
}
public class NestedSource9
{
    public string Y { get; set; } = ""Value9"";
}
public class DestinationTest9
{
    public DestinationTest9A A { get; set; } = new DestinationTest9A();
}
public class DestinationTest9A
{
    [BindTo(""Y"")]
    public string Z { get; set; }
}
```

Design & Structure
Flattening Engine:
Converts objects and types into a flattened dictionary with Next/Previous pointers.
Mapping Engine (v5):
Uses the flattened representation to merge source and destination values. It processes [BindTo] (both relative and absolute) and [ComplexBind] attributes.
Rehydration:
The merged dictionary is rehydrated into a fully instantiated destination object, with intermediate objects and collections created as needed.
Fluent API:
Extension methods simplify mapping code, making it clear and maintainable.
Contributing
Feel free to fork the repository and submit pull requests to add features or fix bugs.

License
DMapper is licensed under the MIT License.

Happy coding with DMapper! ";

arduino
Copy
Edit
        File.WriteAllText("README.md", readme);
        Console.WriteLine("README.md has been generated.");
    }
}
}

## Design & Structure

- **Flattening Engine:**  
  Converts objects and types into a flattened dictionary with Next/Previous pointers.
- **Mapping Engine (v5):**  
  Uses the flattened representation to merge source and destination values. It processes [BindTo] (both relative and absolute) and [ComplexBind] attributes.
- **Rehydration:**  
  The merged dictionary is rehydrated into a fully instantiated destination object, with intermediate objects and collections created as needed.
- **Fluent API:**  
  Extension methods simplify mapping code, making it clear and maintainable.

## Contributing

Feel free to fork the repository and submit pull requests to add features or fix bugs.

## License

DMapper is licensed under the MIT License.

Happy coding with DMapper!
