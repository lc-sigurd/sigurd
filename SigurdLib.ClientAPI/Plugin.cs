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
        internal static Plugin Instance { get; private set; }

        internal static ManualLogSource Log;

        internal static Harmony Harmony;

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
