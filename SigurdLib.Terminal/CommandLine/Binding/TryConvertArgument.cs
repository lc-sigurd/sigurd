// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

using Sigurd.Terminal.CommandLine.Parsing;

namespace Sigurd.Terminal.CommandLine.Binding;

internal delegate bool TryConvertArgument(
    ArgumentResult argumentResult,
    out object? value);
