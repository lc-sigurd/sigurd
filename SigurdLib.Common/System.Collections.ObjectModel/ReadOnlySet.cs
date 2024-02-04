using System;
using System.Collections;
using System.Collections.Generic;

namespace Sigurd.Common.System.Collections.ObjectModel;

internal static class SetExtensionMethods
{
    public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set) => new ReadOnlySet<T>(set);
}

public class ReadOnlySet<T>(ISet<T> set) : IReadOnlySet<T>, ISet<T>
{
    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => set.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (set as IEnumerable).GetEnumerator();

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => set.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(T item) => throw new NotSupportedException("Set is read-only.");

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => set.Count;

    /// <inheritdoc />
    public bool IsReadOnly => true;

    void ICollection<T>.Add(T item) => throw new NotSupportedException("Collection is read-only.");

    bool ISet<T>.Add(T item) => throw new NotSupportedException("Set is read-only.");

    /// <inheritdoc cref="ISet{T}.IsProperSubsetOf" />
    public void ExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");

    /// <inheritdoc cref="ISet{T}.IntersectWith" />
    public void IntersectWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");

    /// <inheritdoc cref="ISet{T}.Clear" />
    public void Clear() => throw new NotSupportedException("Set is read-only.");

    /// <inheritdoc cref="ISet{T}.Contains" />
    public bool Contains(T item) => set.Contains(item);

    /// <inheritdoc cref="ISet{T}.IsProperSubsetOf" />
    public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);

    /// <inheritdoc cref="ISet{T}.IsProperSupersetOf" />
    public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);

    /// <inheritdoc cref="ISet{T}.IsSubsetOf" />
    public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);

    /// <inheritdoc cref="ISet{T}.IsSupersetOf" />
    public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);

    /// <inheritdoc cref="ISet{T}.Overlaps" />
    public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);

    /// <inheritdoc cref="ISet{T}.SetEquals" />
    public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);

    /// <inheritdoc />
    public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");

    /// <inheritdoc />
    public void UnionWith(IEnumerable<T> other) => throw new NotSupportedException("Set is read-only.");
}
