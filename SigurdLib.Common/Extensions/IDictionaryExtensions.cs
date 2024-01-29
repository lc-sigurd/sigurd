using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sigurd.Common.Extensions;

internal static class IDictionaryExtensions
{
    public delegate TValue ValueProvider<TKey, TValue>(TKey key);

    public delegate Task<TValue> ValueProviderAsync<TKey, TValue>(TKey key);

    public static TValue ComputeIfAbsent<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        ValueProvider<TKey, TValue> provider
    )
    {
        TValue value;
        if (dictionary.TryGetValue(key, out value)) return value;

        value = provider(key);
        dictionary[key] = value;
        return value;
    }

    public static async Task<TValue> ComputeIfAbsentAsync<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        ValueProviderAsync<TKey, TValue> provider
    ) {
        TValue value;
        if (dictionary.TryGetValue(key, out value)) return value;

        value = await provider(key);
        dictionary[key] = value;
        return value;
    }
}
