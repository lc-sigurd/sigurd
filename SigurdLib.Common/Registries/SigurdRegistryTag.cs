using System;
using System.Collections.Generic;
using System.Linq;
using Sigurd.Common.Core;
using Sigurd.Common.Extensions;
using Sigurd.Common.Tags;
using Sigurd.Common.Util;

namespace Sigurd.Common.Registries;

internal class SigurdRegistryTag<TValue> : ITag<TValue> where TValue : class
{
    private readonly ITagManager<TValue> _owner;

    public ITagKey<TValue> Key { get; }

    private IHolderSet.Named<TValue>? _holderSet;

    private IList<TValue>? _contents;

    public SigurdRegistryTag(ITagKey<TValue> key, ITagManager<TValue> owner)
    {
        Key = key;
        _owner = owner;
    }

    public IEnumerator<TValue> GetEnumerator() => Contents.GetEnumerator();

    public int Count => _holderSet?.Count ?? 0;

    public bool IsBound => _holderSet is not null;

    public bool Contains(TValue value) => _owner.GetReverseTagOrThrow(value).Contains(this);

    public Optional<TValue> GetRandomElement(Random randomSource) => Contents.GetRandomElement(randomSource);

    private IList<TValue> Contents {
        get {
            if (_holderSet is null)
                return Array.Empty<TValue>();

            return _contents ??= _holderSet.Select(holder => holder.Value).ToList();
        }
    }

    internal void Bind(IHolderSet.Named<TValue>? holderSet)
    {
        _holderSet = holderSet;
        _contents = null;
    }

    internal Optional<IHolderSet.Named<TValue>> HolderSet => Optional<IHolderSet.Named<TValue>>.OfNullable(_holderSet);

    public override string ToString() => $"Tag[key = {Key}, contents = {String.Join(", ", Contents)}]";
}
