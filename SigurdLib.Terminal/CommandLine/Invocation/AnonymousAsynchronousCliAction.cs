// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Sigurd.Terminal.CommandLine.Invocation;

internal sealed class AnonymousAsynchronousCliAction : AsynchronousCliAction
{
    private readonly Func<CliParseResult, CancellationToken, UniTask<int>> _asyncAction;

    internal AnonymousAsynchronousCliAction(Func<CliParseResult, CancellationToken, UniTask<int>> action)
        => _asyncAction = action;

    /// <inheritdoc />
    public override UniTask<int> InvokeAsync(CliParseResult cliParseResult, CancellationToken cancellationToken = default) =>
        _asyncAction(cliParseResult, cancellationToken);
}
