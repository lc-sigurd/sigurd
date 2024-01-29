// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Sigurd.Terminal.Extensions;

namespace Sigurd.Terminal.CommandLine.Parsing;

/// <summary>
/// Parses command line input.
/// </summary>
public static class CliParser
{
    /// <summary>
    /// Parses a list of arguments.
    /// </summary>
    /// <param name="command">The command to use to parse the command line input.</param>
    /// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
    /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
    /// <returns>A <see cref="CliParseResult"/> providing details about the parse operation.</returns>
    public static CliParseResult Parse(CliCommand command, IReadOnlyList<string> args, CliConfiguration? configuration = null)
        => Parse(command, args, null, configuration);

    /// <summary>
    /// Parses a command line string.
    /// </summary>
    /// <param name="command">The command to use to parse the command line input.</param>
    /// <param name="commandLine">The complete command line input prior to splitting and tokenization. This input is not typically available when the parser is called from <c>Program.Main</c>. It is primarily used when calculating completions via the <c>dotnet-suggest</c> tool.</param>
    /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
    /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
    /// <returns>A <see cref="CliParseResult"/> providing details about the parse operation.</returns>
    public static CliParseResult Parse(CliCommand command, string commandLine, CliConfiguration? configuration = null)
        => Parse(command, SplitCommandLine(commandLine).ToArray(), commandLine, configuration);

    /// <summary>
    /// Splits a string into a sequence of strings based on whitespace and quotation marks.
    /// </summary>
    /// <param name="commandLine">A command line input string.</param>
    /// <returns>A sequence of strings.</returns>
    public static IEnumerable<string> SplitCommandLine(string commandLine)
    {
        return RawCommandSplitter.Split(commandLine);
    }

    private static CliParseResult Parse(
        CliCommand command,
        IReadOnlyList<string> arguments,
        string? rawInput,
        CliConfiguration? configuration)
    {
        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        configuration ??= new CliConfiguration(command);

        arguments.Tokenize(
            configuration,
            inferRootCommand: rawInput is not null,
            out List<CliToken> tokens,
            out List<string>? tokenizationErrors);

        var operation = new ParseOperation(
            tokens,
            configuration,
            tokenizationErrors,
            rawInput);

        return operation.Parse();
    }

    private enum Boundary
    {
        TokenStart,
        WordEnd,
        QuoteStart,
        QuoteEnd
    }
}
