using System;
using BepInEx.Bootstrap;
using HarmonyLib;
using JetBrains.Annotations;
using Serilog;
using Sigurd.Util;
using Sigurd.Util.Extensions;

namespace Sigurd.PluginLoader;

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

        ConfigureAutomaticEventSubscriber();
    }

    static void ConfigureAutomaticEventSubscriber()
    {
        var autoSubscriber = new AutomaticEventSubscriber(Logger);

        ChainloaderHooks.Plugin.OnPreLoad += (sender, args) => {
            autoSubscriber.Inject(args.PluginContainer, Side.Client);
        };

        ChainloaderHooks.OnComplete += (sender, args) => {
            autoSubscriber.WarnOfIgnoredSubscribers();
        };
    }
}
