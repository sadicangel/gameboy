using System.Diagnostics;
using System.Numerics;

namespace GameBoy.Core;

[Singleton]
public sealed partial class Cpu(Bus bus, InterruptController interrupts, ILogger<Cpu> logger)
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

    public byte Step()
    {
        if (_halted)
        {
            if (!interrupts.HasPending)
            {
                return 4;
            }

            _halted = false;

            if (_ime)
            {
                return ServiceInterrupts();
            }

            if (_imeLatch)
            {
                _ime = true;
                _imeLatch = false;
            }
        }

        var pc = Registers.PC;
        var instruction = FetchInstruction();

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("0x{PC:X4}: {Instruction}", pc, instruction);
        }

        var cycles = instruction.Exec(this);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("""
                    PC: {PC:X4}, SP: {SP:X4},
                    A: {A:X2}, B: {B:X2}, D: {D:X2}, H: {H:X2}
                    F: {F:X2}, C: {C:X2}, E: {E:X2}, L: {L:X2}
                    Z:  {Z}, N:  {N}, H:  {H}, C:  {C}
                    """,
                Registers.PC, Registers.SP,
                Registers.A, Registers.B, Registers.D, Registers.H,
                Registers.F, Registers.C, Registers.E, Registers.L,
                Convert.ToByte(Registers.Flags.Z), Convert.ToByte(Registers.Flags.N), Convert.ToByte(Registers.Flags.H), Convert.ToByte(Registers.Flags.C));
        }

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
        if (!interrupts.TryPopHighestPending(out var highestPriority))
        {
            return 0;
        }

        _ime = false;

        PUSH(Registers.PC);
        Registers.PC = (ushort)(0x40 + BitOperations.TrailingZeroCount((byte)highestPriority) * 8);

        return 20;
    }
}
