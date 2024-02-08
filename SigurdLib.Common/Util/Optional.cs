using System;

namespace Sigurd.Common.Util;

public static class Optional
{
    public static Optional<T> Some<T>(T value) => Optional<T>.Some(value);
}

public readonly struct Optional<T>
{
    private readonly T? _value = default;
    private readonly bool _isSome = false;

    private Optional(T value)
    {
        _value = value;
        _isSome = true;
    }

    /// <summary>None</summary>
    public static readonly Optional<T> None = default;

    /// <summary>Construct an <see cref="Optional{T}"/> in a Some state</summary>
    /// <param name="value">Value to bind, must be non-null</param>
    /// <returns><see cref="Optional{T}"/></returns>
    public static Optional<T> Some(T value) => new(value);

    /// <summary>
    /// Construct an <see cref="Optional{T}"/> whose state is dependent on whether
    /// the provided value is <see langword="null"/>
    /// </summary>
    /// <param name="value">Value to bind</param>
    /// <returns><see cref="Optional{T}"/></returns>
    public static Optional<T> OfNullable(T? value) => value is null ? None : Some(value);

    public static explicit operator T(Optional<T> maybe) => maybe._isSome ? maybe._value! : throw new InvalidCastException("Optional is not in a Some state");

    public T? ValueUnsafe => _isSome ? _value! : default;

    public bool IsSome => _isSome;

    public bool IsNone => !_isSome;

    public T IfNone(Func<T> noneSupplier) => _isSome ? _value! : noneSupplier() ?? throw new ArgumentException("Supplier must not return null.");

    /// <summary>
    /// Project from one value to another.
    /// </summary>
    /// <param name="projector">Projection function</param>
    /// <typeparam name="V">Resulting value type</typeparam>
    /// <returns>Mapped <see cref="Optional{T}"/></returns>
    public Optional<V> Select<V>(Func<T, V> projector) => _isSome ? Optional.Some(projector(_value!)) : Optional<V>.None;

    /// <summary>
    /// Apply a <see cref="Predicate{T}"/> to the bound value (if in a Some state).
    /// </summary>
    /// <param name="filter"><see cref="Predicate{T}"/> to apply</param>
    /// <returns>
    /// <see langword="this"/> if in a Some state and <paramref name="filter"/>
    /// returns <see langword="true"/>; otherwise, <see cref="None"/>.
    /// </returns>
    public Optional<T> Where(Predicate<T> filter) => _isSome && filter(_value) ? this : None;

    /// <summary>
    /// Attempt to cast the bound value to <typeparamref name="V"/> (if in a Some state).
    /// </summary>
    /// <typeparam name="V">The type to cast the bound value to.</typeparam>
    /// <returns>
    /// <see cref="Optional{T}"/> containing the casted value, if in a Some state and the cast
    /// valid; otherwise, <see cref="None"/>.
    /// </returns>
    public Optional<V> Cast<V>() => _value is V value ? Optional.Some(value) : Optional<V>.None;
}
