/*
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using MonoMod.Cil;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace SigurdLib.PluginLoader;

internal static class ChainloaderHooks
{
    public class EventArgs : System.EventArgs;

    public class StartEventArgs : EventArgs
    {
        public required IDictionary<string, PluginInfo> PluginsByGuid { get; init; }
        public required IList<string> OrderedPluginGuids { get; init; }
    };

    public class CompleteEventArgs : EventArgs;

    public static class Plugin
    {
        /// <inheritdoc />
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public class EventArgs : ChainloaderHooks.EventArgs
        {
            public required PluginContainer PluginContainer { get; init; }
        }

        /// <summary>
        /// Event that is invoked immediately before the <see cref="Chainloader"/> attempts
        /// to load a plugin.
        /// </summary>
        public static event EventHandler<EventArgs>? OnPreLoad;

        /// <summary>
        /// Event that is invoked immediately after a plugin has successfully loaded.
        /// </summary>
        public static event EventHandler<EventArgs>? OnPostLoad;

        internal static void InvokePre(PluginInfo pluginInfo)
        {
            var container = new PluginContainer(pluginInfo);
            PluginList.Instance.AddLoadingPluginContainer(container);
            PluginLoadingContext.Instance.ActiveContainer = container;
            InvokePhaseSafely(
                OnPreLoad,
                new EventArgs { PluginContainer = container },
                $"pre-load initialization for {pluginInfo}"
            );
        }

        internal static void InvokePost(PluginInfo pluginInfo)
        {
            var container = PluginList.Instance.GetPluginContainerByGuidOrThrow(pluginInfo.Metadata.GUID);
            InvokePhaseSafely(
                OnPostLoad,
                new EventArgs { PluginContainer = container },
                $"post-load initialization for {pluginInfo}"
            );
            PluginList.Instance.SetPluginLoaded(container);
            PluginLoadingContext.Instance.ActiveContainer = null;
        }
    }

    /// <summary>
    /// Event that is invoked before BepInEx attempts to load any plugins; just after
    /// <see cref="Chainloader.PluginInfos"/> has been populated.
    /// </summary>
    public static event EventHandler<StartEventArgs>? OnStart;

    /// <summary>
    /// Event that is invoked after all plugins have loaded; just before <see cref="Chainloader._loaded"/>
    /// is set to <see langword="true"/>.
    /// </summary>
    public static event EventHandler<CompleteEventArgs>? OnComplete;

    internal static ManualLogSource Logger = null!;

    [UsedImplicitly]
    static void Start()
    {
        Logger = BepInEx.Logging.Logger.CreateLogSource(PluginLoaderInfo.PRODUCT_NAME);

        var harmony = new Harmony(PluginLoaderInfo.PRODUCT_GUID);
        harmony.PatchAll(typeof(ChainloaderStartPatches));
    }

    private static void InvokePhaseSafely<T>(EventHandler<T>? @event, T eventArgs, string phase, LogLevel level = LogLevel.Debug)
    {
        try {
            InvokePhase(@event, eventArgs, phase, level);
        }
        catch (Exception exc) {
            Logger.LogError(exc);
        }
    }

    private static void InvokePhase<T>(EventHandler<T>? @event, T eventArgs, string phase, LogLevel level = LogLevel.Debug)
    {
        if (@event is null || @event.GetInvocationList() is not [_, ..] delegates) {
            Log($"Skipped {phase} as no actionable tasks were found");
            return;
        }

        Log($"Starting {phase}");
        var exceptions = new LinkedList<Exception>();

        foreach (var @delegate in delegates) {
            if (@delegate is not EventHandler<T> handler) {
                Logger.LogWarning($"Skipping invocation of {phase} listener {@delegate} as it doesn't match its expected signature");
                continue;
            }

            try {
                handler.Invoke(null, eventArgs);
            }
            catch (Exception exception) {
                exceptions.AddLast(exception);
            }
        }

        if (exceptions.Count > 0) {
            throw new AggregateException($"SigurdLib {phase} failed due to potentially multiple exceptions", exceptions);
        }

        Log($"Completed {phase}");

        void Log(string message) => Logger.Log(level, message);
    }

    internal static void InvokeStart(Dictionary<string, PluginInfo> pluginsByGuid, List<string> orderedPluginGuids)
    {
        PluginList.Instance.OrderedPluginInfos = orderedPluginGuids
            .Select(guid => pluginsByGuid[guid])
            .ToArray();

        InvokePhaseSafely(
            OnStart,
            new StartEventArgs {
                PluginsByGuid = new ReadOnlyDictionary<string, PluginInfo>(pluginsByGuid),
                OrderedPluginGuids = new ReadOnlyCollection<string>(orderedPluginGuids),
            },
            "pre-startup initialization",
            LogLevel.Info
        );
    }

    internal static void InvokeComplete()
    {
        InvokePhaseSafely(
            OnComplete,
            new CompleteEventArgs { },
            "post-startup initialization",
            LogLevel.Info
        );
    }

    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    static class ChainloaderStartPatches
    {
        [HarmonyILManipulator]
        public static void Manipulate(ILContext ilContext, MethodBase original, ILLabel retLabel)
        {
            var cursor = new ILCursor(ilContext);

            cursor
                // Match ahead to just before the part where the actual `AddComponent<PluginType>()` is
                .GotoNext(
                    instr => instr.MatchLdloc(23),
                    instr => instr.MatchCall(AccessTools.PropertyGetter(typeof(Chainloader), nameof(Chainloader.ManagerObject))),
                    instr => instr.MatchLdloc(31),
                    instr => instr.MatchLdloc(23),
                    instr => instr.MatchCallvirt(AccessTools.PropertyGetter(typeof(PluginInfo), "TypeName")),
                    instr => instr.MatchCallvirt(AccessTools.Method(typeof(Assembly), nameof(Assembly.GetType), [typeof(string)])),
                    instr => instr.MatchCallvirt(AccessTools.Method(typeof(GameObject), nameof(GameObject.AddComponent), [typeof(Type)]))
                )
                // Load the current plugin info from local variables
                .Emit(OpCodes.Ldloc_S, (byte)23)
                // Invoke `OnPreLoad` event
                .Emit(OpCodes.Call, AccessTools.Method(typeof(Plugin), nameof(Plugin.InvokePre)));

            // Jump over the `AddComponent<PluginType>()` block
            cursor.Index += 7;

            cursor
                // Load the current plugin info from local variables
                .Emit(OpCodes.Ldloc_S, (byte)23)
                // Invoke `OnPostLoad` event
                .Emit(OpCodes.Call, AccessTools.Method(typeof(Plugin), nameof(Plugin.InvokePost)));

            cursor
                // Match ahead to where `Chainloader._loaded` is set to `true`
                .GotoNext(
                    instr => instr.MatchLdcI4(1),
                    instr => instr.MatchStsfld(AccessTools.Field(typeof(Chainloader), "_loaded"))
                )
                // Invoke `OnComplete` event
                .Emit(OpCodes.Call, AccessTools.Method(typeof(ChainloaderHooks), nameof(InvokeComplete)));
        }
    }
}
