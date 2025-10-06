using System.Diagnostics;

namespace GameBoy.Core;

[Singleton]
public sealed partial class Cpu(Bus bus, ILogger<Cpu> logger)
{
    public CpuRegisters Registers = new()
    {
        PC = 0x100,
    };

    //private bool _halted = false;
    //private bool _stepping;
    //private bool _masterInterruptsEnabled = true;

    public ulong Cycles { get; private set; }

    public bool Step()
    {
        //if (!_halted)
        {
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

            Cycles += instruction.Exec(this);
        }

        return true;
    }

    private Instruction FetchInstruction()
    {
        var opcode = (Opcode)bus.Read(Registers.PC);

        if (opcode is Opcode.PREFIX)
        {
            throw new NotImplementedException(opcode.Description);
        }

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
}
