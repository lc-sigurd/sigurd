using System;
using System.Linq;
using BepInEx;
using Sigurd.Util;

namespace Sigurd.PluginLoader;

/// <summary>
/// Container class for <see cref="EventBusSubscriberAttribute"/>.
/// </summary>
public static class SigurdPlugin
{
    /// <summary>
    /// Annotate a class which will be subscribed to an <see cref="EventBus"/> when your plugin is initialised.
    /// Defaults to subscribing to <c>SigurdLib.EventBus</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventBusSubscriberAttribute : Attribute
    {
        private static readonly Side DefaultSide = Side.Client | Side.Server;

        internal string PluginGuid { get; }

        internal Side Side { get; }

        internal Bus EventBus { get; }

        /// <param name="pluginGuid">The <see cref="BepInPlugin.GUID"/> of the <see cref="BepInPlugin"/> this
        /// event listener should be associated with.</param>
        /// <param name="sides"></param>
        public EventBusSubscriberAttribute(string pluginGuid, params Side[] sides) : this(pluginGuid, Bus.Sigurd, sides) { }

        /// <param name="pluginGuid">The <see cref="BepInPlugin.GUID"/> of the <see cref="BepInPlugin"/> this
        /// event listener should be associated with.</param>
        /// <param name="bus"></param>
        /// <param name="sides"></param>
        public EventBusSubscriberAttribute(string pluginGuid, Bus bus, params Side[] sides)
        {
            PluginGuid = pluginGuid;
            EventBus = bus;
            Side = sides.Length > 0 ? sides.Aggregate((accumulator, next) => accumulator | next) : DefaultSide;
        }

        /// <summary>
        /// <see cref="Enum"/> identifying <see cref="EventBus"/> channels that can be listened to
        /// using <see cref="EventBusSubscriberAttribute"/>.
        /// </summary>
        public enum Bus
        {
            /// <summary>
            /// The main Sigurd event bus.
            /// </summary>
            Sigurd,
        }
    }
}
