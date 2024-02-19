using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using SigurdLib.Util;

namespace SigurdLib.PluginLoader;

public class PluginList
{
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
}
