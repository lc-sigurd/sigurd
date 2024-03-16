using System;
using Sigurd.Bus.Api;
using Sigurd.PluginLoader;

namespace Sigurd.Common;

internal sealed class SigurdBindings : IBindingsProvider
{
    public Func<IEventBus> SigurdBusSupplier => () => SigurdLib.EventBus;
}
