using BepInEx;

namespace SigurdLib.PluginLoader;

public class PluginContainer
{
    public PluginInfo Info { get; }

    public string Guid => Info.Metadata.GUID;

    public string Namespace => Guid;

    public PluginContainer(PluginInfo info)
    {
        Info = info;
    }

    /// <inheritdoc />
    public override string ToString() => $"PluginContainer[guid = {Guid}]";
}
