# DMapper

DMapper is a lightweight and efficient .NET object mapping library designed to facilitate object transformation, deep copying, and recursive property binding using advanced reflection techniques.

---

## Features

- **Advanced Recursive Property Mapping (`V4`)**: Copies properties from a source object to a destination object, including nested properties.
- **Extension Methods for Easy Mapping**: Utilize `.MapTo<TDestination>()` and `.BindFrom<T>()` for seamless object transformations.
- **Attribute-Based Mapping**: Use `[BindTo]` and `[ComplexBind]` for precise control over property binding.
- **Deep Copying**: Clone objects with all their properties, including nested and complex types.
- **Property Ignoring**: Skip unwanted properties using `[CopyIgnore]`.
- **Caching for Performance**: Utilizes `ConcurrentDictionary` to cache mappings and constructors for optimized execution.

---

## Installation

To use **DMapper**, simply add the source files to your .NET project. A NuGet package may be available in the future.

---

## Usage

### 1. **Object Mapping with Extension Methods**

The `MappingExtensions` class provides convenient methods to map objects.

#### **Mapping a Source Object to a New Destination Instance**
```csharp
using DMapper.Extensions;

var destinationObject = sourceObject.MapTo<DestinationType>();
```
This creates a new instance of `DestinationType` and maps all matching properties.

#### **Binding Properties from Source to Existing Destination**
```csharp
using DMapper.Extensions;

destinationObject.BindFrom(sourceObject);
```
This copies properties from `sourceObject` into `destinationObject`.

---

### 2. **Deep Copying Objects**

DMapper provides methods to deep copy objects, including nested properties.

#### **Deep Copying the Same Type**
```csharp
var copy = ReflectionHelper.DeepCopy(originalObject);
```

#### **Deep Copying Between Different Types**
```csharp
var destinationCopy = ReflectionHelper.DeepCopy<SourceType, DestinationType>(sourceObject);
```

---

### 3. **Recursive Property Replacement (V4)**

The `V4` version of `ReplacePropertiesRecursive` supports:
- **Nested property mapping**
- **Collections and complex types**
- **Attribute-based custom mappings**
- **Handling cyclical references**

```csharp
var updatedDestination = ReflectionHelper.ReplacePropertiesRecursive_V4(destinationObject, sourceObject);
```

This method efficiently maps properties and handles nested structures recursively.

---

### 4. **Attribute-Based Property Mapping**

You can control how properties are mapped using attributes.

#### **Basic Mapping with `[BindTo]`**
```csharp
public class Source
{
    public string Name { get; set; }
}

public class Destination
{
    [BindTo("Name")]
    public string FullName { get; set; }
}
```
This maps `Source.Name` to `Destination.FullName`.

#### **Complex Mapping with `[ComplexBind]`**
```csharp
public class Source
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Destination
{
    [ComplexBind("FullName", "FirstName, LastName")]
    public string FullName { get; set; }
}
```
This combines `FirstName` and `LastName` into `FullName`.

#### **Ignoring Properties with `[CopyIgnore]`**
```csharp
public class UserDto
{
    public string Username { get; set; }

    [CopyIgnore]
    public string InternalId { get; set; }
}
```
Properties marked with `[CopyIgnore]` will not be copied during mapping.

---

## Performance Optimizations

- **Efficient Reflection with Caching**: The library caches mappings and constructors to avoid redundant processing.
- **Cyclical Reference Handling**: The `ReferenceComparer` ensures objects are not processed multiple times in recursive mappings.
- **Safe Property Assignments**: Uses `Guard.IsNotNull` to prevent null reference errors.

---

## Contributions

We welcome contributions! Feel free to fork this repository and submit pull requests.

---

## License

DMapper is licensed under the **MIT License**.

