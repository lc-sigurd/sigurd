using System;
using Sigurd.EventBus.Api;

namespace Sigurd.EventBus.Listener;

public sealed class ConsumerEventListener : IEventListener
{
    private readonly Action<Event> _consumer;

    public ConsumerEventListener(Action<Event> consumer)
    {
        _consumer = consumer;
    }

    public void Invoke(Event message)
    {
        _consumer.Invoke(message);
    }

    public override string ToString() => _consumer.ToString();
}
