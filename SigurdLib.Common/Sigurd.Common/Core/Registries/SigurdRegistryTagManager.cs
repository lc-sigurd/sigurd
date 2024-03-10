using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sigurd.Common.Core.Resources;
using Sigurd.Common.Core.Tags;
using SigurdLib.Util;
using SigurdLib.Util.Collections.Generic;
using SigurdLib.Util.Collections.ObjectModel;
using SigurdLib.Util.Resources;

namespace Sigurd.Common.Core.Registries;

/// <inheritdoc />
internal class SigurdRegistryTagManager<TValue> : ITagManager<TValue> where TValue : class
{
    private static readonly IEqualityComparer<ITagKey<TValue>> TagKeyComparer = new IdentityEqualityComparer<ITagKey<TValue>>();

    private readonly SigurdRegistry<TValue> _owner;

    private readonly ConcurrentDictionary<ITagKey<TValue>, SigurdRegistryTag<TValue>> _tags = new(TagKeyComparer);

    public SigurdRegistryTagManager(SigurdRegistry<TValue> owner) => _owner = owner;

    public void Bind(IDictionary<ITagKey<TValue>, IHolderSet.Named<TValue>> newTags)
    {
        Parallel.ForEach(_tags.Values, UnbindTag);
        Parallel.ForEach(newTags, BindTag);

        void UnbindTag(SigurdRegistryTag<TValue> tag) => tag.Bind(null);

        void BindTag(KeyValuePair<ITagKey<TValue>, IHolderSet.Named<TValue>> tagEntry)
            => GetTagInternal(tagEntry.Key).Bind(tagEntry.Value);
    }

    /// <inheritdoc />
    public ITag<TValue> GetTag(ITagKey<TValue> key) => GetTagInternal(key);

    internal SigurdRegistryTag<TValue> GetTagInternal(ITagKey<TValue> key)
    {
        if (key is null)
            throw new ArgumentException("Cannot lookup `null` tag key");

        return _tags.GetOrAdd(key, TagFactory);

        SigurdRegistryTag<TValue> TagFactory(ITagKey<TValue> factoryKey) => new(factoryKey, this);
    }

    /// <inheritdoc />
    public Optional<IReverseTag<TValue>> GetReverseTag(TValue value) => _owner.GetDelegate(value)
        .Select(holder => holder as IReverseTag<TValue>);

    /// <inheritdoc />
    public bool IsKnownTagKey(ITagKey<TValue> key) => _tags.TryGetValue(key, out var tag) && tag.IsBound;

    /// <inheritdoc />
    public IEnumerator<ITag<TValue>> GetEnumerator() => Tags.GetEnumerator();

    internal IEnumerable<SigurdRegistryTag<TValue>> Tags => _tags.Values;

    /// <inheritdoc />
    public IEnumerable<ITagKey<TValue>> TagKeys => new ReadOnlyAssemblage<ITagKey<TValue>>(_tags.Keys);

    /// <inheritdoc />
    public ITagKey<TValue> CreateTagKey(ResourceName name)
    {
        if (name is null)
            throw new ArgumentException("Cannot create tag key with `null` name");

        return ITagKey.Create(_owner.RegistryKey, name);
    }
}
