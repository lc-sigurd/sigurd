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
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using MonoMod.Cil;
using Serilog.Events;
using UnityEngine;
using ILogger = Serilog.ILogger;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace SigurdLib.PluginLoader;

internal static class ChainloaderHooks
{
    private static ILogger Logger => Entrypoint.Logger;

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

    private static void InvokePhaseSafely<T>(EventHandler<T>? @event, T eventArgs, string phase, LogEventLevel level = LogEventLevel.Debug)
    {
        try {
            InvokePhase(@event, eventArgs, phase, level);
        }
        catch (Exception exc) {
            Logger.Error(exc, "An error occurred during {Phase}", phase);
        }
    }

    private static void InvokePhase<T>(EventHandler<T>? @event, T eventArgs, string phase, LogEventLevel level = LogEventLevel.Debug)
    {
        if (@event is null || @event.GetInvocationList() is not [_, ..] delegates) {
            Logger.Write(level, "Skipped {Phase} as no actionable tasks were found", phase);
            return;
        }

        Logger.Write(level, "Starting {Phase}", phase);
        var exceptions = new LinkedList<Exception>();

        foreach (var @delegate in delegates) {
            if (@delegate is not EventHandler<T> handler) {
                Logger.Warning($"Skipping invocation of {phase} listener {@delegate} as it doesn't match its expected signature");
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

        Logger.Write(level, "Completed {Phase}", phase);
    }

    internal static void InvokeStart(List<string> orderedPluginGuids, Dictionary<string, PluginInfo> pluginsByGuid)
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
            LogEventLevel.Information
        );
    }

    internal static void InvokeComplete()
    {
        InvokePhaseSafely(
            OnComplete,
            new CompleteEventArgs { },
            "post-startup initialization",
            LogEventLevel.Information
        );
    }

    [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
    static class ChainloaderStartPatches
    {
        [HarmonyILManipulator]
        public static void Manipulate(ILContext ilContext, MethodBase original, ILLabel retLabel)
        {
            var cursor = new ILCursor(ilContext);

            var pluginsByGuidFieldReference = new FieldReference(
                "pluginsByGUID",
                ilContext.Module
                    .ImportReference(typeof(Dictionary<,>))
                    .MakeGenericInstanceType(
                        ilContext.Module.TypeSystem.String,
                        ilContext.Module.ImportReference(typeof(PluginInfo))
                    ),
                ilContext.Method.Body.Variables[0].VariableType
            );

            cursor
                // Match ahead to just after the ordered plugin GUID list is computed
                .GotoNext(
                    MoveType.After,
                    instr => instr.MatchCall(AccessTools.Method(typeof(Enumerable), nameof(Enumerable.ToList), generics: [ typeof(string), ]))
                )
                // Duplicate the reference to the ordered plugin GUID list
                .Emit(OpCodes.Dup)
                // Load the 'V_0' local of a compiler-generated type
                .Emit(OpCodes.Ldloc_0)
                // Load the `pluginsByGUID` field of the compiler-generated type
                .Emit(OpCodes.Ldfld, pluginsByGuidFieldReference)
                // Invoke `OnStart` event
                .Emit(OpCodes.Call, AccessTools.Method(typeof(ChainloaderHooks), nameof(InvokeStart)))
                // Match ahead to just before the actual `AddComponent<PluginType>()`
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
                // Match ahead to just before `Chainloader._loaded` is set to `true`
                .GotoNext(
                    instr => instr.MatchLdcI4(1),
                    instr => instr.MatchStsfld(AccessTools.Field(typeof(Chainloader), "_loaded"))
                )
                // Invoke `OnComplete` event
                .Emit(OpCodes.Call, AccessTools.Method(typeof(ChainloaderHooks), nameof(InvokeComplete)));
        }
    }
}
