﻿using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MonoMod.Cil;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace SigurdLib.PluginLoader;

public static class PluginLoadingHooks
{
    public class PluginLoadEventArgs : EventArgs
    {
        public required PluginInfo PluginInfo { get; init; }
    }

    public static event EventHandler<PluginLoadEventArgs>? Pre;

    public static event EventHandler<PluginLoadEventArgs>? Post;

    internal static ManualLogSource Logger = null!;

    public static void Start()
    {
        Logger = BepInEx.Logging.Logger.CreateLogSource(PluginLoaderInfo.PRODUCT_NAME);

        Pre += (sender, args) => Logger.LogWarning($"Now loading {args.PluginInfo.Metadata.Name}");
        Post += (sender, args) => Logger.LogWarning($"Just finished loading {args.PluginInfo.Metadata.Name}");

        var harmony = new Harmony(PluginLoaderInfo.PRODUCT_GUID);
        harmony.PatchAll(typeof(ChainloaderStartPatches));
    }

    internal static void InvokePre(PluginInfo pluginInfo) => Pre?.Invoke(null, new PluginLoadEventArgs { PluginInfo = pluginInfo });

    internal static void InvokePost(PluginInfo pluginInfo) => Post?.Invoke(null, new PluginLoadEventArgs { PluginInfo = pluginInfo });

    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    static class ChainloaderStartPatches
    {
        [HarmonyILManipulator]
        public static void Manipulate(ILContext ilContext, MethodBase original, ILLabel retLabel)
        {
            var cursor = new ILCursor(ilContext);

            cursor
                .GotoNext(
                    instr => instr.MatchLdloc(23),
                    instr => instr.MatchCall(AccessTools.PropertyGetter(typeof(Chainloader), nameof(Chainloader.ManagerObject))),
                    instr => instr.MatchLdloc(31),
                    instr => instr.MatchLdloc(23),
                    instr => instr.MatchCallvirt(AccessTools.PropertyGetter(typeof(PluginInfo), "TypeName")),
                    instr => instr.MatchCallvirt(AccessTools.Method(typeof(Assembly), nameof(Assembly.GetType), [typeof(string)])),
                    instr => instr.MatchCallvirt(AccessTools.Method(typeof(GameObject), nameof(GameObject.AddComponent), [typeof(Type)]))
                )
                .Emit(OpCodes.Ldloc_S, (byte)23)
                .Emit(OpCodes.Call, AccessTools.Method(typeof(PluginLoadingHooks), nameof(InvokePre)));

            cursor.Index += 7;

            cursor
                .Emit(OpCodes.Ldloc_S, (byte)23)
                .Emit(OpCodes.Call, AccessTools.Method(typeof(PluginLoadingHooks), nameof(InvokePost)));
        }

#if false
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> TranspileChainloaderStart(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions, generator);

            Logger.LogDebugInstructionsFrom(matcher);

            matcher
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldloc_S, (byte)23),
                    new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(Chainloader), nameof(Chainloader.ManagerObject))),
                    new CodeMatch(OpCodes.Ldloc_S, (byte)31),
                    new CodeMatch(OpCodes.Ldloc_S, (byte)23),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(PluginInfo), "TypeName")),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Assembly), nameof(Assembly.GetType), [typeof(string)])),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), nameof(GameObject.AddComponent), [typeof(Type)]))
                )
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, (byte)23),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PluginLoadingHooks), nameof(InvokePre)))
                )
                .Advance(7)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldloc_S, (byte)23),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PluginLoadingHooks), nameof(InvokePost)))
                );

            Logger.LogDebugInstructionsFrom(matcher);

            return matcher.InstructionEnumeration();
        }
#endif
    }
}