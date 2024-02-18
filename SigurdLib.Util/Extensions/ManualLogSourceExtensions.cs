/*
 * https://github.com/Lordfirespeed/Lethal-Company-Augmented-Enhancer/blob/f81aeea588b815c923c624f4353d5048c212556d/Enhancer/Extensions/ManualLogSourceExtensions.cs
 * Copyright (c) 2023 Lordfirespeed
 * Lordfirespeed licenses this file to the Sigurd Team under the CC-BY-NC-4.0 license.
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;

namespace SigurdLib.Util.Extensions;

public static class ManualLogSourceExtensions
{
    class CodeInstructionFormatter(int instructionCount)
    {
        private int _instructionIndexPadLength = instructionCount.ToString().Length;

        public string Format(CodeInstruction instruction, int index)
            => $"    IL_{index.ToString().PadLeft(_instructionIndexPadLength, '0')}: {instruction}";
    }

    public static void LogDebugInstructionsFrom(this ManualLogSource source, CodeMatcher matcher)
    {
        var methodName = new StackTrace().GetFrame(1).GetMethod().Name;

        var instructionFormatter = new CodeInstructionFormatter(matcher.Length);
        var builder = new StringBuilder($"'{methodName}' Matcher Instructions:\n")
            .AppendLine(
                String.Join(
                    "\n",
                    matcher
                        .InstructionEnumeration()
                        .Select(instructionFormatter.Format)
                )
            )
            .AppendLine("End of matcher instructions.");

        source.LogDebug(builder.ToString());
    }

    public static void LogDebugInstructionsFrom(this ManualLogSource source, IEnumerable<CodeInstruction> instructions)
    {
        var methodName = new StackTrace().GetFrame(1).GetMethod().Name;

        var instructionArray = instructions.ToArray();

        var instructionFormatter = new CodeInstructionFormatter(instructionArray.Length);
        var builder = new StringBuilder($"'{methodName}' Enumerable Instructions:\n")
            .AppendLine(
                String.Join(
                    "\n",
                    instructionArray
                        .Select(instructionFormatter.Format)
                )
            )
            .AppendLine("End of matcher instructions.");

        source.LogDebug(builder.ToString());
    }
}
