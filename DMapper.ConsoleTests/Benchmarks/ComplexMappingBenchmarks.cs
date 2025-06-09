using BenchmarkDotNet.Attributes;
using DMapper.Helpers;
using TestCases.Benchmarks.Models;
using NestedSource = DMapper.Tests.Models.MappingTests.MapTo_ComplexBindMapping_ShouldMapAbsoluteKeyCorrectly.NestedSource;

namespace TestCases.Benchmarks;

public class ComplexMappingBenchmarks
{
    private ComplexSource _source;

    [GlobalSetup]
    public void Setup()
    {
        // Create a deeply nested source model.
        _source = new ComplexSource
        {
            Id = 1,
            Name = "Complex Model",
            Nested = new TestCases.Benchmarks.Models.NestedSource
            {
                Description = "Nested description",
                Date = DateTime.Now,
                DeepNested = new List<DeepNestedSource>
                {
                    new DeepNestedSource { Info = "Deep Info 1", Number = 100 },
                    new DeepNestedSource { Info = "Deep Info 2", Number = 200 }
                }
            },
            NestedList = new List<TestCases.Benchmarks.Models.NestedSource>
            {
                new()
                {
                    Description = "List Item 1",
                    Date = DateTime.Now.AddDays(-1),
                    DeepNested = new List<DeepNestedSource>
                    {
                        new DeepNestedSource { Info = "List Deep 1", Number = 300 }
                    }
                },
                new()
                {
                    Description = "List Item 2",
                    Date = DateTime.Now.AddDays(-2),
                    DeepNested = new List<DeepNestedSource>
                    {
                        new DeepNestedSource { Info = "List Deep 2", Number = 400 },
                        new DeepNestedSource { Info = "List Deep 3", Number = 500 }
                    }
                }
            }
        };

        // Create a circular reference:
        _source.SelfReference = _source;
    }
    
    [Benchmark]
    public ComplexDestination MapUsingV4()
    {
        return ReflectionHelper.ReplacePropertiesRecursive_V4(new ComplexDestination(), _source);
    }

    [Benchmark]
    public ComplexDestination MapUsingV5()
    {
        return ReflectionHelper.ReplacePropertiesRecursive_V5(new ComplexDestination(), _source);
    }

    [Benchmark]
    public ComplexDestination MapUsingV6()
    {
        return ReflectionHelper.ReplacePropertiesRecursive_V6(new ComplexDestination(), _source);
    }
}
