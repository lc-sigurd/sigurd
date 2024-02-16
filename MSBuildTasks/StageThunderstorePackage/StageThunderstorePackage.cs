/*
 * Copyright (c) 2024 The Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using MSBuildTasks.Extensions;
using MSBuildTasks.Lib;
using Serilog;

namespace MSBuildTasks.StageThunderstorePackage;

public class StageThunderstorePackage : TaskBase
{
    [Required]
    public ITaskItem[]? Packages { get; set; }

    [Required]
    public string? StagingProfilePath { get; set; }

    private ThunderstorePackageArchive[]? _parsedPackages;

    private DirectoryInfo? _parsedStagingProfilePath;

    [MemberNotNull(nameof(Packages), nameof(StagingProfilePath))]
    private void ValidateInputs()
    {
        if (Packages is not [_, ..])
            throw new ArgumentException("At least one package must be specified for staging.");

        if (String.IsNullOrWhiteSpace(StagingProfilePath))
            throw new ArgumentException("Stage profile path cannot be null, empty, or whitespace.");
    }

    [MemberNotNullWhen(true, nameof(_parsedStagingProfilePath))]
    public bool ParsedStagingProfilePathIsValid => _parsedStagingProfilePath is { Exists: true } && _parsedStagingProfilePath.HasAttributes(FileAttributes.Directory);

    [MemberNotNull(nameof(_parsedStagingProfilePath))]
    private void EnsureValidParsedStageProfilePath()
    {
        if (ParsedStagingProfilePathIsValid) return;
        throw new ArgumentException("Staging profile path does not exist or is not a directory.");
    }

    [MemberNotNull(nameof(_parsedPackages), nameof(_parsedStagingProfilePath))]
    private void ParseInputs()
    {
        ValidateInputs();

        _parsedStagingProfilePath = new DirectoryInfo(StagingProfilePath);
        EnsureValidParsedStageProfilePath();

        _parsedPackages = Packages
            .Select(item => item.ItemSpec)
            .Select(path => new FileInfo(path))
            .Select(fileInfo => new ThunderstorePackageArchive(fileInfo))
            .ToArray();
    }

    public override bool Execute()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.TaskLoggingHelper(Log)
            .CreateLogger();

        Serilog.Log.Information("Plugin staging started.");

        ParseInputs();
        foreach (var packageArchive in _parsedPackages) {
            packageArchive.StageToProfile(_parsedStagingProfilePath);
        }

        return true;
    }
}
