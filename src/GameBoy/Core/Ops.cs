using System.Diagnostics.CodeAnalysis;

namespace GameBoy.Core;

[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "pointer interface")]
public static class Ops
{
    private static void SetZ(ref this CpuFlags flags, int b) => flags.Z = (byte)b == 0;

    private static void SetC(ref this CpuFlags flags, int i) => flags.C = (i >> 8) != 0;

    private static void SetH(ref this CpuFlags flags, byte b1, byte b2) => flags.H = ((b1 & 0xF) + (b2 & 0xF)) > 0xF;

    private static void SetH(ref this CpuFlags flags, ushort w1, ushort w2) => flags.H = ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF;

    private static void SetHCarry(ref this CpuFlags flags, byte b1, byte b2) => flags.H = ((b1 & 0xF) + (b2 & 0xF)) >= 0xF;

    private static void SetHSub(ref this CpuFlags flags, byte b1, byte b2) => flags.H = (b1 & 0xF) < (b2 & 0xF);

    private static void SetHSubCarry(ref this CpuFlags flags, byte b1, byte b2) => flags.H = (b1 & 0xF) < ((b2 & 0xF) + (flags.C ? 1 : 0));

    private static byte INC(this Cpu cpu, ref byte register)
    {
        register++;
        cpu.Registers.Flags.SetZ(register);
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.SetH(register, 1);

        return 4;
    }

    private static byte DEC(this Cpu cpu, ref byte register)
    {
        register--;
        cpu.Registers.Flags.SetZ(register);
        cpu.Registers.Flags.N = true;
        cpu.Registers.Flags.SetHSub(register, 1);

        return 4;
    }

    private static byte DAD(this Cpu cpu, ushort value)
    {
        var result = cpu.Registers.HL + value;
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.SetH(cpu.Registers.HL, value);
        cpu.Registers.Flags.C = result >> 16 != 0;
        cpu.Registers.HL = (ushort)result;

        return 8;
    }

    private static byte ADD(this Cpu cpu, byte value)
    {
        var result = cpu.Registers.A + value;
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.SetH(cpu.Registers.A, value);
        cpu.Registers.Flags.SetC(result);
        cpu.Registers.A = (byte)result;

        return 4;
    }

    private static byte ADC(this Cpu cpu, byte value)
    {
        var carry = cpu.Registers.Flags.C ? 1 : 0;
        var result = cpu.Registers.A + value + carry;
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = false;
        if (cpu.Registers.Flags.C)
            cpu.Registers.Flags.SetHCarry(cpu.Registers.A, value);
        else
            cpu.Registers.Flags.SetH(cpu.Registers.A, value);
        cpu.Registers.Flags.SetC(result);
        cpu.Registers.A = (byte)result;

        return 4;
    }

    private static byte SUB(this Cpu cpu, byte value)
    {
        var result = cpu.Registers.A - value;
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = true;
        cpu.Registers.Flags.SetHSub(cpu.Registers.A, value);
        cpu.Registers.Flags.SetC(result);
        cpu.Registers.A = (byte)result;

        return 4;
    }

    private static byte SBC(this Cpu cpu, byte value)
    {
        var carry = cpu.Registers.Flags.C ? 1 : 0;
        var result = cpu.Registers.A - value - carry;
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = true;
        if (cpu.Registers.Flags.C)
            cpu.Registers.Flags.SetHSubCarry(cpu.Registers.A, value);
        else
            cpu.Registers.Flags.SetHSub(cpu.Registers.A, value);
        cpu.Registers.Flags.SetC(result);
        cpu.Registers.A = (byte)result;

        return 4;
    }

    private static byte AND(this Cpu cpu, byte value)
    {
        var result = (byte)(cpu.Registers.A & value);
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.H = true;
        cpu.Registers.Flags.C = false;
        cpu.Registers.A = result;

        return 4;
    }

    private static byte XOR(this Cpu cpu, byte value)
    {
        var result = (byte)(cpu.Registers.A ^ value);
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.H = false;
        cpu.Registers.Flags.C = false;
        cpu.Registers.A = result;

        return 4;
    }

    private static byte OR(this Cpu cpu, byte value)
    {
        var result = (byte)(cpu.Registers.A | value);
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.H = false;
        cpu.Registers.Flags.C = false;
        cpu.Registers.A = result;

        return 4;
    }

    private static byte CP(this Cpu cpu, byte value)
    {
        var result = cpu.Registers.A - value;
        cpu.Registers.Flags.SetZ(result);
        cpu.Registers.Flags.N = true;
        cpu.Registers.Flags.SetHSub(cpu.Registers.A, value);
        cpu.Registers.Flags.SetC(result);

        return 4;
    }

    private static byte JR(this Cpu cpu, sbyte address, bool flag)
    {
        if (!flag)
        {
            return 8;
        }

        cpu.Registers.PC = (ushort)(cpu.Registers.PC + address);

        return 12;
    }

    private static byte JP(this Cpu cpu, ushort address, bool flag)
    {
        if (!flag)
        {
            return 12;
        }

        cpu.Registers.PC = address;

        return 16;
    }

    private static byte RET(this Cpu cpu, Mmu mmu, bool flag)
    {
        if (!flag)
        {
            return 8;
        }

        cpu.Registers.PC = cpu.POP(mmu);

        return 20;
    }

    private static byte CALL(this Cpu cpu, Mmu mmu, ushort address, bool flag)
    {
        if (!flag)
        {
            return 12;
        }

        cpu.PUSH(mmu, cpu.Registers.PC);
        cpu.Registers.PC = address;

        return 24;
    }

    private static byte RST(this Cpu cpu, Mmu mmu, byte value)
    {
        cpu.PUSH(mmu, cpu.Registers.PC);
        cpu.Registers.PC = value;

        return 16;
    }

    private static void PUSH(this Cpu cpu, Mmu mmu, ushort word)
    {
        cpu.Registers.SP -= 2;
        mmu.WriteWord(cpu.Registers.SP, word);
    }

    private static ushort POP(this Cpu cpu, Mmu mmu)
    {
        var result = mmu.ReadWord(cpu.Registers.SP);
        cpu.Registers.SP += 2;
        return result;
    }

    public static byte NOP(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return 4;
    }

    public static byte LD_BC_d16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.BC = instruction.N16;
        return 12;
    }

    public static byte LD_ptr_BC_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.BC, cpu.Registers.A);
        return 8;
    }

    public static byte INC_BC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.BC++;
        return 8;
    }

    public static byte INC_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.INC(ref cpu.Registers.B);
    }

    public static byte DEC_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DEC(ref cpu.Registers.B);
    }

    public static byte LD_B_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = instruction.N8;
        return 8;
    }

    public static byte RLCA(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.F = 0;
        cpu.Registers.Flags.C = (cpu.Registers.A & 0x80) != 0;
        cpu.Registers.A = (byte)(cpu.Registers.A << 1 | cpu.Registers.A >> 7);
        return 4;
    }

    public static byte LD_ptr_a16_SP(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteWord(cpu.Registers.SP, instruction.N16);
        return 20;
    }

    public static byte ADD_HL_BC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DAD(cpu.Registers.BC);
    }

    public static byte LD_A_ptr_BC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte(cpu.Registers.BC);
        return 8;
    }

    public static byte DEC_BC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.BC--;
        return 8;
    }

    public static byte INC_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.INC(ref cpu.Registers.C);
    }

    public static byte DEC_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DEC(ref cpu.Registers.C);
    }

    public static byte LD_C_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = instruction.N8;
        return 8;
    }

    public static byte RRCA(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.F = 0;
        cpu.Registers.Flags.C = ((cpu.Registers.A & 0x1) != 0);
        cpu.Registers.A = (byte)(cpu.Registers.A >> 1 | cpu.Registers.A << 7);
        return 4;
    }

    public static byte STOP_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public static byte LD_DE_d16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.DE = instruction.N16;
        return 12;
    }

    public static byte LD_ptr_DE_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.DE, cpu.Registers.A);
        return 8;
    }

    public static byte INC_DE(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.DE++;
        return 8;
    }

    public static byte INC_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.INC(ref cpu.Registers.D);
    }

    public static byte DEC_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DEC(ref cpu.Registers.D);
    }

    public static byte LD_D_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = instruction.N8;
        return 8;
    }

    public static byte RLA(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        var prevFlagC = cpu.Registers.Flags.C;
        cpu.Registers.F = 0;
        cpu.Registers.Flags.C = ((cpu.Registers.A & 0x80) != 0);
        cpu.Registers.A = (byte)((cpu.Registers.A << 1) | (prevFlagC ? 1 : 0));
        return 4;
    }

    public static byte JR_e8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JR(instruction.E8, flag: true);
    }

    public static byte ADD_HL_DE(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DAD(cpu.Registers.DE);
    }

    public static byte LD_A_ptr_DE(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte(cpu.Registers.DE);
        return 8;
    }

    public static byte DEC_DE(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.DE--;
        return 8;
    }

    public static byte INC_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.INC(ref cpu.Registers.E);
    }

    public static byte DEC_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DEC(ref cpu.Registers.E);
    }

    public static byte LD_E_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = instruction.N8;
        return 8;
    }

    public static byte RRA(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        var prevFlagC = cpu.Registers.Flags.C;
        cpu.Registers.F = 0;
        cpu.Registers.Flags.C = ((cpu.Registers.A & 0x1) != 0);
        cpu.Registers.A = (byte)((cpu.Registers.A >> 1) | (prevFlagC ? 0x80 : 0));
        return 4;
    }

    public static byte JR_NZ_e8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JR(instruction.E8, !cpu.Registers.Flags.Z);
    }

    public static byte LD_HL_d16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.HL = instruction.N16;
        return 12;
    }

    public static byte LD_HLI_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL++, cpu.Registers.A);
        return 8;
    }

    public static byte INC_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.HL++;
        return 8;
    }

    public static byte INC_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.INC(ref cpu.Registers.H);
    }

    public static byte DEC_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DEC(ref cpu.Registers.H);
    }

    public static byte LD_H_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = instruction.N8;
        return 8;
    }

    public static byte DAA(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        if (cpu.Registers.Flags.N)
        {
            if (cpu.Registers.Flags.C)
                cpu.Registers.A -= 0x60;
            if (cpu.Registers.Flags.H)
                cpu.Registers.A -= 0x6;
        }
        else
        {
            if (cpu.Registers.Flags.C || (cpu.Registers.A > 0x99))
                cpu.Registers.A += 0x60; cpu.Registers.Flags.C = true;
            if (cpu.Registers.Flags.H || (cpu.Registers.A & 0xF) > 0x9)
                cpu.Registers.A += 0x6;
        }
        cpu.Registers.Flags.SetZ(cpu.Registers.A);
        cpu.Registers.Flags.H = false;
        return 4;
    }

    public static byte JR_Z_e8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JR(instruction.E8, cpu.Registers.Flags.Z);
    }

    public static byte ADD_HL_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DAD(cpu.Registers.HL);
    }

    public static byte LD_A_HLI(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte(cpu.Registers.HL++);
        return 8;
    }

    public static byte DEC_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.HL--;
        return 8;
    }

    public static byte INC_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.INC(ref cpu.Registers.L);
    }

    public static byte DEC_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DEC(ref cpu.Registers.L);
    }

    public static byte LD_L_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = instruction.N8;
        return 8;
    }

    public static byte CPL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = (byte)~cpu.Registers.A;
        cpu.Registers.Flags.N = true;
        cpu.Registers.Flags.H = true;
        return 4;
    }

    public static byte JR_NC_e8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JR(instruction.E8, !cpu.Registers.Flags.C);
    }

    public static byte LD_SP_d16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.SP = instruction.N16;
        return 12;
    }

    public static byte LD_HLD_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL--, cpu.Registers.A);
        return 8;
    }

    public static byte INC_SP(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.SP++;
        return 8;
    }

    public static byte INC_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        var value = mmu.ReadByte(cpu.Registers.HL);
        cpu.INC(ref value);
        mmu.WriteByte(cpu.Registers.HL, value);
        return 12;
    }

    public static byte DEC_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        var value = mmu.ReadByte(cpu.Registers.HL);
        cpu.DEC(ref value);
        mmu.WriteByte(cpu.Registers.HL, value);
        return 12;
    }

    public static byte LD_ptr_HL_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, instruction.N8);
        return 12;
    }

    public static byte SCF(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.Flags.C = true;
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.H = false;
        return 4;
    }

    public static byte JR_C_e8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JR(instruction.E8, cpu.Registers.Flags.C);
    }

    public static byte ADD_HL_SP(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DAD(cpu.Registers.SP);
    }

    public static byte LD_A_HLD(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte(cpu.Registers.HL--);
        return 8;
    }

    public static byte DEC_SP(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.SP--;
        return 8;
    }

    public static byte INC_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.INC(ref cpu.Registers.A);
    }

    public static byte DEC_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.DEC(ref cpu.Registers.A);
    }

    public static byte LD_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = instruction.N8;
        return 8;
    }

    public static byte CCF(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.Flags.C = !cpu.Registers.Flags.C;
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.H = false;
        return 4;
    }

    public static byte LD_B_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        // cpu.Registers.B = cpu.Registers.B;
        return 4;
    }

    public static byte LD_B_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = cpu.Registers.C;
        return 4;
    }

    public static byte LD_B_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = cpu.Registers.D;
        return 4;
    }

    public static byte LD_B_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = cpu.Registers.E;
        return 4;
    }

    public static byte LD_B_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = cpu.Registers.H;
        return 4;
    }

    public static byte LD_B_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = cpu.Registers.L;
        return 4;
    }

    public static byte LD_B_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = mmu.ReadByte(cpu.Registers.HL);
        return 8;
    }

    public static byte LD_B_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.B = cpu.Registers.A;
        return 4;
    }

    public static byte LD_C_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = cpu.Registers.B;
        return 4;
    }

    public static byte LD_C_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        // cpu.Registers.C = cpu.Registers.C;
        return 4;
    }

    public static byte LD_C_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = cpu.Registers.D;
        return 4;
    }

    public static byte LD_C_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = cpu.Registers.E;
        return 4;
    }

    public static byte LD_C_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = cpu.Registers.H;
        return 4;
    }

    public static byte LD_C_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = cpu.Registers.L;
        return 4;
    }

    public static byte LD_C_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = mmu.ReadByte(cpu.Registers.HL);
        return 8;
    }

    public static byte LD_C_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.C = cpu.Registers.A;
        return 4;
    }

    public static byte LD_D_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = cpu.Registers.B;
        return 4;
    }

    public static byte LD_D_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = cpu.Registers.C;
        return 4;
    }

    public static byte LD_D_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        // cpu.Registers.D = cpu.Registers.D;
        return 4;
    }

    public static byte LD_D_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = cpu.Registers.E;
        return 4;
    }

    public static byte LD_D_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = cpu.Registers.H;
        return 4;
    }

    public static byte LD_D_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = cpu.Registers.L;
        return 4;
    }

    public static byte LD_D_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = mmu.ReadByte(cpu.Registers.HL);
        return 8;
    }

    public static byte LD_D_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.D = cpu.Registers.A;
        return 4;
    }

    public static byte LD_E_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = cpu.Registers.B;
        return 4;
    }

    public static byte LD_E_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = cpu.Registers.C;
        return 4;
    }

    public static byte LD_E_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = cpu.Registers.D;
        return 4;
    }

    public static byte LD_E_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        // cpu.Registers.E = cpu.Registers.E;
        return 4;
    }

    public static byte LD_E_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = cpu.Registers.H;
        return 4;
    }

    public static byte LD_E_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = cpu.Registers.L;
        return 4;
    }

    public static byte LD_E_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = mmu.ReadByte(cpu.Registers.HL);
        return 8;
    }

    public static byte LD_E_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.E = cpu.Registers.A;
        return 4;
    }

    public static byte LD_H_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = cpu.Registers.B;
        return 4;
    }

    public static byte LD_H_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = cpu.Registers.C;
        return 4;
    }

    public static byte LD_H_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = cpu.Registers.D;
        return 4;
    }

    public static byte LD_H_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = cpu.Registers.E;
        return 4;
    }

    public static byte LD_H_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        // cpu.Registers.H = cpu.Registers.H;
        return 4;
    }

    public static byte LD_H_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = cpu.Registers.L;
        return 4;
    }

    public static byte LD_H_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = mmu.ReadByte(cpu.Registers.HL);
        return 8;
    }

    public static byte LD_H_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.H = cpu.Registers.A;
        return 4;
    }

    public static byte LD_L_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = cpu.Registers.B;
        return 4;
    }

    public static byte LD_L_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = cpu.Registers.C;
        return 4;
    }

    public static byte LD_L_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = cpu.Registers.D;
        return 4;
    }

    public static byte LD_L_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = cpu.Registers.E;
        return 4;
    }

    public static byte LD_L_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = cpu.Registers.H;
        return 4;
    }

    public static byte LD_L_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        // cpu.Registers.L = cpu.Registers.L;
        return 4;
    }

    public static byte LD_L_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = mmu.ReadByte(cpu.Registers.HL);
        return 8;
    }

    public static byte LD_L_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.L = cpu.Registers.A;
        return 4;
    }

    public static byte LD_ptr_HL_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, cpu.Registers.B);
        return 8;
    }

    public static byte LD_ptr_HL_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, cpu.Registers.C);
        return 8;
    }

    public static byte LD_ptr_HL_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, cpu.Registers.D);
        return 8;
    }

    public static byte LD_ptr_HL_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, cpu.Registers.E);
        return 8;
    }

    public static byte LD_ptr_HL_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, cpu.Registers.H);
        return 8;
    }

    public static byte LD_ptr_HL_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, cpu.Registers.L);
        return 8;
    }

    public static byte HALT(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public static byte LD_ptr_HL_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte(cpu.Registers.HL, cpu.Registers.A);
        return 8;
    }

    public static byte LD_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = cpu.Registers.B;
        return 4;
    }

    public static byte LD_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = cpu.Registers.C;
        return 4;
    }

    public static byte LD_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = cpu.Registers.D;
        return 4;
    }

    public static byte LD_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = cpu.Registers.E;
        return 4;
    }

    public static byte LD_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = cpu.Registers.H;
        return 4;
    }

    public static byte LD_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = cpu.Registers.L;
        return 4;
    }

    public static byte LD_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte(cpu.Registers.HL);
        return 8;
    }

    public static byte LD_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        // cpu.Registers.A = cpu.Registers.A;
        return 4;
    }

    public static byte ADD_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADD(cpu.Registers.B);
    }

    public static byte ADD_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADD(cpu.Registers.C);
    }

    public static byte ADD_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADD(cpu.Registers.D);
    }

    public static byte ADD_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADD(cpu.Registers.E);
    }

    public static byte ADD_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADD(cpu.Registers.H);
    }

    public static byte ADD_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADD(cpu.Registers.L);
    }

    public static byte ADD_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.ADD(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte ADD_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADD(cpu.Registers.A);
    }

    public static byte ADC_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADC(cpu.Registers.B);
    }

    public static byte ADC_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADC(cpu.Registers.C);
    }

    public static byte ADC_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADC(cpu.Registers.D);
    }

    public static byte ADC_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADC(cpu.Registers.E);
    }

    public static byte ADC_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADC(cpu.Registers.H);
    }

    public static byte ADC_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADC(cpu.Registers.L);
    }

    public static byte ADC_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.ADC(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte ADC_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.ADC(cpu.Registers.A);
    }

    public static byte SUB_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SUB(cpu.Registers.B);
    }

    public static byte SUB_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SUB(cpu.Registers.C);
    }

    public static byte SUB_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SUB(cpu.Registers.D);
    }

    public static byte SUB_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SUB(cpu.Registers.E);
    }

    public static byte SUB_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SUB(cpu.Registers.H);
    }

    public static byte SUB_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SUB(cpu.Registers.L);
    }

    public static byte SUB_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.SUB(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte SUB_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SUB(cpu.Registers.A);
    }

    public static byte SBC_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SBC(cpu.Registers.B);
    }

    public static byte SBC_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SBC(cpu.Registers.C);
    }

    public static byte SBC_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SBC(cpu.Registers.D);
    }

    public static byte SBC_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SBC(cpu.Registers.E);
    }

    public static byte SBC_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SBC(cpu.Registers.H);
    }

    public static byte SBC_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SBC(cpu.Registers.L);
    }

    public static byte SBC_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.SBC(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte SBC_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.SBC(cpu.Registers.A);
    }

    public static byte AND_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.AND(cpu.Registers.B);
    }

    public static byte AND_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.AND(cpu.Registers.C);
    }

    public static byte AND_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.AND(cpu.Registers.D);
    }

    public static byte AND_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.AND(cpu.Registers.E);
    }

    public static byte AND_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.AND(cpu.Registers.H);
    }

    public static byte AND_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.AND(cpu.Registers.L);
    }

    public static byte AND_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.AND(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte AND_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.AND(cpu.Registers.A);
    }

    public static byte XOR_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.XOR(cpu.Registers.B);
    }

    public static byte XOR_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.XOR(cpu.Registers.C);
    }

    public static byte XOR_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.XOR(cpu.Registers.D);
    }

    public static byte XOR_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.XOR(cpu.Registers.E);
    }

    public static byte XOR_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.XOR(cpu.Registers.H);
    }

    public static byte XOR_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.XOR(cpu.Registers.L);
    }

    public static byte XOR_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.XOR(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte XOR_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.XOR(cpu.Registers.A);
    }

    public static byte OR_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.OR(cpu.Registers.B);
    }

    public static byte OR_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.OR(cpu.Registers.C);
    }

    public static byte OR_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.OR(cpu.Registers.D);
    }

    public static byte OR_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.OR(cpu.Registers.E);
    }

    public static byte OR_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.OR(cpu.Registers.H);
    }

    public static byte OR_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.OR(cpu.Registers.L);
    }

    public static byte OR_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.OR(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte OR_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.OR(cpu.Registers.A);
    }

    public static byte CP_A_B(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CP(cpu.Registers.B);
    }

    public static byte CP_A_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CP(cpu.Registers.C);
    }

    public static byte CP_A_D(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CP(cpu.Registers.D);
    }

    public static byte CP_A_E(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CP(cpu.Registers.E);
    }

    public static byte CP_A_H(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CP(cpu.Registers.H);
    }

    public static byte CP_A_L(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CP(cpu.Registers.L);
    }

    public static byte CP_A_ptr_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.CP(mmu.ReadByte(cpu.Registers.HL)) + 4);
    }

    public static byte CP_A_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CP(cpu.Registers.A);
    }

    public static byte RET_NZ(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RET(mmu, !cpu.Registers.Flags.Z);
    }

    public static byte POP_BC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.BC = cpu.POP(mmu);
        return 12;
    }

    public static byte JP_NZ_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JP(instruction.N16, !cpu.Registers.Flags.Z);
    }

    public static byte JP_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JP(instruction.N16, flag: true);
    }

    public static byte CALL_NZ_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CALL(mmu, instruction.N16, !cpu.Registers.Flags.Z);
    }

    public static byte PUSH_BC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.PUSH(mmu, cpu.Registers.BC);
        return 16;
    }

    public static byte ADD_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.ADD(instruction.N8) + 4);
    }

    public static byte RST_00(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x00);
    }

    public static byte RET_Z(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RET(mmu, cpu.Registers.Flags.Z);
    }

    public static byte RET(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RET(mmu, flag: true);
    }

    public static byte JP_Z_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JP(instruction.N16, cpu.Registers.Flags.Z);
    }

    public static byte PREFIX(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public static byte CALL_Z_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CALL(mmu, instruction.N16, cpu.Registers.Flags.Z);
    }

    public static byte CALL_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CALL(mmu, instruction.N16, flag: true);
    }

    public static byte ADC_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.ADC(instruction.N8) + 4);
    }

    public static byte RST_08(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x08);
    }

    public static byte RET_NC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RET(mmu, !cpu.Registers.Flags.C);
    }

    public static byte POP_DE(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.DE = cpu.POP(mmu);
        return 12;
    }

    public static byte JP_NC_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JP(instruction.N16, !cpu.Registers.Flags.C);
    }

    public static byte ILLEGAL_D3(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte CALL_NC_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CALL(mmu, instruction.N16, !cpu.Registers.Flags.C);
    }

    public static byte PUSH_DE(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.PUSH(mmu, cpu.Registers.DE);
        return 16;
    }

    public static byte SUB_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.SUB(instruction.N8) + 4);
    }

    public static byte RST_10(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x10);
    }

    public static byte RET_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RET(mmu, cpu.Registers.Flags.C);
    }

    public static byte RETI(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new NotImplementedException(instruction.ToString());
        // cpu.RET(mmu, flag: true);
        // cpu.Registers.IME = true;
        // return 16;
    }

    public static byte JP_C_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.JP(instruction.N16, cpu.Registers.Flags.C);
    }

    public static byte ILLEGAL_DB(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte CALL_C_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.CALL(mmu, instruction.N16, cpu.Registers.Flags.C);
    }

    public static byte ILLEGAL_DD(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte SBC_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.SBC(instruction.N8) + 4);
    }

    public static byte RST_18(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x18);
    }

    public static byte LDH_ptr_a8_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte((ushort)(0xFF00 + instruction.N8), cpu.Registers.A);
        return 12;
    }

    public static byte POP_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.HL = cpu.POP(mmu);
        return 12;
    }

    public static byte LDH_ptr_C_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteByte((ushort)(0xFF00 + cpu.Registers.C), cpu.Registers.A);
        return 8;
    }

    public static byte ILLEGAL_E3(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_E4(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte PUSH_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.PUSH(mmu, cpu.Registers.HL);
        return 16;
    }

    public static byte AND_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.AND(instruction.N8) + 4);
    }

    public static byte RST_20(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x20);
    }

    public static byte ADD_SP_e8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.Flags.Z = false;
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.SetH((byte)cpu.Registers.SP, instruction.N8);
        cpu.Registers.Flags.SetC((byte)cpu.Registers.SP + instruction.N8);
        cpu.Registers.SP = (ushort)(cpu.Registers.SP + instruction.E8);

        return 16;
    }

    public static byte JP_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.PC = cpu.Registers.HL;
        return 4;
    }

    public static byte LD_ptr_a16_A(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        mmu.WriteWord(instruction.N16, cpu.Registers.A);
        return 16;
    }

    public static byte ILLEGAL_EB(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_EC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_ED(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte XOR_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.XOR(instruction.N8) + 4);
    }

    public static byte RST_28(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x28);
    }

    public static byte LDH_A_ptr_a8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte((ushort)(0xFF00 + instruction.N8));
        return 12;
    }

    public static byte POP_AF(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.AF = cpu.POP(mmu);
        return 12;
    }

    public static byte LDH_A_ptr_C(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte((ushort)(0xFF00 + cpu.Registers.C));
        return 8;
    }

    public static byte DI(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new NotImplementedException(instruction.ToString());
        // IME = false;
        // return 4;
    }

    public static byte ILLEGAL_F4(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte PUSH_AF(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.PUSH(mmu, cpu.Registers.AF);
        return 16;
    }

    public static byte OR_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.OR(instruction.N8) + 4);
    }

    public static byte RST_30(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x30);
    }

    public static byte LD_HL_SP_e8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.Flags.Z = false;
        cpu.Registers.Flags.N = false;
        cpu.Registers.Flags.SetH((byte)cpu.Registers.SP, instruction.N8);
        cpu.Registers.Flags.SetC((byte)cpu.Registers.SP + instruction.N8);
        cpu.Registers.HL = (ushort)(cpu.Registers.SP + instruction.E8);

        return 12;
    }

    public static byte LD_SP_HL(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.SP = cpu.Registers.HL;
        return 8;
    }

    public static byte LD_A_ptr_a16(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        cpu.Registers.A = mmu.ReadByte(instruction.N16);
        return 16;
    }

    public static byte EI(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new NotImplementedException(instruction.ToString());
        // IMEEnabler = true;
        // return 4;
    }

    public static byte ILLEGAL_FC(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_FD(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte CP_A_d8(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return (byte)(cpu.CP(instruction.N8) + 4);
    }

    public static byte RST_38(this Cpu cpu, Mmu mmu, Instruction instruction)
    {
        return cpu.RST(mmu, 0x38);
    }
}
