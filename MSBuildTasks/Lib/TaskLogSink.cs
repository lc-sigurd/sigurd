/*
 * https://github.com/EvaisaDev/UnityNetcodePatcher/blob/c64eb86e74e85e1badc442adc0bf270bab0df6b6/NetcodePatcher.MSBuild/TaskLogSink.cs
 * UnityNetcodePatcher Copyright (c) 2023 EvaisaDev.
 * EvaisaDev licenses this file to the Sigurd Team under the MIT license.
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
