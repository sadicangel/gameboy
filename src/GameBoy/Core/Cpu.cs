using System.Diagnostics;

namespace GameBoy.Core;

[Singleton]
public sealed partial class Cpu(Bus bus, ILogger<Cpu> logger)
{
    public CpuRegisters Registers = new()
    {
        AF = 0x01B0,
        BC = 0x0013,
        DE = 0x00D8,
        HL = 0x014d,
        SP = 0xFFFE,
        PC = 0x0100,
    };

    private bool _ime;
    private bool _imeLatch;
    private bool _halted;
    private bool _haltBug;

    public event Action<string>? Output { add => bus.Output += value; remove => bus.Output -= value; }

    public byte Step()
    {
        if (_halted)
        {
            if (!bus.HasPendingInterrupts)
            {
                return 4;
            }

            _halted = false;

            if (_ime)
            {
                return ServiceInterrupts();
            }
        }

        var pc = Registers.PC;
        var instruction = FetchInstruction();
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("""
                    0x{PC:X4}: {Instruction}
                    A: 0x{A:X2}, B: 0x{B:X2}, D: 0x{D:X2}, H: 0x{H:X2}
                    F: 0x{F:X2}, C: 0x{C:X2}, E: 0x{E:X2}, L: 0x{L:X2}
                    Z:    {Z}, N:    {N}, H:    {H}, C:    {C}
                    """,
                pc, instruction,
                Registers.A, Registers.B, Registers.D, Registers.H,
                Registers.F, Registers.C, Registers.E, Registers.L,
                Convert.ToByte(Registers.Flags.Z), Convert.ToByte(Registers.Flags.N), Convert.ToByte(Registers.Flags.H), Convert.ToByte(Registers.Flags.C));
        }

        var cycles = instruction.Exec(this);

        if (_ime)
        {
            cycles += ServiceInterrupts();
        }

        if (_imeLatch)
        {
            _ime = true;
            _imeLatch = false;
        }

        return cycles;
    }

    private Instruction FetchInstruction()
    {
        var opcode = (Opcode)bus.Read(Registers.PC);

        if (opcode is Opcode.PREFIX)
        {
            throw new NotImplementedException(opcode.Description);
        }

        if (_haltBug)
            _haltBug = false;
        else
            Registers.PC++;

        var instruction = new Instruction(opcode);

        switch (opcode.ImmediateByteCount)
        {
            case 0:
                break;
            case 1:
                instruction.N8 = bus.Read(Registers.PC);
                Registers.PC++;
                break;
            case 2:
                instruction.N16 = bus.ReadWord(Registers.PC);
                Registers.PC++;
                Registers.PC++;
                break;
            default:
                throw new UnreachableException();
        }

        return instruction;
    }

    private byte ServiceInterrupts()
    {
        if (!bus.HasPendingInterrupts)
        {
            return 0;
        }

        var highestPriority = bus.PendingInterrupts.HighestPriority;

        _ime = false;
        bus.IF &= ~highestPriority;

        PUSH(Registers.PC);
        Registers.PC = (ushort)(0x40 + (byte)highestPriority * 8);

        return 20;
    }
}
