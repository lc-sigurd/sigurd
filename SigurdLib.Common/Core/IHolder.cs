using System;
using LanguageExt;
using Sigurd.Common.Resources;
using Sigurd.Common.Tags;

namespace Sigurd.Common.Core;

public interface IHolder
{
    public enum Kind
    {
        Reference,
        Direct,
    }
#if FALSE
    public abstract Direct<THeld>(THeld Value) : IHolder<THeld>
    {

    }

    public class Reference<THeld> : IHolder<THeld>
    {

    }
#endif
}

public interface IHolder<TValue> : IHolder, IReverseTag<TValue>
{
    bool IReverseTag<TValue>.Contains(ITagKey<TValue, IRegistrar<TValue>> tagKey) => Is(tagKey);

    /// <summary>
    /// The held <see cref="TValue"/>.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Determine whether this <see cref="IHolder{TValue}"/> was loaded with a value.
    /// If this is <see langword="false"/>, the holder is always empty.
    /// </summary>
    /// <returns><see langword="true"/> if this holder was loaded with a value (even if that value was empty)</returns>
    bool IsBound { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    bool Is(ResourceLocation location);

    bool Is(IResourceKey<TValue> resourceKey);

    bool Is(Predicate<IResourceKey<TValue>> predicate);

    bool Is(ITagKey<TValue, IRegistrar<TValue>> tagKey);

    Either<ResourceKey<TValue>, TValue> Unwrap();

    Option<ResourceKey<TValue>> UnwrapKey();

    Kind Kind { get; }

    bool CanSerializeIn(IHolderOwner<TValue> owner);
}
