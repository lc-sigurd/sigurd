using static Sigurd.PluginLoader.SigurdPlugin.EventBusSubscriberAttribute;

namespace Sigurd.PluginLoader;

/// <summary>
/// Automatic <see cref="EventBus"/> subscriber. Reads <see cref="SigurdPlugin.EventBusSubscriberAttribute"/>
/// annotations and passes the annotated types to the <see cref="Bus"/> defined by the annotation, which
/// defaults to <c>SigurdLib.EventBus</c>.
/// </summary>
public class AutomaticEventSubscriber
{

}
