using LanguageExt;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Core;

public class Registries
{
    public static readonly ResourceLocation RootRegistryName = new ResourceLocation("root");

    public static Either<string, int> Unwrap()
    {
        return Either<string, int>.Left("hello, world!");
    }
}
