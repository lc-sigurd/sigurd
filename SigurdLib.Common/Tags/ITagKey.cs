using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Sigurd.Common.Core;
using Sigurd.Common.Extensions;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Tags;

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
    /// <param name="location">The <see cref="ResourceLocation"/> used to identify the tag.</param>
    /// <typeparam name="TValue">The type of object contained by the target registry.</typeparam>
    /// <returns>The newly created <see cref="TagKey{TValue}"/>.</returns>
    public static TagKey<TValue> Create<TValue>(IResourceKey<IRegistrar<TValue>> registryKey, ResourceLocation location)
        where TValue : class
    {
        var internKey = new InternKey(registryKey.Location, location);
        var possibleTagKey = Values.ComputeIfAbsent(
            internKey,
            _ => new WeakReference(KeyFactory())
        ).Target;

        if (possibleTagKey is TagKey<TValue> definiteTagKey) return definiteTagKey;

        definiteTagKey = KeyFactory();
        Values[internKey] = new WeakReference(definiteTagKey);
        return definiteTagKey;

        TagKey<TValue> KeyFactory() => new TagKey<TValue>(registryKey, location);
    }

    /// <summary>
    /// The <see cref="ResourceLocation"/> that uniquely identifies the tag within its target registry.
    /// </summary>
    public ResourceLocation Location { get; }

    [UsedImplicitly]
    private readonly record struct InternKey(ResourceLocation RegistryName, ResourceLocation Location);
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
        where TOtherRegistry : IRegistrar;

    /// <summary>
    /// Cast this <see cref="ITagKey{TValue}"/> to be compatible with a particular registry.
    /// </summary>
    /// <param name="registryKey">Registry to cast for.</param>
    /// <typeparam name="TCasted">Value contained by the target registry.</typeparam>
    /// <returns>The casted <see cref="ITagKey{TValue}"/>, or <see langword="null"/> if the cast was invalid.</returns>
    public ITagKey<TCasted>? Cast<TCasted>(ResourceKey<IRegistrar<TCasted>> registryKey)
        where TCasted : class;
}
