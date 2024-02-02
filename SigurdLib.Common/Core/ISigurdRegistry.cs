using System;
using System.Collections.Generic;
using LanguageExt;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Core;

/// <summary>
/// Covariant interface for <see cref="ISigurdRegistry{TValue}"/>.
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
    ResourceLocation RegistryName { get; }
}

/// <summary>
/// Defines <see langword="delegate"/> types and other <see langword="static"/> members for <see cref="ISigurdRegistry{TValue}"/>.
/// </summary>
public interface ISigurdRegistry
{
    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="ISigurdRegistry{TValue}.OnAdd"/>.
    /// </summary>
    class AddEventArgs<TValue> : EventArgs
    {

    }

    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="ISigurdRegistry{TValue}.OnClear"/>.
    /// </summary>
    class ClearEventArgs<TValue> : EventArgs
    {

    }

    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="ISigurdRegistry{TValue}.OnCreate"/>.
    /// </summary>
    class CreateEventArgs<TValue> : EventArgs
    {

    }

    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="ISigurdRegistry{TValue}.OnValidate"/>.
    /// </summary>
    class ValidateEventArgs<TValue> : EventArgs
    {

    }

    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="ISigurdRegistry{TValue}.OnBake"/>.
    /// </summary>
    class BakeEventArgs<TValue> : EventArgs
    {

    }
}

/// <summary>
/// Main interface for the registry system. Use this to query the registry system.
/// </summary>
/// <typeparam name="TValue">The type registered by the <see cref="ISigurdRegistry{TValue}"/>.</typeparam>
public interface ISigurdRegistry<TValue> : ISigurdRegistrar<TValue>, IReadOnlyCollection<TValue>
    where TValue : class
{
    #region Registration

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void Register(string key, TValue value);

    /// <summary>
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    void Register(ResourceLocation key, TValue value);

    #endregion

    #region Content Queries

    /// <summary>
    /// Determines whether the <see cref="ISigurdRegistry{TValue}"/> contains a specific <see cref="ResourceLocation"/> key.
    /// </summary>
    /// <param name="key">The <see cref="ResourceLocation"/> to locate in the <see cref="ISigurdRegistry{TValue}" />.</param>
    /// <returns><see langword="true"/> if the <paramref name="key"/> is found in the <see cref="ISigurdRegistry{TValue}"/>; Otherwise, <see langword="false"/>.</returns>
    bool ContainsKey(ResourceLocation key);

    /// <summary>
    /// Determines whether the <see cref="ISigurdRegistry{TValue}"/> contains a specific value.
    /// </summary>
    /// <param name="value">The value to locate in the <see cref="ISigurdRegistry{TValue}" />.</param>
    /// <returns><see langword="true"/> if the <paramref name="value"/> is found in the <see cref="ISigurdRegistry{TValue}"/>; Otherwise, <see langword="false"/>.</returns>
    bool ContainsValue(TValue value);

    /// <summary>
    /// Determines whether the <see cref="ISigurdRegistry{TValue}"/> is empty.
    /// </summary>
    /// <returns><see langword="true"/> if the <see cref="ISigurdRegistry{TValue}"/> contains no entries; Otherwise, <see langword="false"/>.</returns>
    bool IsEmpty() => Count == 0;

    #endregion

    #region Content Retrieval

    /// <summary>
    /// Retrieve the value associated with a <see cref="ResourceLocation"/>.
    /// </summary>
    /// <param name="key"><see cref="ResourceLocation"/> to retrieve a value for.</param>
    /// <returns>
    /// The value associated with the provided <paramref name="key"/>,
    /// or <see langword="null"/> if the key is not recognised.
    /// </returns>
    TValue? GetValue(ResourceLocation key);

    /// <summary>
    /// Retrieve the <see cref="ResourceLocation"/> key associated with a value.
    /// </summary>
    /// <param name="value">Value to retrieve a <see cref="ResourceLocation"/> key for.</param>
    /// <returns>
    /// The <see cref="ResourceLocation"/> key associated with the provided value,
    /// or <see langword="null"/> if the value is not recognised.
    /// </returns>
    ResourceLocation? GetKey(TValue value);

    /// <summary>
    /// Retrieve the <see cref="IResourceKey{TValue}"/> key associated with a value.
    /// </summary>
    /// <param name="value">Value to retrieve a <see cref="IResourceKey{TValue}"/> key for.</param>
    /// <returns>
    /// The <see cref="IResourceKey{TValue}"/> key associated with the provided value, wrapped by <see cref="Option{T}"/>.
    /// </returns>
    Option<IResourceKey<TValue>> GetResourceKey(TValue value);

    #endregion

    #region Enumeration

    /// <summary>
    /// <see cref="ISet{T}"/> of all registered <see cref="ResourceLocation"/> keys.
    /// </summary>
    ISet<ResourceLocation> Keys { get; }

    /// <summary>
    /// <see cref="ICollection{T}"/> of all registered values.
    /// </summary>
    ICollection<TValue> Values { get; }

    /// <summary>
    /// <see cref="ISet{T}"/> of all registered entries.
    /// </summary>
    ISet<KeyValuePair<IResourceKey<TValue>, TValue>> Entries { get; }

    #endregion

    #region Tags

    // TODO
    // ITagManager<TValue>? Tags { get; }

    #endregion

    #region Delegate Retrieval

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="IResourceKey{TValue}"/>,
    /// if it exists.
    /// </summary>
    /// <param name="key"><see cref="IResourceKey{TValue}"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key, wrapped by <see cref="Option{T}"/>.</returns>
    Option<IHolder.Reference<TValue>> GetDelegate(ResourceKey<TValue> key);

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="IResourceKey{TValue}"/>,
    /// throwing an error if the delegate could not be found.
    /// </summary>
    /// <param name="key"><see cref="IResourceKey{TValue}"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key.</returns>
    /// <exception cref="ArgumentException">No delegate exists for <paramref name="key"/>.</exception>
    IHolder.Reference<TValue> GetDelegateOrThrow(ResourceKey<TValue> key)
        => GetDelegate(key).IfNone(() => throw new ArgumentException($"No delegate exists for key {key}"));

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="ResourceLocation"/>, if it exists.
    /// </summary>
    /// <param name="key"><see cref="ResourceLocation"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key, wrapped by <see cref="Option{T}"/>.</returns>
    Option<IHolder.Reference<TValue>> GetDelegate(ResourceLocation key);

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <see cref="ResourceLocation"/>,
    /// throwing an error if the delegate could not be found.
    /// </summary>
    /// <param name="key"><see cref="ResourceLocation"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided key</returns>
    /// <exception cref="ArgumentException">No delegate exists for <paramref name="key"/>.</exception>
    IHolder.Reference<TValue> GetDelegateOrThrow(ResourceLocation key)
        => GetDelegate(key).IfNone(() => throw new ArgumentException($"No delegate exists for key {key}"));

    /// <summary>
    /// Retrieve a delegate <see cref="IHolder.Reference{THeld}"/> for a <typeparamref name="TValue"/>, if it exists.
    /// </summary>
    /// <param name="value"><typeparamref name="TValue"/> to retrieve a delegate for.</param>
    /// <returns>The <see cref="IHolder.Reference{THeld}"/> delegate for the provided value, wrapped by <see cref="Option{T}"/>.</returns>
    Option<IHolder.Reference<TValue>> GetDelegate(TValue value);

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
    event EventHandler<ISigurdRegistry.AddEventArgs<TValue>> OnAdd;

    /// <summary>
    /// Event invoked when the registry's contents are cleared. This will be invoked before a registry
    /// is rebuilt.
    /// </summary>
    event EventHandler<ISigurdRegistry.ClearEventArgs<TValue>> OnClear;

    /// <summary>
    /// Event invoked when a registry instance is initially created.
    /// </summary>
    event EventHandler<ISigurdRegistry.CreateEventArgs<TValue>> OnCreate;

    /// <summary>
    /// Event invoked when the registry's contents are validated.
    /// </summary>
    event EventHandler<ISigurdRegistry.ValidateEventArgs<TValue>> OnValidate;

    /// <summary>
    /// Event invoked when the registry has finished processing.
    /// </summary>
    event EventHandler<ISigurdRegistry.BakeEventArgs<TValue>> OnBake;

    #endregion
}
