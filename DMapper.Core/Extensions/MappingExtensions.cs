using DMapper.Enums;
using DMapper.Helpers;
using System.Collections;

namespace DMapper.Extensions
{
    public static class MappingExtensions
    {
        /// <summary>
        /// Maps the source object to a new instance of TDestination using the advanced recursive mapping.
        /// If TDestination is a simple type, it directly converts the source.
        /// </summary>
        public static TDestination MapTo<TDestination>(this object source, DMapperVersion version = DMapperVersion.Latest)
        {
            Guard.IsNotNull(source, nameof(source));

            // If TDestination is a simple type, perform a direct conversion.
            if (ReflectionHelper.IsSimpleType(typeof(TDestination)))
            {
                return (TDestination)Convert.ChangeType(source, typeof(TDestination));
            }

            // Create a new destination instance.
            TDestination destination = Activator.CreateInstance<TDestination>();

            var result = version switch
            {
                DMapperVersion.V2 => ReflectionHelper.ReplacePropertiesRecursive_V2(destination, source),
                DMapperVersion.V3 => ReflectionHelper.ReplacePropertiesRecursive_V3(destination, source),
                DMapperVersion.V4 => ReflectionHelper.ReplacePropertiesRecursive_V4(destination, source),
                DMapperVersion.V5 => ReflectionHelper.ReplacePropertiesRecursive_V5(destination, source),
                DMapperVersion.V6 => ReflectionHelper.ReplacePropertiesRecursive_V6(destination, source),
                DMapperVersion.V8 or _ => ReflectionHelper.ReplacePropertiesRecursive_V8(destination, source),

            };

            return result;
        }

        /// <summary>
        /// Maps the source object to a destination type provided at runtime.
        /// If the destination type is simple, it directly converts the source.
        /// </summary>
        public static object MapTo(this object source, Type destinationType, DMapperVersion version = DMapperVersion.Latest)
        {
            Guard.IsNotNull(source, nameof(source));
            Guard.IsNotNull(destinationType, nameof(destinationType));

            // If destinationType is simple, perform a direct conversion.
            if (ReflectionHelper.IsSimpleType(destinationType))
            {
                return Convert.ChangeType(source, destinationType);
            }

            // Create an instance of the destination type.
            object destination = Activator.CreateInstance(destinationType);

            return version switch
            {
                DMapperVersion.V2 => ReflectionHelper.ReplacePropertiesRecursive_V2(destination, source),
                DMapperVersion.V3 => ReflectionHelper.ReplacePropertiesRecursive_V3(destination, source),
                DMapperVersion.V4 => ReflectionHelper.ReplacePropertiesRecursive_V4(destination, source),
                DMapperVersion.V5 => ReflectionHelper.ReplacePropertiesRecursive_V5(destination, source),
                DMapperVersion.V6 => ReflectionHelper.ReplacePropertiesRecursive_V6(destination, source),
                DMapperVersion.V8 or _ => ReflectionHelper.ReplacePropertiesRecursive_V8(destination, source),
            };
        }

        /// <summary>
        /// Maps each element of the source collection to a destination collection.
        /// This method supports destination types such as List&lt;T&gt; or T[].
        /// Example usages:
        ///     result.MapTo&lt;List&lt;Pesho&gt;&gt;
        ///     result.MapTo&lt;Pesho[]&gt;
        /// where result is any IEnumerable.
        /// </summary>
        /// <typeparam name="TDestination">The destination collection type.</typeparam>
        /// <param name="source">The source collection (any IEnumerable).</param>
        /// <param name="version">The mapping version to use.</param>
        /// <returns>A mapped collection of type TDestination.</returns>
        /// <summary>
        /// Maps each element of the source collection to a destination collection.
        /// This method supports destination types such as List&lt;T&gt; or T[].
        /// Each element is mapped using the same logic as the single-object MapTo.
        /// </summary>
        /// <typeparam name="TDestination">The destination collection type.</typeparam>
        /// <param name="source">The source collection (any IEnumerable).</param>
        /// <param name="version">The mapping version to use.</param>
        /// <returns>A mapped collection of type TDestination.</returns>
        public static TDestination MapTo<TDestination>(this IEnumerable source, DMapperVersion version = DMapperVersion.Latest)
        {
            Guard.IsNotNull(source, nameof(source));

            Type destType = typeof(TDestination);
            // Ensure that TDestination is a collection type (but not string)
            if (!typeof(IEnumerable).IsAssignableFrom(destType) || destType == typeof(string))
                throw new InvalidOperationException("TDestination must be a collection type.");

            // Determine the destination element type.
            Type destElementType = null;
            if (destType.IsArray)
            {
                destElementType = destType.GetElementType();
            }
            else if (destType.IsGenericType)
            {
                // For example, List<T> or any other generic enumerable.
                destElementType = destType.GetGenericArguments().FirstOrDefault();
            }

            if (destElementType == null)
                throw new NotSupportedException("Could not determine the destination element type.");

            // Create a temporary List<destElementType> to hold mapped items.
            IList tempList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(destElementType));
            // Get the generic MapTo<T> method (which maps a single object) using reflection.
            var mapToMethod = typeof(MappingExtensions).GetMethod(nameof(MapTo), new Type[] { typeof(object), typeof(DMapperVersion) });
            var genericMapTo = mapToMethod.MakeGenericMethod(destElementType);

            foreach (var item in source)
            {
                // For each element, call the generic overload.
                object mappedItem = genericMapTo.Invoke(null, new object[] { item, version });
                tempList.Add(mappedItem);
            }

            // If the destination is an array, convert the list to an array.
            if (destType.IsArray)
            {
                Array array = Array.CreateInstance(destElementType, tempList.Count);
                tempList.CopyTo(array, 0);
                return (TDestination)(object)array;
            }
            else
            {
                // Otherwise, if it's e.g. List<T>, try to return the list directly.
                if (destType.IsAssignableFrom(tempList.GetType()))
                {
                    return (TDestination)tempList;
                }

                // Or try to instantiate TDestination with tempList (if it has such a constructor).
                return (TDestination)Activator.CreateInstance(destType, tempList);
            }
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
                DMapperVersion.V6 => ReflectionHelper.ReplacePropertiesRecursive_V6(destination, source),
                DMapperVersion.V8 or _ => ReflectionHelper.ReplacePropertiesRecursive_V8(destination, source),
            };

            return result;
        }
    }
}