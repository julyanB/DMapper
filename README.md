# DMapper

DMapper is a lightweight and efficient .NET object mapping library designed to simplify object transformation, deep copying, and recursive property binding using advanced reflection techniques.

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

To use **DMapper**, simply add the source files to your .NET project.

---

## Usage

### 1. **Mapping Objects**

DMapper provides extension methods for mapping objects fluently. Given a source object, you can map it to a new destination instance as follows:

```csharp
using DMapper.Extensions;

// Example source object
var source = new Src
{
    Name = "Alice",
    Age = 30,
    Test = new Test
    {
        Name = "Nested Test",
        Age = 20,
        Test33 = "Some Value"
    }
};

// Map to a new destination instance.
Dest destination = source.MapTo<Dest>();

// Or copy properties into an existing object.
var anotherDest = new Dest();
source.BindFrom(anotherDest);
```

---

### 2. **Deep Copying**

DMapper provides methods to deep copy objects, including nested properties.

#### **Deep Copying the Same Type**
```csharp
var clone = source.DeepCopy<Src>();
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

### 4. **Custom Attributes**

DMapper provides attributes to control how properties are mapped.

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

#### **Binding Properties with `[BindTo]`**
```csharp
using DMapper.Attributes;

public class Dest
{
    public string Name { get; set; }
    public int Age { get; set; }
    
    // Maps from one of "Test2", "Test3", or "Test4". If none exist, it falls back to "Test".
    [BindTo("Test2, Test3, Test4")]
    public string Test { get; set; }
    
    // Maps src.Name into dest.Dest2.Name.
    [ComplexBind("Dest2.Name", "Name")]
    public Dest2 Dest2 { get; set; }
}

public class Dest2
{
    public string Name { get; set; }
}

public class Src
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Test Test { get; set; }
}

public class Test
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Test33 { get; set; }
}
```

---

### 5. **Fluent API with Extension Methods**

DMapper exposes extension methods for a fluent API, making the code easy to read and maintain:

```csharp
using DMapper.Extensions;

Dest destination = source.MapTo<Dest>();

// Get or set a property value using extension methods:
string name = source.GetPropertyValue<string>("Name");
source.SetPropertyValue("Name", "Bob");
```

---

### 6. **Complete Example**

Below is a complete example that demonstrates how to use DMapper in a console application. All the example classes and usage are contained in one file for clarity.

```csharp
using System;
using DMapper.Extensions;
using DMapper.Attributes;

namespace DMapper.Example
{
    // Define mapping classes.
    public class Dest
    {
        public string Name { get; set; }
        public int Age { get; set; }
        
        // Maps from one of "Test2", "Test3", or "Test4". If none exist, falls back to "Test".
        [BindTo("Test2, Test3, Test4")]
        public string Test { get; set; }
        
        // Maps src.Name into dest.Dest2.Name.
        [ComplexBind("Dest2.Name", "Name")]
        public Dest2 Dest2 { get; set; }
    }

    public class Dest2
    {
        public string Name { get; set; }
    }

    public class Src
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Test Test { get; set; }
    }

    public class Test
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Test33 { get; set; }
    }

    class Program
    {
        static void Main()
        {
            // Create a source object.
            var src = new Src
            {
                Name = "Alice",
                Age = 30,
                Test = new Test
                {
                    Name = "Nested Test",
                    Age = 20,
                    Test33 = "Value from Test.Test33"
                }
            };

            // Map to a new destination object using the fluent extension method.
            Dest dest = src.MapTo<Dest>();

            Console.WriteLine($"Name: {dest.Name}");             // Expected Output: Alice
            Console.WriteLine($"Age: {dest.Age}");               // Expected Output: 30
            Console.WriteLine($"Test: {dest.Test}");             // Output depends on available source properties
            Console.WriteLine($"Dest2.Name: {dest.Dest2?.Name}");  // Expected Output: Alice (via ComplexBind)

            // Deep copy example:
            var srcClone = src.DeepCopy<Src>();
            Console.WriteLine($"Clone Name: {srcClone.Name}");   // Expected Output: Alice
        }
    }
}
```

---

## 🔥 Design & Structure

DMapper is structured to promote **separation of concerns** and **modularity**:

- **Core Reflection Helpers**:
  - General reflection utilities (e.g., deep copy methods, property setters, caching of constructors).

- **Mapping Implementations**:
  - **Basic Reflection Mapping**: For simple, top‑level mapping.
  - **Recursive Reflection Mapping**: For deep, recursive mapping.
  - **Advanced Reflection Mapping**: For scenarios using custom attributes such as `[BindTo]` and `[ComplexBind]`.

- **Mapping Attributes**:
  - Custom attributes (`[CopyIgnore]`, `[BindTo]`, `[ComplexBind]`) control mapping behavior.

- **Extension Methods**:
  - A fluent API that wraps the core functionality, making the mapper easy to use.

---

🚀 **Happy Coding with DMapper!**
