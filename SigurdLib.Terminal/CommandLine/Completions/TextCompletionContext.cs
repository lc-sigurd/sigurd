// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

namespace Sigurd.Terminal.CommandLine.Completions;

/// <summary>
/// Provides details for calculating completions in the context of complete, unsplit command line text.
/// </summary>
public class TextCompletionContext : CompletionContext
{
    private TextCompletionContext(
        CliParseResult cliParseResult,
        string commandLineText,
        int cursorPosition) : base(cliParseResult, GetWordToComplete(cliParseResult, cursorPosition))
    {
        CommandLineText = commandLineText;
        CursorPosition = cursorPosition;
    }

    internal TextCompletionContext(
        CliParseResult cliParseResult,
        string commandLineText) : this(cliParseResult, commandLineText, commandLineText.Length)
    {
    }

    /// <summary>
    /// The position of the cursor within the command line.
    /// </summary>
    public int CursorPosition { get; }

    /// <summary>
    /// The complete text of the command line prior to splitting, including any additional whitespace.
    /// </summary>
    public string CommandLineText { get; }

    /// <summary>
    /// Creates a new instance of <see cref="TextCompletionContext"/> at the specified cursor position.
    /// </summary>
    /// <param name="position">The cursor position at which completions are calculated.</param>
    public TextCompletionContext AtCursorPosition(int position) =>
        new(CliParseResult, CommandLineText, position);
}
