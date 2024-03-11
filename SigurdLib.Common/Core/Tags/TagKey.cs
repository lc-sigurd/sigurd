using Sigurd.Common.Core.Registries;
using Sigurd.Common.Core.Resources;
using Sigurd.Util.Resources;

namespace Sigurd.Common.Core.Tags;

/// <inheritdoc />
internal record TagKey<TValue> : ITagKey<TValue>
    where TValue : class
{
    /// <inheritdoc />
    public required IResourceKey<IRegistrar<TValue>> RegistryKey { get; init; }

    /// <inheritdoc />
    public required ResourceName Name { get; init; }

    /// <inheritdoc />
    public bool IsFor<TOtherRegistry>(ResourceKey<TOtherRegistry> registryKey) where TOtherRegistry : IRegistrar<object>
    {
        return RegistryKey.Equals(registryKey);
    }

    /// <inheritdoc />
    public ITagKey<TCasted>? Cast<TCasted>(ResourceKey<IRegistrar<TCasted>> registryKey)
        where TCasted : class
    {
        return IsFor(registryKey) ? this as TagKey<TCasted> : null;
    }

    /// <inheritdoc />
    public override string ToString() => "TagKey[" + RegistryKey.Name + " / " + Name + "]";
}
