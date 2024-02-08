using System;
using System.Collections;
using System.Collections.Generic;

namespace Sigurd.Common.Collections.ObjectModel;

public static class CollectionExtensions
{
    public static ReadOnlyAssemblage<T> AsReadOnly<T>(this ICollection<T> collection) => new ReadOnlyAssemblage<T>(collection);
}

public class ReadOnlyAssemblage<T>(ICollection<T> assemblage) : IReadOnlyCollection<T>, ICollection<T>
{
    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => assemblage.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void Add(T item) => throw new NotSupportedException("Collection is read-only.");

    /// <inheritdoc />
    public void Clear() => throw new NotSupportedException("Collection is read-only.");

    /// <inheritdoc />
    public bool Contains(T item) => assemblage.Contains(item);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => assemblage.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public bool Remove(T item) => throw new NotSupportedException("Collection is read-only.");

    int ICollection<T>.Count => assemblage.Count;

    /// <inheritdoc />
    public bool IsReadOnly => true;

    int IReadOnlyCollection<T>.Count => assemblage.Count;
}
