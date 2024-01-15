using HarmonyLib;

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