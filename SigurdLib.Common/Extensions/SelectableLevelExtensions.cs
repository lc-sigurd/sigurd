/*
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sigurd.Common.Extensions;

public static class SelectableLevelExtensions
{
    public static IDictionary<string, int> GetScrapItemRarities(this SelectableLevel level)
    {
        if (level is null)
            throw new ArgumentNullException(nameof(level));

        var rarities = level.spawnableScrap
            .ToDictionary(
                itemWithRarity => itemWithRarity.spawnableItem.itemName,
                itemWithRarity => itemWithRarity.rarity
            );

        return new ReadOnlyDictionary<string, int>(rarities);
    }
}
