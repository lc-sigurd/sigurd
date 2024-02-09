using System;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Registries;

/// <summary>
/// Defines <see langword="delegate"/> types and other <see langword="static"/> members for <see cref="ISigurdRegistryModifiable{TValue}"/>.
/// </summary>
public interface ISigurdRegistryModifiable
{
    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="ISigurdRegistryModifiable{TValue}.OnClear"/>.
    /// </summary>
    class ClearEventArgs<TValue> : EventArgs where TValue : class
    {

    }
}

/// <summary>
/// Interface for a modifiable <see cref="ISigurdRegistry{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type registered by the <see cref="ISigurdRegistry{TValue}"/>.</typeparam>
public interface ISigurdRegistryModifiable<TValue> : ISigurdRegistry<TValue>, ISigurdRegistryModifiable
    where TValue : class
{
    /// <summary>
    /// Clear the <see cref="ISigurdRegistryModifiable{TValue}"/>. The <see cref="ISigurdRegistry{TValue}.OnClear"/>
    /// event will be invoked, and all entries will be removed.
    /// </summary>
    void Clear();

    /// <summary>
    /// RemoveValue a specific value from the <see cref="ISigurdRegistryModifiable{TValue}"/>.
    /// </summary>
    /// <param name="key">The <see cref="ResourceName"/> key of the value to remove.</param>
    /// <returns></returns>
    TValue? Remove(ResourceName key);

    /// <summary>
    /// Determines whether the <see cref="ISigurdRegistryModifiable{TValue}"/> is locked, i.e. whether it is
    /// currently modifiable.
    /// </summary>
    bool IsLocked { get; }

    /// <summary>
    /// Event invoked when the registry's contents are cleared. This will be invoked before a registry
    /// is rebuilt.
    /// </summary>
    event EventHandler<ClearEventArgs<TValue>> OnClear;
}
