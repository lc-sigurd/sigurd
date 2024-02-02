using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sigurd.Common;

// https://stackoverflow.com/a/8946825
// Copyright (c) 2012 Jon Skeet
internal sealed class IdentityEqualityComparer<T> : IEqualityComparer<T>
    where T : class
{
    public int GetHashCode(T value) => RuntimeHelpers.GetHashCode(value);

    public bool Equals(T left, T right) => ReferenceEquals(left, right);
}
