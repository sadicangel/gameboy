using System.Runtime.InteropServices;

namespace GameBoy.Core;

[Singleton]
public sealed class Cpu(Bus bus, ILogger<Cpu> logger)
{
    private CpuRegisters _registers = new CpuRegisters
    {
        PC = 0x100,
    };

    //private ushort _memoryDestinaton;
    private Instruction _instruction;
    private ushort _data;
    //private bool _halted = false;
    private ulong _cycles;
    //private bool _stepping;

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
            logger.LogInformation("PC: {PC:X4}, SP {SP:X4}", _registers.PC, _registers.SP);
            _instruction = FetchInstruction();
            _data = FetchData();
            logger.LogInformation("Executing {@Instruction} with Data {@Data}", _instruction, _data);
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
public struct CpuRegisters
{
    [FieldOffset(0)] public ushort AF;
    [FieldOffset(0)] public byte A;
    [FieldOffset(1)] public byte F;

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
