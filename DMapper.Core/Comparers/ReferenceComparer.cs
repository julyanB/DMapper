using System.Runtime.CompilerServices;

namespace DMapper.Comparers;

internal class ReferenceComparer : IEqualityComparer<object>
{
    bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);
    int IEqualityComparer<object>.GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
}