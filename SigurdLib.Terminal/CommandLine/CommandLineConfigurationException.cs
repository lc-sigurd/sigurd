// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using System;

namespace Sigurd.Terminal.CommandLine;

/// <summary>
/// Indicates that a command line configuration is invalid.
/// </summary>
public class CliConfigurationException : Exception
{
    /// <inheritdoc />
    public CliConfigurationException(string message) : base(message)
    {
    }
}
