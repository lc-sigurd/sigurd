using Microsoft.Build.Framework;
using NetcodePatcher.MSBuild;
using Serilog;
using ThunderstoreCLI.Models;

namespace MSBuildTasks.PatchThunderstoreMetadata;

public sealed class PatchThunderstoreMetadata : Microsoft.Build.Utilities.Task
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
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.TaskLoggingHelper(Log)
            .CreateLogger();

        Serilog.Log.Information("Plugin meta-manifest patcher started.");

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
                Readme = BuildReadmePath,
                Icon = BuildIconPath,
                OutDir = BuildOutDir,
                CopyPaths = BuildCopyPaths
                    .Select(ThunderstoreProject.BuildData.CopyPath.FromTaskItem)
                    .Select(item => item.MakeRelativeToFile(ConfigurationFileOutputPath))
                    .ToArray()
            },
        };

        File.WriteAllText(ConfigurationFileOutputPath, project.Serialize());
        return true;
    }
}
