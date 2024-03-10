using System;
using BepInEx.Bootstrap;
using HarmonyLib;
using JetBrains.Annotations;
using Serilog;
using SigurdLib.Util.Extensions;

namespace SigurdLib.PluginLoader;

internal static class Entrypoint
{
    private static ILogger? _logger;
    internal static ILogger Logger => _logger ??= CreateLogger();

    private static ILogger CreateLogger() => new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.BepInExLogger(PluginLoaderInfo.PRODUCT_NAME)
        .CreateLogger();

    [UsedImplicitly]
    static void Start()
    {
        var harmony = new Harmony(PluginLoaderInfo.PRODUCT_GUID);
        try {
            harmony.PatchAllNestedTypesOnly(typeof(ChainloaderHooks));
        }
        catch (Exception exc) {
            Logger.Fatal(exc, "Failed to patch {MethodName}", nameof(Chainloader.Start));
        }
    }
}
