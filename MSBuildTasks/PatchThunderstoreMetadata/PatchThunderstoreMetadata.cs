using Microsoft.Build.Framework;
using NetcodePatcher.MSBuild;
using Serilog;

namespace MSBuildTasks.PatchThunderstoreMetadata;

public sealed class PatchThunderstoreMetadata : Microsoft.Build.Utilities.Task
{
    [Required]
    public string InputPath { get; set; }

    [Required]
    public string OutputPath { get; set; }

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

        var project = ThunderstoreCLI.Models.ThunderstoreProject.Deserialize(File.ReadAllText(InputPath));
        if (project?.Package is null)
            return false;

        var dependenciesToAdd = Dependencies.Select(ThunderstorePackageDependency.FromTaskItem);

        foreach (var dependency in dependenciesToAdd)
        {
            Serilog.Log.Information("Dependency found: {Dependency}", dependency);
            project.Package.Dependencies.Add(dependency.Moniker.FullName, dependency.Moniker.Version.ToVersion().ToString());
        }

        File.WriteAllText(OutputPath, project.Serialize());
        return true;
    }
}
