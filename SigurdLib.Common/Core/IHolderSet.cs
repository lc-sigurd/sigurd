using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sigurd.Common.Core.Tags;
using SigurdLib.Util;

namespace Sigurd.Common.Core;

public interface IHolderSet
{
    public abstract class ListBacked<THeld> : IHolderSet<THeld>
        where THeld : class
    {
        private List<IHolder<THeld>> _contents = new();

        public virtual IEnumerable<IHolder<THeld>> Contents {
            protected get => _contents;
            set => _contents = value.ToList();
        }

        /// <inheritdoc />
        public int Count => _contents.Count;

        /// <inheritdoc />
        public Optional<IHolder<THeld>> GetRandomElement(Random randomSource)
        {
            if (_contents.Count == 0) return Optional<IHolder<THeld>>.None;
            return Optional<IHolder<THeld>>.Some(_contents[randomSource.Next(Count)]);
        }

        /// <inheritdoc />
        public IEnumerator<IHolder<THeld>> GetEnumerator() => Contents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Contents.GetEnumerator();

        /// <inheritdoc />
        public virtual bool CanSerializeIn(IHolderOwner<THeld> owner) => true;

        /// <inheritdoc />
        public abstract bool Contains(IHolder<THeld> holder);

        /// <inheritdoc />
        public abstract Optional<ITagKey<THeld>> Unwrap();
    }

    public sealed class Named<THeld> : ListBacked<THeld>
        where THeld : class
    {
        private readonly IHolderOwner<THeld> _owner;

        public ITagKey<THeld> Key { get; }

        public Named(IHolderOwner<THeld> owner, ITagKey<THeld> key)
        {
            _owner = owner;
            Key = key;
        }

        /// <inheritdoc />
        public override Optional<ITagKey<THeld>> Unwrap()
            => Optional<ITagKey<THeld>>.Some(Key);

        /// <inheritdoc />
        public override bool Contains(IHolder<THeld> holder) => holder.Is(Key);

        /// <inheritdoc />
        public override bool CanSerializeIn(IHolderOwner<THeld> owner) => _owner.CanSerializeIn(owner);

        /// <inheritdoc />
        public override string ToString() => $"NamedSet({Key})[{String.Join(", ", Contents)}]";
    }
}

public interface IHolderSet<THeld> : IHolderSet, IReadOnlyCollection<IHolder<THeld>>
    where THeld : class
{
    bool Contains(IHolder<THeld> holder);

    bool CanSerializeIn(IHolderOwner<THeld> owner);

    Optional<IHolder<THeld>> GetRandomElement(Random randomSource);

    Optional<ITagKey<THeld>> Unwrap();
}
