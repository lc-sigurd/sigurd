using System;
using System.Collections;
using System.Reflection;

namespace SigurdLib.Util.Extensions;

/// <summary>
/// Extension methods for instances of the <see cref="BitArray"/> class.
/// </summary>
public static class BitArrayExtensions
{
    private static readonly FieldInfo BitArrayWordsFieldInfo = typeof(BitArray)
        .GetField("m_array", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static int[] GetWords(this BitArray bitArray)
    {
        int[]? words = BitArrayWordsFieldInfo.GetValue(bitArray) as int[];
        if (words is null)
            throw new ArgumentException("Backing integer array not found for supplied bit array");
        return words;
    }

    /// <summary>
    /// Retrieve the index of the first occurrence of a '0' bit after <paramref name="fromIndex"/>.
    /// </summary>
    /// <param name="bitArray">The <see cref="BitArray"/> to search in.</param>
    /// <param name="fromIndex">The index to start searching from.</param>
    /// <returns>The index of the first '0' bit occurrence after <paramref name="fromIndex"/>.</returns>
    /// <exception cref="ArgumentException">The provided index is negative.</exception>
    public static int NextClearBitIndex(this BitArray bitArray, int fromIndex)
    {
        if (fromIndex < 0)
            throw new ArgumentException("Tried to index bit array with negative index");

        var words = bitArray.GetWords();
        int wordIndex = fromIndex >> 5;

        int word;
        for (word = ~words[wordIndex] & -1 << fromIndex; word == 0; word = ~words[wordIndex]) {
            wordIndex++;
            if (wordIndex == words.Length) return wordIndex * 32;
        }

        return wordIndex * 32 + word.NumberOfTrailingZeroes();
    }

    /// <summary>
    /// Retrieve the index of the first occurrence of a '1' bit after <paramref name="fromIndex"/>.
    /// </summary>
    /// <param name="bitArray">The <see cref="BitArray"/> to search in.</param>
    /// <param name="fromIndex">The index to start searching from.</param>
    /// <returns>The index of the first '1' bit occurrence after <paramref name="fromIndex"/>, if found; Otherwise, -1.</returns>
    /// <exception cref="ArgumentException">The provided index is negative.</exception>
    public static int NextSetBitIndex(this BitArray bitArray, int fromIndex)
    {
        if (fromIndex < 0)
            throw new ArgumentException("Tried to index bit array with negative index");

        var words = bitArray.GetWords();
        int wordIndex = fromIndex >> 5;

        int word;
        for (word = words[wordIndex] & -1 << fromIndex; word == 0; word = words[wordIndex]) {
            wordIndex++;
            if (wordIndex == words.Length) return -1;
        }

        return wordIndex * 32 + word.NumberOfTrailingZeroes();
    }
}
