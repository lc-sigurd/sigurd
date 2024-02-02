using System.Collections.Generic;
using Sigurd.Common.Core;

namespace Sigurd.Common.Tags;


/// <summary>
/// A tag is a collection of elements with an identifying <see cref="ITagKey{TValue}"/>.
/// Tags will always be empty until they are bound.
/// A tag instance provided for a given <see cref="ITagKey{TValue}"/> from a given
/// <see cref="ITagManager"/>
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface ITag<TValue> : IReadOnlyCollection<TValue>
    where TValue : class
{
    /// <summary>
    /// The <see cref="TagKey{TValue,TRegistry}"/> of this tag.
    /// </summary>
    TagKey<TValue> Key { get; }

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
}
