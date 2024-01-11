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
            if (__instance.IsServer && !Player.TryGet(__instance, out Player _))
            {
                GameObject go = UnityEngine.Object.Instantiate(Features.Player.PlayerNetworkPrefab);
                go.SetActive(true);
                go.GetComponent<Player>().PlayerController = __instance;
                go.GetComponent<NetworkObject>().Spawn(false);
            }
        }
    }
}
