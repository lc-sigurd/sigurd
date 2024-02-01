using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Sigurd.Common.Resources;
using Sigurd.Common.Tags;

namespace Sigurd.Common.Core;

public interface IHolderLookup
{
    public interface IHolderDelegate<THeld>
        where THeld : class
    {
        delegate Option<IHolder.Reference<THeld>> ResourceKeyGetter(IResourceKey<THeld> resourceKey);
        ResourceKeyGetter ResourceKeyGet { init; }

        delegate IEnumerable<IHolder.Reference<THeld>> ElementsGetter();
        ElementsGetter ElementsGet { init; }

        delegate Option<IHolderSet.Named<THeld>> TagKeyGetter(ITagKey<THeld, IRegistrar<THeld>> tagKey);
        TagKeyGetter TagKeyGet { init; }

        delegate IEnumerable<IHolderSet.Named<THeld>> TagsGetter();
        TagsGetter TagsGet { init; }
    }

    public class Delegate<THeld> : IHolderLookup<THeld>, IHolderDelegate<THeld>
        where THeld : class
    {
        protected readonly IHolderLookup<THeld> Parent;

        public Delegate(IHolderLookup<THeld> parent)
        {
            Parent = parent;
            ResourceKeyGet = key => Parent.Get(key);
            ElementsGet = () => Parent.Elements;
            TagKeyGet = key => Parent.Get(key);
            TagsGet = () => Parent.Tags;
        }

        /// <inheritdoc />
        public IHolderDelegate<THeld>.ResourceKeyGetter ResourceKeyGet { protected get; init; }

        /// <inheritdoc />
        public Option<IHolder.Reference<THeld>> Get(IResourceKey<THeld> resourceKey) => ResourceKeyGet(resourceKey);

        /// <inheritdoc />
        public IHolderDelegate<THeld>.ElementsGetter ElementsGet { protected get; init; }

        /// <inheritdoc />
        public IEnumerable<IHolder.Reference<THeld>> Elements => ElementsGet();

        /// <inheritdoc />
        public IHolderDelegate<THeld>.TagKeyGetter TagKeyGet { protected get; init; }

        /// <inheritdoc />
        public Option<IHolderSet.Named<THeld>> Get(ITagKey<THeld, IRegistrar<THeld>> tagKey) => TagKeyGet(tagKey);

        /// <inheritdoc />
        public IHolderDelegate<THeld>.TagsGetter TagsGet { protected get; init; }

        /// <inheritdoc />
        public IEnumerable<IHolderSet.Named<THeld>> Tags => TagsGet();
    }

    public interface Provider
    {
        Option<RegistryLookup<THeld>> Lookup<THeld>(IResourceKey<IRegistrar<THeld>> registryKey) where THeld : class;

        RegistryLookup<THeld> LookupOrThrow<THeld>(IResourceKey<IRegistrar<THeld>> registryKey) where THeld : class
            => Lookup(registryKey).IfNone(() => throw new InvalidOperationException($"Registry {registryKey.Location} not found"));

        private class ProviderImpl : Provider
        {
            private readonly IReadOnlyDictionary<IResourceKey<IRegistrar<object>>, RegistryLookup<object>> _map;

            public ProviderImpl(IEnumerable<RegistryLookup<object>> lookupEnumerable)
            {
                _map = lookupEnumerable.ToDictionary(lookup => lookup.Key);
            }

            public Option<RegistryLookup<THeld>> Lookup<THeld>(IResourceKey<IRegistrar<THeld>> registryKey) where THeld : class
            {
                var maybeRegistryLookup = _map[registryKey];
                if (maybeRegistryLookup is not RegistryLookup<THeld> definiteRegistryLookup)
                    return Option<RegistryLookup<THeld>>.None;

                return Option<RegistryLookup<THeld>>.Some(definiteRegistryLookup);
            }
        }

        static Provider Create(IEnumerable<RegistryLookup<object>> lookupEnumerable) => new ProviderImpl(lookupEnumerable);
    }

    public interface RegistryLookup
    {
        public class Delegate<THeld> : RegistryLookup<THeld>, IHolderDelegate<THeld>
            where THeld : class
        {
            protected readonly RegistryLookup<THeld> Parent;

            public Delegate(RegistryLookup<THeld> parent)
            {
                Parent = parent;
                ResourceKeyGet = key => Parent.Get(key);
                ElementsGet = () => Parent.Elements;
                TagKeyGet = key => Parent.Get(key);
                TagsGet = () => Parent.Tags;
            }

            /// <inheritdoc />
            public IResourceKey<IRegistrar<THeld>> Key => Parent.Key;

            /// <inheritdoc />
            public IHolderDelegate<THeld>.ResourceKeyGetter ResourceKeyGet { protected get; init; }

            /// <inheritdoc />
            public Option<IHolder.Reference<THeld>> Get(IResourceKey<THeld> resourceKey) => ResourceKeyGet(resourceKey);

            /// <inheritdoc />
            public IHolderDelegate<THeld>.ElementsGetter ElementsGet { protected get; init; }

            /// <inheritdoc />
            public IEnumerable<IHolder.Reference<THeld>> Elements => ElementsGet();

            /// <inheritdoc />
            public IHolderDelegate<THeld>.TagKeyGetter TagKeyGet { protected get; init; }

            /// <inheritdoc />
            public Option<IHolderSet.Named<THeld>> Get(ITagKey<THeld, IRegistrar<THeld>> tagKey) => TagKeyGet(tagKey);

            /// <inheritdoc />
            public IHolderDelegate<THeld>.TagsGetter TagsGet { protected get; init; }

            /// <inheritdoc />
            public IEnumerable<IHolderSet.Named<THeld>> Tags => TagsGet();
        }
    }

    public interface RegistryLookup<THeld> : IHolderLookup<THeld>, IHolderOwner<THeld>, RegistryLookup
        where THeld : class
    {
        IResourceKey<IRegistrar<THeld>> Key { get; }
    }
}

public interface IHolderLookup<THeld> : IHolderGetter<THeld>, IHolderLookup
    where THeld : class
{
    IEnumerable<IHolder.Reference<THeld>> Elements { get; }

    IEnumerable<IResourceKey<THeld>> ElementIds => Elements.Select(holderReference => holderReference.Key);

    IEnumerable<IHolderSet.Named<THeld>> Tags { get; }

    IEnumerable<ITagKey<THeld, IRegistrar<THeld>>> TagIds => Tags.Select(holderSet => holderSet.Key);

    IHolderLookup<THeld> WithFilteredElements(Predicate<THeld> predicate)
    {
        return new Delegate<THeld>(this) {
            ResourceKeyGet = key => Get(key).Filter(holderReference => predicate(holderReference.Value)),
            ElementsGet = () => Elements.Where(holderReference => predicate(holderReference.Value)),
        };
    }
}
