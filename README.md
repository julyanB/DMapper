# DMapper


DMapper is a lightweight .NET object mapping library designed to simplify property mapping, deep copying, and recursive property replacement.

## Features

- **Attribute-Based Mapping**: Use `[BindTo]` and `[ComplexBind]` to map properties between objects.
- **Deep Copying**: Clone objects, including nested properties and collections.
- **Recursive Property Replacement**: Copy matching properties from a source object to a destination object.
- **Property Ignoring**: Skip specific properties during mapping using `[CopyIgnore]`.
- **Efficient Performance**: Uses caching to speed up repeated operations.

---

## Usage

### 1. Attribute-Based Property Mapping

You can use attributes to specify how properties should be mapped.

#### **Basic Mapping**
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

### 2. Deep Copying

Clone an object, including all its nested properties.

```csharp
var original = new User { Name = "Alice", Age = 25 };
var copy = ReflectionHelper.DeepCopy(original);
```

Or copy from one type to another:

```csharp
var copiedObject = ReflectionHelper.DeepCopy<Source, Destination>(sourceObject);
```

### 3. Recursive Property Replacement

Automatically copy values from a source object to a destination object.

```csharp
var updatedDestination = ReflectionHelper.ReplacePropertiesRecursive(sourceObject, destinationObject);
```

### 4. Ignoring Properties

Use `[CopyIgnore]` to prevent certain properties from being copied.

```csharp
public class UserDto
{
    public string Username { get; set; }

    [CopyIgnore]
    public string InternalId { get; set; }
}
```

---

## License

DMapper is licensed under the MIT License.



