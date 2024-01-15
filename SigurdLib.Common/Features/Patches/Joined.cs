using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.Common.Features.Patches;

[HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerConnectedClientRpc))]
internal static class Joined
{
    private static void Postfix(ulong clientId, int assignedPlayerObjectId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[assignedPlayerObjectId];
        playerController.StartCoroutine(Joined.JoinedCoroutine(playerController));
    }

    internal static IEnumerator JoinedCoroutine(PlayerControllerB controller)
    {
        yield return new WaitUntil(() => StartOfRound.Instance.localPlayerController != null);

        SPlayer player = SPlayer.GetOrAdd(controller);
    }
}

[HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.ConnectClientToPlayerObject))]
internal static class Joined2
{
    private static void Postfix(PlayerControllerB __instance)
    {
        __instance.StartCoroutine(Joined.JoinedCoroutine(__instance));
    }
}