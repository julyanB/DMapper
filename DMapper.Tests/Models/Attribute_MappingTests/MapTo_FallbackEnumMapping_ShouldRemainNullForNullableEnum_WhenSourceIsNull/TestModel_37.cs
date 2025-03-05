using DMapper.Attributes;

namespace DMapper.Tests.Models.Attribute_MappingTests.MapTo_FallbackEnumMapping_ShouldRemainNullForNullableEnum_WhenSourceIsNull;


// Sample enums for testing.
public enum TestEnumA { Value1, Value2 }
public enum TestEnumB { Value1, Value2 }

// Models for non‑nullable enum fallback test.
public class SourceEnumFallback
{
    public TestEnumA Enum { get; set; }
}

public class DestinationEnumFallback
{
    // BindTo attribute uses an invalid candidate ("InvalidCandidate").
    // The mapping logic should then fall back to the property name "Enum".
    [BindTo("InvalidCandidate")]
    public TestEnumB Enum { get; set; }
}

// Models for nullable enum fallback tests.
public class SourceEnumNullableFallback
{
    public TestEnumA? Enum { get; set; }
}

public class DestinationEnumNullableFallback
{
    [BindTo("InvalidCandidate")]
    public TestEnumB? Enum { get; set; }
}