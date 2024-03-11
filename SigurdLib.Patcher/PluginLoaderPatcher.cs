/*
 * https://github.com/BepInEx/BepInEx/blob/6c5766d5abc230a4c9427ebb14acfca05255efb8/BepInEx.Preloader/Preloader.cs#L140C3-L222C4
 * Copyright (c) 2018 Bepis
 * Bepis licenses the basis of this file to the Sigurd Team under the MIT license.
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Preloader;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Sigurd.Patcher;

public static class PluginLoaderPatcher
{
    private const string SigurdLibPluginLoaderAssemblyName = "com.sigurd.sigurd.pluginloader.dll";

    private static readonly IEnumerable<string> SigurdLibPluginLoaderAssemblySearchPaths = [
        Path.Combine(Paths.BepInExAssemblyDirectory, "Sigurd-Sigurd_Plugin_Loader", "SigurdLib.PluginLoader"),
        Path.Combine(Paths.BepInExAssemblyDirectory, "SigurdLib.PluginLoader"),
        Paths.BepInExAssemblyDirectory,
    ];

    private static string? _sigurdLibPluginLoaderAssemblyPath;
    private static string? SigurdLibPluginLoaderAssemblyPath => _sigurdLibPluginLoaderAssemblyPath ??= SigurdLibPluginLoaderAssemblySearchPaths
        .Select(directoryPath => Path.Combine(directoryPath, SigurdLibPluginLoaderAssemblyName))
        .FirstOrDefault(File.Exists);

    public static IEnumerable<string> TargetDLLs => [ Preloader.ConfigEntrypointAssembly.Value ];

    /// <summary>
    /// Inserts Sigurd's PluginLoader entrypoint just before BepInEx's Chainloader.
	/// </summary>
	/// <param name="assembly">The assembly that the <see cref="PluginLoaderPatcher"/> will attempt to patch.</param>
	public static void Patch(AssemblyDefinition assembly)
	{
        if (SigurdLibPluginLoaderAssemblyPath is null)
            throw new InvalidOperationException("SigurdLib.PluginLoader assembly could not be found. It should be in the BepInEx/core/ directory.");

		string entrypointType = Preloader.ConfigEntrypointType.Value;
		string entrypointMethod = Preloader.ConfigEntrypointMethod.Value;

		bool isCctor = entrypointMethod.IsNullOrWhiteSpace() || entrypointMethod == ".cctor";

		var entryType = assembly.MainModule.Types.FirstOrDefault(x => x.Name == entrypointType);

        if (entryType is null)
            return; // fail silently because BepInEx will throw an error anyway

        using var injected = AssemblyDefinition.ReadAssembly(SigurdLibPluginLoaderAssemblyPath);
        var originalStartMethod = injected.MainModule.Types.First(x => x.Name == "Entrypoint").Methods
            .First(x => x.Name == "Start");

        var startMethod = assembly.MainModule.ImportReference(originalStartMethod);

        var methods = new List<MethodDefinition>();

        if (isCctor)
        {
            var cctor = entryType.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);

            if (cctor == null)
            {
                cctor = new MethodDefinition(".cctor",
                    MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
                    | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    assembly.MainModule.ImportReference(typeof(void)));

                entryType.Methods.Add(cctor);
                var il = cctor.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ret));
            }

            methods.Add(cctor);
        }
        else
        {
            methods.AddRange(entryType.Methods.Where(x => x.Name == entrypointMethod));
        }

        if (!methods.Any())
            throw new Exception("The entrypoint method is invalid! Please check your config.ini");

        foreach (var method in methods)
        {
            var il = method.Body.GetILProcessor();

            var ins = il.Body.Instructions.First();

            il.InsertBefore(ins,
                il.Create(OpCodes.Call, startMethod));
        }
    }
}
