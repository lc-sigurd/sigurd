using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using BepInEx;
using SigurdLib.Util.Resources;

namespace SigurdLib.PluginLoader;

public class PluginLoadingContext
{
    private static Type BasePluginType = typeof(BaseUnityPlugin);

    private static readonly ThreadLocal<PluginLoadingContext> Context = new(() => new PluginLoadingContext());

    public static PluginLoadingContext Instance => Context.Value;

    private PluginContainer? _activeContainer;

    public PluginContainer ActiveContainer {
        get => _activeContainer ?? PluginList.Instance.GetPluginContainerByGuidOrThrow(ResourceName.DefaultNamespace);
        internal set => _activeContainer = value;
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
