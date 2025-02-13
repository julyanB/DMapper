using CommunityToolkit.Diagnostics;
using DMapper.Enums;
using DMapper.Helpers;

namespace DMapper.Extensions;

public static class MappingExtensions
{
    /// <summary>
    /// Maps the source object to a new instance of TDestination using the advanced recursive mapping.
    /// </summary>
    public static TDestination MapTo<TDestination>(this object source, DMapperVersion version = DMapperVersion.Latest)
    {
        Guard.IsNotNull(source, nameof(source));

        // Create a new destination instance.
        TDestination destination = Activator.CreateInstance<TDestination>();

        var result = version switch
        {
            DMapperVersion.V2 => ReflectionHelper.ReplacePropertiesRecursive_V2(destination, source),
            DMapperVersion.V3 => ReflectionHelper.ReplacePropertiesRecursive_V3(destination, source),
            DMapperVersion.V4 => ReflectionHelper.ReplacePropertiesRecursive_V4(destination, source),
            DMapperVersion.V5 => ReflectionHelper.ReplacePropertiesRecursive_V5(destination, source),
            _ or DMapperVersion.Latest => ReflectionHelper.ReplacePropertiesRecursive_V5(destination, source)
        };
        
        // Call your advanced mapping method.
        return result;
    }
    
    /// <summary>
    /// Copies properties from the source object into the specified destination object.
    /// </summary>
    public static T BindFrom<T>(this T destination, object source, DMapperVersion version = DMapperVersion.Latest)
    {
        Guard.IsNotNull(source, nameof(source));
        Guard.IsNotNull(destination, nameof(destination));

        var result = version switch
        {
            DMapperVersion.V2 => ReflectionHelper.ReplacePropertiesRecursive_V2(destination, source),
            DMapperVersion.V3 => ReflectionHelper.ReplacePropertiesRecursive_V3(destination, source),
            DMapperVersion.V4 => ReflectionHelper.ReplacePropertiesRecursive_V4(destination, source),
            DMapperVersion.V5 => ReflectionHelper.ReplacePropertiesRecursive_V5(destination, source),
            _ or DMapperVersion.Latest => ReflectionHelper.ReplacePropertiesRecursive_V5(destination, source)
        };
        
        // Call your advanced mapping method.
        return result;
    }
    
}