// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;

namespace Sigurd.Terminal.CommandLine.Invocation;

internal sealed class AnonymousSynchronousCliAction : SynchronousCliAction
{
    private readonly Func<CliParseResult, int> _syncAction;

    internal AnonymousSynchronousCliAction(Func<CliParseResult, int> action)
        => _syncAction = action;

    /// <inheritdoc />
    public override int Invoke(CliParseResult cliParseResult) =>
        _syncAction(cliParseResult);
}
