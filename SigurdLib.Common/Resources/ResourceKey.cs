using Sigurd.Common.Core;

namespace Sigurd.Common.Resources;

/// <inheritdoc />
public class ResourceKey<TValue> : IResourceKey<TValue>
{
    /// <inheritdoc />
    public ResourceLocation RegistryName { get; }

    /// <inheritdoc />
    public ResourceLocation Location { get; }

    internal ResourceKey(ResourceLocation registryName, ResourceLocation location)
    {
        RegistryName = registryName;
        Location = location;
    }

    /// <inheritdoc />
    public int CompareTo(IResourceKey<object>? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var registryComparison = RegistryName.CompareTo(other.RegistryName);
        if (registryComparison != 0) return registryComparison;
        return Location.CompareTo(other.Location);
    }

    /// <inheritdoc />
    public override string ToString() => "ResourceKey[" + RegistryName + " / " + Location + "]";

    /// <inheritdoc />
    public bool IsFor<TRegistry>(IResourceKey<TRegistry> registryKey)
        where TRegistry : IRegistrar
    {
        return RegistryName.Equals(registryKey.Location);
    }

    /// <inheritdoc />
    public IResourceKey<TCasted>? Cast<TCasted, TRegistry>(IResourceKey<TRegistry> registryKey)
        where TRegistry : IRegistrar<TCasted>
        where TCasted : class
    {
        return IsFor(registryKey) ? this as ResourceKey<TCasted> : null;
    }
}
