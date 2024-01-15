using Microsoft.Build.Framework;
using NetcodePatcher.MSBuild;
using Serilog;
using ThunderstoreCLI.Models;

namespace MSBuildTasks.PatchThunderstoreMetadata;

public sealed class PatchThunderstoreMetadata : Microsoft.Build.Utilities.Task
{
    [Required]
    public string ConfigurationFileInputPath { get; set; }

    [Required]
    public string ConfigurationFileOutputPath { get; set; }

    [Required]
    public string PackageNamespace { get; set; }

    [Required]
    public string PackageName { get; set; }

    [Required]
    public string PackageDescription { get; set; }

    [Required]
    public string PackageVersion { get; set; }

    [Required]
    public string PackageWebsiteUrl { get; set; }

    [Required]
    public bool PackageContainsNsfwContent { get; set; }

    [Required]
    public ITaskItem[] PackageDependencies { get; set; }

    [Required]
    public string BuildReadmePath { get; set; }

    [Required]
    public string BuildIconPath { get; set; }

    [Required]
    public string BuildOutDir { get; set; }

    [Required]
    public ITaskItem[] BuildCopyEntries { get; set; }

    public override bool Execute()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.TaskLoggingHelper(Log)
            .CreateLogger();

        Serilog.Log.Information("Plugin meta-manifest patcher started.");

        var project = ThunderstoreProject.Deserialize(File.ReadAllText(ConfigurationFileInputPath));
        if (project is null) {
            Serilog.Log.Fatal("Couldn't read project file.");
            return false;
        }

        project.Package = new ThunderstoreProject.PackageData {
            Name = PackageName,
            Namespace = PackageNamespace,
            Description = PackageDescription,
            VersionNumber = PackageVersion,
            WebsiteUrl = PackageWebsiteUrl,
            ContainsNsfwContent = PackageContainsNsfwContent,
            Dependencies = [],
        };

        var dependenciesToAdd = PackageDependencies.Select(ThunderstorePackageDependency.FromTaskItem);

        foreach (var dependency in dependenciesToAdd)
        {
            Serilog.Log.Information("Dependency found: {Dependency}", dependency);
            project.Package.Dependencies.Add(dependency.Moniker.FullName, dependency.Moniker.Version.ToVersion().ToString());
        }

        project.Build = new ThunderstoreProject.BuildData {
            Readme = BuildReadmePath,
            Icon = BuildIconPath,
            OutDir = BuildOutDir,
            CopyPaths = [],
        };

        File.WriteAllText(ConfigurationFileOutputPath, project.Serialize());
        return true;
    }
}
