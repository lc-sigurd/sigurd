using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sigurd.Bus.Api;
using Sigurd.Bus.Listener;
using Sigurd.Util.Extensions;

namespace Sigurd.Bus;

public class EventBus : IEventBus
{
    const BindingFlags MethodSearchFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private readonly EventTree _eventTree = new();
    private readonly ConcurrentDictionary<object, ConcurrentBag<IEventListener>> _listeners = new();

    private readonly BusConfiguration _configuration;
    private bool _shutdown = false;

    public EventBus(BusConfiguration configuration)
    {
        _configuration = configuration;
        _shutdown = !configuration.StartImmediately;
    }

    #region Registering arbitrary objects (instances / classes annotated with SubscribeEventAttribute)

    private static void CheckSupertypes(Type registeringType) => CheckSupertypes(registeringType, registeringType);

    private static void CheckSupertypes(Type registeringType, Type? supertype)
    {
        if (supertype is null || supertype == typeof(object)) return;

        Check();

        CheckSupertypes(registeringType, supertype.BaseType);
        foreach (var @interface in supertype.GetInterfaces()) {
            CheckSupertypes(registeringType, @interface);
        }
        return;

        void Check()
        {
            if (registeringType == supertype) return;
            foreach (var method in supertype.GetMethods(MethodSearchFlags)) {
                CheckMethod(method);
            }
        }

        void CheckMethod(MethodInfo method)
        {
            var foundAttribute = method.GetCustomAttribute<SubscribeEventAttribute>();
            if (foundAttribute is null) return;
            throw new ArgumentException(
                $"Attempting to register a listener object of type {registeringType},\n"
                + $"however its supertype {supertype} declares a method annotated with {nameof(SubscribeEventAttribute)}: {method}.\n"
                + $"This is not allowed! Only the listener object may declare methods annotated with {nameof(SubscribeEventAttribute)}."
            );
        }
    }

    /// <inheritdoc />
    public virtual void Register(object target)
    {
        if (_listeners.ContainsKey(target)) return;

        var targetIsType = target.GetType() == typeof(Type);
        var targetType = targetIsType ? (Type)target : target.GetType();

        CheckSupertypes(targetType);

        var methods = targetType.GetMethods(MethodSearchFlags)
            .Where(HasSubscribeAnnotation)
            .Tap(EnsureContextMatches)
            .ToArray();

        if (methods.Length == 0) {
            throw new ArgumentException(
                $"{nameof(Register)}() was invoked upon {(targetIsType ? targetType.ToString() : $"an instance of {targetType}")},\n" +
                $"but {targetType} declares no methods annotated with {nameof(SubscribeEventAttribute)}.\n" +
                $"The event bus only recognises listener methods annotated with {nameof(SubscribeEventAttribute)}."
            );
        }

        foreach (var method in methods) {
            Register(target, method);
        }

        bool HasSubscribeAnnotation(MethodInfo method)
        {
            var foundAttribute = method.GetCustomAttribute<SubscribeEventAttribute>();
            return foundAttribute is not null;
        }

        void EnsureContextMatches(MethodInfo method)
        {
            if (targetIsType == method.IsStatic) return;

            if (targetIsType) {
                throw new ArgumentException(
                    $"Expected method annotated with {nameof(SubscribeEventAttribute)} {method} to be static\n" +
                    $"because {nameof(Register)}() was invoked with a Type {targetType}.\n" +
                    $"Either make the method static, or invoke {nameof(Register)}() with an instance of {targetType}."
                );
            }

            throw new ArgumentException(
                $"Expected method annotated with {nameof(SubscribeEventAttribute)} {method} NOT to be static\n" +
                $"because {nameof(Register)}() was invoked with an instance of {targetType}.\n" +
                $"Either make the method static, or invoke {nameof(Register)}() with an instance of {targetType}."
            );
        }
    }

    private void Register(object target, MethodInfo method)
    {
        var parameters = method.GetParameters();
        if (parameters.Length != 1)
            throw new ArgumentException(
                $"Method {method} is annotated with {nameof(SubscribeEventAttribute)},\n" +
                $"but declares {parameters.Length} parameters.\n" +
                $"Event handler methods must declare exactly one parameter whose type is a subtype of {nameof(Event)}."
            );

        var eventType = parameters[0].ParameterType;
        if (!typeof(Event).IsAssignableFrom(eventType))
            throw new ArgumentException(
                $"Method {method} is annotated with {nameof(SubscribeEventAttribute)},\n" +
                $"but declares a parameter of type {eventType}, which is not a subtype of {nameof(Event)}.\n" +
                $"Event handler methods must declare exactly one parameter whose type is a subtype of {nameof(Event)}."
            );

        Register(eventType, target, method);
    }

    private void Register(Type eventType, object target, MethodInfo method)
    {
        var listener = new SubscribeEventListener(target, method);
        AddToListeners(eventType, target, listener, listener.Priority);
    }

    #endregion

    #region Registering consumers (Action<TEvent>) as listeners

    /// <inheritdoc />
    public virtual void AddListener<TEvent>(Action<TEvent> listener) where TEvent : Event
    {
        AddListener(EventPriority.Normal, listener);
    }

    /// <inheritdoc />
    public virtual void AddListener<TEvent>(EventPriority priority, Action<TEvent> listener) where TEvent : Event
    {
        AddListener(priority, false, listener);
    }

    /// <inheritdoc />
    public virtual void AddListener<TEvent>(bool receiveCancelled, Action<TEvent> listener) where TEvent : Event
    {
        AddListener(EventPriority.Normal, receiveCancelled, listener);
    }

    /// <inheritdoc />
    public virtual void AddListener<TEvent>(EventPriority priority, bool receiveCancelled, Action<TEvent> listener) where TEvent : Event
    {
        var wrappedListener = receiveCancelled switch {
            true => PlainWrappedListener(),
            false => CancellationFilteringWrappedListener(),
        };

        AddToListeners(typeof(TEvent), listener, wrappedListener, priority);
        return;

        IEventListener PlainWrappedListener() => new ConsumerEventListener(@event => listener((TEvent)@event));
        IEventListener CancellationFilteringWrappedListener() => new CancellationFilteredEventListener(PlainWrappedListener());
    }

    private void AddToListeners(Type eventType, object target, IEventListener listener, EventPriority priority)
    {
        if (eventType.IsAbstract)
            throw new ArgumentException(
                $"Cannot register listeners for abstract event type {eventType}.\n" +
                $"Register a listener to a subclass of {eventType} instead."
            );
        var eventTreeNode = _eventTree.GetNode(eventType);
        eventTreeNode.RegisterListener(priority, listener);
        ConcurrentBag<IEventListener> others = _listeners.GetOrAdd(target, _ => new ConcurrentBag<IEventListener>());
        others.Add(listener);
    }

    #endregion

    #region Unregistering

    /// <inheritdoc />
    public void Unregister(object target)
    {
        var found = _listeners.TryRemove(target, out var list);
        if (!found) return;
        foreach (var eventTreeNode in _eventTree.PreOrderTraverseNodes()) {
            foreach (var eventListener in list) {
                eventTreeNode.UnregisterListener(eventListener);
            }
        }
    }

    #endregion

    #region Event dispatch

    /// <inheritdoc />
    public TEvent Post<TEvent>(TEvent @event) where TEvent: Event
    {
        return Post(@event, _eventTree.GetNode(typeof(TEvent)).GetListeners());
    }

    /// <inheritdoc />
    public TEvent Post<TEvent>(EventPriority phase, TEvent @event) where TEvent : Event
    {
        if (_shutdown) return @event;
        return Post(@event, _eventTree.GetNode(typeof(TEvent)).GetPriorityListeners(phase));
    }

    private TEvent Post<TEvent>(TEvent @event, IEnumerable<IEventListener> listeners) where TEvent : Event
    {
        foreach (var eventListener in listeners) {
            try {
                eventListener.Invoke(@event);
            }
            catch (Exception exception) {
                throw new EventInvocationException("Uncaught exception during event invocation", exception) {
                    Event = @event,
                    Listener = eventListener,
                };
            }
        }

        return @event;
    }

    #endregion

    #region Lifetime

    /// <inheritdoc />
    public void Start()
    {
        _shutdown = false;
    }

    /// <inheritdoc />
    public void Shutdown()
    {
        _shutdown = true;
    }

    #endregion
}
