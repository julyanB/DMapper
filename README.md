# DMapper

DMapper is a lightweight and efficient .NET object mapping library designed to simplify object transformation, deep copying, and recursive property binding using advanced reflection techniques. The latest version (DMapper (latest version)) uses a flattened-object approach that leverages a flattening engine with Next/Previous pointers for each flattened property. This enables precise control over both relative and absolute property mappings via custom attributes.

## Features

- **Flattening Engine with Next/Previous Pointers:**
  Objects are flattened into a dictionary of properties, where each flattened property includes Next and Previous pointers. These pointers can be used as a fallback when an exact key isn’t found during mapping.
- **Advanced Recursive Mapping (DMapper (latest version)):**
  The DMapper (latest version) engine maps source objects to destination objects solely by merging their flattened representations. It supports both [BindTo] and [ComplexBind] attributes, enabling you to specify relative or absolute property paths (e.g. 'x.y.u').
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

### 1. Mapping with DMapper (latest version)

The DMapper (latest version) mapping engine relies solely on the flattened representation of source and destination objects. It performs the following steps:
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

### Test 15: Cycle Dependency Mapping (Stack Overflow Test)
#### Description
Tests the handling of cyclic dependencies in object mapping to prevent infinite recursion.

#### Source Code
```csharp
public class SourceTest15
{
    public string Name { get; set; } = "Parent";
    public SourceTest15 Child { get; set; }
}

public class DestinationTest15
{
    public string Name { get; set; }
    public DestinationTest15 Child { get; set; }
}
```
#### Expected Behavior
- The mapping should detect circular references and prevent infinite recursion.
- The parent-child structure should be mapped correctly without causing a stack overflow.

### Test 16: Multi-Source with Same Destination
#### Description
Maps multiple source properties to a single destination property.

#### Source Code
```csharp
public class SourceTest1_16
{
    public string Name1 { get; set; } = "Source1";
}

public class SourceTest2_16
{
    public string Name2 { get; set; } = "Source2";
}

public class DestinationTest16
{
    [BindTo("Name1, Name2")]
    public string Name { get; set; }
}
```
#### Expected Behavior
- The destination `Name` should be assigned the value of `Name1` or `Name2` based on availability.

### Test 17: Multi ComplexBinding Source with Same Destination
#### Description
Tests complex binding where multiple sources contribute to the same nested destination object.

#### Source Code
```csharp
public class SourceTest1_17
{
    public string Name1 { get; set; } = "Source1";
}

public class SourceTest2_17
{
    public string Name2 { get; set; } = "Source2";
}

public class DestinationTest1_17
{
    [ComplexBind("DestinationTest2_17.Name", "Name2")]
    [ComplexBind("DestinationTest2_17.Name", "Name1")]
    public DestinationTest2_17 DestinationTest2_17 { get; set; }
}

public class DestinationTest2_17
{
    public string Name { get; set; }
}
```
#### Expected Behavior
- The `DestinationTest2_17.Name` should take the first available value from `Name1` or `Name2`.

### Test 18: Binding to a Complex Object
#### Description
Maps a nested complex source object to a corresponding destination object using `[BindTo]`.

#### Source Code
```csharp
public class SourceTest1_18
{
    public SourceTest2_18 SourceTest2_18 { get; set; } = new();
}

public class SourceTest2_18
{
    public string Name { get; set; } = "Source";
}

public class DestinationTest1_18
{
    public DestinationTest2_18 DestinationTest2_18 { get; set; }
}

public class DestinationTest2_18
{
    [BindTo("SourceTest2_18.Name")]
    public string Name { get; set; }
}
```
#### Expected Behavior
- The `DestinationTest2_18.Name` should correctly map the value from `SourceTest2_18.Name`.

### Test 19: Binding to a Complex Object with a `[BindTo]` Attribute on a Complex Object
#### Description
Maps a complex source object while also binding individual properties inside it.

#### Source Code
```csharp
public class SourceTest1_19
{
    public SourceTest2_19 SourceTest2_19 { get; set; } = new();
}

public class SourceTest2_19
{
    public string Name { get; set; } = "Source";
    public int Age { get; set; } = 25;
}

public class DestinationTest1_19
{
    [BindTo("SourceTest2_19")]
    public DestinationTest2_19 DestinationTest2_19 { get; set; }
}

public class DestinationTest2_19
{
    [BindTo("Name")]
    public string Name2 { get; set; }
    public int Age { get; set; }
}
```
#### Expected Behavior
- `DestinationTest2_19.Name2` should receive the value from `SourceTest2_19.Name`.
- `DestinationTest2_19.Age` should be mapped from `SourceTest2_19.Age`.

# Object Flattening with DMapper

## Overview

The `ObjectFlattener` utility in DMapper provides a way to flatten complex objects into key-value pairs, making it easier to work with deeply nested structures. This is especially useful for serialization, mapping, and transformation scenarios.

The utility supports flattening:
- **Object Instances**: Converts an object into a dictionary with key paths.
- **Type Structures**: Analyzes a type and produces a structure representation.
- **Collections & Arrays**: Handles lists, dictionaries, and arrays gracefully.

---

## Installation

Ensure that your project references the `DMapper.Helpers` namespace:

```csharp
using DMapper.Helpers;
using DMapper.Helpers.Models;
```

If you are using the extension methods:

```csharp
using DMapper.Extensions;
```

---

## Basic Usage

### 1. Flattening an Object Instance

To flatten an instance of an object:

```csharp
var myObject = new
{
    Name = "John Doe",
    Address = new { City = "New York", Zip = "10001" },
    Tags = new[] { "Developer", "Blogger" }
};

FlattenResult result = myObject.Flatten();
```

This will produce key-value pairs like:

```
Name -> "John Doe"
Address.City -> "New York"
Address.Zip -> "10001"
Tags[0] -> "Developer"
Tags[1] -> "Blogger"
```

### 2. Flattening a Type Structure

To get the structure of a type without an instance:

```csharp
FlattenResult result = typeof(MyClass).Flatten();
```

This generates a similar dictionary but with `null` values, representing the type structure.

### 3. Flattening a Generic Type

```csharp
FlattenResult result = Flatten<MyClass>();
```

This is equivalent to calling `Flatten(typeof(MyClass))`.

---

## Understanding `FlattenResult`

A `FlattenResult` contains:
- **`FlattenedType`**: The type of the object that was flattened.
- **`Properties`**: A dictionary mapping flattened property paths to `FlattenedProperty` objects.

Each `FlattenedProperty` consists of:
- **`Value`**: The actual value of the property.
- **`PropertyType`**: The type of the property.
- **`Next` and `Previous`**: Pointers to adjacent properties in sorted order, allowing for sequential access.

Example:

```csharp
foreach (var entry in result.Properties)
{
    Console.WriteLine($"{entry.Key} -> {entry.Value.Value} (Type: {entry.Value.PropertyType.Name})");
}
```

---

## Handling Collections

The `ObjectFlattener` handles lists and arrays using indexed paths:

```csharp
var obj = new { Numbers = new[] { 1, 2, 3 } };
FlattenResult result = obj.Flatten();
```

Output:
```
Numbers[0] -> 1
Numbers[1] -> 2
Numbers[2] -> 3
```

---

## Custom Separator

By default, the separator is `.` but you can customize it:

```csharp
FlattenResult result = myObject.Flatten(separator: "/");
```

Produces:
```
Address/City -> "New York"
```

---

## Advanced Flattening Capabilities

### Flattening with `GlobalConstants.DefaultDotSeparator`

If your application uses `GlobalConstants.DefaultDotSeparator` as the standard separator, you can leverage it:

```csharp
FlattenResult result = myObject.Flatten(separator: GlobalConstants.DefaultDotSeparator);
```

### Flattening Nested Collections

```csharp
var obj = new { 
    Categories = new[] {
        new { Name = "Tech", Items = new[] { "Laptop", "Phone" } },
        new { Name = "Books", Items = new[] { "Fiction", "Non-Fiction" } }
    }
};

FlattenResult result = obj.Flatten();
```

Output:
```
Categories[0].Name -> "Tech"
Categories[0].Items[0] -> "Laptop"
Categories[0].Items[1] -> "Phone"
Categories[1].Name -> "Books"
Categories[1].Items[0] -> "Fiction"
Categories[1].Items[1] -> "Non-Fiction"
```

### Flattening Dictionaries

Dictionaries are also supported and use key-based paths:

```csharp
var obj = new Dictionary<string, object>
{
    ["User"] = new { Name = "John", Age = 30 },
    ["Settings"] = new { Theme = "Dark" }
};

FlattenResult result = obj.Flatten();
```

Output:
```
User.Name -> "John"
User.Age -> 30
Settings.Theme -> "Dark"
```

---

## Rehydrating Objects

The `FlattenResult` allows rehydration into a strongly-typed object:

```csharp
var rehydrated = (MyClass)result.Rehydrate();
```

This reconstructs the object with properties populated from the flattened values.

### Handling Nested Properties

When rehydrating, intermediate objects and collections are instantiated automatically:

```csharp
FlattenResult result = Flatten<MyClass>();
MyClass myObject = (MyClass)result.Rehydrate();
```

# FlattenResult and PropertyMapping Documentation

## Overview

This document provides an in-depth explanation of the `FlattenResult` and `PropertyMapping` classes in DMapper. These classes play a key role in object flattening and property mapping during transformation and serialization processes.

---

## `FlattenResult`

### Purpose

The `FlattenResult` class represents the output of an object flattening operation. It provides access to a structured key-value representation of an object and includes functionality to restore the original object structure.

### Public Members

#### Properties

- **`FlattenedType`**: The type of the object that was flattened.
- **`Properties`**: A dictionary mapping property paths to `FlattenedProperty` objects.

#### Constructor

```csharp
public FlattenResult(Type flattenedType, Dictionary<string, FlattenedProperty> properties)
```
- **`flattenedType`**: Specifies the type of the original object.
- **`properties`**: Stores the flattened representation as key-value pairs.

#### Rehydration

The `Rehydrate` method reconstructs an object from its flattened representation.

```csharp
public object Rehydrate(string separator = ".")
```
- **Returns**: A new instance of the original type with properties and collections properly instantiated.
- **Usage**:

```csharp
FlattenResult result = Flatten<MyClass>();
MyClass rehydratedObject = (MyClass)result.Rehydrate();
```

---

## `PropertyMapping`

### Purpose

The `PropertyMapping` class defines a mapping between a destination property and a chain of source properties, enabling structured object transformation.

### Public Members

#### Properties

- **`DestinationProperty`**: The `PropertyInfo` representing the destination property.
- **`SourcePropertyChain`**: An array of `PropertyInfo` objects representing the property path from the source object.

#### Example Usage

```csharp
var mapping = new PropertyMapping
{
    DestinationProperty = typeof(DestinationClass).GetProperty("NestedProperty"),
    SourcePropertyChain = new[]
    {
        typeof(SourceClass).GetProperty("SubObject"),
        typeof(SubObjectClass).GetProperty("Value")
    }
};
```

This example maps `NestedProperty` in `DestinationClass` from `SourceClass.SubObject.Value`.

---

## Conclusion

The `FlattenResult` and `PropertyMapping` classes are essential for managing object transformation, ensuring that flattened representations maintain structure and allowing for seamless rehydration into objects.


---

## Conclusion

Flattening is useful for mapping, serialization, and data transformation. Using `ObjectFlattener`, you can easily convert objects into structured key-value pairs and rehydrate them back into objects when needed. The framework handles collections, nested properties, and dictionaries, making it a powerful tool for handling structured data.


# DMapper Fluent API Documentation

## Introduction
DMapper's Fluent API provides a powerful and flexible way to define mappings between source and destination objects without relying on attributes. Instead, mappings are configured programmatically using a builder pattern. This approach offers greater control, allowing overrides, multi-source mappings, nested property bindings, and complex binding scenarios.

## Getting Started
### Installation
Ensure that DMapper and its Fluent API extensions are installed in your project.

```csharp
using DMapper.Extensions;
using DMapper.Helpers.FluentConfigurations.Contracts;
```

## Defining Mappings
Mappings are defined in destination classes by implementing the `IDMapperConfiguration` interface. The `ConfigureMapping` method allows specifying source properties using the `builder.Map` method.

### Basic Mapping
In this example, a simple mapping is defined where properties from `SourceFluentTest1` are mapped to `DestinationFluentTest1`.

```csharp
public class SourceFluentTest1
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Source { get; set; }
}

public class DestinationFluentTest1 : IDMapperConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Destination { get; set; }

    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Destination, "Source");
    }
}
```

### Mapping Nested Properties
For mapping nested properties, dot notation can be used.

```csharp
public class SourceFluentTest2
{
    public string Outer { get; set; }
    public InnerSourceFluent Inner { get; set; } = new InnerSourceFluent();
}

public class InnerSourceFluent
{
    public string InnerProp { get; set; } = "InnerValue";
}

public class DestinationFluentTest2 : IDMapperConfiguration
{
    public string Outer { get; set; }
    public InnerDestFluent Inner { get; set; }

    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Outer, "Outer")
            .Map(x => Inner.MyInner, "Inner.InnerProp");
    }
}

public class InnerDestFluent : IDMapperConfiguration
{
    public string MyInner { get; set; }
    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => MyInner, "InnerProp");
    }
}
```

### Mapping Collections
Collections (lists and arrays) can be mapped similarly.

```csharp
public class Src1_Fluent
{
    public string Name { get; set; } = "Pesho";
    public List<Src2_Fluent> Src2List { get; set; } = new List<Src2_Fluent> { new Src2_Fluent(), new Src2_Fluent() };
}

public class Src2_Fluent
{
    public int Age { get; set; } = 10;
    public string Name { get; set; } = "John";
}

public class Dest1_Fluent : IDMapperConfiguration
{
    public string Name { get; set; }
    public List<Dest2_Fluent> Src2List { get; set; }

    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Name, "Name")
            .Map(x => Src2List, "Src2List");
    }
}

public class Dest2_Fluent : IDMapperConfiguration
{
    public int? Age { get; set; }
    public string Name { get; set; }

    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Age, "Age")
            .Map(x => Name, "Name");
    }
}
```

### Multi-Source Mapping
Properties from different source objects can be mapped into a single destination property using multiple candidate keys.

```csharp
public class SourceFluentTest1_16
{
    public string Name1 { get; set; } = "Source1";
}

public class SourceFluentTest2_16
{
    public string Name2 { get; set; } = "Source2";
}

public class DestinationFluentTest16 : IDMapperConfiguration
{
    public string Name { get; set; }

    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Name, "Name1", "Name2");
    }
}
```

### Complex Binding
Complex objects and nested properties can be mapped explicitly.

```csharp
public class SourceFluentTest3
{
    public string Data { get; set; } = "DataFromSource";
    public NestedSource_Fluent Nested { get; set; } = new NestedSource_Fluent();
}

public class NestedSource_Fluent
{
    public string Info { get; set; } = "NestedInfo";
}

public class DestinationFluentTest3 : IDMapperConfiguration
{
    public string Data { get; set; }
    public NestedDest_Fluent NestedDestination { get; set; }

    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Data, "Data")
            .Map(x => NestedDestination.Info, "Nested.Info");
    }
}

public class NestedDest_Fluent
{
    public string Info { get; set; }
}
```

## Handling Circular References
To prevent infinite loops when mapping objects with circular references, mappings should be controlled.

```csharp
public class SourceFluentTest15
{
    public string Name { get; set; } = "Parent";
    public SourceFluentTest15 Child { get; set; }
}

public class DestinationFluentTest15 : IDMapperConfiguration
{
    public string Name { get; set; }
    public DestinationFluentTest15 Child { get; set; }

    public void ConfigureMapping(IDMapperConfigure builder)
    {
        builder.Map(x => Name, "Name"); // Avoids recursion
    }
}
```

## Conclusion
DMapper's Fluent API provides fine-grained control over mapping definitions, supporting nested properties, collections, multi-source inputs, and circular reference handling. By leveraging programmatic configurations, developers can customize mappings without modifying source models.



---

## License

DMapper is licensed under the MIT License.

Happy coding with DMapper! 🚀

