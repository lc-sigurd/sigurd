using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SigurdLib.Util.Collections.Generic;

// https://stackoverflow.com/a/8946825
public sealed class IdentityEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public int GetHashCode(T value) => RuntimeHelpers.GetHashCode(value);

    public bool Equals(T left, T right) => ReferenceEquals(left, right);
}
