using System;
using System.Collections;
using System.Collections.Generic;
using Sigurd.Common.Core.Resources;
using Sigurd.Common.Core.Tags;
using Sigurd.Common.Util;

namespace Sigurd.Common.Core.Registries;

/// <summary>
/// Covariant interface for <see cref="IRegistry{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The registered value type.</typeparam>
public interface ISigurdRegistrar<out TValue> : IEnumerable<TValue>
{
    /// <summary>
    /// The registry's uniquely identifying key, which wraps <see cref="RegistryName"/>.
    /// </summary>
    IResourceKey<ISigurdRegistrar<TValue>> RegistryKey { get; }

    /// <summary>
    /// The registry's uniquely identifying name.
    /// </summary>
    ResourceName RegistryName { get; }
}

/// <summary>
/// Defines <see langword="delegate"/> types and other <see langword="static"/> members for <see cref="IRegistry{TValue}"/>.
/// </summary>
public interface IRegistry
{
    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="IRegistry{TValue}.OnAdd"/>.
    /// </summary>
    class AddEventArgs<TValue> : EventArgs where TValue : class
    {
        /// <summary>
        /// The <see cref="int"/> ID associated with the new entry.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// The <see cref="IResourceKey{TValue}"/> associated with the new entry.
        /// </summary>
        public required IResourceKey<TValue> Key { get; init; }

        /// <summary>
        /// The value associated with the new entry.
        /// </summary>
        public required TValue Value { get; init; }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="IRegistry{TValue}.OnCreate"/>.
    /// </summary>
    class CreateEventArgs<TValue> : EventArgs where TValue : class
    {

    }

    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="IRegistry{TValue}.OnValidate"/>.
    /// </summary>
    class ValidateEventArgs<TValue> : EventArgs where TValue : class
    {
        /// <summary>
        /// The <see cref="int"/> ID associated with the entry being validated.
        /// </summary>
        public required int Id { get; init; }

        /// <summary>
        /// The <see cref="ResourceName"/> associated with the entry being validated.
        /// </summary>
        public required ResourceName Name { get; init; }

        /// <summary>
        /// The value associated with the entry being validated.
        /// </summary>
        public required TValue Value { get; init; }
    }

    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="IRegistry{TValue}.OnBake"/>.
    /// </summary>
    class BakeEventArgs<TValue> : EventArgs where TValue : class
    {

    }
}

/// <summary>
/// Main interface for the registry system. Use this to query the registry system.
/// </summary>
/// <typeparam name="TValue">The type registered by the <see cref="IRegistry{TValue}"/>.</typeparam>
public interface IRegistry<TValue> : ISigurdRegistrar<TValue>, IReadOnlyCollection<TValue>, IRegistry
    where TValue : class
{
    #region Registration

    /// <summary>
    /// Add a new entry to the <see cref="IRegistry{TValue}"/>.
    /// </summary>
    /// <param name="key"><see cref="string"/> path used in <see cref="ResourceName"/> key for the new entry.</param>
    /// <param name="value">Value for the new entry.</param>
    void Register(string key, TValue value);

    /// <summary>
    /// Add a new entry to the <see cref="IRegistry{TValue}"/>.
    /// </summary>
    /// <param name="key"><see cref="ResourceName"/> key for the new entry.</param>
    /// <param name="value">Value for the new entry.</param>
    void Register(ResourceName key, TValue value);

    #endregion

    #region Content Queries

    /// <summary>
    /// Determines whether the <see cref="IRegistry{TValue}"/> contains a specific <see cref="ResourceName"/> key.
    /// </summary>
    /// <param name="key">The <see cref="ResourceName"/> to locate in the <see cref="IRegistry{TValue}" />.</param>
    /// <returns><see langword="true"/> if the <paramref name="key"/> is found in the <see cref="IRegistry{TValue}"/>; Otherwise, <see langword="false"/>.</returns>
    bool ContainsKey(ResourceName key);

    /// <summary>
    /// Determines whether the <see cref="IRegistry{TValue}"/> contains a specific value.
    /// </summary>
    /// <param name="value">The value to locate in the <see cref="IRegistry{TValue}" />.</param>
    /// <returns><see langword="true"/> if the <paramref name="value"/> is found in the <see cref="IRegistry{TValue}"/>; Otherwise, <see langword="false"/>.</returns>
    bool ContainsValue(TValue value);

    /// <summary>
    /// Determines whether the <see cref="IRegistry{TValue}"/> is empty.
    /// </summary>
    /// <returns><see langword="true"/> if the <see cref="IRegistry{TValue}"/> contains no entries; Otherwise, <see langword="false"/>.</returns>
    bool IsEmpty() => Count == 0;

    #endregion

    #region Content Retrieval

    /// <summary>
    /// Retrieve the value associated with a <see cref="ResourceName"/>.
    /// </summary>
    /// <param name="key"><see cref="ResourceName"/> to retrieve a value for.</param>
    /// <returns>
    /// The value associated with the provided <paramref name="key"/>,
    /// or <see langword="null"/> if the key is not recognised.
    /// </returns>
    TValue? GetValue(ResourceName key);

    /// <summary>
    /// Retrieve the <see cref="ResourceName"/> key associated with a value.
    /// </summary>
    /// <param name="value">Value to retrieve a <see cref="ResourceName"/> key for.</param>
    /// <returns>
    /// The <see cref="ResourceName"/> key associated with the provided value,
    /// or <see langword="null"/> if the value is not recognised.
    /// </returns>
    ResourceName? GetKey(TValue value);

    /// <summary>
    /// Retrieve the <see cref="IResourceKey{TValue}"/> key associated with a value.
    /// </summary>
    /// <param name="value">Value to retrieve a <see cref="IResourceKey{TValue}"/> key for.</param>
    /// <returns>
    /// The <see cref="IResourceKey{TValue}"/> key associated with the provided value, wrapped by <see cref="Optional{T}"/>.
    /// </returns>
    Optional<IResourceKey<TValue>> GetResourceKey(TValue value);

    #endregion

    #region Enumeration

    /// <summary>
    /// <see cref="ICollection{T}"/> of all registered <see cref="ResourceName"/> keys.
    /// </summary>
    ICollection<ResourceName> Keys { get; }

    /// <summary>
    /// <see cref="ICollection{T}"/> of all registered values.
    /// </summary>
    ICollection<TValue> Values { get; }

    /// <summary>
    /// <see cref="ICollection{T}"/> of all registered entries.
    /// </summary>
    ICollection<KeyValuePair<IResourceKey<TValue>, TValue>> Entries { get; }

    IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<TValue>).GetEnumerator();

    #endregion

    #region Tags

    ITagManager<TValue>? Tags { get; }

    #endregion

    #region Delegate Retrieval

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="IResourceKey{TValue}"/>,
    /// if it exists.
    /// </summary>
    /// <param name="key"><see cref="IResourceKey{TValue}"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key, wrapped by <see cref="Optional{T}"/>.</returns>
    Optional<IHolder.Reference<TValue>> GetDelegate(IResourceKey<TValue> key);

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="IResourceKey{TValue}"/>,
    /// throwing an error if the delegate could not be found.
    /// </summary>
    /// <param name="key"><see cref="IResourceKey{TValue}"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key.</returns>
    /// <exception cref="ArgumentException">No delegate exists for <paramref name="key"/>.</exception>
    IHolder.Reference<TValue> GetDelegateOrThrow(IResourceKey<TValue> key)
        => GetDelegate(key).IfNone(() => throw new ArgumentException($"No delegate exists for key {key}"));

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="ResourceName"/>, if it exists.
    /// </summary>
    /// <param name="key"><see cref="ResourceName"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key, wrapped by <see cref="Optional{T}"/>.</returns>
    Optional<IHolder.Reference<TValue>> GetDelegate(ResourceName key);

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="ResourceName"/>,
    /// throwing an error if the delegate could not be found.
    /// </summary>
    /// <param name="key"><see cref="ResourceName"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key</returns>
    /// <exception cref="ArgumentException">No delegate exists for <paramref name="key"/>.</exception>
    IHolder.Reference<TValue> GetDelegateOrThrow(ResourceName key)
        => GetDelegate(key).IfNone(() => throw new ArgumentException($"No delegate exists for key {key}"));

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <typeparamref name="TValue"/>, if it exists.
    /// </summary>
    /// <param name="value"><typeparamref name="TValue"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided value, wrapped by <see cref="Optional{T}"/>.</returns>
    Optional<IHolder.Reference<TValue>> GetDelegate(TValue value);

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <typeparamref name="TValue"/>, throwing an error if the
    /// delegate could not be found.
    /// </summary>
    /// <param name="value"><typeparamref name="TValue"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided value.</returns>
    /// <exception cref="ArgumentException">No delegate exists for <paramref name="value"/>.</exception>
    IHolder.Reference<TValue> GetDelegateOrThrow(TValue value)
        => GetDelegate(value).IfNone(() => throw new ArgumentException($"No delegate exists for value {value}"));

    #endregion

    #region Registry events / delegates

    /// <summary>
    /// Event invoked when contents are added to the registry. This will be invoked when the registry
    /// is rebuilt on the client side due to a server-side synchronization.
    /// </summary>
    event EventHandler<AddEventArgs<TValue>> OnAdd;

    /// <summary>
    /// Event invoked when a registry instance is initially created.
    /// </summary>
    event EventHandler<CreateEventArgs<TValue>> OnCreate;

    /// <summary>
    /// Event invoked when the registry's contents are validated.
    /// </summary>
    event EventHandler<ValidateEventArgs<TValue>> OnValidate;

    /// <summary>
    /// Event invoked when the registry has finished processing.
    /// </summary>
    event EventHandler<BakeEventArgs<TValue>> OnBake;

    #endregion
}
