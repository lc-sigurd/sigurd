/*
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System.Reflection;
using BepInEx;

namespace Sigurd.PluginLoader;

public class PluginContainer
{
    public PluginInfo Info { get; }

    public Assembly Assembly { get; }

    public string Guid => Info.Metadata.GUID;

    public string Namespace => Guid;

    public PluginContainer(PluginInfo info, Assembly assembly)
    {
        Info = info;
        Assembly = assembly;
    }

    /// <inheritdoc />
    public override string ToString() => $"PluginContainer[guid = {Guid}]";
}
