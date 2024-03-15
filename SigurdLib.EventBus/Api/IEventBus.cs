// The contents of this file are largely based upon https://github.com/MinecraftForge/EventBus/tree/ec01e75bf78ce21b00e4675ca4370d591b9d1457
// Forge Development LLC licenses EventBus to the Sigurd Team under the LGPL-2.1-only license.
// The Sigurd Team licenses this file to you under the LGPL-3.0-or-later license.

using System;

namespace Sigurd.EventBus.Api;

/// <summary>
/// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Delegate type that handles event dispatch.
    /// </summary>
    /// <seealso cref="IEventListener.Invoke"/>
    delegate void InvokeDispatcher(IEventListener listener, Event @event);

    /// <summary>
    /// <para>
    /// Register an instance <see cref="object"/> or <see cref="Type"/> to the <see cref="IEventBus"/>. Listeners for all
    /// <see cref="SubscribeEventAttribute"/> annotated methods found will be added.
    /// </para>
    /// <para>If <paramref name="target"/> is an <see cref="Object"/> instance, it will be scanned for <b>non-static</b> listener methods.</para>
    /// <para>If <paramref name="target"/> is a <see cref="Type"/>, it will be scanned for <b>static</b> listener methods.</para>
    /// </summary>
    /// <param name="target"><see cref="Object"/> instance or <see cref="Type"/> to scan for listeners to add.</param>
    void Register(object target);

    /// <summary>
    /// Add a consumer listener with default <see cref="EventPriority.Normal"/> priority that will not receive
    /// cancelled events.
    /// </summary>
    /// <param name="listener">Callback to invoke when a matching event is posted.</param>
    /// <typeparam name="TEvent">The derived <see cref="Event"/> type.</typeparam>
    void AddListener<TEvent>(Action<TEvent> listener) where TEvent : Event;

    /// <summary>
    /// Add a consumer listener with the specified <see cref="EventPriority"/> that will not receive
    /// cancelled events.
    /// </summary>
    /// <param name="priority"><see cref="EventPriority"/> for the listener.</param>
    /// <param name="listener">Callback to invoke when a matching event is posted.</param>
    /// <typeparam name="TEvent">The derived <see cref="Event"/> type.</typeparam>
    void AddListener<TEvent>(EventPriority priority, Action<TEvent> listener) where TEvent : Event;

    /// <summary>
    /// Add a consumer listener with default <see cref="EventPriority.Normal"/> priority that will potentially
    /// receive cancelled events.
    /// </summary>
    /// <param name="receiveCancelled">Pass <see langword="true"/> to indicate that the provided <paramref name="listener"/> should receive cancelled events.</param>
    /// <param name="listener">Callback to invoke when a matching event is posted.</param>
    /// <typeparam name="TEvent">The derived <see cref="Event"/> type.</typeparam>
    void AddListener<TEvent>(bool receiveCancelled, Action<TEvent> listener) where TEvent : Event;

    /// <summary>
    /// Add a consumer listener with the specified <see cref="EventPriority"/> that will potentially receive
    /// cancelled events.
    /// </summary>
    /// <param name="priority"><see cref="EventPriority"/> for the listener.</param>
    /// <param name="receiveCancelled">Pass <see langword="true"/> to indicate that the provided <paramref name="listener"/> should receive cancelled events.</param>
    /// <param name="listener">Callback to invoke when a matching event is posted.</param>
    /// <typeparam name="TEvent">The derived <see cref="Event"/> type.</typeparam>
    void AddListener<TEvent>(EventPriority priority, bool receiveCancelled, Action<TEvent> listener) where TEvent : Event;

    /// <summary>
    /// Unregister the provided <see cref="IEventListener"/>, <see cref="Object"/> instance, or <see cref="Type"/> from
    /// this <see cref="IEventBus"/>.
    /// </summary>
    /// <param name="target">The target to unsubscribe from this <see cref="IEventBus"/></param>
    void Unregister(object target);

    /// <summary>
    /// Submit an <see cref="Event"/> for dispatch to the appropriate listeners.
    /// </summary>
    /// <param name="event">The <see cref="Event"/> to dispatch.</param>
    /// <returns>The dispatched event.</returns>
    TEvent Post<TEvent>(TEvent @event) where TEvent: Event;

    /// <summary>
    /// Submit an <see cref="Event"/> for dispatch to listeners registered with a specific <see cref="EventPriority"/>.
    /// </summary>
    /// <param name="phase">The <see cref="EventPriority"/> to dispatch with.</param>
    /// <param name="event">The <see cref="Event"/> to dispatch.</param>
    /// <returns>The dispatched event.</returns>
    TEvent Post<TEvent>(EventPriority phase, TEvent @event) where TEvent: Event;

    /// <summary>
    /// Shut down this <see cref="IEventBus"/>.
    /// Any further call to <see cref="Post{TEvent}(TEvent)"/> or its overloads will effectively no-op.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Start this <see cref="IEventBus"/>, i.e. allow events to be posted.
    /// </summary>
    void Start();
}
