// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;
using System.Collections.Generic;
using Sigurd.Terminal.CommandLine;

namespace Sigurd.Terminal.Extensions
{
    /// <summary>
    /// Provides extension methods for symbols.
    /// </summary>
    internal static class CliSymbolExtensions
    {
        internal static IList<CliArgument> Arguments(this CliSymbol symbol)
        {
            switch (symbol)
            {
                case CliOption option:
                    return new[]
                    {
                        option.Argument
                    };
                case CliCommand command:
                    return command.Arguments;
                case CliArgument argument:
                    return new[]
                    {
                        argument
                    };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
