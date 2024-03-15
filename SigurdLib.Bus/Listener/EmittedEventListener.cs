using Sigurd.EventBus.Api;

namespace Sigurd.EventBus.Listener;

public abstract class EmittedEventListener : IEventListener
{
    public abstract void Invoke(Event message);
}
