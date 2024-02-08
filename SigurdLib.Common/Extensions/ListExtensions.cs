using System;
using System.Collections;
using System.Collections.Generic;
using Sigurd.Common.Util;

namespace Sigurd.Common.Extensions;

/// <summary>
/// Extension methods for <see cref="IList{T}"/> instances.
/// </summary>
internal static class ListExtensions
{
    /// <summary>
    /// Retrieve a random element from this <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="IList{T}"/> to choose from.</param>
    /// <param name="randomSource"><see cref="Random"/> source</param>
    /// <typeparam name="T">The element type of the <see cref="IList{T}"/>.</typeparam>
    /// <returns>
    /// A random element from <paramref name="list"/>, if it has members; otherwise,
    /// <see cref="Optional{T}.None"/>.
    /// </returns>
    public static Optional<T> GetRandomElement<T>(this IList<T> list, Random randomSource)
        => list.Count > 0 ? Optional.Some(list[randomSource.Next(list.Count)]) : Optional<T>.None;

    /// <summary>
    /// Retrieve a random element from this <see cref="IList"/>.
    /// </summary>
    /// <param name="list">The <see cref="IList"/> to choose from.</param>
    /// <param name="randomSource"><see cref="Random"/> source</param>
    /// <returns>
    /// A random element from <paramref name="list"/>, if it has members; otherwise,
    /// <see cref="Optional{T}.None"/>.
    /// </returns>
    public static Optional<object> GetRandomElement(this IList list, Random randomSource)
        => list.Count > 0 ? Optional.Some(list[randomSource.Next(list.Count)]) : Optional<object>.None;
}
