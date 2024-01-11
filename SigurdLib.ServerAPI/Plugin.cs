using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine.SceneManagement;

namespace Sigurd.ServerAPI
{
    /// <summary>
    /// The main Plugin class.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(Common.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
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

            SceneManager.sceneLoaded += OnSceneLoaded;

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} ({MyPluginInfo.PLUGIN_VERSION}) has awoken.");
        }

        // For pre-placed items
        internal void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (GrabbableObject grabbable in FindObjectsOfType<GrabbableObject>())
            {
                if (!grabbable.TryGetComponent(out Features.Item _))
                {
                    grabbable.gameObject.AddComponent<Features.Item>();
                }
            }
        }
    }
}
