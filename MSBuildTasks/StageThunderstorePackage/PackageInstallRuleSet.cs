/*
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace MSBuildTasks.StageThunderstorePackage;

public class PackageInstallRuleSet
{
    // For reference:
    // https://github.com/ebkr/r2modmanPlus/tree/34c4cfdf9009849a80aa43848ad5cefc7e7a7ab0/src/r2mm/installing/default_installation_rules/game_rules/InstallRules_BepInex.ts
    // Copyright (c) 2020 Cade Ayres
    public static PackageInstallRuleSet DefaultBepInExInstallRules = new() {
        Rules = new List<PackageInstallRule> {
            new() {
                Route = Path.Join("BepInEx", "plugins"),
                ImplicitFileExtensions = [".dll"],
                TrackingMethod = TrackingMethod.Subdirectory,
                IsDefaultLocation = true,
            },
            new() {
                Route = Path.Join("BepInEx", "core"),
                TrackingMethod = TrackingMethod.Subdirectory
            },
            new() {
                Route = Path.Join("BepInEx", "patchers"),
                TrackingMethod = TrackingMethod.Subdirectory
            },
            new() {
                Route = Path.Join("BepInEx", "monomod"),
                ImplicitFileExtensions = [".mm.dll"],
                TrackingMethod = TrackingMethod.Subdirectory
            },
            new() {
                Route = Path.Join("BepInEx", "config"),
                TrackingMethod = TrackingMethod.None
            }
        }.AsReadOnly(),
    };

    public required IList<PackageInstallRule> Rules { get; init; }

    private PackageInstallRule? _defaultLocationRule;
    public PackageInstallRule DefaultLocationRule => _defaultLocationRule ??= Rules.First(rule => rule.IsDefaultLocation);

    public PackageInstallRule? MatchImplicitRule(ZipArchiveEntry entry)
        => Rules.SelectMany(rule => rule.ImplicitFileExtensions.Select(extension => (extension, rule)))
            .Where(item => entry.FullName.EndsWith(item.extension))
            .OrderByDescending(item => item.extension.Length)
            .Select(item => item.rule)
            .FirstOrDefault();

    public PackageInstallRule? MatchRouteRule(ZipArchiveEntry entry)
        => Rules.FirstOrDefault(rule => entry.FullName.StartsWith(rule.Route));

    public class PackageInstallRule
    {
        public required string Route { get; init; }

        public string[] ImplicitFileExtensions { get; init; } = Array.Empty<string>();

        public required TrackingMethod TrackingMethod { get; init; }

        public bool IsDefaultLocation { get; init; } = false;

        public string GetEffectiveRoute(ThunderstorePackageArchive archive) => TrackingMethod switch {
            TrackingMethod.Subdirectory => Path.Join(Route, archive.PackageIdentifier),
            TrackingMethod.None => Route,
            _ => throw new ArgumentOutOfRangeException()
        };

        public override string ToString() => $"PackageInstallRule[{Route}]";
    }

    public enum TrackingMethod
    {
        Subdirectory,
        None,
    }
}
