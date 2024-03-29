using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Sigurd.Common.Core.Registries;
using SigurdLib.Util.Extensions;
using SigurdLib.Util.Resources;

namespace Sigurd.Common.Core.Resources;

public interface IResourceKey
{
    private static readonly ConcurrentDictionary<InternKey, WeakReference> Values = new();

    public static IResourceKey<TValue> Create<TValue>(IResourceKey<IRegistrar<TValue>> registryKey, ResourceName name)
        where TValue : class
    {
        return Create<TValue>(registryKey.Name, name);
    }

    public static IResourceKey<IRegistrar<TValue>> CreateRegistryKey<TValue>(ResourceName registryName)
         where TValue : class
    {
        return Create<IRegistrar<TValue>>(SigurdRegistries.RootRegistryName, registryName);
    }

    private static IResourceKey<TValue> Create<TValue>(ResourceName registryName, ResourceName name)
    {
        var internKey = new InternKey(registryName, name);
        var possibleResourceKey = Values.ComputeIfAbsent(
            internKey,
            _ => new WeakReference(KeyFactory())
        ).Target;

        if (possibleResourceKey is IResourceKey<TValue> definiteResourceKey) return definiteResourceKey;

        definiteResourceKey = KeyFactory();
        Values[internKey] = new WeakReference(definiteResourceKey);
        return definiteResourceKey;

        IResourceKey<TValue> KeyFactory() => new ResourceKey<TValue>(registryName, name);
    }

    /// <summary>
    /// The <see cref="ResourceName"/> name of the <see cref="IRegistrar{TValue}"/> the
    /// <see cref="IResourceKey"/> belongs to.
    /// </summary>
    public ResourceName RegistryName { get; }

    /// <summary>
    /// The <see cref="ResourceName"/> that uniquely identifies the object within its registry.
    /// </summary>
    public ResourceName Name { get; }

    [UsedImplicitly]
    private readonly record struct InternKey(ResourceName RegistryName, ResourceName Name);
}

/// <summary>
/// Used to uniquely identify objects of a particular type, for example in an <see cref="IRegistry{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type of object to be identified</typeparam>
public interface IResourceKey<out TValue> : IResourceKey, IComparable<IResourceKey<object>>
{
    public bool IsFor<TRegistry>(IResourceKey<TRegistry> registryKey)
        where TRegistry : IRegistrar<object>;

    public IResourceKey<TCasted>? Cast<TCasted>(IResourceKey<IRegistrar<TCasted>> registryKey)
        where TCasted : class;
}
