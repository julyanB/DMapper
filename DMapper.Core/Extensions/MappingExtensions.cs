using CommunityToolkit.Diagnostics;
using DMapper.Helpers;

namespace DMapper.Extensions;

public static class MappingExtensions
{
    /// <summary>
    /// Maps the source object to a new instance of TDestination using the advanced recursive mapping.
    /// </summary>
    public static TDestination MapTo<TDestination>(this object source)
    {
        Guard.IsNotNull(source, nameof(source));

        // Create a new destination instance.
        TDestination destination = Activator.CreateInstance<TDestination>();

        // Call your advanced mapping method.
        return ReflectionHelper.ReplacePropertiesRecursive_V4(destination, source);
    }
    
    /// <summary>
    /// Copies properties from the source object into the specified destination object.
    /// </summary>
    public static T BindFrom<T>(this T destination, object source)
    {
        Guard.IsNotNull(source, nameof(source));
        Guard.IsNotNull(destination, nameof(destination));

        // Call your advanced mapping method.
        return ReflectionHelper.ReplacePropertiesRecursive_V4(destination, source);
    }
    
}