using Sigurd.Common.Resources;

namespace Sigurd.Common.Core;

/// <summary>
/// Interface for a modifiable <see cref="ISigurdRegistry{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type registered by the <see cref="ISigurdRegistry{TValue}"/>.</typeparam>
public interface ISigurdRegistryModifiable<TValue> : ISigurdRegistry<TValue> where TValue : class
{
    /// <summary>
    /// Clear the <see cref="ISigurdRegistryModifiable{TValue}"/>. The <see cref="ISigurdRegistry{TValue}.OnClear"/>
    /// event will be invoked, and all entries will be removed.
    /// </summary>
    void Clear();

    /// <summary>
    /// Remove a specific value from the <see cref="ISigurdRegistryModifiable{TValue}"/>.
    /// </summary>
    /// <param name="key">The <see cref="ResourceLocation"/> key of the value to remove.</param>
    /// <returns></returns>
    TValue? Remove(ResourceLocation key);

    /// <summary>
    /// Determines whether the <see cref="ISigurdRegistryModifiable{TValue}"/> is locked, i.e. whether it is
    /// currently modifiable.
    /// </summary>
    bool IsLocked { get; }
}
