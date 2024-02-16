/*
 * The contents of this file have been sourced from various projects, in addition to contributions by the Sigurd Team.
 *
 * https://github.com/serilog-contrib/serilog-sinks-msbuild/blob/08bfd97b4ac0523a2b19407ba2325e75b22d6d84/Serilog.Sinks.MSBuild/MSBuildSink.cs
 * Copyright 2019 Theodore Tsirpanis
 * Theodore Tsirpanis licenses the contents of the referenced file to the Sigurd Team under the Apache-2.0 license.
 *
 * https://github.com/EvaisaDev/UnityNetcodePatcher/blob/c64eb86e74e85e1badc442adc0bf270bab0df6b6/NetcodePatcher.MSBuild/TaskLogSink.cs
 * UnityNetcodePatcher Copyright (c) 2023 EvaisaDev
 * EvaisaDev licenses the contents of the referenced file to the Sigurd Team under the MIT license.
 *
 * Copyright (c) 2024 The Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace MSBuildTasks.Lib;

public class TaskLogSink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider = null;
    private readonly TaskLoggingHelper _taskLoggingHelper;

    public TaskLogSink(TaskLoggingHelper taskLoggingHelper, IFormatProvider? formatProvider)
    {
        _taskLoggingHelper = taskLoggingHelper;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);

        switch (logEvent.Level)
        {
            case LogEventLevel.Debug:
                _taskLoggingHelper.LogMessage(MessageImportance.Low, message);
                break;
            case LogEventLevel.Verbose:
                _taskLoggingHelper.LogMessage(MessageImportance.Normal, message);
                break;
            case LogEventLevel.Information:
                _taskLoggingHelper.LogMessage(MessageImportance.High, message);
                break;
            case LogEventLevel.Warning:
                _taskLoggingHelper.LogWarning(message);
                if (logEvent.Exception is not null)
                    _taskLoggingHelper.LogWarningFromException(logEvent.Exception);
                break;
            case LogEventLevel.Error:
            case LogEventLevel.Fatal:
                _taskLoggingHelper.LogError(message);
                if (logEvent.Exception is not null)
                    _taskLoggingHelper.LogErrorFromException(logEvent.Exception);
                break;
        }
    }
}

public static class TaskLogSinkExtensions
{
    public static LoggerConfiguration TaskLoggingHelper(
        this LoggerSinkConfiguration loggerConfiguration,
        TaskLoggingHelper taskLoggingHelper,
        IFormatProvider? formatProvider = null
    ) {
        return loggerConfiguration.Sink(new TaskLogSink(taskLoggingHelper, formatProvider));
    }
}
