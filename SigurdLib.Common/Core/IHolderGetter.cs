using System;
using Sigurd.Common.Core.Registries;
using Sigurd.Common.Core.Resources;
using Sigurd.Common.Core.Tags;
using Sigurd.Common.Util;

namespace Sigurd.Common.Core;

public interface IHolderGetter
{
    public interface Provider
    {
        Optional<IHolderGetter<THeld>> Lookup<THeld>(IResourceKey<ISigurdRegistrar<THeld>> resourceKey)
            where THeld : class;

        IHolderGetter<THeld> LookupOrThrow<THeld>(IResourceKey<ISigurdRegistrar<THeld>> resourceKey)
            where THeld : class
            => Lookup(resourceKey)
                .IfNone(() => throw new InvalidOperationException($"Registry {resourceKey.Name} not found"));
    }
}

public interface IHolderGetter<THeld> : IHolderGetter
    where THeld : class
{
    Optional<IHolder.Reference<THeld>> Get(IResourceKey<THeld> resourceKey);

    IHolder.Reference<THeld> GetOrThrow(IResourceKey<THeld> resourceKey) => Get(resourceKey)
        .IfNone(() => throw new InvalidOperationException($"Missing element {resourceKey}"));

    Optional<IHolderSet.Named<THeld>> Get(ITagKey<THeld> tagKey);

    IHolderSet.Named<THeld> GetOrThrow(ITagKey<THeld> tagKey) => Get(tagKey)
        .IfNone(() => throw new InvalidOperationException($"Missing tag {tagKey}"));
}
