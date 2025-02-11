# DMapper

**DMapper** is a reflection-based object mapping library for .NET that simplifies the process of copying and transforming data between objects. It supports deep copying, recursive property mapping, and advanced mapping scenarios with custom attributes. The library is designed for high performance by leveraging caching and extension methods for a fluent API.

## Features

- **Deep Copy:**  
  Create deep clones of objects using reflection or JSON serialization.

- **Basic & Recursive Mapping:**  
  Map properties from a source to a destination object either at a single level or recursively.

- **Advanced Mapping with Custom Attributes:**  
  Control the mapping process using attributes:
  - `[CopyIgnore]`: Exclude properties from mapping.
  - `[BindTo]`: Specify alternative source property names (supports dot‑notation).
  - `[ComplexBind]`: Map nested destination properties from a specific source property.

- **Fluent Extension Methods:**  
  Easily invoke mapping functionality using extension methods for clean, readable code.

- **Performance Optimizations:**  
  Caches parameterless constructors and property mappings to minimize reflection overhead.

## Installation

You can include DMapper in your project by either cloning the repository or using a NuGet package. If you are using NuGet, run the following command in the Package Manager Console:

```bash
Install-Package DMapper

