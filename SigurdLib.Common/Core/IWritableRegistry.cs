using Sigurd.Common.Resources;

namespace Sigurd.Common.Core;

public interface IWritableRegistry<TValue> : IRegistry<TValue>
    where TValue : class
{
    IHolder.Reference<TValue> Register(ResourceKey<TValue> key, TValue value);

    bool IsEmpty();
}
