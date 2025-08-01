namespace GameBoy.Core;

public struct Register
{
    public byte Hi;
    public byte Lo;

    public static implicit operator Register(ushort value) => new() { Hi = (byte)(value >> 8), Lo = (byte)(value & 0xFF) };

    public static implicit operator ushort(Register r) => (ushort)((r.Hi << 8) | r.Lo);
}
