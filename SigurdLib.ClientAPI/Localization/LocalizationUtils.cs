using BepInEx;
using BepInEx.Bootstrap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sigurd.ClientAPI.Localization;

/// <summary>
/// Provides an easy-to-use localization file registration.
/// </summary>
public static class LocalizationUtils
{
    /// <summary>
    /// Register locale files from a specific directory.
    /// </summary>
    /// <param name="path">The path of the locales. If left blank, will use (PluginInfo.Location)/locales/(CurrentCulture.Name)</param>
    /// <returns>A <see cref="Locale"/>.</returns>
    /// <exception cref="Exception">Thrown when locale path is not found.</exception>
    public static Locale RegisterLocale(string path = "")
    {
        if (path.IsNullOrWhiteSpace())
        {
            MethodBase m = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = m.ReflectedType.Assembly;

            BepInPlugin attribute = (assembly.GetTypes().First(t => t.IsClass && !t.IsAbstract && typeof(BaseUnityPlugin).IsAssignableFrom(t)).GetCustomAttribute(typeof(BepInPlugin)) as BepInPlugin)!;

            PluginInfo pluginInfo = Chainloader.PluginInfos[attribute.GUID];

            path = Path.Combine(Path.GetDirectoryName(pluginInfo.Location), "locales", System.Globalization.CultureInfo.CurrentCulture.Name.ToLower(), ".json");
        }

        if (!File.Exists(path)) throw new Exception($"Locale file not found at: {path}");

        return new Locale(JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path))!);
    }
}
