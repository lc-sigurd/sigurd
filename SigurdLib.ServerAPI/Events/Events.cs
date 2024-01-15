namespace Sigurd.ServerAPI.Events;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class CustomEventHandlers
{
    public delegate void CustomEventHandler<TEventArgs>(TEventArgs ev)
        where TEventArgs : System.EventArgs;

    public delegate void CustomEventHandler();
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
