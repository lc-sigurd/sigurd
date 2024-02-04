using Sigurd.Common.Resources;

namespace Sigurd.Common.Core;

/// <summary>
/// Interface for internal <see cref="SigurdRegistry{TValue}"/> members.
/// </summary>
/// <typeparam name="TValue">The type registered by the <see cref="ISigurdRegistry{TValue}"/>.</typeparam>
public interface ISigurdRegistryInternal<TValue> : ISigurdRegistry<TValue> where TValue : class
{
    /// <summary>
    /// Add a new entry to the <see cref="ISigurdRegistry{TValue}"/>.
    /// </summary>
    /// <param name="id"><see cref="int"/> ID for the new entry.</param>
    /// <param name="key"><see cref="ResourceLocation"/> key for the new entry.</param>
    /// <param name="value">Value for the new entry.</param>
    void Register(int id, ResourceLocation key, TValue value);

    /// <summary>
    /// Retrieve the value associated with an <see cref="int"/> ID.
    /// </summary>
    /// <param name="id"><see cref="int"/> to retrieve a value for.</param>
    /// <returns>
    /// The value associated with the provided <paramref name="id"/>,
    /// or <see langword="null"/> if the ID is not recognised.
    /// </returns>
    TValue? GetValue(int id);
}
