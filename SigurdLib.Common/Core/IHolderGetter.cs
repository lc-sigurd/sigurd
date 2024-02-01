using System;
using LanguageExt;
using Sigurd.Common.Resources;
using Sigurd.Common.Tags;

namespace Sigurd.Common.Core;

public interface IHolderGetter
{
    public interface Provider
    {
        Option<IHolderGetter<THeld>> Lookup<THeld>(IResourceKey<IRegistrar<THeld>> resourceKey)
            where THeld : class;

        IHolderGetter<THeld> LookupOrThrow<THeld>(IResourceKey<IRegistrar<THeld>> resourceKey)
            where THeld : class
            => Lookup(resourceKey)
                .IfNone(() => throw new InvalidOperationException($"Registry {resourceKey.Location} not found"));
    }
}

public interface IHolderGetter<THeld> : IHolderGetter
    where THeld : class
{
    Option<IHolder.Reference<THeld>> Get(IResourceKey<THeld> resourceKey);

    IHolder.Reference<THeld> GetOrThrow(IResourceKey<THeld> resourceKey) => Get(resourceKey)
        .IfNone(() => throw new InvalidOperationException($"Missing element {resourceKey}"));

    Option<IHolderSet.Named<THeld>> Get(ITagKey<THeld, IRegistrar<THeld>> tagKey);

    IHolderSet.Named<THeld> GetOrThrow(ITagKey<THeld, IRegistrar<THeld>> tagKey) => Get(tagKey)
        .IfNone(() => throw new InvalidOperationException($"Missing tag {tagKey}"));
}
