using Microsoft.Build.Framework;
using NetcodePatcher.MSBuild;
using Serilog;

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
    public string Description { get; set; }

    [Required]
    public string Version { get; set; }

    [Required]
    public ITaskItem[] Dependencies { get; set; }

    public override bool Execute()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.TaskLoggingHelper(Log)
            .CreateLogger();

        Serilog.Log.Information("Plugin meta-manifest patcher started. There are {DepCount} items to consider", Dependencies.Length);

        var project = ThunderstoreCLI.Models.ThunderstoreProject.Deserialize(File.ReadAllText(ConfigurationFileInputPath));
        if (project?.Package is null)
            return false;

        var dependenciesToAdd = Dependencies.Select(ThunderstorePackageDependency.FromTaskItem);

        foreach (var dependency in dependenciesToAdd)
        {
            Serilog.Log.Information("Dependency found: {Dependency}", dependency);
            project.Package.Dependencies.Add(dependency.Moniker.FullName, dependency.Moniker.Version.ToVersion().ToString());
        }

        project.Package.Namespace = PackageNamespace;
        project.Package.Name = PackageName;
        project.Package.Description = Description;

        File.WriteAllText(ConfigurationFileOutputPath, project.Serialize());
        return true;
    }
}
