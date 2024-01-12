using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace Sigurd;

/// <summary>
/// The main Plugin class.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Common.MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ClientAPI.MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Networking.MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ServerAPI.MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin
{
}
