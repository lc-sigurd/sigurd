using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine.EventSystems;

namespace Sigurd.ClientAPI.ChatCommands;

[HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SubmitChat_performed))]
internal static class SubmitChatPatch
{
    private static bool HandleMessage(HUDManager manager)
    {
        string message = manager.chatTextField.text;

        if (!message.IsNullOrWhiteSpace() && message.StartsWith(CommandRegistry.CommandPrefix.Value))
        {
            string[] split = message.Split(' ');

            string command = split[0].Substring(CommandRegistry.CommandPrefix.Value.Length);

            if (CommandRegistry.TryGetCommandHandler(command, out CommandHandler handler))
            {
                string[] arguments = split.Skip(1).ToArray();
                try
                {
                    handler.Handle(arguments);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogError($"Error handling command: {command}");
                    Plugin.Log.LogError(ex);
                }

                manager.localPlayer.isTypingChat = false;
                manager.chatTextField.text = "";
                EventSystem.current.SetSelectedGameObject(null);
                manager.typingIndicator.enabled = false;

                return true;
            }
        }

        return false;
    }

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = new List<CodeInstruction>(instructions);

        Label returnLabel = generator.DefineLabel();

        newInstructions[newInstructions.Count - 1].labels.Add(returnLabel);

        int index = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldfld &&
                                                   (FieldInfo)i.operand == AccessTools.Field(typeof(PlayerControllerB), nameof(PlayerControllerB.isPlayerDead))) - 2;

        newInstructions.InsertRange(index, new CodeInstruction[]
        {
            // if (SubmitChatPatch.HandleMessage(this)) return;
            new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SubmitChatPatch), nameof(SubmitChatPatch.HandleMessage))),
            new CodeInstruction(OpCodes.Brtrue, returnLabel)
        });

        for (int z = 0; z < newInstructions.Count; z++) yield return newInstructions[z];
    }
}