using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Sigurd.ClientAPI.ChatCommands;
using System;

namespace Sigurd.ClientAPI
{
    /// <summary>
    /// The main Plugin class.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public sealed class Plugin : BaseUnityPlugin
    {
#pragma warning disable CS8618 // Non-nullable variable must contain a non-null value when exiting constructor.
        internal static Plugin Instance { get; private set; }

        internal static ManualLogSource Log { get; private set; }

        internal static Harmony Harmony { get; private set; }
#pragma warning restore

        private void Awake()
        {
            Instance = this;

            Log = Logger;

            CommandRegistry.CommandPrefix = Config.Bind("Commands", "Prefix", "/", "Command prefix");

            Harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{DateTime.Now.Ticks}");
            Harmony.PatchAll();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} ({MyPluginInfo.PLUGIN_VERSION}) has awoken.");
        }
    }
}
