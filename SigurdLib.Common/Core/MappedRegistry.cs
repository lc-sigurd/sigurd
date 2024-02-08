using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BepInEx.Logging;
using Sigurd.Common.Extensions;
using Sigurd.Common.Resources;
using Sigurd.Common.Tags;
using Generic = System.Collections.Generic;

namespace Sigurd.Common.Core;

public interface IWithPluginLogger
{
    private static readonly ManualLogSource Logger = Plugin.Log;
}

public interface MappedRegistry
{
    protected static readonly Generic.HashSet<ResourceName> KnownRegistriesBacker = new();

    public static IImmutableSet<ResourceName> KnownRegistries => KnownRegistriesBacker.ToImmutableHashSet();
}

public class MappedRegistry<TValue>
    where TValue : class
{
    private readonly List<IHolder.Reference<TValue>> _byId = new(256);

    private readonly Dictionary<TValue, int> _toId = new(new IdentityEqualityComparer<TValue>());

    private readonly Dictionary<ResourceName, IHolder.Reference<TValue>> _byLocation = new();

    private readonly Dictionary<IResourceKey<TValue>, IHolder.Reference<TValue>> _byKey = new();

    private readonly Dictionary<TValue, IHolder.Reference<TValue>> _byValue = new(new IdentityEqualityComparer<TValue>());

    private readonly Dictionary<ITagKey<TValue>, IHolderSet.Named<TValue>> _tags = new(new IdentityEqualityComparer<ITagKey<TValue>>());

    private ImmutableList<IHolder.Reference<TValue>>? _holdersInOrder;

    private int _nextAvailableId;

    private readonly IHolderLookup.RegistryLookup<TValue> _lookup;

    /// <inheritdoc />
    public IResourceKey<IRegistrar<TValue>> Key { get; private init; }

    public MappedRegistry(ResourceKey<IRegistrar<TValue>> key)
    {
        _nextAvailableId = 0;

        _lookup = new IHolderLookup.RegistryLookup.Delegate<TValue> {
            Key = key,
            ResourceKeyGet = null,
            ElementsGet = null,
            TagKeyGet = null,
            TagsGet = null,
        };

        Key = key;
    }

    /// <inheritdoc />
    public override string ToString() => $"Registry[{Key}]";

    protected virtual void InvalidateCaches()
    {
        _holdersInOrder = null;
    }

    private ImmutableList<IHolder.Reference<TValue>> HoldersInOrder => _holdersInOrder ??= _byId.Where(holder => holder is not null).ToImmutableList();

    protected virtual void ValidateWrite() { }

    protected virtual void ValidateWrite(ResourceKey<TValue> key) { }

    protected void MarkKnown() => MappedRegistry.KnownRegistriesBacker.Add(Key.Name);

    /// <inheritdoc />
    public IHolder.Reference<TValue> Register(ResourceKey<TValue>? key, TValue? value)
    {
        MarkKnown();
        ValidateWrite();

        if (key is null)
            throw new ArgumentException("Tried to register a mapping with 'null' key");
        if (value is null)
            throw new ArgumentException("Tried to register a mapping with 'null' value");

        if (_byLocation.ContainsKey(key.Name))
            throw new InvalidOperationException($"Cannot add duplicate key '{key}' to registry");
        if (_byValue.ContainsKey(value))
            throw new InvalidOperationException($"Cannot add duplicate value '{value}' to registry");

        IHolder.Reference<TValue> reference = _byKey.ComputeIfAbsent(key, _ => new IHolder.Reference<TValue>(HolderOwner, key, value));
        reference.Value = value;

        _byKey[key] = reference;
        _byLocation[key.Name] = reference;
        _byValue[value] = reference;
        _toId[value] = _byId.Count;
        _byId.Add(reference);

        InvalidateCaches();
        return reference;
    }

    /// <inheritdoc />
    public ResourceName? GetName(TValue? value)
    {
        if (value is null) return null;
        return _byValue[value]?.Key.Name;
    }

    /// <inheritdoc />
    public Optional<IResourceKey<TValue>> GetKey(TValue? value)
    {
        if (value is null) return Optional<IResourceKey<TValue>>.None;
        var maybeKey = _byValue[value];
        if (maybeKey is null) return Optional<IResourceKey<TValue>>.None;
        return Optional<IResourceKey<TValue>>.Some(maybeKey.Key);
    }

    protected int GetId(TValue? value)
    {
        if (value is null) return -1;
        return _toId[value];
    }

    /// <inheritdoc />
    public TValue? Get(ResourceName? name)
    {
        if (name is null) return null;
        return _byLocation[name]?.Value;
    }

    /// <inheritdoc />
    public TValue? Get(IResourceKey<TValue>? key)
    {
        if (key is null) return null;
        return _byKey[key]?.Value;
    }

    protected TValue? Get(int id)
    {
        if (id < 0 || id >= _byId.Count) return null;
        return _byId[id]?.Value;
    }

    /// <inheritdoc />
    public bool ContainsName(ResourceName name) => _byLocation.ContainsKey(name);

    /// <inheritdoc />
    public bool ContainsKey(IResourceKey<TValue> key) => _byKey.ContainsKey(key);

    protected Optional<IHolder.Reference<TValue>> GetHolder(int id)
    {
        if (id < 0 || id >= _byId.Count) return Optional<IHolder.Reference<TValue>>.None;
        var maybeHolder = _byId[id];
        if (maybeHolder is null) return Optional<IHolder.Reference<TValue>>.None;
        return Optional<IHolder.Reference<TValue>>.Some(maybeHolder);
    }

    /// <inheritdoc />
    public Optional<IHolder.Reference<TValue>> GetHolder(IResourceKey<TValue> key)
    {
        if (_byKey.TryGetValue(key, out var holder))
            return Optional<IHolder.Reference<TValue>>.Some(holder);

        return Optional<IHolder.Reference<TValue>>.None;
    }

    /// <inheritdoc />
    public IHolder<TValue> WrapAsHolder(TValue value)
    {
        if (_byValue.TryGetValue(value, out var reference))
            return reference;
        return new IHolder.Direct<TValue>(value);
    }

    /// <inheritdoc />
    public int Count => _byKey.Count;

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => HoldersInOrder
        .Select(holder => holder.Value)
        .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<TValue>).GetEnumerator();

    /// <inheritdoc />
    public ISet<ResourceName> NameSet => _byLocation.Keys.ToImmutableHashSet();

    /// <inheritdoc />
    public ISet<IResourceKey<TValue>> KeySet => _byKey.Keys.ToImmutableHashSet();

    /// <inheritdoc />
    public ISet<KeyValuePair<IResourceKey<TValue>, TValue>> EntrySet => _byKey
        .Select(pair => new KeyValuePair<IResourceKey<TValue>, TValue>(pair.Key, pair.Value.Value)).ToImmutableHashSet();

    /// <inheritdoc />
    public IEnumerable<IHolder.Reference<TValue>> Holders => HoldersInOrder;

    /// <inheritdoc />
    public IEnumerable<KeyValuePair<ITagKey<TValue>, IHolderSet.Named<TValue>>> Tags => _tags;

    /// <inheritdoc />
    public IEnumerable<ITagKey<TValue>> TagKeys => _tags.Keys;

    /// <inheritdoc />
    public Optional<IHolderSet.Named<TValue>> GetTag(ITagKey<TValue> tagKey)
    {
        var maybeTag = _tags[tagKey];
        if (maybeTag is null) return Optional<IHolderSet.Named<TValue>>.None;
        return Optional<IHolderSet.Named<TValue>>.Some(maybeTag);
    }

    /// <inheritdoc />
    public void ResetTags()
    {
        foreach (var (_, value) in _tags) {
            if (value is null) continue;
            value.Contents = Array.Empty<IHolder<TValue>>();
        }

        foreach (var (_, value) in _byKey) {
            if (value is null) continue;
            value.Tags = Array.Empty<ITagKey<TValue>>();
        }
    }

    /// <inheritdoc />
    public void BindTags(Dictionary<ITagKey<TValue>, List<IHolder<TValue>>> tagMap)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public bool IsEmpty() => _byKey.Count == 0;

    /// <inheritdoc />
    public IHolderOwner<TValue> HolderOwner => _lookup;

    /// <inheritdoc />
    public IHolderLookup.RegistryLookup<TValue> AsLookup() => _lookup;
}
