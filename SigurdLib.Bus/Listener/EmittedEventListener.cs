using Sigurd.Bus.Api;

namespace Sigurd.Bus.Listener;

public abstract class EmittedEventListener : IEventListener
{
    public abstract void Invoke(Event message);
}
