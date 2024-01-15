using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Sigurd.ServerAPI.Features;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sigurd.ServerAPI;

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

        SceneManager.sceneLoaded += OnSceneLoaded;

        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} ({MyPluginInfo.PLUGIN_VERSION}) has awoken.");

        InitializeNetworking();
    }

    // For pre-placed items
    internal void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (GrabbableObject grabbable in FindObjectsOfType<GrabbableObject>())
        {
            if (!grabbable.TryGetComponent(out SItemNetworking _))
            {
                grabbable.gameObject.AddComponent<SItemNetworking>();
            }
        }
    }

    internal void InitializeNetworking()
    {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }
}