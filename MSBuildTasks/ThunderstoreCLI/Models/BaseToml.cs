using System.Diagnostics.CodeAnalysis;
using Tomlet;

namespace ThunderstoreCLI.Models;

// https://github.com/thunderstore-io/thunderstore-cli/blob/10b73c843f2dd1a9ed9c6cb687dbbaa555626052/ThunderstoreCLI/Models/BaseToml.cs
// thunderstore-cli Copyright (c) 2021 Thunderstore
public abstract class BaseToml<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : ISerialize<T>
    where T : BaseToml<T>
{
    public string Serialize() => TomletMain.TomlStringFrom(this);

    public static T? Deserialize(string toml) => TomletMain.To<T>(toml);
}
