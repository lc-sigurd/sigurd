/*
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sigurd.Common.Extensions;

public static class StartOfRoundExtensions
{
    public static IDictionary<ScrapItemRarityKey, int> GetScrapItemRarities(this StartOfRound startOfRound)
    {
        if (startOfRound is null)
            throw new ArgumentNullException(nameof(startOfRound));

        var levelScrapItemRarityEntries = startOfRound.levels
            .Select(level => (level.PlanetName, Rarities: level.GetScrapItemRarities()))
            .SelectMany(planetInfo => planetInfo.Rarities.ToDictionary(
                itemRarityEntry => new ScrapItemRarityKey(planetInfo.PlanetName, itemRarityEntry.Key),
                itemRarityEntry => itemRarityEntry.Value
            ));

        var levelScrapItemRarities = new Dictionary<ScrapItemRarityKey, int>(levelScrapItemRarityEntries);
        return new ReadOnlyDictionary<ScrapItemRarityKey, int>(levelScrapItemRarities);
    }

    public static string RenderScrapItemRaritiesByPlanet(this StartOfRound startOfRound)
    {
        if (startOfRound is null)
            throw new ArgumentNullException(nameof(startOfRound));

        var scrapItemRaritiesByPlanet = startOfRound
            .GetScrapItemRarities()
            .GroupBy(entry => entry.Key.PlanetName);

        var renderedPlanetGroupings = scrapItemRaritiesByPlanet.Select(RenderPlanetGrouping);
        return String.Join($"{Environment.NewLine}{Environment.NewLine}", renderedPlanetGroupings);

        string RenderPlanetGrouping(IGrouping<string, KeyValuePair<ScrapItemRarityKey, int>> grouping)
            => $"{grouping.Key}: {{{Environment.NewLine}{String.Join(Environment.NewLine, grouping.Select(RenderRarityEntry))}{Environment.NewLine}}}";

        string RenderRarityEntry(KeyValuePair<ScrapItemRarityKey, int> entry) => $"    {entry.Key.ItemName}: {entry.Value}";
    }

    public static string RenderScrapItemRaritiesByItem(this StartOfRound startOfRound)
    {
        if (startOfRound is null)
            throw new ArgumentNullException(nameof(startOfRound));

        var scrapItemRaritiesByItem = startOfRound
            .GetScrapItemRarities()
            .GroupBy(entry => entry.Key.ItemName);

        var renderedItemGroupings = scrapItemRaritiesByItem.Select(RenderItemGrouping);
        return String.Join($"{Environment.NewLine}{Environment.NewLine}", renderedItemGroupings);

        string RenderItemGrouping(IGrouping<string, KeyValuePair<ScrapItemRarityKey, int>> grouping)
            => $"{grouping.Key}: {{{Environment.NewLine}{String.Join(Environment.NewLine, grouping.Select(RenderRarityEntry))}{Environment.NewLine}}}";

        string RenderRarityEntry(KeyValuePair<ScrapItemRarityKey, int> entry) => $"    {entry.Key.PlanetName}: {entry.Value}";
    }
}

public sealed class ScrapItemRarityKey : Tuple<string, string>
{
    public ScrapItemRarityKey(string planetName, string itemName) : base(planetName, itemName) { }

    public string PlanetName => Item1;

    public string ItemName => Item2;
}
