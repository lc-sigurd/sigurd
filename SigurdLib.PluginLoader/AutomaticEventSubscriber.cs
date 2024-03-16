using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using Serilog;
using Sigurd.Bus;
using Sigurd.Bus.Api;
using Sigurd.Util;
using Sigurd.Util.Collections.Generic;
using static Sigurd.PluginLoader.SigurdPlugin;

namespace Sigurd.PluginLoader;

/// <summary>
/// Automatic <see cref="EventBus"/> subscriber. Reads <see cref="SigurdPlugin.EventBusSubscriberAttribute"/>
/// annotations and passes the annotated types to the <see cref="Bus"/> defined by the annotation, which
/// defaults to <c>SigurdLib.EventBus</c>.
/// </summary>
internal class AutomaticEventSubscriber
{
    private ILogger _logger;

    private static readonly IEqualityComparer<Type> TypeComparer = new IdentityEqualityComparer<Type>();

    private readonly ConcurrentDictionary<Type, byte> AutoSubscribedTypes = new(TypeComparer);

    public AutomaticEventSubscriber(ILogger logger)
    {
        _logger = logger;
    }

    public void Inject(PluginContainer plugin, Side context)
    {
        var eventBusSubscribers = AccessTools.GetTypesFromAssembly(plugin.Assembly)
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<EventBusSubscriberAttribute>()))
            .Where(pair => pair.Attribute is not null)
            .Where(pair => pair.Attribute.PluginGuid == plugin.Guid);

        foreach (var (registerType, subscriberAttribute) in eventBusSubscribers) {
            AutoSubscribedTypes.TryAdd(registerType, 0);
            if (!subscriberAttribute.Side.HasFlag(context)) continue;

            try {
                _logger.Debug("Auto-subscribing {SubscriberType} to {Bus}", registerType, subscriberAttribute.EventBus);
                GetBus(subscriberAttribute.EventBus).Register(registerType);
            }
            catch (Exception exc) {
                throw new InvalidOperationException($"Failed to auto-subscribe {registerType} to {subscriberAttribute.EventBus}", exc);
            }
        }
    }

    public void WarnOfIgnoredSubscribers()
    {
        var ignoredSubscribers = AccessTools.AllTypes()
            .Select(type => (Type: type, Attribute: type.GetCustomAttribute<EventBusSubscriberAttribute>()))
            .Where(pair => pair.Attribute is not null)
            .Where(pair => !AutoSubscribedTypes.ContainsKey(pair.Type));

        foreach (var (registerType, subscriberAttribute) in ignoredSubscribers) {
            // plugin is not registered at all
            if (!Chainloader.PluginInfos.ContainsKey(subscriberAttribute.PluginGuid)) {
                _logger.Warning(
                    "{SubscriberType} is annotated with {AutoSubscribeAttribute},\n" +
                    "but BepInPlugin with GUID {PluginGuid} is not recognised,\n" +
                    "so it has been ignored.",
                    registerType,
                    typeof(EventBusSubscriberAttribute),
                    subscriberAttribute.PluginGuid
                );
                return;
            }

            // plugin is registered, but the chainloader never attempted to load it
            var maybeContainer = PluginList.Instance.GetPluginContainerByGuid(subscriberAttribute.PluginGuid);
            if (maybeContainer.IsNone) return;

            // plugin either loaded or failed to load
            _logger.Warning(
                "{SubscriberType} is annotated with {AutoSubscribeAttribute},\n" +
                "but is not defined in the same assembly as BepInPlugin with GUID {PluginGuid},\n" +
                "so it has been ignored.",
                registerType,
                typeof(EventBusSubscriberAttribute),
                subscriberAttribute.PluginGuid
            );
        }
    }

    private static IEventBus GetBus(EventBusSubscriberAttribute.Bus bus)
    {
        return bus switch {
            EventBusSubscriberAttribute.Bus.Sigurd => Bindings.SigurdBusSupplier.Invoke(),
            _ => throw new ArgumentOutOfRangeException(nameof(bus), bus, null)
        };
    }
}
