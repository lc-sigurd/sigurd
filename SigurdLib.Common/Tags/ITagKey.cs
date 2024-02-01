using System;
using System.Collections.Concurrent;
using Sigurd.Common.Core;
using Sigurd.Common.Extensions;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Tags;

/// <summary>
/// An <see cref="ITagKey{TValue,TRegistry}"/> is used to uniquely identify an <see cref="ITag{TValue}"/>.
/// </summary>
public interface ITagKey
{
    private static readonly ConcurrentDictionary<InternKey, WeakReference> Values = new();

    /// <summary>
    /// Create a new <see cref="TagKey{TValue,TRegistry}"/>.
    /// </summary>
    /// <param name="registryKey">The <see cref="IResourceKey{TValue}"/> of the target registry.</param>
    /// <param name="location">The <see cref="ResourceLocation"/> used to identify the tag.</param>
    /// <typeparam name="TRegistry">The target registry type.</typeparam>
    /// <typeparam name="TValue">The type of object contained by the target registry.</typeparam>
    /// <returns>The newly created <see cref="TagKey{TValue,TRegistry}"/>.</returns>
    public static TagKey<TValue, TRegistry> Create<TRegistry, TValue>(IResourceKey<TRegistry> registryKey, ResourceLocation location)
        where TRegistry : IRegistrar<TValue>
        where TValue : class
    {
        var internKey = new InternKey(registryKey.Location, location);
        var possibleTagKey = Values.ComputeIfAbsent(
            internKey,
            _ => new WeakReference(KeyFactory())
        ).Target;

        if (possibleTagKey is TagKey<TValue, TRegistry> definiteTagKey) return definiteTagKey;

        definiteTagKey = KeyFactory();
        Values[internKey] = new WeakReference(definiteTagKey);
        return definiteTagKey;

        TagKey<TValue, TRegistry> KeyFactory() => new TagKey<TValue, TRegistry>(registryKey, location);
    }

    /// <summary>
    /// The <see cref="ResourceLocation"/> that uniquely identifies the tag within its target registry.
    /// </summary>
    public ResourceLocation Location { get; }

    private readonly record struct InternKey(ResourceLocation RegistryName, ResourceLocation Location);
}

/// <inheritdoc />
public interface ITagKey<out TValue, out TRegistry> : ITagKey
    where TRegistry : IRegistrar<TValue>
    where TValue : class
{
    /// <summary>
    /// The <see cref="IResourceKey{TValue}"/> of the registry the <see cref="ITagKey{TValue,TRegistry}"/> targets.
    /// </summary>
    public IResourceKey<TRegistry> RegistryKey { get; }

    public bool IsFor<TOtherRegistry>(ResourceKey<TOtherRegistry> registryKey)
        where TOtherRegistry : IRegistrar;

    public TagKey<TCasted, TOtherRegistry>? Cast<TCasted, TOtherRegistry>(ResourceKey<TOtherRegistry> registryKey)
        where TOtherRegistry : IRegistrar<TCasted>
        where TCasted : class;
}
