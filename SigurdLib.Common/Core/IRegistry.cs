using System;
using System.Collections.Generic;
using Sigurd.Common.Resources;

namespace Sigurd.Common.Core;

public interface IRegistrar
{
    public static TValue Register<TValue, TRegistryValue>(IRegistry<TRegistryValue> registry, ResourceLocation name, TValue value)
        where TValue : TRegistryValue
    {
        throw new NotImplementedException();
    }

    public static TValue Register<TValue, TRegistryValue>(IRegistry<TRegistryValue> registry, ResourceKey<TRegistryValue> key, TValue value)
        where TValue : TRegistryValue
    {
        throw new NotImplementedException();
    }

    HashSet<ResourceLocation> NameSet { get; }

    bool ContainsName(ResourceLocation name);
}

public interface IKeyed<out T>
{
    IResourceKey<T> Key { get; }
}

public interface IRegistrar<out TValue> : IRegistrar, IKeyed<TValue>
{

}

public interface IRegistry<TValue> : IRegistrar<TValue>
{
    HashSet<KeyValuePair<IResourceKey<TValue>, TValue>> EntrySet { get; }

    HashSet<IResourceKey<TValue>> KeySet { get; }

    bool ContainsKey(IResourceKey<TValue> key);
}
