/*
 * This file is largely based upon
 * https://github.com/Lordfirespeed/Lethal-Company-Augmented-Enhancer/blob/482db3cff81ad007a2b484c5ff5c63d81b81e570/Enhancer/Extensions/HarmonyExtensions.cs
 * Copyright (c) 2023 Mama Llama, Flowerful
 * Copyright (c) 2024 Joe Clack
 * Joe Clack licenses the referenced file to the Sigurd Team under the CC-BY-NC-4.0 license.
 *
 * Copyright (c) 2024 Sigurd Team
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Reflection;
using HarmonyLib;

namespace Sigurd.Util.Extensions;

/// <summary>
/// Extension methods for <see cref="Harmony"/> instances.
/// </summary>
public static class HarmonyExtensions
{
    private const BindingFlags SearchNestedTypeBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

    public static void PatchAllNestedTypesOnly(this Harmony harmony, Type type)
    {
        foreach (var nestedType in type.GetNestedTypes(SearchNestedTypeBindingFlags)) {
            PatchAllWithNestedTypes(harmony, nestedType);
        }
    }

    public static void PatchAllWithNestedTypes(this Harmony harmony, Type type)
    {
        harmony.PatchAll(type);
        PatchAllNestedTypesOnly(harmony, type);
    }
}
