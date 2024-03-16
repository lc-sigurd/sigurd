using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Sigurd.Bus.Api;

namespace Sigurd.PluginLoader;

public class Bindings
{
    public static Bindings Instance { get; } = new();

    private readonly IBindingsProvider _provider;

    private Bindings()
    {
        var providers = ServiceLoad<IBindingsProvider>();
        try {
            _provider = providers.Single();
        }
        catch (Exception exc) {
            throw new InvalidOperationException("Could not find bindings provider.", exc);
        }
    }

    public static Func<IEventBus> SigurdBusSupplier => Instance._provider.SigurdBusSupplier;

    private static IEnumerable<T> ServiceLoad<T>()
    {
        var it = typeof(T);

        return AccessTools.AllTypes()
            .Where(type => type != it && !type.IsInterface)
            .Where(it.IsAssignableFrom)
            .Select(type => (T)Activator.CreateInstance(type));
    }
}
