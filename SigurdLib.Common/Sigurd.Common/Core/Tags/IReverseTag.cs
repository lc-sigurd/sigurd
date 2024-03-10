using System.Collections.Generic;

namespace Sigurd.Common.Core.Tags;

/// <summary>
/// A reverse tag is an object aware of what tags it is contained by.
/// <see cref="IHolder"/>s implement this interface.
/// A reverse tag makes no guarantees about its persistence relative to a registry value.
/// Reverse tags should be looked-up on-demand from a <see cref="ITagManager"/> rather than caching
/// the reverse tag somewhere.
/// </summary>
public interface IReverseTag<TValue>
    where TValue : class
{
    /// <summary>
    /// The tags the <see cref="IReverseTag{TValue}"/> is contained by.
    /// </summary>
    IEnumerable<ITagKey<TValue>> Tags { get; }

    /// <summary>
    /// Determines whether the <see cref="IReverseTag{TValue}"/> belongs to a
    /// particular <see cref="ITagKey{TValue}"/>.
    /// </summary>
    /// <param name="tagKey">The <see cref="ITagKey{TValue}"/> to locate in the <see cref="IReverseTag{TValue}"/>'s tags.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="tagKey" /> is found in the <see cref="IReverseTag{TValue}"/>'s tags; otherwise, <see langword="false" />.</returns>
    bool Contains(ITagKey<TValue> tagKey);

    /// <summary>
    /// Determines whether the <see cref="IReverseTag{TValue}"/> belongs to a
    /// particular <see cref="ITag{TValue}"/>.
    /// </summary>
    /// <param name="tag">The <see cref="ITag{TValue}"/> to locate in the <see cref="IReverseTag{TValue}"/>'s tags.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="tag" /> is found in the <see cref="IReverseTag{TValue}"/>'s tags; otherwise, <see langword="false" />.</returns>
    bool Contains(ITag<TValue> tag) => Contains(tag.Key);
}
