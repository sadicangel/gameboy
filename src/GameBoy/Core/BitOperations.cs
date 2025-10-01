using System.Numerics;

namespace GameBoy.Core;

public static class BitOperations
{
    public static bool HasBitSet<T>(this T value, int bit) where T : struct, IBinaryInteger<T>, IShiftOperators<T, int, T> =>
        (value & (T.One << bit)) != T.Zero;

    public static void SetBit<T>(ref this T value, int bit, bool set) where T : struct, IBinaryInteger<T>, IShiftOperators<T, int, T>
    {
        if (set)
            value |= (T.One << bit);
        else
            value &= ~(T.One << bit);
    }

    public static bool InBetween<T>(this T value, T min, T max) where T : struct, IBinaryInteger<T> =>
        value >= min && value <= max;
}
