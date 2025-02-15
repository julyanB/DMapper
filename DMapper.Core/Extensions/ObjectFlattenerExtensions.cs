using DMapper.Constants;
using DMapper.Helpers;
using DMapper.Helpers.Models;

namespace DMapper.Extensions;

public static class ObjectFlattenerExtensions
{
    /// <summary>
    /// Flattens the object instance into a FlattenResult using the provided prefix and separator.
    /// </summary>
    public static FlattenResult Flatten(this object obj, string prefix = "", string separator = GlobalConstants.DefaultDotSeparator)
    {
        return ObjectFlattener.Flatten(obj, prefix, separator);
    }

    /// <summary>
    /// Flattens the structure of the given type into a FlattenResult using the provided prefix and separator.
    /// </summary>
    public static FlattenResult Flatten(this Type type, string prefix = "", string separator = GlobalConstants.DefaultDotSeparator)
    {
        return ObjectFlattener.Flatten(type, prefix, separator);
    }

    /// <summary>
    /// Flattens the structure of the generic type T into a FlattenResult using the provided prefix and separator.
    /// </summary>
    public static FlattenResult Flatten<T>(this T obj, string prefix = "", string separator = GlobalConstants.DefaultDotSeparator)
    {
        // Even if the instance is null, we use the type T for flattening its structure.
        return ObjectFlattener.Flatten(typeof(T), prefix, separator);
    }
}