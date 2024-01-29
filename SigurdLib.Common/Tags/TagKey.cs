using Sigurd.Common.Core;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Tags;

/// <inheritdoc />
public class TagKey<TValue, TRegistry> : ITagKey<TValue, TRegistry>
    where TRegistry : IRegistrar<TValue>
{
    /// <inheritdoc />
    public IResourceKey<TRegistry> RegistryKey { get; }

    /// <inheritdoc />
    public ResourceLocation Location { get; }

    internal TagKey(IResourceKey<TRegistry> registryKey, ResourceLocation location)
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

    /// <inheritdoc />
    public TagKey<TCasted, TOtherRegistry>? Cast<TCasted, TOtherRegistry>(ResourceKey<TOtherRegistry> registryKey) where TOtherRegistry : IRegistrar<TCasted>
    {
        return IsFor(registryKey) ? this as TagKey<TCasted, TOtherRegistry> : null;
    }
}
