using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;

namespace Sigurd.Common.Collections.Generic;

/// <inheritdoc />
public class BiDictionary<TKey, TValue> : IBiDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _forward;
    private readonly Dictionary<TValue, TKey> _backward;
    private InverseImpl? _inverse;

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDictionary{TKey, TValue}"/> class that contains no elements,
    /// has the default initial capacity, and uses the specified equality comparers for the key and value types.
    /// The default equality comparers for the key and value types will be used if they are not specified, or if
    /// their specified value is <see langword="null"/>.
    /// </summary>
    /// <param name="keyComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the key type.
    /// </param>
    /// <param name="valueComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the value type.
    /// </param>
    public BiDictionary(IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
        : this(0, keyComparer, valueComparer) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDictionary{TKey, TValue}"/> class that contains no elements,
    /// has the specified initial capacity, and uses the specified equality comparers for the key and value types.
    /// The default equality comparers for the key and value types will be used if they are not specified, or if
    /// their specified value is <see langword="null"/>.
    /// </summary>
    /// <param name="capacity">
    /// The initial number of elements that the <see cref="BiDictionary{TKey,TValue}"/> can contain.
    /// </param>
    /// <param name="keyComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the key type.
    /// </param>
    /// <param name="valueComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the value type.
    /// </param>
    public BiDictionary(int capacity, IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
    {
        _forward = new Dictionary<TKey, TValue>(capacity, keyComparer);
        _backward = new Dictionary<TValue, TKey>(capacity, valueComparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDictionary{TKey, TValue}"/> class that contains elements
    /// copied from the specified <see cref="IDictionary{TKey,TValue}"/> and uses the specified equality comparers
    /// for the key and value types.
    /// The default equality comparers for the key and value types will be used if they are not specified, or if
    /// their specified value is <see langword="null"/>.
    /// </summary>
    /// <param name="dictionary">
    /// The <see cref="IDictionary{TKey,TValue}"/> whose elements are copied to the new
    /// <see cref="BiDictionary{TKey,TValue}"/>.
    /// </param>
    /// <param name="keyComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the key type.
    /// </param>
    /// <param name="valueComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the value type.
    /// </param>
    public BiDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
    {
        _forward = new Dictionary<TKey, TValue>(dictionary, keyComparer);
        _backward = _forward.ToDictionary(item => item.Value, item => item.Key, valueComparer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BiDictionary{TKey, TValue}"/> class that contains elements
    /// copied from the specified <see cref="IEnumerable{T}"/> and uses the specified equality comparers.
    /// The default equality comparers for the key and value types will be used if they are not specified, or if
    /// their provided value is <see langword="null"/>.
    /// </summary>
    /// <param name="entries">
    /// The <see cref="IEnumerable{T}"/> whose elements are copied to the new <see cref="BiDictionary{TKey,TValue}"/>.
    /// </param>
    /// <param name="keyComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the key type.
    /// </param>
    /// <param name="valueComparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or <see langword="null"/> to
    /// use the default <see cref="EqualityComparer{T}"/> for the value type.
    /// </param>
    public BiDictionary(IEnumerable<KeyValuePair<TKey, TValue>> entries, IEqualityComparer<TKey>? keyComparer = null, IEqualityComparer<TValue>? valueComparer = null)
    {
        _forward = new Dictionary<TKey, TValue>(entries);
        _backward = _forward.ToDictionary(item => item.Value, item => item.Key, valueComparer);
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read | CollectionAccessType.ModifyExistingContent)]
    public IBiDictionary<TValue, TKey> Inverse => _inverse ??= new InverseImpl(this);

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _forward.GetEnumerator();

    [CollectionAccess(CollectionAccessType.Read)]
    IEnumerator IEnumerable.GetEnumerator() => (_forward as IEnumerable).GetEnumerator();

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
    public void Clear()
    {
        _forward.Clear();
        _backward.Clear();
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (!_forward.TryGetValue(item.Key, out var value))
            return false;

        if (ReferenceEquals(value, null))
            return false;

        return _backward.Comparer.Equals(value, item.Value);
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => (_forward as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var forwardSuccess = (_forward as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        var backwardSuccess = (_backward as ICollection<KeyValuePair<TValue, TKey>>).Remove(new KeyValuePair<TValue, TKey>(item.Value, item.Key));

        Debug.Assert(forwardSuccess == backwardSuccess);

        return forwardSuccess;
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public int Count => _forward.Count;

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.None)]
    public bool IsReadOnly => false;

    /// <inheritdoc />
    /// <exception cref="T:System.ArgumentException">An element with the same value already exists in the <see cref="IBiDictionary{TKey,TValue}"/>.</exception>
    [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
    public void Add(TKey key, TValue value)
    {
        if (_backward.ContainsKey(value))
            throw new ArgumentException("An element with the specified value is already present.");

        _forward.Add(key, value);
        _backward.Add(value, key);
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public bool ContainsKey(TKey key) => _forward.ContainsKey(key);

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public bool ContainsValue(TValue value) => _backward.ContainsKey(value);

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.Read)]
    public bool Remove(TKey key)
    {
        var keyWasFound = _forward.Remove(key, out var value);
        if (!keyWasFound) return false;
        var valueWasFound = _backward.Remove(value);

        Debug.Assert(valueWasFound);

        return true;
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.ModifyExistingContent | CollectionAccessType.Read)]
    public bool RemoveValue(TValue value)
    {
        var valueWasFound = _backward.Remove(value, out var key);
        if (!valueWasFound) return false;
        var keyWasFound = _forward.Remove(key);

        Debug.Assert(keyWasFound);

        return true;
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public bool TryGetValue(TKey key, [UnscopedRef] out TValue value) => _forward.TryGetValue(key, out value);

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public bool TryGetKey(TValue value, [UnscopedRef]  out TKey key) => _backward.TryGetValue(value, out key);

    /// <inheritdoc />
    /// <exception cref="T:System.InvalidOperationException">The property is set and the value is already present, bound to a different key.</exception>
    public TValue this[TKey key] {
        [CollectionAccess(CollectionAccessType.Read)]
        get => _forward[key];

        [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
        set {
            if (_backward.TryGetValue(value, out var oldValueKey) && !_forward.Comparer.Equals(oldValueKey, key))
                throw new InvalidOperationException("The value is already present and bound to a different key.");

            _forward[key] = value;
            _backward[value] = key;
        }
    }

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public ICollection<TKey> Keys => _forward.Keys;

    /// <inheritdoc />
    [CollectionAccess(CollectionAccessType.Read)]
    public ICollection<TValue> Values => _forward.Values;

    private sealed class InverseImpl : IBiDictionary<TValue, TKey>
    {
        private readonly BiDictionary<TKey, TValue> _owner;

        public InverseImpl(BiDictionary<TKey, TValue> owner) => _owner = owner;

        public IEnumerator<KeyValuePair<TValue, TKey>> GetEnumerator() => _owner._backward.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(KeyValuePair<TValue, TKey> item) => _owner.Add(item.Value, item.Key);

        public void Clear() => _owner.Clear();

        public bool Contains(KeyValuePair<TValue, TKey> item)
        {
            if (!_owner._backward.TryGetValue(item.Key, out var key))
                return false;

            if (ReferenceEquals(key, null))
                return false;

            return _owner._forward.Comparer.Equals(key, item.Value);
        }

        public void CopyTo(KeyValuePair<TValue, TKey>[] array, int arrayIndex)
            => (_owner._backward as ICollection<KeyValuePair<TValue, TKey>>).CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<TValue, TKey> item)
        {
            var forwardSuccess = (_owner._forward as ICollection<KeyValuePair<TKey, TValue>>).Remove(new KeyValuePair<TKey, TValue>(item.Value, item.Key));
            var backwardSuccess = (_owner._backward as ICollection<KeyValuePair<TValue, TKey>>).Remove(item);

            Debug.Assert(forwardSuccess == backwardSuccess);

            return forwardSuccess;
        }

        public int Count => _owner.Count;

        public bool IsReadOnly => _owner.IsReadOnly;

        public void Add(TValue key, TKey value) => _owner.Add(value, key);

        public bool ContainsKey(TValue key) => _owner.ContainsValue(key);

        public bool Remove(TValue key) => _owner.RemoveValue(key);

        public bool TryGetValue(TValue key, [UnscopedRef] out TKey value) => _owner.TryGetKey(key, out value);

        public TKey this[TValue valueKey] {
            [CollectionAccess(CollectionAccessType.Read)]
            get => _owner._backward[valueKey];

            [CollectionAccess(CollectionAccessType.ModifyExistingContent)]
            set {
                if (_owner._forward.TryGetValue(value, out var oldValue) && !_owner._backward.Comparer.Equals(oldValue, valueKey))
                    throw new InvalidOperationException("The key is already present and bound to a different value.");

                _owner._backward[valueKey] = value;
                _owner._forward[value] = valueKey;
            }
        }

        public ICollection<TValue> Keys => _owner.Values;

        public ICollection<TKey> Values => _owner.Keys;

        public IBiDictionary<TKey, TValue> Inverse => _owner;
        public bool ContainsValue(TKey value) => _owner.ContainsKey(value);

        public bool RemoveValue(TKey value) => _owner.Remove(value);

        public bool TryGetKey(TKey value, [UnscopedRef] out TValue key) => _owner.TryGetValue(value, out key);
    }
}
