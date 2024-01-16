using System.Collections.Generic;

namespace Sigurd.ClientAPI.Localization;

/// <summary>
/// A simple localizer.
/// </summary>
public class Localizer
{
    internal Dictionary<string, Locale> locales;

    internal string userLocale = System.Globalization.CultureInfo.CurrentCulture.Name.ToLower();

    internal Localizer(Dictionary<string, Locale> locales)
    {
        this.locales = locales;
    }

    /// <summary>
    /// Gets the current system language's value for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to use.</param>
    /// <param name="defaultValue">The default value if there is no locale file for the language, or if <paramref name="key"/> is not found.</param>
    /// <returns>The defined localized string.</returns>
    public string Get(string key, string defaultValue = "")
    {
        if (!locales.TryGetValue(userLocale, out Locale locale)) return defaultValue;

        return locale.Get(key, defaultValue);
    }

    /// <summary>
    /// Gets the specified <paramref name="locale"/> value for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to use.</param>
    /// <param name="locale">The locale to use.</param>
    /// <param name="defaultValue">The default value if there is no locale file for the language, or if <paramref name="key"/> is not found.</param>
    /// <returns>The defined localized string.</returns>
    public string Get(string key, string locale, string defaultValue = "")
    {
        if (!locales.TryGetValue(locale.ToLower(), out Locale loc)) return defaultValue;

        return loc.Get(key, defaultValue);
    }
}

internal class Locale
{
    internal Dictionary<string, string> pairs;

    internal Locale(Dictionary<string, string> pairings)
    {
        pairs = pairings;
    }

    internal string Get(string key, string defaultValue = "")
    {
        return pairs.TryGetValue(key, out string value) ? value : defaultValue;
    }
}
