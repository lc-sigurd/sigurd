using System.Collections.Generic;

namespace Sigurd.ClientAPI.Localization;

/// <summary>
/// A locale container
/// </summary>
public class Locale
{
    internal Dictionary<string, string> pairs;

    internal Locale(Dictionary<string, string> pairings)
    {
        pairs = pairings;
    }

    /// <summary>
    /// Gets a localized string from a key with an optional default value.
    /// </summary>
    /// <param name="key">The key in the json file to retrieve.</param>
    /// <param name="defaultValue">An optional default value if not found.</param>
    /// <returns>A localized string.</returns>
    public string Get(string key, string defaultValue = "")
    {
        if (pairs.TryGetValue(key, out string value)) return value;

        if (!string.IsNullOrWhiteSpace(defaultValue)) return defaultValue;

        return key;
    }
}
