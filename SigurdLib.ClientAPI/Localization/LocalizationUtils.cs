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
    /// <param name="path">The path of the locales. If left blank, will use (PluginInfo.Location)/locales</param>
    /// <returns>A <see cref="Localizer"/>.</returns>
    /// <exception cref="Exception">Thrown when locales directory is not found.</exception>
    public static Localizer RegisterLocales(string path = "")
    {
        if (path == string.Empty)
        {
            MethodBase m = new StackTrace().GetFrame(1).GetMethod();
            Assembly assembly = m.ReflectedType.Assembly;

            BepInPlugin attribute = (assembly.GetTypes().First(t => t.IsClass && !t.IsAbstract && typeof(BaseUnityPlugin).IsAssignableFrom(t)).GetCustomAttribute(typeof(BepInPlugin)) as BepInPlugin)!;

            PluginInfo pluginInfo = Chainloader.PluginInfos[attribute.GUID];

            path = Path.Combine(Path.GetDirectoryName(pluginInfo.Location), "locales");
        }

        if (!Directory.Exists(path)) throw new Exception($"Locales directory not found at: {path}");

        Dictionary<string, Locale> locales = new Dictionary<string, Locale>();

        foreach (string filePath in Directory.GetFiles(path, "*.json"))
        {
            locales.Add(Path.GetFileNameWithoutExtension(filePath).ToLower(), new Locale(JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(filePath))!));
        }

        return new Localizer(locales);
    }
}
