using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace ThunderstoreCLI.Models;

// https://github.com/thunderstore-io/thunderstore-cli/blob/10b73c843f2dd1a9ed9c6cb687dbbaa555626052/ThunderstoreCLI/Models/ISerialize.cs
// thunderstore-cli Copyright (c) 2021 Thunderstore
public interface ISerialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>
    where T : ISerialize<T>
{
    public string Serialize();
#if NET8_0
    public static abstract T? Deserialize(string input);
    public static virtual ValueTask<T?> DeserializeAsync(string input)
    {
        return new(T.Deserialize(input));
    }
    public static virtual async ValueTask<T?> DeserializeAsync(Stream input)
    {
        using StreamReader reader = new(input);
        return T.Deserialize(await reader.ReadToEndAsync());
    }
#endif
}
