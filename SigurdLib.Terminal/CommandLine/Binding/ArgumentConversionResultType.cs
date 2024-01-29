// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in SigurdLib.Terminal/CommandLine/ for full license information.

namespace Sigurd.Terminal.CommandLine.Binding;

internal enum ArgumentConversionResultType
{
    NoArgument, // NoArgumentConversionResult
    Successful, // SuccessfulArgumentConversionResult
    Failed, // FailedArgumentConversionResult
    FailedArity, // FailedArgumentConversionArityResult
    FailedType, // FailedArgumentTypeConversionResult
    FailedTooManyArguments, // TooManyArgumentsConversionResult
    FailedMissingArgument, // MissingArgumentConversionResult
}
