/*
 * Copyright (c) 2024 The Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;

namespace MSBuildTasks.Extensions;

// https://stackoverflow.com/a/6086148/11045433
public static class BuildEngineExtensions
{
    private const BindingFlags BindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

    public static ProjectInstance GetProjectInstance(this IBuildEngine buildEngine)
    {
        var buildEngineType = buildEngine.GetType();
        var targetBuilderCallbackField = buildEngineType.GetField("_targetBuilderCallback", BindingFlags);
        if (targetBuilderCallbackField is null)
            throw new InvalidOperationException($"Could not extract _targetBuilderCallback from {buildEngineType.FullName}");

        var targetBuilderCallback = targetBuilderCallbackField.GetValue(buildEngine);
        var targetCallbackType = targetBuilderCallback!.GetType();
        var projectInstanceField = targetCallbackType.GetField("_projectInstance", BindingFlags);

        if (projectInstanceField is null)
            throw new InvalidOperationException($"Could not extract _projectInstance from {targetCallbackType.FullName}");

        if (projectInstanceField.GetValue(targetBuilderCallback) is not ProjectInstance project)
            throw new InvalidOperationException();

        return project;
    }
}
