using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.ServerAPI.Features.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Start))]
    class PlayerControllerBStartPatch
    {
        private static void Postfix(PlayerControllerB __instance)
        {
            if (__instance.IsServer && !PlayerNetworking.TryGet(__instance, out PlayerNetworking _))
            {
                GameObject go = UnityEngine.Object.Instantiate(PlayerNetworking.PlayerNetworkPrefab);
                go.SetActive(true);
                go.GetComponent<NetworkObject>().Spawn(false);
            }
        }
    }
}
