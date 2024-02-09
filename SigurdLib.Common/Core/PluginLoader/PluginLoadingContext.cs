using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BepInEx;
using Sigurd.Common.Core.Resources;

namespace Sigurd.Common.Core.PluginLoader;

public class PluginLoadingContext
{
    private static Type BasePluginType = typeof(BaseUnityPlugin);

    private static readonly ThreadLocal<PluginLoadingContext> Context = new(() => new PluginLoadingContext());

    public static PluginLoadingContext Instance => Context.Value;

    private PluginContainer? _activeContainer;

    public PluginContainer ActiveContainer {
        get => _activeContainer ?? PluginList.Instance.GetPluginContainerByGuidOrThrow(ResourceName.DefaultNamespace);
        set => _activeContainer = value;
    }

    public string ActiveNamespace => _activeContainer?.Namespace ?? ResourceName.DefaultNamespace;

    public static PluginContainer Infer()
    {
        var pluginType = new StackTrace()
            .GetFrames()
            ?.Select(frame => frame.GetMethod().ReflectedType)
            .FirstOrDefault(type => BasePluginType.IsAssignableFrom(type));

        if (pluginType is null)
            throw new InvalidOperationException("Could not discern calling plugin.");

        return PluginList.Instance.GetPluginContainerByTypeOrThrow(pluginType);
    }
}
