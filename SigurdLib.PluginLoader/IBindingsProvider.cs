using System;
using Sigurd.Bus.Api;

namespace Sigurd.PluginLoader;

public interface IBindingsProvider
{
    public Func<IEventBus> SigurdBusSupplier { get; }
}
