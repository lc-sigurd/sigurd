using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using BepInEx.Logging;
using JetBrains.Annotations;
using Sigurd.Common.Collections.Generic;
using Sigurd.Common.Collections.ObjectModel;
using Sigurd.Common.Core.PluginLoader;
using Sigurd.Common.Core.Resources;
using Sigurd.Common.Core.Tags;
using Sigurd.Common.Extensions;
using Sigurd.Common.Util;

namespace Sigurd.Common.Core.Registries;

internal abstract class SigurdRegistry
{
    protected static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource($"{Plugin.Guid}/Registries");
}

internal class SigurdRegistry<TValue> : SigurdRegistry, IRegistryInternal<TValue>, IRegistryModifiable<TValue>
    where TValue : class
{
    private static readonly IEqualityComparer<TValue> ByReferenceValueComparer = new IdentityEqualityComparer<TValue>();

    private const int InitialCapacity = 16;

    private readonly BiDictionary<int, TValue> _ids = new(InitialCapacity, valueComparer: ByReferenceValueComparer);

    private readonly BiDictionary<ResourceName, TValue> _names = new(InitialCapacity, valueComparer: ByReferenceValueComparer);

    private readonly BiDictionary<IResourceKey<TValue>, TValue> _keys = new(InitialCapacity, valueComparer: ByReferenceValueComparer);

    private readonly BiDictionary<EntryOwner, TValue> _owners = new(InitialCapacity, valueComparer: ByReferenceValueComparer);

    private ICollection<ResourceName>? _readonlyNameCollection;

    private ICollection<TValue>? _readonlyValueCollection;

    private ICollection<KeyValuePair<IResourceKey<TValue>, TValue>>? _readonlyEntryCollection;

    public event EventHandler<IRegistry.AddEventArgs<TValue>>? OnAdd;

    public event EventHandler<ISigurdRegistryModifiable.ClearEventArgs<TValue>>? OnClear;

    public event EventHandler<IRegistry.CreateEventArgs<TValue>>? OnCreate;

    public event EventHandler<IRegistry.ValidateEventArgs<TValue>>? OnValidate;

    public event EventHandler<IRegistry.BakeEventArgs<TValue>>? OnBake;

    private readonly BitArray _availabilityMap;

    private readonly Dictionary<int, IHolder.Reference<TValue>> _delegatesById = new();

    private readonly Dictionary<ResourceName, IHolder.Reference<TValue>> _delegatesByName = new();

    private readonly Dictionary<TValue, IHolder.Reference<TValue>> _delegatesByValue = new(ByReferenceValueComparer);

    private readonly IHolderLookup.RegistryLookup<TValue> _delegateLookup;

    private readonly SigurdRegistryTagManager<TValue>? _tagManager;

    private readonly int _minId;

    private readonly int _maxId;

    private readonly bool _isModifiable;

    private bool _isFrozen = false;

    public ResourceName RegistryName { get; }

    public IResourceKey<IRegistrar<TValue>> RegistryKey { get; }

    public SigurdRegistry(ResourceName name, RegistryConfiguration<TValue> configuration)
    {
        RegistryName = name;
        RegistryKey = IResourceKey.CreateRegistryKey<TValue>(name);
        _minId = configuration.MinId;
        _maxId = configuration.MaxId;
        _availabilityMap = new BitArray(Math.Min(_maxId + 1, 0x0FFF));
        _delegateLookup = new RegistryLookupImpl(this);

        OnCreate += configuration.CreateCallback;
        OnAdd += configuration.AddCallback;
        OnClear += configuration.ClearCallback;
        OnValidate += configuration.ValidateCallback;
        OnBake += configuration.BakeCallback;

        _isModifiable = configuration.AllowModifications;
        IsTaggable = configuration.Taggable;
        _tagManager = configuration.Taggable ? new SigurdRegistryTagManager<TValue>(this) : null;

        OnCreate?.Invoke(this, new IRegistry.CreateEventArgs<TValue>());
    }

    #region Utility/Helpers

    private void LogRegistry(LogLevel level, string message)
    {
        Logger.Log(level, $"Registry {RegistryName}: {message}");
    }

    /// <summary>
    /// Check a name for a namespace prefix. If not present, infer it from the current active plugin container.
    /// </summary>
    /// <param name="name">The name or <see cref="ResourceName"/> string</param>
    /// <returns>The <see cref="ResourceName"/> with provided or inferred namespace</returns>
    private ResourceName ValidatePrefix(string? name)
    {
        string activeNamespace = PluginLoadingContext.Instance.ActiveNamespace;
        (var providedNamespace, name) = ResourceName.Decompose(name, activeNamespace);

        if (!providedNamespace.Equals(activeNamespace)) {
            Logger.LogDebug($"Mod `{activeNamespace}` attempting to register `{name}` to the namespace `{providedNamespace}`. This could be intentional, but likely indicates the presence of a registration event listener without a mod GUID.");
        }

        return new ResourceName(providedNamespace, name);
    }

    #endregion

    #region Registration

    public void Register(string key, TValue value)
    {
        Register(ValidatePrefix(key), value);
    }

    public void Register(ResourceName key, TValue value)
    {
        Add(-1, key, value);
    }

    public void Register(int id, ResourceName key, TValue value)
    {
        Add(id, key, value, key.Namespace);
    }

    private int Add(int id, ResourceName name, TValue value)
    {
        string ownerNamespace = PluginLoadingContext.Instance.ActiveNamespace;
        return Add(id, name, value, ownerNamespace);
    }

    private int Add(int id, ResourceName name, TValue value, string ownerNamespace)
    {
        if (name is null)
            throw new ArgumentException($"Can't use a `null` name for the registry; value {value}.");
        if (value is null)
            throw new ArgumentException($"Can't add `null` value to the registry; name {name}.");

        int selectedId = id;
        if (selectedId < 0 || _availabilityMap.Get(selectedId))
            selectedId = _availabilityMap.NextClearBitIndex(_minId);

        if (selectedId > _maxId)
            throw new InvalidOperationException($"Invalid id {selectedId} - maximum ID range exceeded.");

        var existingValue = GetValue(name);

        if (ReferenceEquals(existingValue, value)) {
            LogRegistry(LogLevel.Warning, $"The value {value} has been registered twice for the same name {name}.");
            return GetId(value);
        }

        if (existingValue is not null)
            throw new ArgumentException($"The name {name} has been registered twice, for {existingValue} and {value}.");

        if (_ids.ContainsValue(value)) {
            throw new ArgumentException($"The value {value} has been registered twice, using the names {GetKey(value)} and {name}.");
        }

        if (IsLocked)
            throw new InvalidOperationException($"It is too late to register value {value} (name {name}).");

        IResourceKey<TValue> resourceKey = IResourceKey.Create(RegistryKey, name);
        _names[name] = value;
        _keys[resourceKey] = value;
        _ids[selectedId] = value;
        _availabilityMap[selectedId] = true;
        _owners[new EntryOwner(ownerNamespace, resourceKey)] = value;

        if (IsTaggable) BindDelegate(resourceKey, value);

        OnAdd?.Invoke(this, new IRegistry.AddEventArgs<TValue> {
            Id = selectedId,
            Key = resourceKey,
            Value = value,
        });

        return selectedId;
    }

    #endregion

    #region Content Queries

    public bool ContainsKey(ResourceName? key)
    {
        if (key is null)
            throw new ArgumentException("Cannot lookup a `null` key in the registry.");

        return _names.ContainsKey(key);
    }

    public bool ContainsValue(TValue? value)
    {
        if (value is null)
            throw new ArgumentException("Cannot lookup a `null` value in the registry.");

        return _names.ContainsValue(value);
    }

    public int Count => _names.Count;

    #endregion

    #region Content Retrieval

    public TValue? GetValue(ResourceName? key)
    {
        if (key is null) return null;
        return _names.TryGetValue(key, out var value) ? value : null;
    }

    public TValue? GetValue(int id) => _ids.TryGetValue(id, out var value) ? value : null;

    public ResourceName? GetKey(TValue? value) => GetResourceKey(value)
        .Select(resourceKey => resourceKey.Name)
        .ValueUnsafe;

    public Optional<IResourceKey<TValue>> GetResourceKey(TValue? value)
    {
        if (value is null)
            return Optional<IResourceKey<TValue>>.None;

        return GetOwner(value).Select(owner => owner.Key);
    }

    private Optional<EntryOwner> GetOwner(TValue value)
    {
        if (_owners.TryGetKey(value, out var owner))
            return Optional<EntryOwner>.Some(owner);

        return Optional<EntryOwner>.None;
    }

    public int GetId(TValue value)
    {
        if (_ids.TryGetKey(value, out var id))
            return id;

        return -1;
    }

    public int GetId(ResourceName key)
    {
        if (_names.TryGetValue(key, out var value))
            return GetId(value);

        return -1;
    }

    #endregion

    #region Enumeration

    public ICollection<ResourceName> Keys => _readonlyNameCollection ??= _names.Keys.AsReadOnly();

    public ICollection<TValue> Values => _readonlyValueCollection ??= _names.Values.AsReadOnly();

    public ICollection<KeyValuePair<IResourceKey<TValue>, TValue>> Entries => _readonlyEntryCollection ??= _keys.AsReadOnly();

    public IEnumerator<TValue> GetEnumerator() => new RegistryEnumerator(this);

    #endregion

    #region Tags

    [MemberNotNullWhen(true, nameof(Tags), nameof(_tagManager))]
    public bool IsTaggable { get; }

    public ITagManager<TValue>? Tags => _tagManager;

    public void BindTags(IDictionary<ITagKey<TValue>, IHolderSet.Named<TValue>> tags)
    {
        if (!IsTaggable) return;
        _tagManager.Bind(tags);
    }

    #endregion

    #region Delegates

    public IHolderOwner<TValue> HolderOwner => _delegateLookup;

    public IHolderLookup.RegistryLookup<TValue> AsLookup => _delegateLookup;

    private IHolder.Reference<TValue> BindDelegate(IResourceKey<TValue> resourceKey, TValue value)
    {
        var id = GetId(value);

        IHolder.Reference<TValue> @delegate = _delegatesByName.ComputeIfAbsent(
            resourceKey.Name,
            _ => new IHolder.Reference<TValue>(_delegateLookup, resourceKey)
        );

        @delegate.Key = resourceKey;
        @delegate.Value = value;

        _delegatesByValue[value] = @delegate;
        _delegatesById[id] = @delegate;
        return @delegate;
    }

    public void ResetDelegates()
    {
        if (!IsTaggable) return;

        foreach (var (resourceKey, value) in _keys) {
            BindDelegate(resourceKey, value);
        }
    }

    public Optional<IHolder.Reference<TValue>> GetDelegate(IResourceKey<TValue> key)
    {
        if (key is null)
            throw new ArgumentException("Cannot lookup a `null` key in the registry.");

        return GetDelegate(key.Name);
    }

    public Optional<IHolder.Reference<TValue>> GetDelegate(ResourceName key)
    {
        if (key is null)
            throw new ArgumentException("Cannot lookup a `null` name in the registry.");

        if (_delegatesByName.TryGetValue(key, out var @delegate))
            return Optional.Some(@delegate);

        return Optional<IHolder.Reference<TValue>>.None;
    }

    public Optional<IHolder.Reference<TValue>> GetDelegate(TValue value)
    {
        if (value is null)
            throw new ArgumentException("Cannot lookup a `null` value in the registry.");

        if (_delegatesByValue.TryGetValue(value, out var @delegate))
            return Optional.Some(@delegate);

        return Optional<IHolder.Reference<TValue>>.None;
    }

    public Optional<IHolder.Reference<TValue>> GetDelegate(int id)
    {
        if (id < _minId || _maxId < id)
            throw new ArgumentException($"Cannot lookup ID {id} as it is out of the allowed bounds [{_minId}, {_maxId}]");

        if (_delegatesById.TryGetValue(id, out var @delegate))
            return Optional.Some(@delegate);

        return Optional<IHolder.Reference<TValue>>.None;
    }

    #endregion

    #region Modification/State

    /// <summary>
    /// Used to control when this registry can be modified.
    /// Users should only ever register things in the <see cref="" /> events.
    /// <!-- todo! -->
    /// </summary>
    public bool IsLocked {
        get => _isFrozen;
        set => _isFrozen = value;
    }

    public void Clear()
    {
        if (!_isModifiable)
            throw new InvalidOperationException("Cannot clear a non-modifiable Sigurd registry.");

        if (IsLocked)
            throw new InvalidOperationException("It is too late to clear the registry.");

        OnClear?.Invoke(this, new ISigurdRegistryModifiable.ClearEventArgs<TValue>());

        _ids.Clear();
        _names.Clear();
        _keys.Clear();
        _availabilityMap.SetAll(false);

        _delegatesByName.Clear();
        _delegatesByValue.Clear();
        _delegatesById.Clear();
    }

    public TValue? Remove(ResourceName? key)
    {
        if (key is null)
            throw new ArgumentException("Cannot remove a `null` name from the registry.");

        if (!_names.Remove(key, out var value))
            return null;

        if (!_keys.RemoveValue(value))
            throw new InvalidOperationException($"Removed entry for value {value} has no associated resource key. Strange happenings are afoot.");

        if (!_ids.RemoveValue(value))
            throw new InvalidOperationException($"Removed entry for value {value} has no associated ID. Strange happenings are afoot.");

        return value;
    }

    #endregion

    #region Lifetime

    void ValidateContent()
    {
        foreach (var value in this) {
            ValidateValue(value);
        }
    }

    void ValidateValue(TValue value)
    {
        int id = GetId(value);
        var name = GetKey(value);

        if (id < 0)
            throw new InvalidOperationException($"Entry for value {value} has no associated ID.");

        if (name is null)
            throw new InvalidOperationException($"Entry for value {value}, id {id} has no associated name.");

        if (id > _maxId)
            throw new InvalidOperationException($"Entry for value {value}, name {name} is associated with ID {id} which exceeds the maximum permitted ID {_maxId}.");

        {
            TValue? otherValue = GetValue(id);
            if (!ReferenceEquals(otherValue, value))
                throw new InvalidOperationException($"Entry for value {value}, name {name} is associated with ID {id}, but that ID is associated with value {otherValue}.");

            otherValue = GetValue(name);
            if (!ReferenceEquals(otherValue, value))
                throw new InvalidOperationException($"Entry for value {value}, ID {id} is associated with name {name}, but that name is associated with value {otherValue}");
        }

        {
            int otherId = GetId(name);
            if (otherId != id)
                throw new InvalidOperationException($"Entry for ID {id} is associated with name {name}, but that name is associated with ID {otherId}.");
        }

        OnValidate?.Invoke(this, new IRegistry.ValidateEventArgs<TValue> {
            Id = id,
            Name = name,
            Value = value,
        });
    }

    void Bake()
    {
        OnBake?.Invoke(this, new IRegistry.BakeEventArgs<TValue>());
    }

    #endregion

    [UsedImplicitly]
    private readonly record struct EntryOwner(string OwnerNamespace, IResourceKey<TValue> Key);

    private class RegistryLookupImpl : IHolderLookup.RegistryLookup<TValue>
    {
        private SigurdRegistry<TValue> _registry;

        public RegistryLookupImpl(SigurdRegistry<TValue> registry) => _registry = registry;

        public IResourceKey<IRegistrar<TValue>> Key => _registry.RegistryKey;

        public Optional<IHolder.Reference<TValue>> Get(IResourceKey<TValue> resourceKey) => _registry.GetDelegate(resourceKey);

        public Optional<IHolderSet.Named<TValue>> Get(ITagKey<TValue> tagKey)
        {
            if (!_registry.IsTaggable)
                throw new InvalidOperationException($"Cannot lookup TagKey {tagKey} in non-taggable registry.");

            return _registry._tagManager!
                .GetTagInternal(tagKey)
                .HolderSet;
        }

        public IEnumerable<IHolder.Reference<TValue>> Elements => _registry._delegatesByName.Values;

        public IEnumerable<IHolderSet.Named<TValue>> Tags {
            get {
                if (!_registry.IsTaggable)
                    throw new InvalidOperationException("Cannot retrieve tags for non-taggable registry.");

                return _registry._tagManager!
                    .Tags
                    .Where(tag => tag.IsBound)
                    .Select(tag => tag.HolderSet.ValueUnsafe!);
            }
        }
    }

    private struct RegistryEnumerator : IEnumerator<TValue>
    {
        private readonly SigurdRegistry<TValue> _registry;
        private int _currentId = -1;

        internal RegistryEnumerator(SigurdRegistry<TValue> registry)
        {
            _registry = registry;
        }

        public bool MoveNext()
        {
            _currentId = _registry._availabilityMap.NextSetBitIndex(_currentId + 1);
            Debug.Assert(Current is not null);
            return _currentId != -1;
        }

        public void Reset() => _currentId = -1;

        public TValue Current {
            get {
                try {
                    return _registry._ids[_currentId];
                }
                catch (KeyNotFoundException e) {
                    if (_currentId < 0)
                        throw new InvalidOperationException("Cannot retrieve element before advancing position from initial state.");
                    throw new InvalidOperationException("ID not found, but availabilityMap shows ID should be assigned.", e);
                }
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}



