using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using Sigurd.Common.Features;
using Sigurd.ServerAPI.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Reflection.Emit;
using Unity.Netcode;
using UnityEngine;

namespace Sigurd.ServerAPI.Events.Patches.Player
{
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayer))]
    internal class Dying
    {
        private static DyingEventArgs CallEvent(PlayerControllerB playerController, Vector3 force, bool spawnBody,
            CauseOfDeath causeOfDeath, int deathAnimation)
        {
            DyingEventArgs ev = new DyingEventArgs(Common.Features.SPlayer.GetOrAdd(playerController), force, spawnBody,
                causeOfDeath, deathAnimation);

        Handlers.Player.OnDying(ev);

        return ev;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = new List<CodeInstruction>(instructions);
        const int offset = 3;

        int index = newInstructions.FindLastIndex(i => i.OperandIs(AccessTools.Method(typeof(PlayerControllerB),
            nameof(PlayerControllerB.AllowPlayerDeath)))) + offset;

        Label notAllowedLabel = generator.DefineLabel();
        Label skipLabel = generator.DefineLabel();

        CodeInstruction[] inst = new CodeInstruction[]
        {
            // DyingEventArgs ev = Dying.CallEvent(PlayerControllerB, Vector3, bool, CauseOfDeath, int)
            new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Ldarg_2),
            new CodeInstruction(OpCodes.Ldarg_3),
            new CodeInstruction(OpCodes.Ldarg, 4),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dying), nameof(Dying.CallEvent))),
            new CodeInstruction(OpCodes.Dup),

            // if (!ev.IsAllowed) return
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(DyingEventArgs), nameof(DyingEventArgs.IsAllowed))),
            new CodeInstruction(OpCodes.Brfalse_S, notAllowedLabel),

            // Duplicating the stack is more memory efficient than making a local
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Dup),
            new CodeInstruction(OpCodes.Dup),

            // bodyVelocity = ev.Force
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(DyingEventArgs), nameof(DyingEventArgs.Force))),
            new CodeInstruction(OpCodes.Starg_S, 1),

            // spawnBody = ev.SpawnBody
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(DyingEventArgs), nameof(DyingEventArgs.SpawnBody))),
            new CodeInstruction(OpCodes.Starg_S, 2),

            // causeOfDeath = ev.CauseOfDeath
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(DyingEventArgs), nameof(DyingEventArgs.CauseOfDeath))),
            new CodeInstruction(OpCodes.Starg_S, 3),

            // deathAnimation = ev.DeathAnimation
            new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(DyingEventArgs), nameof(DyingEventArgs.DeathAnimation))),
            new CodeInstruction(OpCodes.Starg_S, 4),

            new CodeInstruction(OpCodes.Br, skipLabel),
            new CodeInstruction(OpCodes.Pop).WithLabels(notAllowedLabel),
            new CodeInstruction(OpCodes.Ret)
        };

        newInstructions.InsertRange(index, inst);

        newInstructions[index + inst.Length].labels.Add(skipLabel);

        for (int i = 0; i < newInstructions.Count; i++) yield return newInstructions[i];
    }
}

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
    internal class Died
    {
        private static void CallDyingEvent(PlayerControllerB playerController, bool spawnBody, Vector3 bodyVelocity,
            int causeOfDeath, int deathAnimation)
        {
            SPlayer player = SPlayer.Get(playerController);
            if (player.IsLocalPlayer) return;

        Handlers.Player.OnDying(new DyingEventArgs(player, bodyVelocity,
            spawnBody, (CauseOfDeath)causeOfDeath, deathAnimation));
    }

        private static void CallDiedEvent(PlayerControllerB playerController, bool spawnBody, Vector3 bodyVelocity,
            int causeOfDeath, int deathAnimation)
        {
            Handlers.Player.OnDied(new DiedEventArgs(SPlayer.GetOrAdd(playerController), bodyVelocity,
                spawnBody, (CauseOfDeath)causeOfDeath, deathAnimation));
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = new List<CodeInstruction>(instructions);

            {
                const int offset = 1;

                int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Nop) + offset;

                CodeInstruction[] inst = new CodeInstruction[]
                {
                    // Died.CallDyingEvent(PlayerControllerB, Vector3, bool, CauseOfDeath, int)
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldarg, 4),
                    new CodeInstruction(OpCodes.Ldarg, 5),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Died), nameof(Died.CallDyingEvent))),
                };

                newInstructions.InsertRange(index, inst);
            }

            {
                int index = newInstructions.Count - 1;

                CodeInstruction[] inst = new CodeInstruction[]
                {
                    // Died.CallDiedEvent(PlayerControllerB, Vector3, bool, CauseOfDeath, int)
                    new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                    new CodeInstruction(OpCodes.Ldarg_2),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldarg, 4),
                    new CodeInstruction(OpCodes.Ldarg, 5),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Died), nameof(Died.CallDiedEvent))),
                };

                newInstructions.InsertRange(index, inst);
            }

            for (int i = 0; i < newInstructions.Count; i++) yield return newInstructions[i];
        }
    }
}
