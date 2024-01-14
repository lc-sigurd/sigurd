using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.SceneManagement;

namespace Sigurd.Common
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

            SceneManager.sceneLoaded += OnSceneLoaded;

            Harmony = new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{DateTime.Now.Ticks}");
            Harmony.PatchAll();

            Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} ({MyPluginInfo.PLUGIN_VERSION}) has awoken.");
        }

        // For pre-placed items
        internal void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (GrabbableObject grabbable in FindObjectsOfType<GrabbableObject>())
            {
                if (!grabbable.TryGetComponent(out Features.SItem _))
                {
                    grabbable.gameObject.AddComponent<Features.SItem>();
                }
            }
        }
    }
}
