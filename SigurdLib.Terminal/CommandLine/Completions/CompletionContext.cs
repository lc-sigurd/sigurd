// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;
using System.Linq;
using Sigurd.Terminal.CommandLine.Parsing;

namespace Sigurd.Terminal.CommandLine.Completions;

/// <summary>
/// Supports command line completion operations.
/// </summary>
public class CompletionContext
{
    private static CompletionContext? _empty;

    internal CompletionContext(CliParseResult cliParseResult) : this(cliParseResult, GetWordToComplete(cliParseResult))
    {
    }

    internal CompletionContext(CliParseResult cliParseResult, string wordToComplete)
    {
        CliParseResult = cliParseResult;
        WordToComplete = wordToComplete;
    }

    /// The text of the word to be completed, if any.
    public string WordToComplete { get; }

    /// The parse result for which completions are being requested.
    public CliParseResult CliParseResult { get; }

    /// <summary>
    /// Gets an empty CompletionContext.
    /// </summary>
    /// <remarks>Can be used for testing purposes.</remarks>
    public static CompletionContext Empty => _empty ??= new CompletionContext(CliParseResult.Empty());

    internal bool IsEmpty => ReferenceEquals(this, _empty);

    /// <summary>
    /// Gets the text to be matched for completion, which can be used to filter a list of completions.
    /// </summary>
    /// <param name="cliParseResult">A parse result.</param>
    /// <param name="position">The position within the raw input, if available, at which to provide completions.</param>
    /// <returns>A string containing the user-entered text to be matched for completions.</returns>
    protected static string GetWordToComplete(
        CliParseResult cliParseResult,
        int? position = null)
    {
        CliToken? lastToken = cliParseResult.Tokens.LastOrDefault();

        string? textToMatch = null;
        string? rawInput = cliParseResult.CommandLineText;

        if (rawInput is not null)
        {
            if (position is not null)
            {
                if (position > rawInput.Length)
                {
                    rawInput += ' ';
                    position = Math.Min(rawInput.Length, position.Value);
                }
            }
            else
            {
                position = rawInput.Length;
            }
        }
        else if (lastToken is not null)
        {
            position = null;
            textToMatch = lastToken.Value;
        }

        if (string.IsNullOrWhiteSpace(rawInput))
        {
            if (cliParseResult.UnmatchedTokens.Count > 0 ||
                lastToken?.Type == CliTokenType.Argument)
            {
                return textToMatch ?? "";
            }
        }
        else
        {
            var textBeforeCursor = rawInput!.Substring(0, position!.Value);

            var textAfterCursor = rawInput.Substring(position.Value);

            return textBeforeCursor.Split(' ').LastOrDefault() +
                   textAfterCursor.Split(' ').FirstOrDefault();
        }

        return "";
    }
}
