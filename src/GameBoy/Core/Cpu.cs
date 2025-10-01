using System.Runtime.InteropServices;

namespace GameBoy.Core;

[Singleton]
public sealed partial class Cpu(Bus bus, ILogger<Cpu> logger)
{
    private CpuRegisters _registers = new()
    {
        PC = 0x100,
    };

    //private ushort _memoryDestinaton;
    private Instruction _instruction;
    private ushort _data;
    //private bool _halted = false;
    private ulong _cycles;
    //private bool _stepping;
    private bool _masterInterruptsEnabled = true;

    public ushort Read(RegisterType register) => register switch
    {
        RegisterType.A => _registers.A,
        RegisterType.F => _registers.F,
        RegisterType.B => _registers.B,
        RegisterType.C => _registers.C,
        RegisterType.D => _registers.D,
        RegisterType.E => _registers.E,
        RegisterType.H => _registers.H,
        RegisterType.L => _registers.L,
        RegisterType.AF => _registers.AF,
        RegisterType.BC => _registers.BC,
        RegisterType.DE => _registers.DE,
        RegisterType.HL => _registers.HL,
        RegisterType.SP => _registers.SP,
        RegisterType.PC => _registers.PC,
        _ => 0,
    };

    public bool Step()
    {
        //if (!_halted)
        {
            _instruction = FetchInstruction();
            _data = FetchData();
            logger.LogInformation("""
                0x{PC:X4}: {Instruction} (0x{OpCode:X2} 0x{PC1:X2} 0x{PC2:X2})
                A: 0x{A:X2}, B: 0x{B:X2}, D: 0x{D:X2}, H: 0x{H:X2} 
                F: 0x{F:X2}, C: 0x{C:X2}, E: 0x{E:X2}, L: 0x{L:X2}
                Z: {Z:X2}, N: {N:X2}, H: {H:X2}, C: {C:X2}
                """, _registers.PC, _instruction.Type.ToString().PadRight(4), _instruction.OpCode, bus.Read((ushort)(_registers.PC + 1)), bus.Read((ushort)(_registers.PC + 2)),
                _registers.A, _registers.B, _registers.D, _registers.H,
                _registers.F, _registers.C, _registers.E, _registers.L,
                _registers.Flags.Z, _registers.Flags.N, _registers.Flags.H, _registers.Flags.C);
            _instruction.Exec(this);
        }

        return true;
    }

    private Instruction FetchInstruction()
    {
        var instruction = Instruction.FromOpCode(bus.Read(_registers.PC));
        if (instruction.Type is InstructionType.NONE)
        {
            throw new NotImplementedException();
        }

        _registers.PC++;

        return instruction;
    }

    private ushort FetchData()
    {
        var data = _data;

        switch (_instruction.Mode)
        {
            case AddressMode.IMP:
                break;

            case AddressMode.R:
                data = Read(_instruction.Register1);
                break;

            case AddressMode.R_D8:
                data = bus.Read(_registers.PC);
                _cycles++;
                _registers.PC++;
                break;

            case AddressMode.D16:
                var lo = bus.Read(_registers.PC);
                _cycles++;
                var hi = bus.Read((ushort)(_registers.PC + 1));
                _cycles++;
                data = (ushort)(lo | (hi << 8));
                _registers.PC += 2;
                break;

            default:
                throw new NotImplementedException(_instruction.Mode.ToString());
        }

        return data;
    }
}

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

public record struct CpuFlags
{
    public byte _0;

    public bool Z { readonly get => _0.HasBitSet(7); set => _0.SetBit(7, value); }
    public bool N { readonly get => _0.HasBitSet(6); set => _0.SetBit(6, value); }
    public bool H { readonly get => _0.HasBitSet(5); set => _0.SetBit(5, value); }
    public bool C { readonly get => _0.HasBitSet(4); set => _0.SetBit(4, value); }
}
