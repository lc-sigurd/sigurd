using Sigurd.Bus.Api;

namespace Sigurd.Bus.Listener;

public sealed class CancellationFilteredEventListener : IEventListener
{
    public IEventListener Inner { get; }

    public CancellationFilteredEventListener(IEventListener inner)
    {
        Inner = inner;
    }

    public void Invoke(Event message)
    {
        if (message is ICancellableEvent { IsCanceled: true }) return;
        Inner.Invoke(message);
    }
}
