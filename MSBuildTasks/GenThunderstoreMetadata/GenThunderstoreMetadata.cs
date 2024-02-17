/*
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using MSBuildTasks.Extensions;
using ThunderstoreCLI.Models;

namespace MSBuildTasks.GenThunderstoreMetadata;

public sealed class GenThunderstoreMetadata : TaskBase
{
    public string ConfigurationFileSchemaVersion { get; set; } = "0.0.1";

    [Required]
    public string ConfigurationFileOutputPath { get; set; } = null!;

    [Required]
    public string PackageNamespace { get; set; } = null!;

    [Required]
    public string PackageName { get; set; } = null!;

    [Required]
    public string PackageDescription { get; set; } = null!;

    [Required]
    public string PackageVersion { get; set; } = null!;

    [Required]
    public string PackageWebsiteUrl { get; set; } = null!;

    [Required]
    public bool PackageContainsNsfwContent { get; set; }

    [Required]
    public ITaskItem[] PackageDependencies { get; set; } = null!;

    [Required]
    public string BuildReadmePath { get; set; } = null!;

    [Required]
    public string BuildIconPath { get; set; } = null!;

    [Required]
    public string BuildOutDir { get; set; } = null!;

    [Required]
    public ITaskItem[] BuildCopyPaths { get; set; } = null!;

    public string PublishRepository { get; set; } = "https://thunderstore.io";

    [Required]
    public ITaskItem[] PublishCommunities { get; set; } = null!;

    public override bool Execute()
    {
        InitializeSerilog();

        Serilog.Log.Information("Generating {ProjectName:l} Thunderstore package meta-manifest...", ProjectName);

        var project = new ThunderstoreProject {
            Config = new ThunderstoreProject.ConfigData {
                SchemaVersion = ConfigurationFileSchemaVersion
            },
            Package = new ThunderstoreProject.PackageData {
                Name = PackageName,
                Namespace = PackageNamespace,
                Description = PackageDescription,
                VersionNumber = PackageVersion,
                WebsiteUrl = PackageWebsiteUrl,
                ContainsNsfwContent = PackageContainsNsfwContent,
                Dependencies = PackageDependencies
                    .Select(ThunderstorePackageDependency.FromTaskItem)
                    .ToDictionary(
                        dep => dep.Moniker.FullName,
                        dep => dep.Moniker.Version.ToVersion().ToString()
                    ),
            },
            Build = new ThunderstoreProject.BuildData {
                Readme = new FileInfo(BuildReadmePath)
                    .GetFullNameRelativeToFile(ConfigurationFileOutputPath),
                Icon = new FileInfo(BuildIconPath)
                    .GetFullNameRelativeToFile(ConfigurationFileOutputPath),
                OutDir = new DirectoryInfo(BuildOutDir)
                    .GetFullNameRelativeToFile(ConfigurationFileOutputPath),
                CopyPaths = BuildCopyPaths
                    .Select(ThunderstoreProject.BuildData.CopyPath.FromTaskItem)
                    .Select(item => item.MakeRelativeToFile(ConfigurationFileOutputPath))
                    .ToArray()
            },
            Publish = new ThunderstoreProject.PublishData {
                Repository = PublishRepository,
                Communities = PublishCommunities
                    .Select(item => item.ItemSpec)
                    .ToHashSet()
                    .ToArray(),
            }
        };

        var communityCategories = project.Publish.Communities
            .ToDictionary(
                community => community,
                _ => new HashSet<string>()
            );

        foreach (var item in PublishCommunities) {
            var itemCategories = item
                .GetMetadata("CategorySlugs")
                .Split(";")
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .ToHashSet();
            communityCategories[item.ItemSpec]
                .UnionWith(itemCategories);
        }

        project.Publish.Categories = new ThunderstoreProject.CategoryDictionary {
            Categories = communityCategories
                .Select(item => (item.Key, item.Value.ToArray()))
                .ToDictionary()
        };

        Directory.CreateDirectory(Path.GetDirectoryName(ConfigurationFileOutputPath)!);
        File.WriteAllText(ConfigurationFileOutputPath, project.Serialize());
        Serilog.Log.Information("Successfully generated {ConfigurationFileName:l} for {ProjectName:l}", Path.GetFileName(ConfigurationFileOutputPath), ProjectName);
        return true;
    }
}
