namespace Sigurd.Common.Extensions;

/// <summary>
/// Extension methods for instances of the <see cref="int"/> class.
/// </summary>
public static class Int32Extensions
{
    // https://stackoverflow.com/a/32725808
    /// <summary>
    /// Counts the number of leading zero
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static int NumberOfTrailingZeroes(this int input)
    {
        int accumulator;
        if (input == 0) return 32;
        int n = 31;
        accumulator = input << 16; if (accumulator != 0) { n -= 16; input = accumulator; }
        accumulator = input << 08; if (accumulator != 0) { n -= 08; input = accumulator; }
        accumulator = input << 04; if (accumulator != 0) { n -= 04; input = accumulator; }
        accumulator = input << 02; if (accumulator != 0) { n -= 02; input = accumulator; }
        return n - ((input << 1) >>> 31);
    }
}
