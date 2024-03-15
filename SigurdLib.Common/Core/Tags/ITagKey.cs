using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Sigurd.Common.Core.Registries;
using Sigurd.Common.Core.Resources;
using Sigurd.Util.Extensions;
using Sigurd.Util.Resources;

namespace Sigurd.Common.Core.Tags;

/// <summary>
/// An <see cref="ITagKey{TValue}"/> is used to uniquely identify an <see cref="ITag{TValue}"/>.
/// </summary>
public interface ITagKey
{
    private static readonly ConcurrentDictionary<InternKey, WeakReference> Values = new();

    /// <summary>
    /// Create a new <see cref="TagKey{TValue}"/>.
    /// </summary>
    /// <param name="registryKey">The <see cref="IResourceKey{TValue}"/> of the target registry.</param>
    /// <param name="name">The <see cref="ResourceName"/> used to identify the tag.</param>
    /// <typeparam name="TValue">The type of object contained by the target registry.</typeparam>
    /// <returns>The newly created <see cref="TagKey{TValue}"/>.</returns>
    public static ITagKey<TValue> Create<TValue>(IResourceKey<IRegistrar<TValue>> registryKey, ResourceName name)
        where TValue : class
    {
        var internKey = new InternKey(registryKey.Name, name);
        var possibleTagKey = Values.GetOrAdd(
            internKey,
            _ => new WeakReference(KeyFactory())
        ).Target;

        if (possibleTagKey is ITagKey<TValue> definiteTagKey) return definiteTagKey;

        definiteTagKey = KeyFactory();
        Values[internKey] = new WeakReference(definiteTagKey);
        return definiteTagKey;

        ITagKey<TValue> KeyFactory() => new TagKey<TValue> {
            RegistryKey = registryKey,
            Name = name,
        };
    }

    /// <summary>
    /// The <see cref="ResourceName"/> that uniquely identifies the tag within its target registry.
    /// </summary>
    public ResourceName Name { get; }

    [UsedImplicitly]
    private readonly record struct InternKey(ResourceName RegistryName, ResourceName Name);
}

/// <inheritdoc />
public interface ITagKey<out TValue> : ITagKey where TValue : class
{
    /// <summary>
    /// The <see cref="IResourceKey{TValue}"/> of the registry the <see cref="ITagKey{TValue}"/> targets.
    /// </summary>
    public IResourceKey<IRegistrar<TValue>> RegistryKey { get; }

    /// <summary>
    /// Check whether this <see cref="ITagKey{TValue}"/> is for a particular registry.
    /// </summary>
    /// <param name="registryKey">Registry to test against.</param>
    /// <typeparam name="TOtherRegistry">Type of the test registry.</typeparam>
    /// <returns><see langword="true"/> when compatible with the provided registry; Otherwise, <see langword="false"/>.</returns>
    public bool IsFor<TOtherRegistry>(ResourceKey<TOtherRegistry> registryKey)
        where TOtherRegistry : IRegistrar<object>;

    /// <summary>
    /// Cast this <see cref="ITagKey{TValue}"/> to be compatible with a particular registry.
    /// </summary>
    /// <param name="registryKey">Registry to cast for.</param>
    /// <typeparam name="TCasted">Value contained by the target registry.</typeparam>
    /// <returns>The casted <see cref="ITagKey{TValue}"/>, or <see langword="null"/> if the cast was invalid.</returns>
    public ITagKey<TCasted>? Cast<TCasted>(ResourceKey<IRegistrar<TCasted>> registryKey)
        where TCasted : class;
}
