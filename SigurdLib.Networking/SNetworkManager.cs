using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Unity.Netcode;

namespace Sigurd.Networking;

internal class SNetworkManager
{
    /*
     * This section of code is taken and modified from https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/36368846c5bfe6cfb93adc36282507614955955c/com.unity.netcode.gameobjects/Runtime/Core/NetworkManager.cs
     * in com.unity.netcode.gameobjects, which is released under the MIT License.
     * See file libs/unity-ngo/LICENSE.md or go to https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/blob/develop/LICENSE.md for full license details.
     * Copyright: © 2024 Unity Technologies
     */

    internal static readonly Dictionary<Type, List<FieldInfo>> NetworkVariableFields = [];

    internal static SNetworkManager Singleton { get; private set; } = null!;

    internal SNetworkBehaviourUpdater SBehaviourUpdater = null!;

    // ReSharper disable once UnusedParameter.Global
    internal static List<FieldInfo> InitializeVariables<T>(T obj) where T : SNetworkBehaviour
    {
        return NetworkVariableFields[typeof(T)];
    }

    /// <summary>
    /// Set this NetworkManager instance as the static NetworkManager singleton
    /// </summary>
    internal void SetSingleton()
    {
        Singleton = this;
    }

    internal void OnEnable()
    {
        if (Singleton == null)
        {
            SetSingleton();
        }
    }

    internal void Initialize(NetworkManager networkManager)
    {

        // Don't allow the user to start a network session if the NetworkManager is
        // still parented under another GameObject
        if (networkManager.NetworkManagerCheckForParent(true))
        {
            return;
        }

        if (networkManager.NetworkConfig.NetworkTransport == null)
        {
            if (NetworkLog.CurrentLogLevel <= LogLevel.Error)
            {
                NetworkLog.LogError("No transport has been selected!");
            }

            return;
        }

        SBehaviourUpdater = new SNetworkBehaviourUpdater();
        SBehaviourUpdater.Initialize(networkManager);
    }
}

[HarmonyPatch(typeof(NetworkManager))]
[HarmonyPriority(Priority.VeryHigh)]
[HarmonyWrapSafe]
internal class BehaviourUpdaterPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(NetworkManager.OnEnable))]
    private static void OnEnable()
    {
        var sNetworkManager = new SNetworkManager();

        sNetworkManager.OnEnable();
    }

    [HarmonyPatch(nameof(NetworkManager.Initialize))]
    private static void Initialize(NetworkManager __instance)
    {
        SNetworkManager.Singleton.Initialize(__instance);
    }
}
