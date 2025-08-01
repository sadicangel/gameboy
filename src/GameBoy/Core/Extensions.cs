using System.Numerics;

namespace GameBoy.Core;
internal static class Extensions
{
    extension<T>(T value) where T : IBinaryInteger<T>
    {
        public bool HasBitSet(int bitPosition) =>
        (value & (T.One << bitPosition)) != T.Zero;
    }
}
