using System;
using System.Collections.Generic;
using System.Linq;
using Sigurd.Common.Core.Resources;
using Sigurd.Common.Core.Tags;
using SigurdLib.Util;
using SigurdLib.Util.Resources;

namespace Sigurd.Common.Core;

public interface IHolder
{
    public sealed class Reference<THeld> : IHolder<THeld>
        where THeld : class
    {
        private readonly IHolderOwner<THeld> _owner;
        private HashSet<ITagKey<THeld>> _tags = [];
        private IResourceKey<THeld>? _key;
        private THeld? _value;

        public Reference(IHolderOwner<THeld> owner, IResourceKey<THeld>? key)
        {
            _owner = owner;
            _key = key;
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
        public bool Is(ResourceName name) => Key.Name.Equals(name);

        /// <inheritdoc />
        public bool Is(IResourceKey<THeld> resourceKey) => Key.Equals(resourceKey);

        /// <inheritdoc />
        public bool Is(Predicate<IResourceKey<THeld>> predicate) => predicate(Key);

        /// <inheritdoc />
        public bool Is(ITagKey<THeld> tagKey) => _tags.Contains(tagKey);

        /// <inheritdoc />
        public Optional<IResourceKey<THeld>> Unwrap() => Optional.Some(Key);

        /// <inheritdoc />
        public bool CanSerializeIn(IHolderOwner<THeld> owner) => _owner.CanSerializeIn(owner);
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

    bool Is(ResourceName name);

    bool Is(IResourceKey<TValue> resourceKey);

    bool Is(Predicate<IResourceKey<TValue>> predicate);

    bool Is(ITagKey<TValue> tagKey);

    Optional<IResourceKey<TValue>> Unwrap();

    bool CanSerializeIn(IHolderOwner<TValue> owner);
}
