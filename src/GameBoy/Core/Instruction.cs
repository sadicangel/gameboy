using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameBoy.Core;

[StructLayout(LayoutKind.Explicit)]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public struct Instruction(Opcode opcode)
{
    [field: FieldOffset(0)] public Opcode Opcode { get; init; } = opcode;
    [field: FieldOffset(1)] public byte N8 { get; set; }
    [field: FieldOffset(1)] public sbyte E8 { get; set; }
    [field: FieldOffset(1)] public ushort N16 { get; set; }
    public readonly byte Exec(Cpu cpu) => Opcode.Exec(cpu, this);

    public override readonly string ToString() => Opcode.Description
        .Replace("n8", $"{N8:X2}")
        .Replace("n16", $"{N16:X2}")
        .Replace("a8", $"[{N8:X2}]")
        .Replace("a16", $"[{N16:X2}]")
        .Replace("e8", $"{E8:X2}");

    private readonly string GetDebuggerDisplay() => ToString();
}
