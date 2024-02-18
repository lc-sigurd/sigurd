/*
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using Microsoft.Build.Execution;
using MSBuildTasks.Extensions;
using Serilog;

namespace MSBuildTasks;

public abstract class TaskBase : Microsoft.Build.Utilities.Task
{
    private ProjectInstance? _project;
    protected ProjectInstance Project => _project ??= BuildEngine.GetProjectInstance();

    protected string ProjectName => Project.GetPropertyValue("ProjectName");

    protected void InitializeSerilog()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.MSBuildTask(this)
            .CreateLogger();
    }
}
