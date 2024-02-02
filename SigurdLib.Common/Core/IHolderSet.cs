using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Sigurd.Common.Tags;
using Generic = System.Collections.Generic;

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
        public Option<IHolder<THeld>> GetRandomElement(Random randomSource)
        {
            if (_contents.Count == 0) return Option<IHolder<THeld>>.None;
            return Option<IHolder<THeld>>.Some(_contents[randomSource.Next(Count)]);
        }

        /// <inheritdoc />
        public IEnumerator<IHolder<THeld>> GetEnumerator() => Contents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Contents.GetEnumerator();

        /// <inheritdoc />
        public virtual bool CanSerializeIn(IHolderOwner<THeld> owner) => true;

        /// <inheritdoc />
        public abstract bool Contains(IHolder<THeld> holder);

        /// <inheritdoc />
        public abstract Either<ITagKey<THeld>, IEnumerable<IHolder<THeld>>> Unwrap();

        /// <inheritdoc />
        public abstract Option<ITagKey<THeld>> UnwrapKey();
    }

    public sealed class Direct<THeld> : ListBacked<THeld>
        where THeld : class
    {
        private readonly List<IHolder<THeld>> _contents;
        private Generic.HashSet<IHolder<THeld>>? _contentsSet;

        public Direct(IEnumerable<IHolder<THeld>> contents)
        {
            _contents = contents.ToList();
        }

        /// <inheritdoc />
        public override IEnumerable<IHolder<THeld>> Contents {
            protected get => _contents;
            set { }
        }

        /// <inheritdoc />
        public override Either<ITagKey<THeld>, IEnumerable<IHolder<THeld>>> Unwrap()
            => Either<ITagKey<THeld>, IEnumerable<IHolder<THeld>>>.Right(_contents);

        /// <inheritdoc />
        public override Option<ITagKey<THeld>> UnwrapKey()
            => Option<ITagKey<THeld>>.None;

        /// <inheritdoc />
        public override bool Contains(IHolder<THeld> holder)
        {
            if (_contentsSet is null) {
                _contentsSet = _contents.ToHashSet();
            }

            return _contentsSet.Contains(holder);
        }

        /// <inheritdoc />
        public override string ToString() => $"DirectSet[{String.Join(", ", _contents)}]";
    }

    public sealed class Named<THeld> : ListBacked<THeld>
        where THeld : class
    {
        private readonly IHolderOwner<THeld> _owner;

        public ITagKey<THeld> Key { get; }

        public Named(IHolderOwner<THeld> owner, TagKey<THeld> key)
        {
            _owner = owner;
            Key = key;
        }

        /// <inheritdoc />
        public override Either<ITagKey<THeld>, IEnumerable<IHolder<THeld>>> Unwrap()
            => Either<ITagKey<THeld>, IEnumerable<IHolder<THeld>>>.Left(Key);

        /// <inheritdoc />
        public override Option<ITagKey<THeld>> UnwrapKey()
            => Option<ITagKey<THeld>>.Some(Key);

        /// <inheritdoc />
        public override bool Contains(IHolder<THeld> holder) => holder.Is(Key);

        /// <inheritdoc />
        public override bool CanSerializeIn(IHolderOwner<THeld> owner) => _owner.canSerializeIn(owner);

        /// <inheritdoc />
        public override string ToString() => $"NamedSet({Key})[{String.Join(", ", Contents)}]";
    }
}

public interface IHolderSet<THeld> : IHolderSet, IReadOnlyCollection<IHolder<THeld>>
    where THeld : class
{
    bool Contains(IHolder<THeld> holder);

    bool CanSerializeIn(IHolderOwner<THeld> owner);

    Option<IHolder<THeld>> GetRandomElement(Random randomSource);

    Either<ITagKey<THeld>, IEnumerable<IHolder<THeld>>> Unwrap();

    Option<ITagKey<THeld>> UnwrapKey();
}
