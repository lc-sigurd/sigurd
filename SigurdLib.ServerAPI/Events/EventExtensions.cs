using System;
using static Sigurd.ServerAPI.Events.CustomEventHandlers;

namespace Sigurd.ServerAPI.Events;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class EventExtensions
{
    public static void InvokeSafely<T>(this CustomEventHandler<T> ev, T arg)
        where T : System.EventArgs
    {
        if (ev == null)
            return;

        foreach (CustomEventHandler<T> handler in ev.GetInvocationList())
        {
            try
            {
                handler(arg);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
            }
        }
    }

    public static void InvokeSafely(this CustomEventHandler ev)
    {
        if (ev == null)
            return;

        foreach (CustomEventHandler handler in ev.GetInvocationList())
        {
            try
            {
                handler();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
