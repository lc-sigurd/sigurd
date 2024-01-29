// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Sigurd.Terminal.CommandLine.Invocation;

/// Defines an asynchronous behavior associated with a command line symbol.
public abstract class AsynchronousCliAction : CliAction
{
    /// <summary>
    /// Performs an action when the associated symbol is invoked on the command line.
    /// </summary>
    /// <param name="cliParseResult">Provides the parse results.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A value that can be used as the exit code for the process.</returns>
    public abstract UniTask<int> InvokeAsync(CliParseResult cliParseResult, CancellationToken cancellationToken = default);
}
