using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sigurd.Common.Collections.Generic;

/// <summary>
/// Represents a generic collection of key/value pairs that supports inverse mapping operations.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public interface IBiDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    /// <summary>Gets or sets the element with the specified value.</summary>
    /// <param name="valueKey">The value of the element to get or set.</param>
    /// <returns>The element with the specified value.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="valueKey" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and <paramref name="valueKey" /> is not found.</exception>
    /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="IBiDictionary{TKey, TValue}" /> is read-only.</exception>
    /// <exception cref="T:System.InvalidOperationException">The property is set and the key is already present, bound to a different value.</exception>
    TKey this[TValue valueKey] { get; set; }

    /// <summary>Determines whether the <see cref="IBiDictionary{TKey, TValue}" /> contains an element with the specified value.</summary>
    /// <param name="value">The value to locate in the <see cref="IBiDictionary{TKey, TValue}" />.</param>
    /// <returns>
    /// <see langword="true" /> if the <see cref="IBiDictionary{TKey, TValue}" /> contains an element with the value; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="value" /> is <see langword="null" />.</exception>
    bool ContainsValue(TValue value);

    /// <summary>Removes the element with the specified value from the <see cref="IBiDictionary{TKey, TValue}" />.</summary>
    /// <param name="value">The value of the element to remove.</param>
    /// <returns>
    /// <see langword="true" /> if the element is successfully removed; otherwise, <see langword="false" />.  This method also returns <see langword="false" /> if <paramref name="value" /> was not found in the original <see cref="IBiDictionary{TKey, TValue}" />.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="IBiDictionary{TKey, TValue}" /> is read-only.</exception>
    bool Remove(TValue value);

    /// <summary>Gets the key associated with the specified value.</summary>
    /// <param name="value">The value whose key to get.</param>
    /// <param name="key">When this method returns, the key associated with the specified value, if the value is found; otherwise, the default value for the type of the <paramref name="key" /> parameter. This parameter is passed uninitialized.</param>
    /// <returns>
    /// <see langword="true" /> if the object that implements <see cref="IBiDictionary{TKey, TValue}" /> contains an element with the specified value; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
    bool TryGetKey(TValue value, out TKey key);
}
