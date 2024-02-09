using System.Collections.Generic;
using Sigurd.Common.Core.Resources;
using Sigurd.Common.Core.Tags;
using Sigurd.Common.Util;

namespace Sigurd.Common.Core.Registries;

/// <summary>
/// Interface for internal <see cref="SigurdRegistry{TValue}"/> members.
/// </summary>
/// <typeparam name="TValue">The type registered by the <see cref="ISigurdRegistry{TValue}"/>.</typeparam>
internal interface ISigurdRegistryInternal<TValue> : ISigurdRegistry<TValue> where TValue : class
{
    /// <summary>
    /// Add a new entry to the <see cref="ISigurdRegistry{TValue}"/>.
    /// </summary>
    /// <param name="id"><see cref="int"/> ID for the new entry.</param>
    /// <param name="key"><see cref="ResourceName"/> key for the new entry.</param>
    /// <param name="value">Value for the new entry.</param>
    void Register(int id, ResourceName key, TValue value);

    /// <summary>
    /// Retrieve the value associated with an <see cref="int"/> ID.
    /// </summary>
    /// <param name="id"><see cref="int"/> to retrieve a value for.</param>
    /// <returns>
    /// The value associated with the provided <paramref name="id"/>, if found; otherwise, <see langword="null"/>
    /// </returns>
    TValue? GetValue(int id);

    /// <summary>
    /// Retrieve the <see cref="int"/> ID associated with a value.
    /// </summary>
    /// <param name="value">value to retrieve an <see cref="int"/> ID for.</param>
    /// <returns>
    /// The ID associated with the provided value, if found; otherwise, -1.
    /// </returns>
    int GetId(TValue value);

    /// <summary>
    /// Retrieve the <see cref="int"/> ID associated with a <see cref="ResourceName"/>.
    /// </summary>
    /// <param name="key"><see cref="ResourceName"/> to retrieve an <see cref="int"/> ID for.</param>
    /// <returns>
    /// The ID associated with the provided <see cref="ResourceName"/>, if found; otherwise, -1.
    /// </returns>
    int GetId(ResourceName key);

    Optional<IHolder.Reference<TValue>>  GetDelegate(int id);

    void BindTags(IDictionary<ITagKey<TValue>, IHolderSet.Named<TValue>> tags);
}
