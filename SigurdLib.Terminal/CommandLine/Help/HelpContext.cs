// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;
using System.IO;

namespace Sigurd.Terminal.CommandLine.Help;

/// <summary>
/// Supports formatting command line help.
/// </summary>
public class HelpContext
{
    /// <param name="helpBuilder">The current help builder.</param>
    /// <param name="command">The command for which help is being formatted.</param>
    /// <param name="output">A text writer to write output to.</param>
    /// <param name="parseResult">The result of the current parse operation.</param>
    public HelpContext(
        HelpBuilder helpBuilder,
        CliCommand command,
        TextWriter output,
        CliParseResult? parseResult = null)
    {
        HelpBuilder = helpBuilder ?? throw new ArgumentNullException(nameof(helpBuilder));
        Command = command ?? throw new ArgumentNullException(nameof(command));
        Output = output ?? throw new ArgumentNullException(nameof(output));
        CliParseResult = parseResult ?? CliParseResult.Empty();
    }

    /// <summary>
    /// The help builder for the current operation.
    /// </summary>
    public HelpBuilder HelpBuilder { get; }

    /// <summary>
    /// The result of the current parse operation.
    /// </summary>
    public CliParseResult CliParseResult { get; }

    /// <summary>
    /// The command for which help is being formatted.
    /// </summary>
    public CliCommand Command { get; }

    /// <summary>
    /// A text writer to write output to.
    /// </summary>
    public TextWriter Output { get; }
}
