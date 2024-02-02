using Sigurd.Common.Core;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Tags;

/// <inheritdoc />
public class TagKey<TValue> : ITagKey<TValue>
    where TValue : class
{
    /// <inheritdoc />
    public IResourceKey<IRegistrar<TValue>> RegistryKey { get; }

    /// <inheritdoc />
    public ResourceLocation Location { get; }

    internal TagKey(IResourceKey<IRegistrar<TValue>> registryKey, ResourceLocation location)
    {
        RegistryKey = registryKey;
        Location = location;
    }

    /// <inheritdoc />
    public override string ToString() => "TagKey[" + RegistryKey.Location + " / " + Location + "]";

    /// <inheritdoc />
    public bool IsFor<TOtherRegistry>(ResourceKey<TOtherRegistry> registryKey) where TOtherRegistry : IRegistrar
    {
        return RegistryKey.Equals(registryKey);
    }

    public TagKey<TCasted>? Cast<TCasted>(ResourceKey<IRegistrar<TCasted>> registryKey)
        where TCasted : class
    {
        return IsFor(registryKey) ? this as TagKey<TCasted> : null;
    }
}
