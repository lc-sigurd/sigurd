using System;
using System.Collections;
using System.Collections.Generic;
using Sigurd.Common.Core.Registries;
using Sigurd.Common.Core.Resources;
using SigurdLib.Util;
using SigurdLib.Util.Resources;

namespace Sigurd.Common.Core.Tags;

/// <summary>
/// A tag manager holds information about all tags currently bound to a registry.
/// This should be preferred to any <see cref="IHolder"/>-related methods.
/// </summary>
public interface ITagManager<TValue> : IEnumerable<ITag<TValue>> where TValue : class
{
    /// <summary>
    /// Queries this <see cref="ITagManager{TValue}"/> for an <see cref="ITag{TValue}"/>
    /// with the given <see cref="ITagKey{TValue}"/>.
    /// If no <see cref="ITag{TValue}"/> is found, an empty one will be created.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <seealso cref="IsKnownTagKey"/>
    ITag<TValue> GetTag(ITagKey<TValue> key);

    /// <summary>
    /// Queries this <see cref="ITagManager{TValue}"/> for an <see cref="IReverseTag{TValue}"/>
    /// for the given value.
    /// </summary>
    /// <param name="value">The value to query an <see cref="IReverseTag{TValue}"/> for.</param>
    /// <returns>
    /// An <see cref="Optional"/> containing a <see cref="IReverseTag{TValue}"/> for the given value,
    /// if found; otherwise, <see cref="Optional{A}.None"/>.
    /// </returns>
    Optional<IReverseTag<TValue>> GetReverseTag(TValue value);

    /// <summary>
    /// Queries this <see cref="ITagManager{TValue}"/> for an <see cref="IReverseTag{TValue}"/>
    /// for the given value.
    /// </summary>
    /// <param name="value">The value to query an <see cref="IReverseTag{TValue}"/> for.</param>
    /// <returns><see cref="IReverseTag{TValue}"/> for the given value.</returns>
    /// <exception cref="ArgumentException">The value could not be found.</exception>
    IReverseTag<TValue> GetReverseTagOrThrow(TValue value) => GetReverseTag(value)
        .IfNone(() => throw new ArgumentException($"The value {value} could not be found."));

    /// <summary>
    /// Checks whether the given <see cref="ITagKey{TValue}"/> exists in this <see cref="ITagManager{TValue}"/>
    /// and is bound.
    /// This will <b>not</b> create the tag if it does not exist.
    /// </summary>
    /// <param name="key">The <see cref="ITagKey{TValue}"/> to lookup.</param>
    /// <returns><see langword="true"/> if found; otherwise, <see langword="false"/></returns>
    /// <seealso cref="GetTag"/>
    bool IsKnownTagKey(ITagKey<TValue> key);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// An enumerable of known <see cref="ITagKey{TValue}"/> tag keys.
    /// Includes both bound & unbound keys.
    /// </summary>
    IEnumerable<ITagKey<TValue>> TagKeys { get; }

    /// <summary>
    /// Creates a new <see cref="ITagKey{TValue}"/> for the <see cref="ITagManager{TValue}"/>
    /// associated with this <see cref="name"/>.
    /// </summary>
    /// <param name="name"><see cref="ITagKey{TValue}"/> to use for the name of the new <see cref="IRegistry{TValue}"/>.</param>
    /// <returns></returns>
    ITagKey<TValue> CreateTagKey(ResourceName name);
}
