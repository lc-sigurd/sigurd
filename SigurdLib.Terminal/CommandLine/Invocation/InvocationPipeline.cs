// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Sigurd.Terminal.CommandLine.Invocation;

internal static class InvocationPipeline
{
    internal static async UniTask<int> InvokeAsync(CliParseResult cliParseResult, CancellationToken cancellationToken)
    {
        if (cliParseResult.Action is null)
        {
            return ReturnCodeForMissingAction(cliParseResult);
        }

        ProcessTerminationHandler? terminationHandler = null;
        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            if (cliParseResult.PreActions is not null)
            {
                for (int i = 0; i < cliParseResult.PreActions.Count; i++)
                {
                    var action = cliParseResult.PreActions[i];

                    switch (action)
                    {
                        case SynchronousCliAction syncAction:
                            syncAction.Invoke(cliParseResult);
                            break;
                        case AsynchronousCliAction asyncAction:
                            await asyncAction.InvokeAsync(cliParseResult, cts.Token);
                            break;
                    }
                }
            }

            switch (cliParseResult.Action)
            {
                case SynchronousCliAction syncAction:
                    return syncAction.Invoke(cliParseResult);

                case AsynchronousCliAction asyncAction:
                    var startedInvocation = asyncAction.InvokeAsync(cliParseResult, cts.Token);
                    if (cliParseResult.Configuration.ProcessTerminationTimeout.HasValue)
                    {
                        terminationHandler = new(cts, startedInvocation, cliParseResult.Configuration.ProcessTerminationTimeout.Value);
                    }

                    if (terminationHandler is null)
                    {
                        return await startedInvocation;
                    }
                    else
                    {
                        // Handlers may not implement cancellation.
                        // In such cases, when CancelOnProcessTermination is configured and user presses Ctrl+C,
                        // ProcessTerminationCompletionSource completes first, with the result equal to native exit code for given signal.
                        var (wonIndex, invocationComplete, invocationCancelled) = await UniTask.WhenAny(startedInvocation, terminationHandler.ProcessTerminationCompletionSource.Task);

                        if (wonIndex == 0)
                            return invocationComplete;

                        return invocationCancelled;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(cliParseResult.Action));
            }
        }
        catch (Exception ex) when (cliParseResult.Configuration.EnableDefaultExceptionHandler)
        {
            return DefaultExceptionHandler(ex, cliParseResult.Configuration);
        }
        finally
        {
            terminationHandler?.Dispose();
        }
    }

    internal static int Invoke(CliParseResult cliParseResult)
    {
        switch (cliParseResult.Action)
        {
            case null:
                return ReturnCodeForMissingAction(cliParseResult);

            case SynchronousCliAction syncAction:
                try
                {
                    if (cliParseResult.PreActions is not null)
                    {
#if DEBUG
                        for (var i = 0; i < cliParseResult.PreActions.Count; i++)
                        {
                            var action = cliParseResult.PreActions[i];

                            if (action is not SynchronousCliAction)
                            {
                                cliParseResult.Configuration.EnableDefaultExceptionHandler = false;
                                throw new Exception(
                                    $"This should not happen. An instance of {nameof(AsynchronousCliAction)} ({action}) was called within {nameof(InvocationPipeline)}.{nameof(Invoke)}. This is supposed to be detected earlier resulting in a call to {nameof(InvocationPipeline)}{nameof(InvokeAsync)}");
                            }
                        }
#endif

                        for (var i = 0; i < cliParseResult.PreActions.Count; i++)
                        {
                            if (cliParseResult.PreActions[i] is SynchronousCliAction syncPreAction)
                            {
                                syncPreAction.Invoke(cliParseResult);
                            }
                        }
                    }

                    return syncAction.Invoke(cliParseResult);
                }
                catch (Exception ex) when (cliParseResult.Configuration.EnableDefaultExceptionHandler)
                {
                    return DefaultExceptionHandler(ex, cliParseResult.Configuration);
                }

            default:
                throw new InvalidOperationException($"{nameof(AsynchronousCliAction)} called within non-async invocation.");
        }
    }

    private static int DefaultExceptionHandler(Exception exception, CliConfiguration config)
    {
        if (exception is not OperationCanceledException)
        {
            config.Error.Write(LocalizationResources.ExceptionHandlerHeader());
            config.Error.WriteLine(exception.ToString());
        }
        return 1;
    }

    private static int ReturnCodeForMissingAction(CliParseResult cliParseResult)
    {
        if (cliParseResult.Errors.Count > 0)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
