using System.Collections;
using DMapper.Extensions;

namespace DMapper.Tests;

public class CollectionsMappingTests
{
    [Fact]
    public void MapTo_ListOfInt_ShouldMapCorrectly()
    {
        // Arrange: A simple list of integers.
        List<int> source = new List<int> { 1, 2, 3, 4, 5 };

        // Act: Map the source to a List<int> using the collection extension.
        var dest = source.MapTo<List<int>>();

        // Assert: The mapped list should have the same elements.
        Assert.Equal(source, dest);
    }

    [Fact]
    public void MapTo_ArrayOfInt_ShouldMapCorrectly()
    {
        // Arrange: A list of integers.
        List<int> source = new List<int> { 10, 20, 30 };

        // Act: Map the source to an int array.
        var dest = source.MapTo<int[]>();

        // Assert: The array should have the same elements.
        Assert.Equal(source, dest);
    }

    [Fact]
    public void MapTo_ListOfComplexObjects_ShouldMapCorrectly()
    {
        // Arrange: A list of source objects.
        var source = new List<SimpleSource>
        {
            new SimpleSource { Value = "One" },
            new SimpleSource { Value = "Two" }
        };

        // Act: Map to a list of destination objects.
        var dest = source.MapTo<List<SimpleDestination>>();

        // Assert: Each source item is mapped correctly.
        Assert.Equal(source.Count, dest.Count);
        for (int i = 0; i < source.Count; i++)
        {
            Assert.Equal(source[i].Value, dest[i].Value);
        }
    }

    [Fact]
    public void MapTo_ArrayOfComplexObjects_ShouldMapCorrectly()
    {
        // Arrange: An array of source objects.
        SimpleSource[] source = new SimpleSource[]
        {
            new SimpleSource { Value = "Alpha" },
            new SimpleSource { Value = "Beta" }
        };

        // Act: Map to an array of destination objects using the non‑generic extension.
        var dest = ((IEnumerable)source).MapTo<SimpleDestination[]>();

        // Assert: The array is mapped correctly.
        Assert.Equal(source.Length, dest.Length);
        for (int i = 0; i < source.Length; i++)
        {
            Assert.Equal(source[i].Value, dest[i].Value);
        }
    }

    [Fact]
    public void MapTo_EmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange: An empty list of strings.
        List<string> source = new List<string>();

        // Act: Map to an array of strings.
        var dest = source.MapTo<string[]>();

        // Assert: The resulting array is empty.
        Assert.Empty(dest);
    }
}