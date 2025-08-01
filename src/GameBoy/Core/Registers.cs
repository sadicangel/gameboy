namespace GameBoy.Core;

public struct Registers
{
    public Register AF; // Accumulator and Flags
    public Register BC; // Register Pair BC
    public Register DE; // Register Pair DE
    public Register HL; // Register Pair HL
    public Register PC; // Program Counter
    public Register SP; // Stack Pointer

    public Registers()
    {
        AF = 0x01B0;
        BC = 0x0013;
        DE = 0x00D8;
        HL = 0x014D;
        PC = 0x100;
        SP = 0xFFFE;
    }
}
