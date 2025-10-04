using System.Runtime.InteropServices;

namespace GameBoy.Core;

[StructLayout(LayoutKind.Explicit)]
public record struct CpuRegisters
{
    [FieldOffset(0)] public ushort AF;
    [FieldOffset(0)] public byte A;
    [FieldOffset(1)] public byte F;

    [FieldOffset(1)] public CpuFlags Flags;

    [FieldOffset(2)] public ushort BC;
    [FieldOffset(2)] public byte B;
    [FieldOffset(3)] public byte C;

    [FieldOffset(4)] public ushort DE;
    [FieldOffset(4)] public byte D;
    [FieldOffset(5)] public byte E;

    [FieldOffset(6)] public ushort HL;
    [FieldOffset(6)] public byte H;
    [FieldOffset(7)] public byte L;

    [FieldOffset(8)] public ushort PC;
    [FieldOffset(10)] public ushort SP;
}
