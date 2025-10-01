namespace GameBoy.Core;

public readonly record struct Instruction(
    byte OpCode,
    InstructionType Type,
    AddressMode Mode = AddressMode.IMP,
    RegisterType Register1 = RegisterType.NONE,
    RegisterType Register2 = RegisterType.NONE,
    ConditionType Condition = ConditionType.None,
    byte Parameter = 0)
{
    public static Instruction FromOpCode(byte opCode) => opCode switch
    {
        0x00 => new Instruction(opCode, InstructionType.NOP),
        0x05 => new Instruction(opCode, InstructionType.DEC, AddressMode.R, RegisterType.B),
        0x0E => new Instruction(opCode, InstructionType.LD, AddressMode.R_D8, RegisterType.C),
        0xAF => new Instruction(opCode, InstructionType.XOR, AddressMode.R, RegisterType.A),
        0xC3 => new Instruction(opCode, InstructionType.JP, AddressMode.D16),
        0xF3 => new Instruction(opCode, InstructionType.DI),
        _ => new Instruction(opCode, InstructionType.NONE)
    };
}

public enum InstructionType : byte
{
    NONE,
    NOP,
    LD,
    INC,
    DEC,
    RLCA,
    ADD,
    RRCA,
    STOP,
    RLA,
    JR,
    RRA,
    DAA,
    CPL,
    SCF,
    CCF,
    HALT,
    ADC,
    SUB,
    SBC,
    AND,
    XOR,
    OR,
    CP,
    POP,
    JP,
    PUSH,
    RET,
    CB,
    CALL,
    RETI,
    LDH,
    JPHL,
    DI,
    EI,
    RST,
    ERR,
    RLC,
    RRC,
    RL,
    RR,
    SLA,
    SRA,
    SWAP,
    SRL,
    BIT,
    RES,
    SET,
}

public enum AddressMode : byte
{
    IMP,
    R_D16,
    R_R,
    MR_R,
    R,
    R_D8,
    R_MR,
    R_HLI,
    R_HLD,
    HLI_R,
    HLD_R,
    R_A8,
    A8_R,
    HL_SPR,
    D16,
    D8,
    D16_R,
    MR_D8,
    MR,
    A16_R,
    R_A16,
}

public enum RegisterType : byte
{
    NONE,
    A,
    F,
    B,
    C,
    D,
    E,
    H,
    L,
    AF,
    BC,
    DE,
    HL,
    SP,
    PC,
}

public enum ConditionType : byte
{
    None,
    NZ,
    Z,
    NC,
    C,
}
