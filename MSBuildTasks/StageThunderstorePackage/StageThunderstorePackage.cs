/*
 * Copyright (c) 2024 The Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuildTasks.Extensions;
using NetcodePatcher.MSBuild;
using Serilog;

namespace MSBuildTasks.StageThunderstorePackage;

public class StageThunderstorePackage : Task
{
    [Required]
    public ITaskItem[]? Packages { get; set; }

    [Required]
    public string? StageProfilePath { get; set; }

    private ThunderstorePackageArchive[]? _parsedPackages;

    private DirectoryInfo? _parsedStageProfilePath;

    [MemberNotNull(nameof(Packages), nameof(StageProfilePath))]
    private void ValidateInputs()
    {
        if (Packages is not [_, ..])
            throw new ArgumentException("At least one package must be specified for staging.");

        if (String.IsNullOrWhiteSpace(StageProfilePath))
            throw new ArgumentException("Stage profile path cannot be null, empty, or whitespace.");
    }

    [MemberNotNullWhen(true, nameof(_parsedStageProfilePath))]
    public bool ParsedStageProfilePathIsValid => _parsedStageProfilePath is { Exists: true } && _parsedStageProfilePath.HasAttributes(FileAttributes.Directory);

    [MemberNotNull(nameof(_parsedStageProfilePath))]
    private void EnsureValidParsedStageProfilePath()
    {
        if (ParsedStageProfilePathIsValid) return;
        throw new ArgumentException("Stage profile path does not exist or is not a directory.");
    }

    [MemberNotNull(nameof(_parsedPackages), nameof(_parsedStageProfilePath))]
    private void ParseInputs()
    {
        ValidateInputs();

        _parsedStageProfilePath = new DirectoryInfo(StageProfilePath);
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
            packageArchive.StageToProfile(_parsedStageProfilePath);
        }

        return true;
    }
}
