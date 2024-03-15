using System.Reflection;
using Sigurd.EventBus.Api;

namespace Sigurd.EventBus.Listener;

public sealed class SubscribeEventListener : IEventListener
{
    private readonly IEventListener _handler;
    private readonly SubscribeEventAttribute _subscriptionAttribute;
    private readonly string _readable;

    public SubscribeEventListener(object target, MethodInfo method)
    {
        _handler = EventListenerFactoryManager.Instance.Create(method, target);
        _subscriptionAttribute = method.GetCustomAttribute<SubscribeEventAttribute>();
        _readable = $"@SubscribeEvent: {target} {method.Name}";
    }

    public void Invoke(Event message)
    {
        if (_handler is null) return;
        if (!_subscriptionAttribute.ReceiveCancelled && message is ICancellableEvent { IsCanceled: true }) return;
        _handler.Invoke(message);
    }

    public EventPriority Priority => _subscriptionAttribute.Priority;
}
