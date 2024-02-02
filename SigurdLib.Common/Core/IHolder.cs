using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Sigurd.Common.Resources;
using Sigurd.Common.Tags;
using Generic = System.Collections.Generic;

namespace Sigurd.Common.Core;

public interface IHolder
{
    public enum Kind
    {
        Reference,
        Direct,
    }

    public sealed record Direct<THeld>(THeld Value) : IHolder<THeld>
        where THeld : class
    {
        /// <inheritdoc />
        public IEnumerable<ITagKey<THeld>> Tags => Array.Empty<ITagKey<THeld>>();

        /// <inheritdoc />
        public bool IsBound => true;

        /// <inheritdoc />
        public bool Is(ResourceLocation location) => false;

        /// <inheritdoc />
        public bool Is(IResourceKey<THeld> resourceKey) => false;

        /// <inheritdoc />
        public bool Is(Predicate<IResourceKey<THeld>> predicate) => false;

        /// <inheritdoc />
        public bool Is(ITagKey<THeld> tagKey) => false;

        /// <inheritdoc />
        public Either<IResourceKey<THeld>, THeld> Unwrap() => Either<IResourceKey<THeld>, THeld>.Right(Value);

        /// <inheritdoc />
        public Option<IResourceKey<THeld>> UnwrapKey() => Option<IResourceKey<THeld>>.None;

        /// <inheritdoc />
        public Kind Kind => Kind.Direct;

        /// <inheritdoc />
        public bool CanSerializeIn(IHolderOwner<THeld> owner) => true;
    }

    public sealed class Reference<THeld> : IHolder<THeld>
        where THeld : class
    {
        private readonly IHolderOwner<THeld> _owner;
        private Generic.HashSet<ITagKey<THeld>> _tags = [];
        private IResourceKey<THeld>? _key;
        private THeld? _value;

        public Reference(IHolderOwner<THeld> owner, IResourceKey<THeld>? key, THeld? value)
        {
            _owner = owner;
            _key = key;
            _value = value;
        }

        /// <inheritdoc />
        public IEnumerable<ITagKey<THeld>> Tags {
            get => _tags.AsEnumerable();
            set => _tags = value.ToHashSet();
        }

        public IResourceKey<THeld> Key {
            get {
                if (_key is not null) return _key;
                throw new InvalidOperationException($"Trying to access unbound value '{_value}' from registry {_owner}");
            }
            set => _key = value;
        }

        public THeld Value {
            get {
                if (_value is not null) return _value;
                throw new InvalidOperationException($"Trying to access unbound value '{_key}' from registry {_owner}");
            }
            set => _value = value;
        }

        /// <inheritdoc />
        public bool IsBound => _key is not null && _value is not null;

        /// <inheritdoc />
        public bool Is(ResourceLocation location) => Key.Location.Equals(location);

        /// <inheritdoc />
        public bool Is(IResourceKey<THeld> resourceKey) => Key.Equals(resourceKey);

        /// <inheritdoc />
        public bool Is(Predicate<IResourceKey<THeld>> predicate) => predicate(Key);

        /// <inheritdoc />
        public bool Is(ITagKey<THeld> tagKey) => _tags.Contains(tagKey);

        /// <inheritdoc />
        public Either<IResourceKey<THeld>, THeld> Unwrap() => Either<IResourceKey<THeld>, THeld>.Left(Key);

        /// <inheritdoc />
        public Option<IResourceKey<THeld>> UnwrapKey() => Option<IResourceKey<THeld>>.Some(Key);

        /// <inheritdoc />
        public Kind Kind => Kind.Reference;

        /// <inheritdoc />
        public bool CanSerializeIn(IHolderOwner<THeld> owner) => _owner.canSerializeIn(owner);
    }
}

public interface IHolder<TValue> : IHolder, IReverseTag<TValue>
    where TValue : class
{
    bool IReverseTag<TValue>.Contains(ITagKey<TValue> tagKey) => Is(tagKey);

    /// <summary>
    /// The held <typeparamref name="TValue"/>.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Determine whether this <see cref="IHolder{TValue}"/> was loaded with a value.
    /// If this is <see langword="false"/>, the holder is always empty.
    /// </summary>
    /// <returns><see langword="true"/> if this holder was loaded with a value (even if that value was empty)</returns>
    bool IsBound { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    bool Is(ResourceLocation location);

    bool Is(IResourceKey<TValue> resourceKey);

    bool Is(Predicate<IResourceKey<TValue>> predicate);

    bool Is(ITagKey<TValue> tagKey);

    Either<IResourceKey<TValue>, TValue> Unwrap();

    Option<IResourceKey<TValue>> UnwrapKey();

    Kind Kind { get; }

    bool CanSerializeIn(IHolderOwner<TValue> owner);
}
