namespace DMapper.Tests.Models.Attribute_MappingTests.Map_NonNullable_To_NonNullable_ShouldMapCorrectly;

// Define two sample enums with matching values.
public enum TestEnum21_1
{
    Test = 0,
    Test2 = 1
}

public enum TestEnum21_2
{
    Test = 0,
    Test2 = 1
}

// Test case 1: Non-nullable source to non-nullable destination.
public class SourceEnumNonNullable
{
    public TestEnum21_1 Enum { get; set; }
}

public class DestinationEnumNonNullable
{
    public TestEnum21_2 Enum { get; set; }
}

// Test case 2: Non-nullable source to nullable destination.
public class DestinationEnumNullable
{
    public TestEnum21_2? Enum { get; set; }
}

// Test case 3 and 4: Nullable source.
public class SourceEnumNullable
{
    public TestEnum21_1? Enum { get; set; }
}

// Mapping from nullable source to non-nullable destination.
public class DestinationEnumNonNullable2
{
    public TestEnum21_2 Enum { get; set; }
}

// Mapping from nullable source to nullable destination.
public class DestinationEnumNullable2
{
    public TestEnum21_2? Enum { get; set; }
}