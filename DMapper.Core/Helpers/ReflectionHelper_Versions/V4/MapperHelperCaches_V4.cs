using System.Collections.Concurrent;
using System.Reflection;
using DMapper.Helpers.Models;

namespace DMapper.Helpers;

public class MapperHelperCaches_V4
{
    #region Caching Fields

    // Cache mapping information between source and destination types.
    public static readonly ConcurrentDictionary<(Type currentSourceType, Type rootSourceType, Type destinationType), List<PropertyMapping>> MappingCache =
        new ConcurrentDictionary<(Type, Type, Type), List<PropertyMapping>>();

    // Cache for parameterless constructors to avoid repeated reflection.
    public static readonly ConcurrentDictionary<Type, ConstructorInfo> CtorCache =
        new ConcurrentDictionary<Type, ConstructorInfo>();

    /// <summary>
    /// Returns the parameterless constructor for the given type, or null if none exists.
    /// </summary>
    public static ConstructorInfo GetParameterlessConstructor(Type type)
    {
        return CtorCache.GetOrAdd(type, t => t.GetConstructor(Type.EmptyTypes));
    }

    #endregion
}