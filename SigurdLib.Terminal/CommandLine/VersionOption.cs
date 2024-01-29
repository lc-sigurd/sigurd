// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;
using System.Linq;
using Sigurd.Terminal.CommandLine.Invocation;
using Sigurd.Terminal.CommandLine.Parsing;

namespace Sigurd.Terminal.CommandLine;

/// <summary>
/// A standard option that indicates that version information should be displayed for the app.
/// </summary>
public sealed class VersionOption : CliOption<bool>
{
    private CliAction? _action;

    /// <summary>
    /// When added to a <see cref="CliCommand"/>, it enables the use of a <c>--version</c> option, which when
    /// specified in command line input will short circuit normal command handling and instead write out version
    /// information before exiting.
    /// </summary>
    public VersionOption() : this("--version", Array.Empty<string>())
    {
    }

    /// <summary>
    /// When added to a <see cref="CliCommand"/>, it enables the use of an option of the provided name and aliases,
    /// which when specified in command line input will short circuit normal command handling and instead
    /// write out version information before exiting.
    /// </summary>
    public VersionOption(string name, params string[] aliases)
        : base(name, aliases, new CliArgument<bool>(name) { Arity = ArgumentArity.Zero })
    {
        Description = LocalizationResources.VersionOptionDescription();
        AddValidators();
    }

    /// <inheritdoc />
    public override CliAction? Action
    {
        get => _action ??= new VersionOptionAction();
        set => _action = value ?? throw new ArgumentNullException(nameof(value));
    }

    private void AddValidators()
    {
        Validators.Add(static result =>
        {
            if (result.Parent is CommandResult parent &&
                parent.Children.Any(r => r is not OptionResult { Option: VersionOption }))
            {
                result.AddError(LocalizationResources.VersionOptionCannotBeCombinedWithOtherArguments(result.IdentifierToken?.Value ?? result.Option.Name));
            }
        });
    }

    internal override bool Greedy => false;

    private sealed class VersionOptionAction : SynchronousCliAction
    {
        public override int Invoke(CliParseResult cliParseResult)
        {
            // Todo: This needs to look in the command registry to provide version information for the mod that registered the command
            cliParseResult.Configuration.Output.WriteLine(CliRootCommand.ExecutableVersion);
            return 0;
        }
    }
}
