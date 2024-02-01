using System;
using System.Collections.Generic;
using Sigurd.Common.Resources;

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

    HashSet<ResourceLocation> NameSet { get; }

    bool ContainsName(ResourceLocation? name);
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
    ResourceLocation? GetName(TValue value);

    HashSet<IResourceKey<TValue>> KeySet { get; }

    bool ContainsKey(IResourceKey<TValue>? key);

    TValue? Get(IResourceKey<TValue>? key);

    IResourceKey<TValue>? GetKey(TValue value);

    HashSet<KeyValuePair<IResourceKey<TValue>, TValue>> EntrySet { get; }
}
