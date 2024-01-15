using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sigurd.Common.Features.Patches;

[HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnLocalDisconnect))]
internal static class LocalLeft
{
    private static void Prefix()
    {
        SPlayer.Dictionary.Clear();
        SItem.Dictionary.Clear();
    }
}