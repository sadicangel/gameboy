namespace GameBoy.Core;

[Flags]
public enum Flags : byte
{
    Zero = 1 << 7,
    Subtract = 1 << 6,
    HalfCarry = 1 << 5,
    Carry = 1 << 4,
}

public static class FlagsExtensions
{
    public static bool IsFlagSet(byte value, Flags flag) =>
        (value & (byte)flag) != 0;
    public static void SetFlag(ref byte value, Flags flag, bool on) =>
        value = on ? (byte)(value | (byte)flag) : (byte)(value & ~(byte)flag);
}
