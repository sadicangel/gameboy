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

    private static byte INC(this ref CpuRegisters registers, ref byte register)
    {
        register++;
        registers.Flags.SetZ(register);
        registers.Flags.N = false;
        registers.Flags.SetH(register, 1);

        return 4;
    }

    private static byte DEC(this ref CpuRegisters registers, ref byte register)
    {
        register--;
        registers.Flags.SetZ(register);
        registers.Flags.N = true;
        registers.Flags.SetHSub(register, 1);

        return 4;
    }

    private static byte DAD(this ref CpuRegisters registers, ushort value)
    {
        var result = registers.HL + value;
        registers.Flags.N = false;
        registers.Flags.SetH(registers.HL, value);
        registers.Flags.C = result >> 16 != 0;
        registers.HL = (ushort)result;

        return 8;
    }

    private static byte ADD(this ref CpuRegisters registers, byte value)
    {
        var result = registers.A + value;
        registers.Flags.SetZ(result);
        registers.Flags.N = false;
        registers.Flags.SetH(registers.A, value);
        registers.Flags.SetC(result);
        registers.A = (byte)result;

        return 4;
    }

    private static byte ADC(this ref CpuRegisters registers, byte value)
    {
        var carry = registers.Flags.C ? 1 : 0;
        var result = registers.A + value + carry;
        registers.Flags.SetZ(result);
        registers.Flags.N = false;
        if (registers.Flags.C)
            registers.Flags.SetHCarry(registers.A, value);
        else
            registers.Flags.SetH(registers.A, value);
        registers.Flags.SetC(result);
        registers.A = (byte)result;

        return 4;
    }

    private static byte SUB(this ref CpuRegisters registers, byte value)
    {
        var result = registers.A - value;
        registers.Flags.SetZ(result);
        registers.Flags.N = true;
        registers.Flags.SetHSub(registers.A, value);
        registers.Flags.SetC(result);
        registers.A = (byte)result;

        return 4;
    }

    private static byte SBC(this ref CpuRegisters registers, byte value)
    {
        var carry = registers.Flags.C ? 1 : 0;
        var result = registers.A - value - carry;
        registers.Flags.SetZ(result);
        registers.Flags.N = true;
        if (registers.Flags.C)
            registers.Flags.SetHSubCarry(registers.A, value);
        else
            registers.Flags.SetHSub(registers.A, value);
        registers.Flags.SetC(result);
        registers.A = (byte)result;

        return 4;
    }

    private static byte AND(this ref CpuRegisters registers, byte value)
    {
        var result = (byte)(registers.A & value);
        registers.Flags.SetZ(result);
        registers.Flags.N = false;
        registers.Flags.H = true;
        registers.Flags.C = false;
        registers.A = result;

        return 4;
    }

    private static byte XOR(this ref CpuRegisters registers, byte value)
    {
        var result = (byte)(registers.A ^ value);
        registers.Flags.SetZ(result);
        registers.Flags.N = false;
        registers.Flags.H = false;
        registers.Flags.C = false;
        registers.A = result;

        return 4;
    }

    private static byte OR(this ref CpuRegisters registers, byte value)
    {
        var result = (byte)(registers.A | value);
        registers.Flags.SetZ(result);
        registers.Flags.N = false;
        registers.Flags.H = false;
        registers.Flags.C = false;
        registers.A = result;

        return 4;
    }

    private static byte CP(this ref CpuRegisters registers, byte value)
    {
        var result = registers.A - value;
        registers.Flags.SetZ(result);
        registers.Flags.N = true;
        registers.Flags.SetHSub(registers.A, value);
        registers.Flags.SetC(result);

        return 4;
    }

    private static byte JR(this ref CpuRegisters registers, sbyte address, bool flag)
    {
        if (!flag)
        {
            return 8;
        }

        registers.PC = (ushort)(registers.PC + address);

        return 12;
    }

    private static byte JP(this ref CpuRegisters registers, ushort address, bool flag)
    {
        if (!flag)
        {
            return 12;
        }

        registers.PC = address;

        return 16;
    }

    private static byte RET(this ref CpuRegisters registers, Bus bus, bool flag)
    {
        if (!flag)
        {
            return 8;
        }

        registers.PC = registers.POP(bus);

        return 20;
    }

    private static byte CALL(this ref CpuRegisters registers, Bus bus, ushort address, bool flag)
    {
        if (!flag)
        {
            return 12;
        }

        registers.PUSH(bus, registers.PC);
        registers.PC = address;

        return 24;
    }

    private static byte RST(this ref CpuRegisters registers, Bus bus, byte value)
    {
        registers.PUSH(bus, registers.PC);
        registers.PC = value;

        return 16;
    }

    private static void PUSH(this CpuRegisters registers, Bus bus, ushort word)
    {
        registers.SP -= 2;
        bus.WriteWord(registers.SP, word);
    }

    private static ushort POP(this CpuRegisters registers, Bus bus)
    {
        var result = bus.ReadWord(registers.SP);
        registers.SP += 2;
        return result;
    }

    public static byte NOP(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return 4;
    }

    public static byte LD_BC_d16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.BC = instruction.N16;
        return 12;
    }

    public static byte LD_ptr_BC_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.BC, registers.A);
        return 8;
    }

    public static byte INC_BC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.BC++;
        return 8;
    }

    public static byte INC_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.INC(ref registers.B);
    }

    public static byte DEC_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DEC(ref registers.B);
    }

    public static byte LD_B_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = instruction.N8;
        return 8;
    }

    public static byte RLCA(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.F = 0;
        registers.Flags.C = (registers.A & 0x80) != 0;
        registers.A = (byte)(registers.A << 1 | registers.A >> 7);
        return 4;
    }

    public static byte LD_ptr_a16_SP(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteWord(registers.SP, instruction.N16);
        return 20;
    }

    public static byte ADD_HL_BC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DAD(registers.BC);
    }

    public static byte LD_A_ptr_BC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte(registers.BC);
        return 8;
    }

    public static byte DEC_BC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.BC--;
        return 8;
    }

    public static byte INC_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.INC(ref registers.C);
    }

    public static byte DEC_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DEC(ref registers.C);
    }

    public static byte LD_C_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = instruction.N8;
        return 8;
    }

    public static byte RRCA(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.F = 0;
        registers.Flags.C = ((registers.A & 0x1) != 0);
        registers.A = (byte)(registers.A >> 1 | registers.A << 7);
        return 4;
    }

    public static byte STOP_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public static byte LD_DE_d16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.DE = instruction.N16;
        return 12;
    }

    public static byte LD_ptr_DE_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.DE, registers.A);
        return 8;
    }

    public static byte INC_DE(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.DE++;
        return 8;
    }

    public static byte INC_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.INC(ref registers.D);
    }

    public static byte DEC_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DEC(ref registers.D);
    }

    public static byte LD_D_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = instruction.N8;
        return 8;
    }

    public static byte RLA(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        var prevFlagC = registers.Flags.C;
        registers.F = 0;
        registers.Flags.C = ((registers.A & 0x80) != 0);
        registers.A = (byte)((registers.A << 1) | (prevFlagC ? 1 : 0));
        return 4;
    }

    public static byte JR_e8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JR(instruction.E8, flag: true);
    }

    public static byte ADD_HL_DE(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DAD(registers.DE);
    }

    public static byte LD_A_ptr_DE(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte(registers.DE);
        return 8;
    }

    public static byte DEC_DE(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.DE--;
        return 8;
    }

    public static byte INC_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.INC(ref registers.E);
    }

    public static byte DEC_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DEC(ref registers.E);
    }

    public static byte LD_E_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = instruction.N8;
        return 8;
    }

    public static byte RRA(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        var prevFlagC = registers.Flags.C;
        registers.F = 0;
        registers.Flags.C = ((registers.A & 0x1) != 0);
        registers.A = (byte)((registers.A >> 1) | (prevFlagC ? 0x80 : 0));
        return 4;
    }

    public static byte JR_NZ_e8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JR(instruction.E8, !registers.Flags.Z);
    }

    public static byte LD_HL_d16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.HL = instruction.N16;
        return 12;
    }

    public static byte LD_HLI_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL++, registers.A);
        return 8;
    }

    public static byte INC_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.HL++;
        return 8;
    }

    public static byte INC_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.INC(ref registers.H);
    }

    public static byte DEC_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DEC(ref registers.H);
    }

    public static byte LD_H_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = instruction.N8;
        return 8;
    }

    public static byte DAA(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        if (registers.Flags.N)
        {
            // sub
            if (registers.Flags.C) { registers.A -= 0x60; }
            if (registers.Flags.H) { registers.A -= 0x6; }
        }
        else
        {
            // add
            if (registers.Flags.C || (registers.A > 0x99)) { registers.A += 0x60; registers.Flags.C = true; }
            if (registers.Flags.H || (registers.A & 0xF) > 0x9) { registers.A += 0x6; }
        }
        registers.Flags.SetZ(registers.A);
        registers.Flags.H = false;
        return 4;
    }

    public static byte JR_Z_e8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JR(instruction.E8, registers.Flags.Z);
    }

    public static byte ADD_HL_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DAD(registers.HL);
    }

    public static byte LD_A_HLI(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte(registers.HL++);
        return 8;
    }

    public static byte DEC_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.HL--;
        return 8;
    }

    public static byte INC_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.INC(ref registers.L);
    }

    public static byte DEC_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DEC(ref registers.L);
    }

    public static byte LD_L_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = instruction.N8;
        return 8;
    }

    public static byte CPL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = (byte)~registers.A;
        registers.Flags.N = true;
        registers.Flags.H = true;
        return 4;
    }

    public static byte JR_NC_e8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JR(instruction.E8, !registers.Flags.C);
    }

    public static byte LD_SP_d16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.SP = instruction.N16;
        return 12;
    }

    public static byte LD_HLD_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL--, registers.A);
        return 8;
    }

    public static byte INC_SP(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.SP++;
        return 8;
    }

    public static byte INC_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        var value = bus.ReadByte(registers.HL);
        registers.INC(ref value);
        bus.WriteByte(registers.HL, value);
        return 12;
    }

    public static byte DEC_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        var value = bus.ReadByte(registers.HL);
        registers.DEC(ref value);
        bus.WriteByte(registers.HL, value);
        return 12;
    }

    public static byte LD_ptr_HL_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, instruction.N8);
        return 12;
    }

    public static byte SCF(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.Flags.C = true;
        registers.Flags.N = false;
        registers.Flags.H = false;
        return 4;
    }

    public static byte JR_C_e8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JR(instruction.E8, registers.Flags.C);
    }

    public static byte ADD_HL_SP(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DAD(registers.SP);
    }

    public static byte LD_A_HLD(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte(registers.HL--);
        return 8;
    }

    public static byte DEC_SP(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.SP--;
        return 8;
    }

    public static byte INC_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.INC(ref registers.A);
    }

    public static byte DEC_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.DEC(ref registers.A);
    }

    public static byte LD_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = instruction.N8;
        return 8;
    }

    public static byte CCF(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.Flags.C = !registers.Flags.C;
        registers.Flags.N = false;
        registers.Flags.H = false;
        return 4;
    }

    public static byte LD_B_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        // registers.B = registers.B;
        return 4;
    }

    public static byte LD_B_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = registers.C;
        return 4;
    }

    public static byte LD_B_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = registers.D;
        return 4;
    }

    public static byte LD_B_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = registers.E;
        return 4;
    }

    public static byte LD_B_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = registers.H;
        return 4;
    }

    public static byte LD_B_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = registers.L;
        return 4;
    }

    public static byte LD_B_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = bus.ReadByte(registers.HL);
        return 8;
    }

    public static byte LD_B_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.B = registers.A;
        return 4;
    }

    public static byte LD_C_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = registers.B;
        return 4;
    }

    public static byte LD_C_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        // registers.C = registers.C;
        return 4;
    }

    public static byte LD_C_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = registers.D;
        return 4;
    }

    public static byte LD_C_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = registers.E;
        return 4;
    }

    public static byte LD_C_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = registers.H;
        return 4;
    }

    public static byte LD_C_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = registers.L;
        return 4;
    }

    public static byte LD_C_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = bus.ReadByte(registers.HL);
        return 8;
    }

    public static byte LD_C_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.C = registers.A;
        return 4;
    }

    public static byte LD_D_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = registers.B;
        return 4;
    }

    public static byte LD_D_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = registers.C;
        return 4;
    }

    public static byte LD_D_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        // registers.D = registers.D;
        return 4;
    }

    public static byte LD_D_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = registers.E;
        return 4;
    }

    public static byte LD_D_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = registers.H;
        return 4;
    }

    public static byte LD_D_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = registers.L;
        return 4;
    }

    public static byte LD_D_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = bus.ReadByte(registers.HL);
        return 8;
    }

    public static byte LD_D_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.D = registers.A;
        return 4;
    }

    public static byte LD_E_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = registers.B;
        return 4;
    }

    public static byte LD_E_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = registers.C;
        return 4;
    }

    public static byte LD_E_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = registers.D;
        return 4;
    }

    public static byte LD_E_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        // registers.E = registers.E;
        return 4;
    }

    public static byte LD_E_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = registers.H;
        return 4;
    }

    public static byte LD_E_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = registers.L;
        return 4;
    }

    public static byte LD_E_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = bus.ReadByte(registers.HL);
        return 8;
    }

    public static byte LD_E_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.E = registers.A;
        return 4;
    }

    public static byte LD_H_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = registers.B;
        return 4;
    }

    public static byte LD_H_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = registers.C;
        return 4;
    }

    public static byte LD_H_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = registers.D;
        return 4;
    }

    public static byte LD_H_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = registers.E;
        return 4;
    }

    public static byte LD_H_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        // registers.H = registers.H;
        return 4;
    }

    public static byte LD_H_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = registers.L;
        return 4;
    }

    public static byte LD_H_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = bus.ReadByte(registers.HL);
        return 8;
    }

    public static byte LD_H_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.H = registers.A;
        return 4;
    }

    public static byte LD_L_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = registers.B;
        return 4;
    }

    public static byte LD_L_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = registers.C;
        return 4;
    }

    public static byte LD_L_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = registers.D;
        return 4;
    }

    public static byte LD_L_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = registers.E;
        return 4;
    }

    public static byte LD_L_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = registers.H;
        return 4;
    }

    public static byte LD_L_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        // registers.L = registers.L;
        return 4;
    }

    public static byte LD_L_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = bus.ReadByte(registers.HL);
        return 8;
    }

    public static byte LD_L_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.L = registers.A;
        return 4;
    }

    public static byte LD_ptr_HL_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, registers.B);
        return 8;
    }

    public static byte LD_ptr_HL_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, registers.C);
        return 8;
    }

    public static byte LD_ptr_HL_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, registers.D);
        return 8;
    }

    public static byte LD_ptr_HL_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, registers.E);
        return 8;
    }

    public static byte LD_ptr_HL_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, registers.H);
        return 8;
    }

    public static byte LD_ptr_HL_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, registers.L);
        return 8;
    }

    public static byte HALT(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public static byte LD_ptr_HL_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte(registers.HL, registers.A);
        return 8;
    }

    public static byte LD_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = registers.B;
        return 4;
    }

    public static byte LD_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = registers.C;
        return 4;
    }

    public static byte LD_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = registers.D;
        return 4;
    }

    public static byte LD_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = registers.E;
        return 4;
    }

    public static byte LD_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = registers.H;
        return 4;
    }

    public static byte LD_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = registers.L;
        return 4;
    }

    public static byte LD_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte(registers.HL);
        return 8;
    }

    public static byte LD_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        // registers.A = registers.A;
        return 4;
    }

    public static byte ADD_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADD(registers.B);
    }

    public static byte ADD_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADD(registers.C);
    }

    public static byte ADD_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADD(registers.D);
    }

    public static byte ADD_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADD(registers.E);
    }

    public static byte ADD_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADD(registers.H);
    }

    public static byte ADD_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADD(registers.L);
    }

    public static byte ADD_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.ADD(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte ADD_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADD(registers.A);
    }

    public static byte ADC_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADC(registers.B);
    }

    public static byte ADC_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADC(registers.C);
    }

    public static byte ADC_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADC(registers.D);
    }

    public static byte ADC_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADC(registers.E);
    }

    public static byte ADC_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADC(registers.H);
    }

    public static byte ADC_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADC(registers.L);
    }

    public static byte ADC_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.ADC(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte ADC_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.ADC(registers.A);
    }

    public static byte SUB_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SUB(registers.B);
    }

    public static byte SUB_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SUB(registers.C);
    }

    public static byte SUB_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SUB(registers.D);
    }

    public static byte SUB_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SUB(registers.E);
    }

    public static byte SUB_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SUB(registers.H);
    }

    public static byte SUB_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SUB(registers.L);
    }

    public static byte SUB_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.SUB(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte SUB_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SUB(registers.A);
    }

    public static byte SBC_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SBC(registers.B);
    }

    public static byte SBC_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SBC(registers.C);
    }

    public static byte SBC_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SBC(registers.D);
    }

    public static byte SBC_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SBC(registers.E);
    }

    public static byte SBC_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SBC(registers.H);
    }

    public static byte SBC_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SBC(registers.L);
    }

    public static byte SBC_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.SBC(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte SBC_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.SBC(registers.A);
    }

    public static byte AND_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.AND(registers.B);
    }

    public static byte AND_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.AND(registers.C);
    }

    public static byte AND_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.AND(registers.D);
    }

    public static byte AND_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.AND(registers.E);
    }

    public static byte AND_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.AND(registers.H);
    }

    public static byte AND_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.AND(registers.L);
    }

    public static byte AND_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.AND(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte AND_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.AND(registers.A);
    }

    public static byte XOR_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.XOR(registers.B);
    }

    public static byte XOR_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.XOR(registers.C);
    }

    public static byte XOR_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.XOR(registers.D);
    }

    public static byte XOR_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.XOR(registers.E);
    }

    public static byte XOR_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.XOR(registers.H);
    }

    public static byte XOR_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.XOR(registers.L);
    }

    public static byte XOR_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.XOR(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte XOR_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.XOR(registers.A);
    }

    public static byte OR_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.OR(registers.B);
    }

    public static byte OR_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.OR(registers.C);
    }

    public static byte OR_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.OR(registers.D);
    }

    public static byte OR_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.OR(registers.E);
    }

    public static byte OR_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.OR(registers.H);
    }

    public static byte OR_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.OR(registers.L);
    }

    public static byte OR_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.OR(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte OR_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.OR(registers.A);
    }

    public static byte CP_A_B(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CP(registers.B);
    }

    public static byte CP_A_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CP(registers.C);
    }

    public static byte CP_A_D(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CP(registers.D);
    }

    public static byte CP_A_E(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CP(registers.E);
    }

    public static byte CP_A_H(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CP(registers.H);
    }

    public static byte CP_A_L(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CP(registers.L);
    }

    public static byte CP_A_ptr_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.CP(bus.ReadByte(registers.HL)) + 4);
    }

    public static byte CP_A_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CP(registers.A);
    }

    public static byte RET_NZ(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RET(bus, !registers.Flags.Z);
    }

    public static byte POP_BC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.BC = registers.POP(bus);
        return 12;
    }

    public static byte JP_NZ_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JP(instruction.N16, !registers.Flags.Z);
    }

    public static byte JP_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JP(instruction.N16, flag: true);
    }

    public static byte CALL_NZ_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CALL(bus, instruction.N16, !registers.Flags.Z);
    }

    public static byte PUSH_BC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.PUSH(bus, registers.BC);
        return 16;
    }

    public static byte ADD_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.ADD(instruction.N8) + 4);
    }

    public static byte RST_00(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x00);
    }

    public static byte RET_Z(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RET(bus, registers.Flags.Z);
    }

    public static byte RET(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RET(bus, flag: true);
    }

    public static byte JP_Z_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JP(instruction.N16, registers.Flags.Z);
    }

    public static byte PREFIX(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public static byte CALL_Z_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CALL(bus, instruction.N16, registers.Flags.Z);
    }

    public static byte CALL_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CALL(bus, instruction.N16, flag: true);
    }

    public static byte ADC_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.ADC(instruction.N8) + 4);
    }

    public static byte RST_08(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x08);
    }

    public static byte RET_NC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RET(bus, !registers.Flags.C);
    }

    public static byte POP_DE(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.DE = registers.POP(bus);
        return 12;
    }

    public static byte JP_NC_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JP(instruction.N16, !registers.Flags.C);
    }

    public static byte ILLEGAL_D3(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte CALL_NC_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CALL(bus, instruction.N16, !registers.Flags.C);
    }

    public static byte PUSH_DE(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.PUSH(bus, registers.DE);
        return 16;
    }

    public static byte SUB_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.SUB(instruction.N8) + 4);
    }

    public static byte RST_10(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x10);
    }

    public static byte RET_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RET(bus, registers.Flags.C);
    }

    public static byte RETI(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new NotImplementedException(instruction.ToString());
        // registers.RET(bus, flag: true);
        // registers.IME = true;
        // return 16;
    }

    public static byte JP_C_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.JP(instruction.N16, registers.Flags.C);
    }

    public static byte ILLEGAL_DB(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte CALL_C_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.CALL(bus, instruction.N16, registers.Flags.C);
    }

    public static byte ILLEGAL_DD(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte SBC_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.SBC(instruction.N8) + 4);
    }

    public static byte RST_18(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x18);
    }

    public static byte LDH_ptr_a8_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte((ushort)(0xFF00 + instruction.N8), registers.A);
        return 12;
    }

    public static byte POP_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.HL = registers.POP(bus);
        return 12;
    }

    public static byte LDH_ptr_C_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteByte((ushort)(0xFF00 + registers.C), registers.A);
        return 8;
    }

    public static byte ILLEGAL_E3(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_E4(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte PUSH_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.PUSH(bus, registers.HL);
        return 16;
    }

    public static byte AND_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.AND(instruction.N8) + 4);
    }

    public static byte RST_20(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x20);
    }

    public static byte ADD_SP_e8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.Flags.Z = false;
        registers.Flags.N = false;
        registers.Flags.SetH((byte)registers.SP, instruction.N8);
        registers.Flags.SetC((byte)registers.SP + instruction.N8);
        registers.SP = (ushort)(registers.SP + instruction.E8);

        return 16;
    }

    public static byte JP_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.PC = registers.HL;
        return 4;
    }

    public static byte LD_ptr_a16_A(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        bus.WriteWord(instruction.N16, registers.A);
        return 16;
    }

    public static byte ILLEGAL_EB(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_EC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_ED(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte XOR_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.XOR(instruction.N8) + 4);
    }

    public static byte RST_28(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x28);
    }

    public static byte LDH_A_ptr_a8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte((ushort)(0xFF00 + instruction.N8));
        return 12;
    }

    public static byte POP_AF(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.AF = registers.POP(bus);
        return 12;
    }

    public static byte LDH_A_ptr_C(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte((ushort)(0xFF00 + registers.C));
        return 8;
    }

    public static byte DI(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new NotImplementedException(instruction.ToString());
        // IME = false;
        // return 4;
    }

    public static byte ILLEGAL_F4(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte PUSH_AF(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.PUSH(bus, registers.AF);
        return 16;
    }

    public static byte OR_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.OR(instruction.N8) + 4);
    }

    public static byte RST_30(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x30);
    }

    public static byte LD_HL_SP_e8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.Flags.Z = false;
        registers.Flags.N = false;
        registers.Flags.SetH((byte)registers.SP, instruction.N8);
        registers.Flags.SetC((byte)registers.SP + instruction.N8);
        registers.HL = (ushort)(registers.SP + instruction.E8);

        return 12;
    }

    public static byte LD_SP_HL(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.SP = registers.HL;
        return 8;
    }

    public static byte LD_A_ptr_a16(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        registers.A = bus.ReadByte(instruction.N16);
        return 16;
    }

    public static byte EI(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new NotImplementedException(instruction.ToString());
        // IMEEnabler = true;
        // return 4;
    }

    public static byte ILLEGAL_FC(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte ILLEGAL_FD(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public static byte CP_A_d8(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return (byte)(registers.CP(instruction.N8) + 4);
    }

    public static byte RST_38(Instruction instruction, Bus bus, ref CpuRegisters registers)
    {
        return registers.RST(bus, 0x38);
    }
}
