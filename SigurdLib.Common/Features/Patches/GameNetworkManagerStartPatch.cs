using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.Common.Features.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))]
    [HarmonyPriority(int.MinValue)]
    class GameNetworkManagerStartPatch
    {
        private static void Postfix(GameNetworkManager __instance)
        {
            NetworkManager networkManager = __instance.GetComponent<NetworkManager>();

            foreach (NetworkPrefab prefab in networkManager.NetworkConfig.Prefabs.Prefabs)
            {
                if (prefab.Prefab != null && prefab.Prefab.GetComponent<GrabbableObject>() != null)
                {
                    prefab.Prefab.AddComponent<SItem>();
                }
            }
        }
    }
}
