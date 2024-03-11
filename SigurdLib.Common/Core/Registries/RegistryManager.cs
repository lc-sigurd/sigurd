using System;
using Sigurd.Common.Core.Resources;
using Sigurd.Util.Collections.Generic;
using Sigurd.Util.Resources;

namespace Sigurd.Common.Core.Registries;

public class RegistryManager
{
    public static readonly RegistryManager Active = new RegistryManager("Active");

    private readonly BiDictionary<ResourceName, IRegistrar<object>> _registries = new();

    public string Name { get; }

    internal RegistryManager(string name)
    {
        Name = name;
    }

    public IRegistry<TValue> GetRegistry<TValue>(IResourceKey<IRegistrar<TValue>> key) where TValue : class
        => GetRegistry<TValue>(key.Name);

    public IRegistry<TValue> GetRegistry<TValue>(ResourceName name) where TValue : class
    {
        var registrar = GetRegistrar<TValue>(name);

        if (registrar is not IRegistry<TValue> registry)
            throw new InvalidCastException("Registrar does not implement Registry (!?!).");

        return registry;
    }

    internal SigurdRegistry<TValue> GetRegistryInternal<TValue>(IResourceKey<IRegistrar<TValue>> key) where TValue : class
        => GetRegistryInternal<TValue>(key.Name);

    internal SigurdRegistry<TValue> GetRegistryInternal<TValue>(ResourceName name) where TValue : class
    {
        var registrar = GetRegistrar<TValue>(name);

        if (registrar is not SigurdRegistry<TValue> registry)
            throw new InvalidCastException("Registrar is not implemented by Sigurd (!?!).");

        return registry;
    }

    private IRegistrar<TValue> GetRegistrar<TValue>(ResourceName name) where TValue : class
    {
        if (name is null)
            throw new ArgumentException("Cannot lookup registry with `null` name.");

        if (!_registries.TryGetValue(name, out IRegistrar<object>? maybeRegistry))
            throw new ArgumentException($"No registry with name `{name}` could be found.");

        if (maybeRegistry is not IRegistrar<TValue> definiteRegistry)
            throw new InvalidCastException($"Registry of name `{name}` does not support type {typeof(TValue)}.");

        return definiteRegistry;
    }

    public ResourceName GetName<TValue>(IRegistrar<TValue> registry) where TValue : class
    {
        if (registry is null)
            throw new ArgumentException("Cannot lookup name of `null` registry.");

        if (!_registries.TryGetKey(registry, out ResourceName? name))
            throw new ArgumentException($"The name for registry {registry} could not be found.");

        return name;
    }

    internal SigurdRegistry<TValue> CreateRegistry<TValue>(ResourceName name, RegistryConfiguration<TValue> configuration) where TValue : class
    {
        if (name is null)
            throw new ArgumentException("Cannot create registry with `null` name.");
        if (configuration is null)
            throw new ArgumentException("Cannot create registry with `null` configuration.");

        if (_registries.ContainsKey(name))
            throw new InvalidOperationException($"Attempted to create registry with name `{name}` but a registry by that name already exists.");

        var registry = new SigurdRegistry<TValue>(name, configuration);
        _registries[name] = registry;

        return registry;
    }
}
