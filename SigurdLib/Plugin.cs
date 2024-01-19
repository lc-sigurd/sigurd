using BepInEx;

namespace Sigurd;

/// <summary>
/// The main Plugin class.
/// </summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Common.Plugin.Guid, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ClientAPI.Plugin.Guid, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(Networking.Plugin.Guid, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ServerAPI.Plugin.Guid, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin
{
}
