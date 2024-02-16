/*
 * https://github.com/thunderstore-io/thunderstore-cli/blob/10b73c843f2dd1a9ed9c6cb687dbbaa555626052/ThunderstoreCLI/Models/ThunderstoreProject.cs
 * thunderstore-cli Copyright (c) 2021 Thunderstore.
 * Thunderstore expressly permits Lordfirespeed to use and redistribute the source of thunderstore-cli as Lordfirespeed sees fit.
 * Lordfirespeed licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

namespace ThunderstoreCLI.Models;

[TomlDoNotInlineObject]
public class ThunderstoreProject : BaseToml<ThunderstoreProject>
{
    public struct CategoryDictionary
    {
        public Dictionary<string, string[]> Categories;
    }

    static ThunderstoreProject()
    {
        TomletMain.RegisterMapper(
            dict => TomletMain.ValueFrom(dict.Categories),
            toml => toml switch
            {
                TomlArray arr => new CategoryDictionary
                {
                    Categories = new Dictionary<string, string[]>
                    {
                        { "", arr.ArrayValues.Select(v => v.StringValue).ToArray() }
                    }
                },
                TomlTable table => new CategoryDictionary { Categories = TomletMain.To<Dictionary<string, string[]>>(table) },
                _ => throw new NotSupportedException()
            });
    }

    [TomlDoNotInlineObject]
    public class ConfigData
    {
        [TomlProperty("schemaVersion")]
        public string SchemaVersion { get; set; } = "0.0.1";
    }

    [TomlProperty("config")]
    public ConfigData? Config { get; set; } = new();

    [TomlDoNotInlineObject]
    public class PackageData
    {
        [TomlProperty("namespace")]
        public string Namespace { get; set; } = "AuthorName";
        [TomlProperty("name")]
        public string Name { get; set; } = "PackageName";
        [TomlProperty("versionNumber")]
        public string? VersionNumber { get; set; }
        [TomlProperty("description")]
        public string Description { get; set; } = "Example mod description";
        [TomlProperty("websiteUrl")]
        public string WebsiteUrl { get; set; } = "https://thunderstore.io";
        [TomlProperty("containsNsfwContent")]
        public bool ContainsNsfwContent { get; set; } = false;
        [TomlProperty("dependencies")]
        [TomlDoNotInlineObject]
        public Dictionary<string, string> Dependencies { get; set; } = new() { { "AuthorName-PackageName", "0.0.1" } };
    }
    [TomlProperty("package")]
    public PackageData? Package { get; set; }

    [TomlDoNotInlineObject]
    public class BuildData
    {
        [TomlProperty("icon")]
        public string Icon { get; set; } = "./icon.png";
        [TomlProperty("readme")]
        public string Readme { get; set; } = "./README.md";
        [TomlProperty("outdir")]
        public string OutDir { get; set; } = "./build";

        [TomlDoNotInlineObject]
        public class CopyPath
        {
            [TomlProperty("source")]
            public string Source { get; set; } = "./dist";
            [TomlProperty("target")]
            public string Target { get; set; } = "";

            public static CopyPath FromTaskItem(ITaskItem item)
            {
                return new() {
                    Source = item.ItemSpec,
                    Target = item.GetMetadata("Destination"),
                };
            }

            public CopyPath MakeRelativeToFile(string path)
            {
                return MakeRelativeTo(Path.GetDirectoryName(path)!);
            }

            public CopyPath MakeRelativeTo(string directory)
            {
                Source = Path.GetRelativePath(directory, Source);
                return this;
            }
        }

        [TomlProperty("copy")]
        public CopyPath[] CopyPaths { get; set; } = new CopyPath[] { new CopyPath() };
    }
    [TomlProperty("build")]
    public BuildData? Build { get; set; }

    [TomlDoNotInlineObject]
    public class PublishData
    {
        [TomlProperty("repository")]
        public string Repository { get; set; } = "https://thunderstore.io";
        [TomlProperty("communities")]
        public string[] Communities { get; set; } =
        {
            "riskofrain2"
        };

        [TomlProperty("categories")]
        [TomlDoNotInlineObject]
        public CategoryDictionary Categories { get; set; } = new()
        {
            Categories = new Dictionary<string, string[]>
            {
                { "riskofrain2", new[] { "items", "skills" } }
            }
        };
    }
    [TomlProperty("publish")]
    public PublishData? Publish { get; set; }

    [TomlDoNotInlineObject]
    public class InstallData
    {
        [TomlDoNotInlineObject]
        public class InstallerDeclaration
        {
            [TomlProperty("identifier")]
            public string? Identifier { get; set; }
        }

        [TomlProperty("installers")]
        public InstallerDeclaration[] InstallerDeclarations { get; set; } = Array.Empty<InstallerDeclaration>();
    }
    [TomlProperty("install")]
    public InstallData? Install { get; set; }

    public ThunderstoreProject() { }

    public ThunderstoreProject(bool initialize)
    {
        if (!initialize)
            return;

        Package = new PackageData();
        Build = new BuildData();
        Publish = new PublishData();
        Install = new InstallData();
    }
}
