using System.Runtime.InteropServices;

namespace GameBoy.Core;

[StructLayout(LayoutKind.Explicit)]
public record struct CpuRegisters
{
    [FieldOffset(0)] public ushort AF;
    [FieldOffset(1)] public byte A;
    [FieldOffset(0)] public byte F;
    [FieldOffset(0)] public CpuFlags Flags;

    [FieldOffset(2)] public ushort BC;
    [FieldOffset(3)] public byte B;
    [FieldOffset(2)] public byte C;

    [FieldOffset(4)] public ushort DE;
    [FieldOffset(5)] public byte D;
    [FieldOffset(4)] public byte E;

    [FieldOffset(6)] public ushort HL;
    [FieldOffset(7)] public byte H;
    [FieldOffset(6)] public byte L;

    [FieldOffset(8)] public ushort PC;
    [FieldOffset(10)] public ushort SP;
}
