using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Sigurd.Common.Core;
using Sigurd.Common.Extensions;

namespace Sigurd.Common.Resources;

public interface IResourceKey
{
    private static readonly ConcurrentDictionary<InternKey, WeakReference> Values = new();

    public static ResourceKey<TValue> Create<TValue>(IResourceKey<IRegistrar<TValue>> registryKey, ResourceLocation location)
        where TValue : class
    {
        return Create<TValue>(registryKey.Location, location);
    }

    public static ResourceKey<IRegistry<TValue>> CreateRegistryKey<TValue>(ResourceLocation registryName)
         where TValue : class
    {
        return Create<IRegistry<TValue>>(Registries.RootRegistryName, registryName);
    }

    private static ResourceKey<TValue> Create<TValue>(ResourceLocation registryName, ResourceLocation location)
    {
        var internKey = new InternKey(registryName, location);
        var possibleResourceKey = Values.ComputeIfAbsent(
            internKey,
            _ => new WeakReference(KeyFactory())
        ).Target;

        if (possibleResourceKey is ResourceKey<TValue> definiteResourceKey) return definiteResourceKey;

        definiteResourceKey = KeyFactory();
        Values[internKey] = new WeakReference(definiteResourceKey);
        return definiteResourceKey;

        ResourceKey<TValue> KeyFactory() => new ResourceKey<TValue>(registryName, location);
    }

    /// <summary>
    /// The <see cref="ResourceLocation"/> name of the <see cref="IRegistrar{TValue}"/> the
    /// <see cref="IResourceKey"/> belongs to.
    /// </summary>
    public ResourceLocation RegistryName { get; }

    /// <summary>
    /// The <see cref="ResourceLocation"/> that uniquely identifies the object within its registry.
    /// </summary>
    public ResourceLocation Location { get; }

    [UsedImplicitly]
    private readonly record struct InternKey(ResourceLocation RegistryName, ResourceLocation Location);
}

/// <summary>
/// Used to uniquely identify objects of a particular type, for example in an <see cref="IRegistry{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type of object to be identified</typeparam>
public interface IResourceKey<out TValue> : IResourceKey, IComparable<IResourceKey<object>>
{
    public bool IsFor<TRegistry>(IResourceKey<TRegistry> registryKey)
        where TRegistry : IRegistrar;

    public IResourceKey<TCasted>? Cast<TCasted>(IResourceKey<IRegistrar<TCasted>> registryKey)
        where TCasted : class;
}
