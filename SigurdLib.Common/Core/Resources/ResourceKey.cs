using Sigurd.Common.Core.Registries;

namespace Sigurd.Common.Core.Resources;

/// <inheritdoc />
public class ResourceKey<TValue> : IResourceKey<TValue>
{
    /// <inheritdoc />
    public ResourceName RegistryName { get; }

    /// <inheritdoc />
    public ResourceName Name { get; }

    internal ResourceKey(ResourceName registryName, ResourceName name)
    {
        RegistryName = registryName;
        Name = name;
    }

    /// <inheritdoc />
    public int CompareTo(IResourceKey<object>? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var registryComparison = RegistryName.CompareTo(other.RegistryName);
        if (registryComparison != 0) return registryComparison;
        return Name.CompareTo(other.Name);
    }

    /// <inheritdoc />
    public override string ToString() => "ResourceKey[" + RegistryName + " / " + Name + "]";

    /// <inheritdoc />
    public bool IsFor<TRegistry>(IResourceKey<TRegistry> registryKey)
        where TRegistry : ISigurdRegistrar<object>
    {
        return RegistryName.Equals(registryKey.Name);
    }

    /// <inheritdoc />
    public IResourceKey<TCasted>? Cast<TCasted>(IResourceKey<ISigurdRegistrar<TCasted>> registryKey)
        where TCasted : class
    {
        return IsFor(registryKey) ? this as ResourceKey<TCasted> : null;
    }
}
