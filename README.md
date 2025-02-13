# DMapper

DMapper is a lightweight and efficient .NET object mapping library designed to simplify object transformation, deep copying, and recursive property binding using advanced reflection techniques. The latest version (v5) uses a flattened‐object approach that leverages a flattening engine with Next/Previous pointers for each flattened property. This enables precise control over both relative and absolute property mappings via custom attributes.

---

## Features

- **Flattening Engine with Next/Previous Pointers**: Objects are flattened into a dictionary of properties, where each flattened property includes Next and Previous pointers. These pointers can be used as a fallback when an exact key isn’t found during mapping.
- **Advanced Recursive Mapping (v5)**: The v5 engine maps source objects to destination objects solely by merging their flattened representations. It supports both [BindTo] and [ComplexBind] attributes, enabling you to specify relative or absolute property paths (e.g. 'x.y.u').
- **Attribute-Based Mapping**:
  - **[BindTo]**: Use this attribute to map a property from a source key. If the candidate key is relative (i.e. does not contain the separator), the engine prepends the parent path to form a fully qualified key. For example, if the destination flattened key is 'A.B.C' and you specify [BindTo("B.C")], it will look up 'A.B.C'.
  - **[ComplexBind]**: Use this attribute for mapping complex or nested properties by specifying an exact (absolute) flattened key. The attribute’s destination key is used exactly as provided.
- **Array and Collection Support**: Intermediate collections are created as List<T> and converted into arrays during rehydration, ensuring that array-typed properties are properly instantiated.
- **Deep Copying and Fluent Extensions**: In addition to mapping, DMapper provides methods for deep copying objects and extension methods for a fluent API.

---

## Installation

Simply add the DMapper source files to your .NET project. No external dependencies are required.

---

## Usage

### 1. Mapping with v5

The v5 mapping engine relies solely on the flattened representation of source and destination objects. It performs the following steps:
1. **Flatten the Source**: The source object is converted into a dictionary of flattened properties with actual values.
2. **Flatten the Destination Structure**: The destination type’s structure is flattened (values are initially null).
3. **Merge**: The engine merges direct key matches, processes [BindTo] attributes (adding parent paths to relative candidate keys), and applies [ComplexBind] attributes using absolute keys.
4. **Rehydrate**: A new destination instance is created by rehydrating the merged flatten dictionary, with all intermediate objects and collections instantiated.

#### Example: Basic Mapping
```csharp
var source = new Source1
{
    Id = 1,
    Name = "Alice",
    Source = "SourceValue",
    Source2 = new Source2 { SourceName2 = "SourceName2" }
};

Destination1 dest = new Destination1();
dest = ReflectionHelper.ReplacePropertiesRecursive_V5<Destination1, Source1>(dest, source);

Console.WriteLine($"Id: {dest.Id}");
Console.WriteLine($"Name: {dest.Name}");
Console.WriteLine($"Destination (from [BindTo("Source")]): {dest.Destination}");
Console.WriteLine($"Destination2.DestinationName3 (from [BindTo("SourceName2")]): {dest.Source2?.DestinationName3}");
```

### 2. Using Nested [BindTo] Attributes with Dot‑Separated Paths

You can specify relative or absolute keys for inner properties.

- **Relative BindTo:**
```csharp
public class DestinationTest7A
{
    // Candidate "B.C" is relative. For a flattened key "A.B.C", the candidate is transformed to "A.B.C".
    [BindTo("B.C")]
    public string X { get; set; }
}
```

- **Absolute BindTo:**
```csharp
public class DestinationTest8
{
    // Candidate "N.O" is used exactly as is.
    [BindTo("N.O")]
    public string X { get; set; }
}
```

- **Nested Relative BindTo on Inner Property:**
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

---

## Test Cases

DMapper includes multiple test cases demonstrating various mapping scenarios:

1. **Test 1:** Basic mapping with [BindTo] on top-level properties.
2. **Test 2:** Inner mapping with [BindTo] on nested properties.
3. **Test 3:** Mapping using [ComplexBind] with an absolute destination key.
4. **Test 4:** [ComplexBind] on an inner property using an absolute key.
5. **Test 5:** Array mapping with [BindTo].
6. **Test 6:** Fallback candidate using [BindTo] when the source key doesn’t match the destination name.
7. **Test 7:** Nested relative [BindTo] with a candidate like "B.C" that becomes "A.B.C".
8. **Test 8:** Absolute [BindTo] with a full path (e.g. candidate "N.O" used as is).
9. **Test 9:** Nested relative [BindTo] on an inner property (e.g. candidate "Y" mapping to "A.Y").

---

## Design & Structure

DMapper is designed with modularity in mind:

- **Flattening Engine:** Converts objects and types into a flattened dictionary with Next/Previous pointers.
- **Mapping Engine (v5):** Uses the flattened representation to merge source and destination values, processing [BindTo] (both relative and absolute) and [ComplexBind] attributes.
- **Rehydration:** The merged dictionary is rehydrated into a fully instantiated destination object, with intermediate objects and collections created as needed.
- **Fluent API:** Extension methods make mapping code clear and maintainable.

---

## Contributing

Feel free to fork the repository and submit pull requests to add features or fix bugs.

---

## License

DMapper is licensed under the MIT License.

Happy coding with DMapper!
