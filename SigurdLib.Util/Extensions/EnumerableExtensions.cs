using System.Collections.Generic;
using System.Linq;

namespace SigurdLib.Util.Extensions;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/> instances.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Unpack an <see cref="IEnumerable{T}"/> of <see cref="Optional{T}"/> into an <see cref="IEnumerable{T}"/> of
    /// the contained value by filtering on <see cref="Optional{T}.IsSome"/> and projecting to
    /// <see cref="Optional{T}.ValueUnsafe"/>.
    /// </summary>
    /// <param name="enumerable">A sequence of <see cref="Optional{T}"/> values to unpack.</param>
    /// <typeparam name="T">The type of elements contained by the <see cref="Optional{T}"/> elements.</typeparam>
    /// <returns>An unpacked <see cref="IEnumerable{T}"/> of definite values whose elements are the result
    /// of unpacking the values contained by the elements of the input sequence that were in a
    /// <see cref="Optional{T}.Some"/> state.</returns>
    public static IEnumerable<T> SelectValueWhereSome<T>(this IEnumerable<Optional<T>> enumerable)
        => enumerable
            .Where(maybe => maybe.IsSome)
            .Select(definite => definite.ValueUnsafe!);

    /// <summary>
    /// Unpack an <see cref="IEnumerable{T}"/> of <see cref="Optional{T}"/> <see cref="IEnumerable{T}"/> into an
    /// <see cref="IEnumerable{T}"/> of the contained value by filtering on <see cref="Optional{T}.IsSome"/>,
    /// projecting to <see cref="Optional{T}.ValueUnsafe"/>, and flattening the resulting sequences into one
    /// sequence.
    /// </summary>
    /// <param name="enumerable">A sequence of <see cref="Optional{T}"/> sequences to unpack.</param>
    /// <typeparam name="T">The type of elements within the <see cref="Optional{T}"/> sequences.</typeparam>
    /// <returns>An unpacked <see cref="IEnumerable{T}"/> of definite values whose elements are the result of
    /// flattening the sequences contained by the elements of the input sequence that were in a
    /// <see cref="Optional{T}.Some"/> state.</returns>
    public static IEnumerable<T> SelectManyValueWhereSome<T>(this IEnumerable<Optional<IEnumerable<T>>> enumerable)
        => enumerable
            .Where(maybe => maybe.IsSome)
            .SelectMany(definite => definite.ValueUnsafe!);
}
