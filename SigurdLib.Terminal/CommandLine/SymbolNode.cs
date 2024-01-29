// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

namespace Sigurd.Terminal.CommandLine;

internal sealed class SymbolNode
{
    internal SymbolNode(CliSymbol symbol) => Symbol = symbol;

    internal CliSymbol Symbol { get; }

    internal SymbolNode? Next { get; set; }
}
