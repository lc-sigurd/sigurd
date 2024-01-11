using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sigurd.Networking
{
    /// <summary>
    /// The main Plugin class.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance { get; private set; }

        internal static ManualLogSource Log { get; private set; }

        internal static Harmony Harmony { get; private set; }

        private void Awake()
        {
            Instance = this;

            Log = Logger;

            Harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{DateTime.Now.Ticks}");
            Harmony.PatchAll();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} ({MyPluginInfo.PLUGIN_VERSION}) has awoken.");

            Network.Init();
        }
    }
}