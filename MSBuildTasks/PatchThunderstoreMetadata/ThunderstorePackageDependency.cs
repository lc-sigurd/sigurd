/*
 * https://github.com/Cryptoc1/lc-plugin-sdk/blob/cc85330eb6f219ec1944fcf11620fc8c6d54e414/src/Internal/ThunderDependency.cs
 * LethalCompany.Plugin.SDK Copyright 2023 Samuel Steele.
 * Samuel Steele licenses this file to the Sigurd Team under the MIT license.
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using System.Linq;
using Microsoft.Build.Framework;

namespace MSBuildTasks.PatchThunderstoreMetadata;

internal sealed record ThunderstorePackageDependency(ThunderstorePackageMoniker Moniker) : IEquatable<string>
{
    /// <summary> Glob patterns of plugin assets that shouldn't be referenced. </summary>
    /// <remarks> May be used to exclude DLLs from being referenced. </remarks>
    public string[] ExcludeAssets { get; init; } = [];

    /// <summary> Glob patterns of plugin assets that should be referenced. </summary>
    /// <remarks> May be used to include DLLs to be referenced. </remarks>
    public string[] IncludeAssets { get; init; } = ["*.dll", @"plugins\**\*.dll", @"BepInEx\core\**\*.dll", @"BepInEx\plugins\**\*.dll;"];

    public static ThunderstorePackageDependency FromTaskItem(ITaskItem item)
    {
        var moniker = ThunderstorePackageMoniker.FromTaskItem(item);
        return new(moniker)
        {
            ExcludeAssets = AssetPatterns(item, nameof(ExcludeAssets)),
            IncludeAssets = AssetPatterns(item, nameof(IncludeAssets)),
        };

        static string[] AssetPatterns(ITaskItem item, string name) => [
            ..item.GetMetadata(name)
                ?.Split([";"], StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Distinct()];
    }

    public bool Equals(string? other) => Moniker.Value.Equals(other, StringComparison.Ordinal);

    public static implicit operator string(ThunderstorePackageDependency dependency) => dependency.Moniker;
}

