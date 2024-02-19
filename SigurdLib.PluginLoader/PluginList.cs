using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using SigurdLib.Util;
using SigurdLib.Util.Collections.Generic;

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
    private Dictionary<string, PluginContainer>? _containerByGuid;

    private readonly object _writeLock = new();

    internal IReadOnlyCollection<PluginInfo> OrderedPluginInfos {
        set {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            lock (_writeLock) {
                _loadedPluginIdMap.SetAll(false);
                _containerByGuid = null;

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

    internal IEnumerable<PluginContainer> LoadedPlugins {
        set {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            lock (_writeLock) {
                if (_infoById is null)
                    throw new InvalidOperationException($"Must assign {nameof(OrderedPluginInfos)} before assigning {nameof(LoadedPlugins)}");

                _containerByGuid = new();

                foreach (var container in value) {
                    AddLoadedPlugin(container);
                }

                void AddLoadedPlugin(PluginContainer container)
                {
                    int id;
                    try {
                        id = _infoById.Inverse[container.Info];
                    }
                    catch (KeyNotFoundException exc) {
                        ChainloaderHooks.Logger.LogError($"Can't add {container} to {nameof(PluginList)} as its GUID-Version combination was not recognised.\n{exc}");
                        return;
                    }

                    _loadedPluginIdMap[id] = true;
                    _containerByGuid![container.Guid] = container;
                }
            }
        }
    }

    public Optional<PluginContainer> GetPluginContainerByGuid(string guid)
    {
        if (_containerByGuid is null)
            return Optional<PluginContainer>.None;

        if (_containerByGuid.TryGetValue(guid, out var container))
            return Optional<PluginContainer>.Some(container);
        return Optional<PluginContainer>.None;
    }

    public PluginContainer GetPluginContainerByGuidOrThrow(string guid)
        => GetPluginContainerByGuid(guid)
            .IfNone(() => throw new ArgumentException($"Missing plugin with guid: {guid}"));

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
