// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System.Collections;
using System.Linq;
using System.Text;
using Sigurd.Terminal.CommandLine.Binding;

namespace Sigurd.Terminal.CommandLine.Parsing;

/// <summary>
/// Implements static methods for rendering diagrams explaining the parse result for the command line input.
/// </summary>
internal static class ParseDiagram
{
    /// <summary>
    /// Formats a string explaining a parse result.
    /// </summary>
    /// <param name="cliParseResult">The parse result to be diagrammed.</param>
    /// <returns>A string containing a diagram of the parse result.</returns>
    internal static StringBuilder Diagram(CliParseResult cliParseResult)
    {
        var builder = new StringBuilder(100);

        Diagram(builder, cliParseResult.RootCommandResult, cliParseResult);

        var unmatchedTokens = cliParseResult.UnmatchedTokens;
        if (unmatchedTokens.Count > 0)
        {
            builder.Append("   ???-->");

            for (var i = 0; i < unmatchedTokens.Count; i++)
            {
                var error = unmatchedTokens[i];
                builder.Append(' ');
                builder.Append(error);
            }
        }

        return builder;
    }

    private static void Diagram(
        StringBuilder builder,
        SymbolResult symbolResult,
        CliParseResult cliParseResult)
    {
        if (cliParseResult.Errors.Any(e => e.SymbolResult == symbolResult))
        {
            builder.Append('!');
        }

        switch (symbolResult)
        {
            case ArgumentResult argumentResult:
            {
                var includeArgumentName =
                    argumentResult.Argument.FirstParent!.Symbol is CliCommand { HasArguments: true, Arguments.Count: > 1 };

                if (includeArgumentName)
                {
                    builder.Append("[ ");
                    builder.Append(argumentResult.Argument.Name);
                    builder.Append(' ');
                }

                if (argumentResult.Argument.Arity.MaximumNumberOfValues > 0)
                {
                    ArgumentConversionResult conversionResult = argumentResult.GetArgumentConversionResult();
                    switch (conversionResult.Result)
                    {
                        case ArgumentConversionResultType.NoArgument:
                            break;
                        case ArgumentConversionResultType.Successful:
                            switch (conversionResult.Value)
                            {
                                case string s:
                                    builder.Append($"<{s}>");
                                    break;

                                case IEnumerable items:
                                    builder.Append('<');
                                    builder.Append(
                                        string.Join("> <",
                                            items.Cast<object>().ToArray()));
                                    builder.Append('>');
                                    break;

                                default:
                                    builder.Append('<');
                                    builder.Append(conversionResult.Value);
                                    builder.Append('>');
                                    break;
                            }

                            break;

                        default: // failures
                            builder.Append('<');
                            builder.Append(string.Join("> <", symbolResult.Tokens.Select(t => t.Value)));
                            builder.Append('>');

                            break;
                    }
                }

                if (includeArgumentName)
                {
                    builder.Append(" ]");
                }

                break;
            }

            default:
            {
                OptionResult? optionResult = symbolResult as OptionResult;

                if (optionResult is { Implicit: true })
                {
                    builder.Append('*');
                }

                builder.Append("[ ");

                if (optionResult is not null)
                {
                    builder.Append(optionResult.IdentifierToken?.Value ?? optionResult.Option.Name);
                }
                else
                {
                    builder.Append(((CommandResult)symbolResult).IdentifierToken.Value);
                }

                foreach (SymbolResult child in symbolResult.SymbolResultTree.GetChildren(symbolResult))
                {
                    if (child is ArgumentResult arg &&
                        (arg.Argument.ValueType == typeof(bool) ||
                         arg.Argument.Arity.MaximumNumberOfValues == 0))
                    {
                        continue;
                    }

                    builder.Append(' ');

                    Diagram(builder, child, cliParseResult);
                }

                builder.Append(" ]");
                break;
            }
        }
    }
}
