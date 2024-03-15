using System;

namespace Sigurd.Bus.Api;

public class EventInvocationException : Exception
{
    public EventInvocationException() { }

    public EventInvocationException(string message) : base(message) { }

    public EventInvocationException(string message, Exception inner) : base(message, inner) { }

    public required Event Event { get; init; }

    public required IEventListener? Listener { get; init; }
}
