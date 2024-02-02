using System;
using System.Collections.Generic;
using LanguageExt;
using Sigurd.Common.Resources;
using Sigurd.Common.Tags;

namespace Sigurd.Common.Core;

public interface IRegistrar
{
    public static TValue Register<TValue>(IRegistrar<TValue> registry, ResourceLocation name, TValue value)
        where TValue : class
        => Register(registry, IResourceKey.Create(registry.Key, name), value);

    public static TValue Register<TValue, TValueBase>(IRegistrar<TValueBase> registry, ResourceKey<TValueBase> key, TValue value)
        where TValue : class, TValueBase
        where TValueBase : class
    {
        throw new NotImplementedException();
    }

    public static IHolder.Reference<THeld> RegisterForHolder<THeld>(IRegistrar<THeld> registry, ResourceLocation name, THeld value)
        where THeld : class
        => RegisterForHolder(registry, IResourceKey.Create(registry.Key, name), value);

    public static IHolder.Reference<THeld> RegisterForHolder<THeld>(IRegistrar<THeld> registry, ResourceKey<THeld> key, THeld value)
        where THeld : class
    {
        throw new NotImplementedException();
    }

    ISet<ResourceLocation> NameSet { get; }

    bool ContainsName(ResourceLocation name);
}

public interface IKeyed<out T>
{
    IResourceKey<T> Key { get; }
}

public interface IRegistrar<out TValue> : IRegistrar, IKeyed<IRegistrar<TValue>> where TValue : class
{
    TValue? Get(ResourceLocation? name);
}

public interface IRegistry<TValue> : IRegistrar<TValue> where TValue : class
{
    ResourceLocation? GetName(TValue? value);

    Option<IResourceKey<TValue>> GetKey(TValue? value);

    TValue? Get(IResourceKey<TValue>? key);

    Option<IHolder.Reference<TValue>> GetHolder(IResourceKey<TValue> key);

    IHolder.Reference<TValue> GetHolderOrThrow(IResourceKey<TValue> key) => GetHolder(key)
        .IfNone(() => throw new InvalidOperationException($"Missing key in {Key}: {key}"));

    IHolder<TValue> WrapAsHolder(TValue value);

    ISet<IResourceKey<TValue>> KeySet { get; }

    bool ContainsKey(IResourceKey<TValue> key);

    ISet<KeyValuePair<IResourceKey<TValue>, TValue>> EntrySet { get; }

    IEnumerable<IHolder.Reference<TValue>> Holders { get; }

    IEnumerable<KeyValuePair<ITagKey<TValue>, IHolderSet.Named<TValue>>> Tags { get; }

    IEnumerable<ITagKey<TValue>> TagKeys { get; }

    Option<IHolderSet.Named<TValue>> GetTag(ITagKey<TValue> tagKey);

    void ResetTags();

    void BindTags(Dictionary<ITagKey<TValue>, List<IHolder<TValue>>> tagMap);

    IHolderOwner<TValue> HolderOwner { get; }

    IHolderLookup.RegistryLookup<TValue> AsLookup();
}
