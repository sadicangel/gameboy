namespace GameBoy.Core;

partial class Cpu
{
    public readonly record struct Instruction
    {
        public readonly required InstructionType Type { get; init; }
        public readonly required byte OpCode { get; init; }
        public readonly AddressMode Mode { get; init; }
        public readonly RegisterType Register1 { get; init; }
        public readonly RegisterType Register2 { get; init; }
        public readonly ConditionType Condition { get; init; }
        public readonly byte Parameter { get; init; }
        public readonly required Action<Cpu> Exec { get; init; }

        public readonly bool IsConditionFlagSet(CpuFlags flags) => Condition switch
        {
            ConditionType.Z => flags.Z,
            ConditionType.NZ => !flags.Z,
            ConditionType.C => flags.C,
            ConditionType.NC => !flags.C,
            ConditionType.None => true,
            _ => throw new InvalidOperationException($"Invalid {nameof(ConditionType)} '{Condition}'")
        };

        public static Instruction FromOpCode(byte opCode) => opCode switch
        {
            0x00 => NOP,
            0x05 => DEC,
            0x0E => LD,
            0xAF => XOR,
            0xC3 => JP,
            0xF3 => DI,
            _ => new()
            {
                OpCode = opCode,
                Type = InstructionType.NONE,
                Exec = cpu => throw new InvalidOperationException($"Invalid instruction with opcode '{opCode}'")
            },
        };

        public static readonly Instruction NOP = new()
        {
            OpCode = 0x00,
            Type = InstructionType.NOP,
            Exec = static cpu => { }
        };

        public static readonly Instruction DEC = new()
        {
            OpCode = 0x05,
            Type = InstructionType.DEC,
            Mode = AddressMode.R,
            Register1 = RegisterType.B,
            Exec = static cpu => throw new NotImplementedException()
        };

        public static readonly Instruction LD = new()
        {
            OpCode = 0x0E,
            Type = InstructionType.LD,
            Mode = AddressMode.R_D8,
            Register1 = RegisterType.C,
            Exec = static cpu => throw new NotImplementedException()
        };

        public static readonly Instruction XOR = new()
        {
            OpCode = 0xAF,
            Type = InstructionType.XOR,
            Mode = AddressMode.R,
            Register1 = RegisterType.A,
            Exec = static cpu =>
            {
                cpu._registers.A ^= (byte)(cpu._data & 0xFF);
                cpu._registers.Flags.Z = cpu._registers.A == 0;
            }
        };

        public static readonly Instruction JP = new()
        {
            OpCode = 0xC3,
            Type = InstructionType.JP,
            Mode = AddressMode.D16,
            Exec = static cpu =>
            {
                cpu._registers.PC = cpu._data;
                cpu._cycles++;
            }
        };

        public static readonly Instruction DI = new()
        {
            OpCode = 0xF3,
            Type = InstructionType.DI,
            Exec = static cpu =>
            {
                cpu._masterInterruptsEnabled = false;
                _ = cpu._masterInterruptsEnabled;
            }
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
}
