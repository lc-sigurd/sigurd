/*
 * https://github.com/Cryptoc1/lc-plugin-sdk/blob/cc85330eb6f219ec1944fcf11620fc8c6d54e414/src/Internal/ThunderDependency.cs
 * LethalCompany.Plugin.SDK Copyright 2023 Samuel Steele.
 * Samuel Steele licenses this file to the Sigurd Team under the MIT license.
 * The Sigurd Team licenses this file to you under the LGPL-3.0-OR-LATER license.
 */

using System;
using Microsoft.Build.Framework;
using Semver;

namespace MSBuildTasks.PatchThunderstoreMetadata;

internal sealed record ThunderstorePackageMoniker(string Name, string Namespace, SemVersion Version) : IEquatable<string>
{
    public readonly string FullName = $"{Namespace}-{Name}";

    public readonly string Value = $"{Namespace}-{Name}-{Version}";

    public bool Equals(string? other) => Value.Equals(Value, StringComparison.Ordinal);

    public static ThunderstorePackageMoniker FromTaskItem(ITaskItem item)
    {
        var names = item.ItemSpec.Split('-');
        if (names.Length < 2) throw new ArgumentException($"Thunderstore identifier '{item.ItemSpec}' is not valid.", nameof(item));

        return new(
            names[1],
            names[0],
            SemVersion.Parse(names.Length is 3 ? names[2] : item.GetMetadata(nameof(Version)), SemVersionStyles.Any)
        );
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static ThunderstorePackageMoniker Parse(string identity)
    {
        var names = identity.Split('-');
        if (names.Length is not 3) throw new ArgumentException($"Thunderstore identifier '{identity}' is not valid.", nameof(identity));

        return new(names[1], names[0], SemVersion.Parse(names[2], SemVersionStyles.Any));
    }

    public static ThunderstorePackageMoniker Parse(string identity, SemVersion version)
    {
        var names = identity.Split('-');
        if (names.Length is not 2) throw new ArgumentException($"Thunderstore identifier '{identity}' is not valid.", nameof(identity));

        return new(names[1], names[0], version);
    }

    public override string ToString() => Value;

    public static implicit operator string(ThunderstorePackageMoniker moniker) => moniker.Value;
}
