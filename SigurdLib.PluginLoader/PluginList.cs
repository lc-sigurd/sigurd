using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using SigurdLib.Util;
using SigurdLib.Util.Collections.Generic;

namespace SigurdLib.PluginLoader;

public class PluginList
{
    private static readonly IEqualityComparer<PluginInfo> InfoComparer = new MetadataPluginInfoComparer();

    private static PluginList? _instance;

    public static PluginList Instance => _instance ??= new PluginList();

    private ConcurrentDictionary<string, PluginContainer>? _containersByGuid;

    internal IEnumerable<PluginContainer> LoadedPlugins {
        set {
            _containersByGuid = new(
                value.ToDictionary(
                    item => item.Info.Metadata.GUID
                )
            );
        }
    }

    public Optional<PluginContainer> GetPluginContainerByGuid(string guid)
    {
        if (_containersByGuid is null)
            return Optional<PluginContainer>.None;

        if (_containersByGuid.TryGetValue(guid, out var container))
            return Optional<PluginContainer>.Some(container);
        return Optional<PluginContainer>.None;
    }

    public PluginContainer GetPluginContainerByGuidOrThrow(string guid)
        => GetPluginContainerByGuid(guid)
            .IfNone(() => throw new ArgumentException($"Missing plugin with guid: {guid}"));

    public Optional<PluginContainer> GetPluginContainerByType(Type pluginType)
    {
        var maybePluginInfo = Chainloader.PluginInfos.Values
            .FirstOrDefault(info => info.Instance.GetType() == pluginType);

        if (maybePluginInfo is null) return Optional<PluginContainer>.None;

        return GetPluginContainerByGuid(maybePluginInfo.Metadata.GUID);
    }

    public PluginContainer GetPluginContainerByTypeOrThrow(Type pluginType)
        => GetPluginContainerByType(pluginType)
            .IfNone(() => throw new ArgumentException($"Missing plugin of type: {pluginType}"));

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
