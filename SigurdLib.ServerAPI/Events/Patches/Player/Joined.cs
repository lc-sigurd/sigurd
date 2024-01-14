using GameNetcodeStuff;
using HarmonyLib;
using Sigurd.ServerAPI.Events.EventArgs.Player;
using Sigurd.ServerAPI.Features;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.ServerAPI.Events.Patches.Player
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
    internal static class Joined
    {
        private static void Postfix(ulong clientId, int assignedPlayerObjectId)
        {
            PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[assignedPlayerObjectId];
            if (!Cache.Player.ConnectedPlayers.Contains(clientId))
            {
                Cache.Player.ConnectedPlayers.Add(clientId);

                playerController.StartCoroutine(JoinedCoroutine(playerController));
            }
        }

        // Since we have to wait for players' client id to sync to the player instance, we have to constantly check
        // if the player and its controller were linked yet. Very annoying.
        internal static IEnumerator JoinedCoroutine(PlayerControllerB controller)
        {
            yield return new WaitUntil(() => StartOfRound.Instance.localPlayerController != null);

            Common.Features.SPlayer player = Common.Features.SPlayer.GetOrAdd(controller);

            while (player == null)
            {
                yield return new WaitForSeconds(0.1f);

                player = Common.Features.SPlayer.GetOrAdd(controller);
            }

            PlayerNetworking playerNetworking = PlayerNetworking.GetOrAdd(player);

            while (playerNetworking == null)
            {
                yield return new WaitForSeconds(0.1f);

                playerNetworking = PlayerNetworking.GetOrAdd(player);
            }

            if (player.IsLocalPlayer)
            {
                Common.Features.SPlayer.LocalPlayer = player;
            }

            if (player.IsHost)
            {
                Common.Features.SPlayer.HostPlayer = player;
            }

            Handlers.Player.OnJoined(new JoinedEventArgs(player));
        }
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
    internal static class Joined2
    {
        private static void Postfix(PlayerControllerB __instance)
        {
            if (!Cache.Player.ConnectedPlayers.Contains(__instance.actualClientId))
            {
                Cache.Player.ConnectedPlayers.Add(__instance.actualClientId);

                __instance.StartCoroutine(Joined.JoinedCoroutine(__instance));
            }
        }
    }
}
