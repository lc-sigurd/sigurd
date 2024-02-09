using System;
using System.Collections;
using System.Collections.Generic;
using Sigurd.Common.Util;

namespace Sigurd.Common.Core.Tags;


/// <summary>
/// A tag is a collection of elements with an identifying <see cref="ITagKey{TValue}"/>.
/// Tags will always be empty until they are bound.
/// <br/>
/// A tag instance provided for a given <see cref="ITagKey{TValue}"/> from a given
/// <see cref="ITagManager{TValue}"/> will always return the same instance on future invocations.
/// </summary>
/// <typeparam name="TValue">The type of value that is categorised</typeparam>
public interface ITag<TValue> : IReadOnlyCollection<TValue>
    where TValue : class
{
    /// <summary>
    /// The <see cref="ITagKey{TValue}"/> of this tag.
    /// </summary>
    ITagKey<TValue> Key { get; }

    /// <summary>
    /// Determine whether this tag was loaded with a value. If this is <see langword="false"/>, the tag is
    /// always empty.
    /// </summary>
    /// <returns><see langword="true"/> if this tag was loaded with a value (even if that value was empty)</returns>
    bool IsBound { get; }

    /// <summary>
    /// Determines whether the <see cref="ITag{TValue}" /> contains a specific value.</summary>
    /// <param name="value">The object to locate in the <see cref="ITag{TValue}" />.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="value" /> is found in the <see cref="ITag{TValue}" />; otherwise, <see langword="false" />.</returns>
    bool Contains(TValue value);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Get a random element from this <see cref="ITag{TValue}"/>'s members.
    /// </summary>
    /// <param name="randomSource"><see cref="Random"/> source</param>
    /// <returns>
    /// An <see cref="Optional{TValue}"/> containing the randomly selected value, if
    /// this tag has members; otherwise, <see cref="Optional{TValue}.None"/>.
    /// </returns>
    Optional<TValue> GetRandomElement(Random randomSource);
}
