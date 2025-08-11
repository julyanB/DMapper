using DMapper.Extensions;
using DMapper.Helpers;
using Xunit;

namespace DMapper.Tests;

public class CachingTests
{
    private class SimpleSource { public int Id { get; set; } }
    private class SimpleDestination { public int Id { get; set; } }

    [Fact]
    public void MapTo_ReusesMappingCaches()
    {
        ReflectionHelper.ClearV6Caches();
        var src = new SimpleSource { Id = 1 };
        src.MapTo<SimpleDestination>(DMapper.Enums.DMapperVersion.V6);
        int afterFirst = GetMappingCacheCount();
        src.MapTo<SimpleDestination>(DMapper.Enums.DMapperVersion.V6);
        int afterSecond = GetMappingCacheCount();
        Assert.Equal(afterFirst, afterSecond);
    }

    private static int GetMappingCacheCount()
    {
        var field = typeof(ReflectionHelper).GetField("_mappingCache_V6", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var dict = (System.Collections.IDictionary)field.GetValue(null);
        return dict.Count;
    }

    [Fact]
    public void MapTo_V7_ShouldProduceSameResult()
    {
        var src = new SimpleSource { Id = 42 };
        var dest = src.MapTo<SimpleDestination>(DMapper.Enums.DMapperVersion.V7);
        Assert.Equal(42, dest.Id);
    }
}
