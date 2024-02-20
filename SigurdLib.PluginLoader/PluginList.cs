using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using SigurdLib.Util;
using SigurdLib.Util.Collections.Generic;
using SigurdLib.Util.Extensions;

namespace SigurdLib.PluginLoader;

/// <summary>
/// Utility class that keeps track of <see cref="PluginContainer"/> instances and exposes methods for their
/// enumeration and lookup.
/// </summary>
public class PluginList
{
    private static readonly IEqualityComparer<PluginInfo> InfoComparer = new MetadataPluginInfoComparer();

    private static PluginList? _instance;

    /// <summary>
    /// The global <see cref="PluginList"/> instance.
    /// </summary>
    public static PluginList Instance => _instance ??= new PluginList();

    // If someone manages to install 4095 plugins, they have bigger problems than this
    private readonly BitArray _loadedPluginIdMap = new BitArray(0xFFF);

    private BiDictionary<int, PluginInfo>? _infoById;
    private Dictionary<string, PluginContainer> _containerByGuid = new();

    private readonly object _writeLock = new();

    internal IReadOnlyCollection<PluginInfo> OrderedPluginInfos {
        set {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            lock (_writeLock) {
                _loadedPluginIdMap.SetAll(false);
                _containerByGuid.Clear();

                _infoById = new BiDictionary<int, PluginInfo>(
                    value
                        .Select((info, index) => (Info: info, Index: index))
                        .ToDictionary(
                            item => item.Index,
                            item => item.Info
                        ),
                    valueComparer: InfoComparer
                );
            }
        }
    }

    internal void AddLoadingPluginContainer(PluginContainer container)
    {
        if (container is null)
            throw new ArgumentNullException(nameof(container));
        if (_infoById is null)
            throw new InvalidOperationException($"Must assign {nameof(OrderedPluginInfos)} before trying to add loading plugin containers");

        lock (_writeLock) {
            try {
                _ = _infoById.Inverse[container.Info];
            }
            catch (KeyNotFoundException exc) {
                throw new ArgumentException($"Can't add {container} to {nameof(PluginList)} as its GUID-Version combination was not recognised", nameof(container), exc);
            }

            _containerByGuid[container.Guid] = container;
        }
    }

    internal void SetPluginLoaded(PluginContainer container)
    {
        if (container is null)
            throw new ArgumentNullException(nameof(container));
        if (_infoById is null)
            throw new InvalidOperationException($"Must assign {nameof(OrderedPluginInfos)} before trying to set loaded plugin containers");

        lock (_writeLock) {
            int id;
            try {
                id = _infoById.Inverse[container.Info];
            }
            catch (KeyNotFoundException exc) {
                throw new ArgumentException($"Can't mark {container} as loaded in {nameof(PluginList)} as its GUID-Version combination was not recognised", nameof(container), exc);
            }

            PluginContainer cachedContainer;
            try {
                cachedContainer = _containerByGuid[container.Guid];
            }
            catch (KeyNotFoundException exc) {
                throw new ArgumentException($"Can't mark {container} as loaded in {nameof(PluginList)} as its container has not been added", nameof(container), exc);
            }

            if (!ReferenceEquals(container, cachedContainer))
                throw new ArgumentException($"Provided {container} does not match cached {cachedContainer}", nameof(container));

            _loadedPluginIdMap[id] = true;
        }
    }

    public PluginContainer GetPluginContainerByGuidOrThrow(string guid)
    {
        if (guid is null)
            throw new ArgumentNullException(nameof(guid));
        if (_infoById is null)
            throw new InvalidOperationException($"Must assign {nameof(OrderedPluginInfos)} before trying to get plugin containers");

        return _containerByGuid[guid];
    }

    public Optional<PluginContainer> GetPluginContainerByGuid(string guid)
    {
        try {
            return Optional.Some(GetPluginContainerByGuidOrThrow(guid));
        }
        catch (Exception exc) when (exc is ArgumentNullException or InvalidOperationException or KeyNotFoundException) {
            return Optional<PluginContainer>.None;
        }
    }

    public PluginContainer GetPluginContainerByIdOrThrow(int id)
    {
        if (_infoById is null)
            throw new InvalidOperationException($"Must assign {nameof(OrderedPluginInfos)} before trying to get plugin containers");

        var info = _infoById[id];
        return _containerByGuid[info.Metadata.GUID];
    }

    public Optional<PluginContainer> GetPluginContainerById(int id)
    {
        try {
            return Optional.Some(GetPluginContainerByIdOrThrow(id));
        }
        catch (Exception exc) when (exc is ArgumentNullException or InvalidOperationException or KeyNotFoundException) {
            return Optional<PluginContainer>.None;
        }
    }

    public IEnumerable<PluginContainer> PluginContainersInOrder {
        get {
            var enumerator = new Enumerator(this);
            while (enumerator.MoveNext()) {
                yield return enumerator.Current;
            }
        }
    }

    /// <inheritdoc />
    public struct Enumerator : IEnumerator<PluginContainer>
    {
        private readonly PluginList _list;
        private int _currentId = -1;

        internal Enumerator(PluginList list)
        {
            _list = list;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            _currentId = _list._loadedPluginIdMap.NextSetBitIndex(_currentId + 1);
            Debug.Assert(_currentId == -1 || Current is not null);
            return _currentId != -1;
        }

        /// <inheritdoc />
        public void Reset() => _currentId = -1;

        /// <inheritdoc />
        public PluginContainer Current {
            get {
                if (_list._infoById is null)
                    throw new InvalidOperationException($"Must assign {nameof(OrderedPluginInfos)} before trying to enumerate {nameof(PluginList)}");

                try {
                    return _list._containerByGuid[_list._infoById[_currentId].Metadata.GUID];
                }
                catch (KeyNotFoundException exc) {
                    if (_currentId < 0)
                        throw new InvalidOperationException("Cannot retrieve element before advancing position from initial state", exc);
                    throw new InvalidOperationException($"ID not found, but {nameof(_list._loadedPluginIdMap)} shows ID should be assigned", exc);
                }
            }
        }

        /// <inheritdoc />
        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() { }
    }

    private class MetadataPluginInfoComparer : IEqualityComparer<PluginInfo>
    {
        private static readonly IEqualityComparer<BepInPlugin> PluginMetadataComparer = new BepInPluginComparer();

        public bool Equals(PluginInfo x, PluginInfo y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return PluginMetadataComparer.Equals(x.Metadata, y.Metadata);
        }

        public int GetHashCode(PluginInfo obj) => PluginMetadataComparer.GetHashCode(obj.Metadata);
    }

    private class BepInPluginComparer : IEqualityComparer<BepInPlugin>
    {
        public bool Equals(BepInPlugin x, BepInPlugin y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.GUID == y.GUID && x.Version == y.Version;
        }

        public int GetHashCode(BepInPlugin obj) => HashCode.Combine(obj.GUID, obj.Version);
    }
}
