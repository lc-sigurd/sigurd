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
            if (__instance.IsServer && !SPlayerNetworking.TryGet(__instance, out SPlayerNetworking _))
            {
                GameObject go = UnityEngine.Object.Instantiate(SPlayerNetworking.PlayerNetworkPrefab);
                go.SetActive(true);
                go.GetComponent<NetworkObject>().Spawn(false);
            }
        }
    }
}
