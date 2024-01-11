using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.ServerAPI.Events.Patches.Internal
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Start))]
    class PlayerControllerBStartPatch
    {
        private static void Postfix(PlayerControllerB __instance)
        {
            if (__instance.IsServer && !Features.Player.TryGet(__instance, out Features.Player _))
            {
                GameObject go = UnityEngine.Object.Instantiate(Features.Player.PlayerNetworkPrefab);
                go.SetActive(true);
                go.GetComponent<Features.Player>().PlayerController = __instance;
                go.GetComponent<NetworkObject>().Spawn(false);
            }
        }
    }
}
