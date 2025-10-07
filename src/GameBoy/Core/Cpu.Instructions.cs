using System.Diagnostics.CodeAnalysis;

namespace GameBoy.Core;

[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "function pointer interface")]
partial class Cpu
{
    private byte INC(ref byte register)
    {
        register++;
        Registers.Flags.SetZ(register);
        Registers.Flags.N = false;
        Registers.Flags.SetH(register, 1);

        return 4;
    }

    private byte DEC(ref byte register)
    {
        register--;
        Registers.Flags.SetZ(register);
        Registers.Flags.N = true;
        Registers.Flags.SetHSub(register, 1);

        return 4;
    }

    private byte DAD(ushort value)
    {
        var result = Registers.HL + value;
        Registers.Flags.N = false;
        Registers.Flags.SetH(Registers.HL, value);
        Registers.Flags.C = result >> 16 != 0;
        Registers.HL = (ushort)result;

        return 8;
    }

    private byte ADD(byte value)
    {
        var result = Registers.A + value;
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.SetH(Registers.A, value);
        Registers.Flags.SetC(result);
        Registers.A = (byte)result;

        return 4;
    }

    private byte ADC(byte value)
    {
        var carry = Registers.Flags.C ? 1 : 0;
        var result = Registers.A + value + carry;
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        if (Registers.Flags.C)
            Registers.Flags.SetHCarry(Registers.A, value);
        else
            Registers.Flags.SetH(Registers.A, value);
        Registers.Flags.SetC(result);
        Registers.A = (byte)result;

        return 4;
    }

    private byte SUB(byte value)
    {
        var result = Registers.A - value;
        Registers.Flags.SetZ(result);
        Registers.Flags.N = true;
        Registers.Flags.SetHSub(Registers.A, value);
        Registers.Flags.SetC(result);
        Registers.A = (byte)result;

        return 4;
    }

    private byte SBC(byte value)
    {
        var carry = Registers.Flags.C ? 1 : 0;
        var result = Registers.A - value - carry;
        Registers.Flags.SetZ(result);
        Registers.Flags.N = true;
        if (Registers.Flags.C)
            Registers.Flags.SetHSubCarry(Registers.A, value);
        else
            Registers.Flags.SetHSub(Registers.A, value);
        Registers.Flags.SetC(result);
        Registers.A = (byte)result;

        return 4;
    }

    private byte AND(byte value)
    {
        var result = (byte)(Registers.A & value);
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = true;
        Registers.Flags.C = false;
        Registers.A = result;

        return 4;
    }

    private byte XOR(byte value)
    {
        var result = (byte)(Registers.A ^ value);
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = false;
        Registers.A = result;

        return 4;
    }

    private byte OR(byte value)
    {
        var result = (byte)(Registers.A | value);
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = false;
        Registers.A = result;

        return 4;
    }

    private byte CP(byte value)
    {
        var result = Registers.A - value;
        Registers.Flags.SetZ(result);
        Registers.Flags.N = true;
        Registers.Flags.SetHSub(Registers.A, value);
        Registers.Flags.SetC(result);

        return 4;
    }

    private byte JR(sbyte address, bool flag)
    {
        if (!flag)
        {
            return 8;
        }

        Registers.PC = (ushort)(Registers.PC + address);

        return 12;
    }

    private byte JP(ushort address, bool flag)
    {
        if (!flag)
        {
            return 12;
        }

        Registers.PC = address;

        return 16;
    }

    private byte RET(bool flag)
    {
        if (!flag)
        {
            return 8;
        }

        Registers.PC = POP();

        return 20;
    }

    private byte CALL(ushort address, bool flag)
    {
        if (!flag)
        {
            return 12;
        }

        PUSH(Registers.PC);
        Registers.PC = address;

        return 24;
    }

    private byte RST(byte value)
    {
        PUSH(Registers.PC);
        Registers.PC = value;

        return 16;
    }

    private void PUSH(ushort word)
    {
        Registers.SP -= 2;
        bus.WriteWord(Registers.SP, word);
    }

    private ushort POP()
    {
        var result = bus.ReadWord(Registers.SP);
        Registers.SP += 2;
        return result;
    }

    public byte NOP(Instruction instruction)
    {
        return 4;
    }

    public byte LD_BC_d16(Instruction instruction)
    {
        Registers.BC = instruction.N16;
        return 12;
    }

    public byte LD_ptr_BC_A(Instruction instruction)
    {
        bus.Write(Registers.BC, Registers.A);
        return 8;
    }

    public byte INC_BC(Instruction instruction)
    {
        Registers.BC++;
        return 8;
    }

    public byte INC_B(Instruction instruction)
    {
        return INC(ref Registers.B);
    }

    public byte DEC_B(Instruction instruction)
    {
        return DEC(ref Registers.B);
    }

    public byte LD_B_d8(Instruction instruction)
    {
        Registers.B = instruction.N8;
        return 8;
    }

    public byte RLCA(Instruction instruction)
    {
        Registers.F = 0;
        Registers.Flags.C = (Registers.A & 0x80) != 0;
        Registers.A = (byte)(Registers.A << 1 | Registers.A >> 7);
        return 4;
    }

    public byte LD_ptr_a16_SP(Instruction instruction)
    {
        bus.WriteWord(Registers.SP, instruction.N16);
        return 20;
    }

    public byte ADD_HL_BC(Instruction instruction)
    {
        return DAD(Registers.BC);
    }

    public byte LD_A_ptr_BC(Instruction instruction)
    {
        Registers.A = bus.Read(Registers.BC);
        return 8;
    }

    public byte DEC_BC(Instruction instruction)
    {
        Registers.BC--;
        return 8;
    }

    public byte INC_C(Instruction instruction)
    {
        return INC(ref Registers.C);
    }

    public byte DEC_C(Instruction instruction)
    {
        return DEC(ref Registers.C);
    }

    public byte LD_C_d8(Instruction instruction)
    {
        Registers.C = instruction.N8;
        return 8;
    }

    public byte RRCA(Instruction instruction)
    {
        Registers.F = 0;
        Registers.Flags.C = ((Registers.A & 0x1) != 0);
        Registers.A = (byte)(Registers.A >> 1 | Registers.A << 7);
        return 4;
    }

    public byte STOP_d8(Instruction instruction)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public byte LD_DE_d16(Instruction instruction)
    {
        Registers.DE = instruction.N16;
        return 12;
    }

    public byte LD_ptr_DE_A(Instruction instruction)
    {
        bus.Write(Registers.DE, Registers.A);
        return 8;
    }

    public byte INC_DE(Instruction instruction)
    {
        Registers.DE++;
        return 8;
    }

    public byte INC_D(Instruction instruction)
    {
        return INC(ref Registers.D);
    }

    public byte DEC_D(Instruction instruction)
    {
        return DEC(ref Registers.D);
    }

    public byte LD_D_d8(Instruction instruction)
    {
        Registers.D = instruction.N8;
        return 8;
    }

    public byte RLA(Instruction instruction)
    {
        var prevFlagC = Registers.Flags.C;
        Registers.F = 0;
        Registers.Flags.C = ((Registers.A & 0x80) != 0);
        Registers.A = (byte)((Registers.A << 1) | (prevFlagC ? 1 : 0));
        return 4;
    }

    public byte JR_e8(Instruction instruction)
    {
        return JR(instruction.E8, flag: true);
    }

    public byte ADD_HL_DE(Instruction instruction)
    {
        return DAD(Registers.DE);
    }

    public byte LD_A_ptr_DE(Instruction instruction)
    {
        Registers.A = bus.Read(Registers.DE);
        return 8;
    }

    public byte DEC_DE(Instruction instruction)
    {
        Registers.DE--;
        return 8;
    }

    public byte INC_E(Instruction instruction)
    {
        return INC(ref Registers.E);
    }

    public byte DEC_E(Instruction instruction)
    {
        return DEC(ref Registers.E);
    }

    public byte LD_E_d8(Instruction instruction)
    {
        Registers.E = instruction.N8;
        return 8;
    }

    public byte RRA(Instruction instruction)
    {
        var prevFlagC = Registers.Flags.C;
        Registers.F = 0;
        Registers.Flags.C = ((Registers.A & 0x1) != 0);
        Registers.A = (byte)((Registers.A >> 1) | (prevFlagC ? 0x80 : 0));
        return 4;
    }

    public byte JR_NZ_e8(Instruction instruction)
    {
        return JR(instruction.E8, !Registers.Flags.Z);
    }

    public byte LD_HL_d16(Instruction instruction)
    {
        Registers.HL = instruction.N16;
        return 12;
    }

    public byte LD_HLI_A(Instruction instruction)
    {
        bus.Write(Registers.HL++, Registers.A);
        return 8;
    }

    public byte INC_HL(Instruction instruction)
    {
        Registers.HL++;
        return 8;
    }

    public byte INC_H(Instruction instruction)
    {
        return INC(ref Registers.H);
    }

    public byte DEC_H(Instruction instruction)
    {
        return DEC(ref Registers.H);
    }

    public byte LD_H_d8(Instruction instruction)
    {
        Registers.H = instruction.N8;
        return 8;
    }

    public byte DAA(Instruction instruction)
    {
        if (Registers.Flags.N)
        {
            if (Registers.Flags.C)
                Registers.A -= 0x60;
            if (Registers.Flags.H)
                Registers.A -= 0x6;
        }
        else
        {
            if (Registers.Flags.C || (Registers.A > 0x99))
                Registers.A += 0x60; Registers.Flags.C = true;
            if (Registers.Flags.H || (Registers.A & 0xF) > 0x9)
                Registers.A += 0x6;
        }
        Registers.Flags.SetZ(Registers.A);
        Registers.Flags.H = false;
        return 4;
    }

    public byte JR_Z_e8(Instruction instruction)
    {
        return JR(instruction.E8, Registers.Flags.Z);
    }

    public byte ADD_HL_HL(Instruction instruction)
    {
        return DAD(Registers.HL);
    }

    public byte LD_A_HLI(Instruction instruction)
    {
        Registers.A = bus.Read(Registers.HL++);
        return 8;
    }

    public byte DEC_HL(Instruction instruction)
    {
        Registers.HL--;
        return 8;
    }

    public byte INC_L(Instruction instruction)
    {
        return INC(ref Registers.L);
    }

    public byte DEC_L(Instruction instruction)
    {
        return DEC(ref Registers.L);
    }

    public byte LD_L_d8(Instruction instruction)
    {
        Registers.L = instruction.N8;
        return 8;
    }

    public byte CPL(Instruction instruction)
    {
        Registers.A = (byte)~Registers.A;
        Registers.Flags.N = true;
        Registers.Flags.H = true;
        return 4;
    }

    public byte JR_NC_e8(Instruction instruction)
    {
        return JR(instruction.E8, !Registers.Flags.C);
    }

    public byte LD_SP_d16(Instruction instruction)
    {
        Registers.SP = instruction.N16;
        return 12;
    }

    public byte LD_HLD_A(Instruction instruction)
    {
        bus.Write(Registers.HL--, Registers.A);
        return 8;
    }

    public byte INC_SP(Instruction instruction)
    {
        Registers.SP++;
        return 8;
    }

    public byte INC_ptr_HL(Instruction instruction)
    {
        var value = bus.Read(Registers.HL);
        INC(ref value);
        bus.Write(Registers.HL, value);
        return 12;
    }

    public byte DEC_ptr_HL(Instruction instruction)
    {
        var value = bus.Read(Registers.HL);
        DEC(ref value);
        bus.Write(Registers.HL, value);
        return 12;
    }

    public byte LD_ptr_HL_d8(Instruction instruction)
    {
        bus.Write(Registers.HL, instruction.N8);
        return 12;
    }

    public byte SCF(Instruction instruction)
    {
        Registers.Flags.C = true;
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        return 4;
    }

    public byte JR_C_e8(Instruction instruction)
    {
        return JR(instruction.E8, Registers.Flags.C);
    }

    public byte ADD_HL_SP(Instruction instruction)
    {
        return DAD(Registers.SP);
    }

    public byte LD_A_HLD(Instruction instruction)
    {
        Registers.A = bus.Read(Registers.HL--);
        return 8;
    }

    public byte DEC_SP(Instruction instruction)
    {
        Registers.SP--;
        return 8;
    }

    public byte INC_A(Instruction instruction)
    {
        return INC(ref Registers.A);
    }

    public byte DEC_A(Instruction instruction)
    {
        return DEC(ref Registers.A);
    }

    public byte LD_A_d8(Instruction instruction)
    {
        Registers.A = instruction.N8;
        return 8;
    }

    public byte CCF(Instruction instruction)
    {
        Registers.Flags.C = !Registers.Flags.C;
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        return 4;
    }

    public byte LD_B_B(Instruction instruction)
    {
        // Registers.B = Registers.B;
        return 4;
    }

    public byte LD_B_C(Instruction instruction)
    {
        Registers.B = Registers.C;
        return 4;
    }

    public byte LD_B_D(Instruction instruction)
    {
        Registers.B = Registers.D;
        return 4;
    }

    public byte LD_B_E(Instruction instruction)
    {
        Registers.B = Registers.E;
        return 4;
    }

    public byte LD_B_H(Instruction instruction)
    {
        Registers.B = Registers.H;
        return 4;
    }

    public byte LD_B_L(Instruction instruction)
    {
        Registers.B = Registers.L;
        return 4;
    }

    public byte LD_B_ptr_HL(Instruction instruction)
    {
        Registers.B = bus.Read(Registers.HL);
        return 8;
    }

    public byte LD_B_A(Instruction instruction)
    {
        Registers.B = Registers.A;
        return 4;
    }

    public byte LD_C_B(Instruction instruction)
    {
        Registers.C = Registers.B;
        return 4;
    }

    public byte LD_C_C(Instruction instruction)
    {
        // Registers.C = Registers.C;
        return 4;
    }

    public byte LD_C_D(Instruction instruction)
    {
        Registers.C = Registers.D;
        return 4;
    }

    public byte LD_C_E(Instruction instruction)
    {
        Registers.C = Registers.E;
        return 4;
    }

    public byte LD_C_H(Instruction instruction)
    {
        Registers.C = Registers.H;
        return 4;
    }

    public byte LD_C_L(Instruction instruction)
    {
        Registers.C = Registers.L;
        return 4;
    }

    public byte LD_C_ptr_HL(Instruction instruction)
    {
        Registers.C = bus.Read(Registers.HL);
        return 8;
    }

    public byte LD_C_A(Instruction instruction)
    {
        Registers.C = Registers.A;
        return 4;
    }

    public byte LD_D_B(Instruction instruction)
    {
        Registers.D = Registers.B;
        return 4;
    }

    public byte LD_D_C(Instruction instruction)
    {
        Registers.D = Registers.C;
        return 4;
    }

    public byte LD_D_D(Instruction instruction)
    {
        // Registers.D = Registers.D;
        return 4;
    }

    public byte LD_D_E(Instruction instruction)
    {
        Registers.D = Registers.E;
        return 4;
    }

    public byte LD_D_H(Instruction instruction)
    {
        Registers.D = Registers.H;
        return 4;
    }

    public byte LD_D_L(Instruction instruction)
    {
        Registers.D = Registers.L;
        return 4;
    }

    public byte LD_D_ptr_HL(Instruction instruction)
    {
        Registers.D = bus.Read(Registers.HL);
        return 8;
    }

    public byte LD_D_A(Instruction instruction)
    {
        Registers.D = Registers.A;
        return 4;
    }

    public byte LD_E_B(Instruction instruction)
    {
        Registers.E = Registers.B;
        return 4;
    }

    public byte LD_E_C(Instruction instruction)
    {
        Registers.E = Registers.C;
        return 4;
    }

    public byte LD_E_D(Instruction instruction)
    {
        Registers.E = Registers.D;
        return 4;
    }

    public byte LD_E_E(Instruction instruction)
    {
        // Registers.E = Registers.E;
        return 4;
    }

    public byte LD_E_H(Instruction instruction)
    {
        Registers.E = Registers.H;
        return 4;
    }

    public byte LD_E_L(Instruction instruction)
    {
        Registers.E = Registers.L;
        return 4;
    }

    public byte LD_E_ptr_HL(Instruction instruction)
    {
        Registers.E = bus.Read(Registers.HL);
        return 8;
    }

    public byte LD_E_A(Instruction instruction)
    {
        Registers.E = Registers.A;
        return 4;
    }

    public byte LD_H_B(Instruction instruction)
    {
        Registers.H = Registers.B;
        return 4;
    }

    public byte LD_H_C(Instruction instruction)
    {
        Registers.H = Registers.C;
        return 4;
    }

    public byte LD_H_D(Instruction instruction)
    {
        Registers.H = Registers.D;
        return 4;
    }

    public byte LD_H_E(Instruction instruction)
    {
        Registers.H = Registers.E;
        return 4;
    }

    public byte LD_H_H(Instruction instruction)
    {
        // Registers.H = Registers.H;
        return 4;
    }

    public byte LD_H_L(Instruction instruction)
    {
        Registers.H = Registers.L;
        return 4;
    }

    public byte LD_H_ptr_HL(Instruction instruction)
    {
        Registers.H = bus.Read(Registers.HL);
        return 8;
    }

    public byte LD_H_A(Instruction instruction)
    {
        Registers.H = Registers.A;
        return 4;
    }

    public byte LD_L_B(Instruction instruction)
    {
        Registers.L = Registers.B;
        return 4;
    }

    public byte LD_L_C(Instruction instruction)
    {
        Registers.L = Registers.C;
        return 4;
    }

    public byte LD_L_D(Instruction instruction)
    {
        Registers.L = Registers.D;
        return 4;
    }

    public byte LD_L_E(Instruction instruction)
    {
        Registers.L = Registers.E;
        return 4;
    }

    public byte LD_L_H(Instruction instruction)
    {
        Registers.L = Registers.H;
        return 4;
    }

    public byte LD_L_L(Instruction instruction)
    {
        // Registers.L = Registers.L;
        return 4;
    }

    public byte LD_L_ptr_HL(Instruction instruction)
    {
        Registers.L = bus.Read(Registers.HL);
        return 8;
    }

    public byte LD_L_A(Instruction instruction)
    {
        Registers.L = Registers.A;
        return 4;
    }

    public byte LD_ptr_HL_B(Instruction instruction)
    {
        bus.Write(Registers.HL, Registers.B);
        return 8;
    }

    public byte LD_ptr_HL_C(Instruction instruction)
    {
        bus.Write(Registers.HL, Registers.C);
        return 8;
    }

    public byte LD_ptr_HL_D(Instruction instruction)
    {
        bus.Write(Registers.HL, Registers.D);
        return 8;
    }

    public byte LD_ptr_HL_E(Instruction instruction)
    {
        bus.Write(Registers.HL, Registers.E);
        return 8;
    }

    public byte LD_ptr_HL_H(Instruction instruction)
    {
        bus.Write(Registers.HL, Registers.H);
        return 8;
    }

    public byte LD_ptr_HL_L(Instruction instruction)
    {
        bus.Write(Registers.HL, Registers.L);
        return 8;
    }

    public byte HALT(Instruction instruction)
    {
        if (_ime || !bus.HasPendingInterrupts)
        {
            _halted = true;
        }
        else
        {
            _haltBug = true;
        }

        return 4;
    }

    public byte LD_ptr_HL_A(Instruction instruction)
    {
        bus.Write(Registers.HL, Registers.A);
        return 8;
    }

    public byte LD_A_B(Instruction instruction)
    {
        Registers.A = Registers.B;
        return 4;
    }

    public byte LD_A_C(Instruction instruction)
    {
        Registers.A = Registers.C;
        return 4;
    }

    public byte LD_A_D(Instruction instruction)
    {
        Registers.A = Registers.D;
        return 4;
    }

    public byte LD_A_E(Instruction instruction)
    {
        Registers.A = Registers.E;
        return 4;
    }

    public byte LD_A_H(Instruction instruction)
    {
        Registers.A = Registers.H;
        return 4;
    }

    public byte LD_A_L(Instruction instruction)
    {
        Registers.A = Registers.L;
        return 4;
    }

    public byte LD_A_ptr_HL(Instruction instruction)
    {
        Registers.A = bus.Read(Registers.HL);
        return 8;
    }

    public byte LD_A_A(Instruction instruction)
    {
        // Registers.A = Registers.A;
        return 4;
    }

    public byte ADD_A_B(Instruction instruction)
    {
        return ADD(Registers.B);
    }

    public byte ADD_A_C(Instruction instruction)
    {
        return ADD(Registers.C);
    }

    public byte ADD_A_D(Instruction instruction)
    {
        return ADD(Registers.D);
    }

    public byte ADD_A_E(Instruction instruction)
    {
        return ADD(Registers.E);
    }

    public byte ADD_A_H(Instruction instruction)
    {
        return ADD(Registers.H);
    }

    public byte ADD_A_L(Instruction instruction)
    {
        return ADD(Registers.L);
    }

    public byte ADD_A_ptr_HL(Instruction instruction)
    {
        return (byte)(ADD(bus.Read(Registers.HL)) + 4);
    }

    public byte ADD_A_A(Instruction instruction)
    {
        return ADD(Registers.A);
    }

    public byte ADC_A_B(Instruction instruction)
    {
        return ADC(Registers.B);
    }

    public byte ADC_A_C(Instruction instruction)
    {
        return ADC(Registers.C);
    }

    public byte ADC_A_D(Instruction instruction)
    {
        return ADC(Registers.D);
    }

    public byte ADC_A_E(Instruction instruction)
    {
        return ADC(Registers.E);
    }

    public byte ADC_A_H(Instruction instruction)
    {
        return ADC(Registers.H);
    }

    public byte ADC_A_L(Instruction instruction)
    {
        return ADC(Registers.L);
    }

    public byte ADC_A_ptr_HL(Instruction instruction)
    {
        return (byte)(ADC(bus.Read(Registers.HL)) + 4);
    }

    public byte ADC_A_A(Instruction instruction)
    {
        return ADC(Registers.A);
    }

    public byte SUB_A_B(Instruction instruction)
    {
        return SUB(Registers.B);
    }

    public byte SUB_A_C(Instruction instruction)
    {
        return SUB(Registers.C);
    }

    public byte SUB_A_D(Instruction instruction)
    {
        return SUB(Registers.D);
    }

    public byte SUB_A_E(Instruction instruction)
    {
        return SUB(Registers.E);
    }

    public byte SUB_A_H(Instruction instruction)
    {
        return SUB(Registers.H);
    }

    public byte SUB_A_L(Instruction instruction)
    {
        return SUB(Registers.L);
    }

    public byte SUB_A_ptr_HL(Instruction instruction)
    {
        return (byte)(SUB(bus.Read(Registers.HL)) + 4);
    }

    public byte SUB_A_A(Instruction instruction)
    {
        return SUB(Registers.A);
    }

    public byte SBC_A_B(Instruction instruction)
    {
        return SBC(Registers.B);
    }

    public byte SBC_A_C(Instruction instruction)
    {
        return SBC(Registers.C);
    }

    public byte SBC_A_D(Instruction instruction)
    {
        return SBC(Registers.D);
    }

    public byte SBC_A_E(Instruction instruction)
    {
        return SBC(Registers.E);
    }

    public byte SBC_A_H(Instruction instruction)
    {
        return SBC(Registers.H);
    }

    public byte SBC_A_L(Instruction instruction)
    {
        return SBC(Registers.L);
    }

    public byte SBC_A_ptr_HL(Instruction instruction)
    {
        return (byte)(SBC(bus.Read(Registers.HL)) + 4);
    }

    public byte SBC_A_A(Instruction instruction)
    {
        return SBC(Registers.A);
    }

    public byte AND_A_B(Instruction instruction)
    {
        return AND(Registers.B);
    }

    public byte AND_A_C(Instruction instruction)
    {
        return AND(Registers.C);
    }

    public byte AND_A_D(Instruction instruction)
    {
        return AND(Registers.D);
    }

    public byte AND_A_E(Instruction instruction)
    {
        return AND(Registers.E);
    }

    public byte AND_A_H(Instruction instruction)
    {
        return AND(Registers.H);
    }

    public byte AND_A_L(Instruction instruction)
    {
        return AND(Registers.L);
    }

    public byte AND_A_ptr_HL(Instruction instruction)
    {
        return (byte)(AND(bus.Read(Registers.HL)) + 4);
    }

    public byte AND_A_A(Instruction instruction)
    {
        return AND(Registers.A);
    }

    public byte XOR_A_B(Instruction instruction)
    {
        return XOR(Registers.B);
    }

    public byte XOR_A_C(Instruction instruction)
    {
        return XOR(Registers.C);
    }

    public byte XOR_A_D(Instruction instruction)
    {
        return XOR(Registers.D);
    }

    public byte XOR_A_E(Instruction instruction)
    {
        return XOR(Registers.E);
    }

    public byte XOR_A_H(Instruction instruction)
    {
        return XOR(Registers.H);
    }

    public byte XOR_A_L(Instruction instruction)
    {
        return XOR(Registers.L);
    }

    public byte XOR_A_ptr_HL(Instruction instruction)
    {
        return (byte)(XOR(bus.Read(Registers.HL)) + 4);
    }

    public byte XOR_A_A(Instruction instruction)
    {
        return XOR(Registers.A);
    }

    public byte OR_A_B(Instruction instruction)
    {
        return OR(Registers.B);
    }

    public byte OR_A_C(Instruction instruction)
    {
        return OR(Registers.C);
    }

    public byte OR_A_D(Instruction instruction)
    {
        return OR(Registers.D);
    }

    public byte OR_A_E(Instruction instruction)
    {
        return OR(Registers.E);
    }

    public byte OR_A_H(Instruction instruction)
    {
        return OR(Registers.H);
    }

    public byte OR_A_L(Instruction instruction)
    {
        return OR(Registers.L);
    }

    public byte OR_A_ptr_HL(Instruction instruction)
    {
        return (byte)(OR(bus.Read(Registers.HL)) + 4);
    }

    public byte OR_A_A(Instruction instruction)
    {
        return OR(Registers.A);
    }

    public byte CP_A_B(Instruction instruction)
    {
        return CP(Registers.B);
    }

    public byte CP_A_C(Instruction instruction)
    {
        return CP(Registers.C);
    }

    public byte CP_A_D(Instruction instruction)
    {
        return CP(Registers.D);
    }

    public byte CP_A_E(Instruction instruction)
    {
        return CP(Registers.E);
    }

    public byte CP_A_H(Instruction instruction)
    {
        return CP(Registers.H);
    }

    public byte CP_A_L(Instruction instruction)
    {
        return CP(Registers.L);
    }

    public byte CP_A_ptr_HL(Instruction instruction)
    {
        return (byte)(CP(bus.Read(Registers.HL)) + 4);
    }

    public byte CP_A_A(Instruction instruction)
    {
        return CP(Registers.A);
    }

    public byte RET_NZ(Instruction instruction)
    {
        return RET(!Registers.Flags.Z);
    }

    public byte POP_BC(Instruction instruction)
    {
        Registers.BC = POP();
        return 12;
    }

    public byte JP_NZ_a16(Instruction instruction)
    {
        return JP(instruction.N16, !Registers.Flags.Z);
    }

    public byte JP_a16(Instruction instruction)
    {
        return JP(instruction.N16, flag: true);
    }

    public byte CALL_NZ_a16(Instruction instruction)
    {
        return CALL(instruction.N16, !Registers.Flags.Z);
    }

    public byte PUSH_BC(Instruction instruction)
    {
        PUSH(Registers.BC);
        return 16;
    }

    public byte ADD_A_d8(Instruction instruction)
    {
        return (byte)(ADD(instruction.N8) + 4);
    }

    public byte RST_00(Instruction instruction)
    {
        return RST(0x00);
    }

    public byte RET_Z(Instruction instruction)
    {
        return RET(Registers.Flags.Z);
    }

    public byte RET(Instruction instruction)
    {
        return RET(flag: true);
    }

    public byte JP_Z_a16(Instruction instruction)
    {
        return JP(instruction.N16, Registers.Flags.Z);
    }

    public byte PREFIX(Instruction instruction)
    {
        throw new NotImplementedException(instruction.Opcode.Description);
        // return 4;
    }

    public byte CALL_Z_a16(Instruction instruction)
    {
        return CALL(instruction.N16, Registers.Flags.Z);
    }

    public byte CALL_a16(Instruction instruction)
    {
        return CALL(instruction.N16, flag: true);
    }

    public byte ADC_A_d8(Instruction instruction)
    {
        return (byte)(ADC(instruction.N8) + 4);
    }

    public byte RST_08(Instruction instruction)
    {
        return RST(0x08);
    }

    public byte RET_NC(Instruction instruction)
    {
        return RET(!Registers.Flags.C);
    }

    public byte POP_DE(Instruction instruction)
    {
        Registers.DE = POP();
        return 12;
    }

    public byte JP_NC_a16(Instruction instruction)
    {
        return JP(instruction.N16, !Registers.Flags.C);
    }

    public byte ILLEGAL_D3(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte CALL_NC_a16(Instruction instruction)
    {
        return CALL(instruction.N16, !Registers.Flags.C);
    }

    public byte PUSH_DE(Instruction instruction)
    {
        PUSH(Registers.DE);
        return 16;
    }

    public byte SUB_A_d8(Instruction instruction)
    {
        return (byte)(SUB(instruction.N8) + 4);
    }

    public byte RST_10(Instruction instruction)
    {
        return RST(0x10);
    }

    public byte RET_C(Instruction instruction)
    {
        return RET(Registers.Flags.C);
    }

    public byte RETI(Instruction instruction)
    {
        RET(flag: true);
        _ime = true;
        return 16;
    }

    public byte JP_C_a16(Instruction instruction)
    {
        return JP(instruction.N16, Registers.Flags.C);
    }

    public byte ILLEGAL_DB(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte CALL_C_a16(Instruction instruction)
    {
        return CALL(instruction.N16, Registers.Flags.C);
    }

    public byte ILLEGAL_DD(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte SBC_A_d8(Instruction instruction)
    {
        return (byte)(SBC(instruction.N8) + 4);
    }

    public byte RST_18(Instruction instruction)
    {
        return RST(0x18);
    }

    public byte LDH_ptr_a8_A(Instruction instruction)
    {
        bus.Write((ushort)(0xFF00 + instruction.N8), Registers.A);
        return 12;
    }

    public byte POP_HL(Instruction instruction)
    {
        Registers.HL = POP();
        return 12;
    }

    public byte LDH_ptr_C_A(Instruction instruction)
    {
        bus.Write((ushort)(0xFF00 + Registers.C), Registers.A);
        return 8;
    }

    public byte ILLEGAL_E3(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte ILLEGAL_E4(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte PUSH_HL(Instruction instruction)
    {
        PUSH(Registers.HL);
        return 16;
    }

    public byte AND_A_d8(Instruction instruction)
    {
        return (byte)(AND(instruction.N8) + 4);
    }

    public byte RST_20(Instruction instruction)
    {
        return RST(0x20);
    }

    public byte ADD_SP_e8(Instruction instruction)
    {
        Registers.Flags.Z = false;
        Registers.Flags.N = false;
        Registers.Flags.SetH((byte)Registers.SP, instruction.N8);
        Registers.Flags.SetC((byte)Registers.SP + instruction.N8);
        Registers.SP = (ushort)(Registers.SP + instruction.E8);

        return 16;
    }

    public byte JP_HL(Instruction instruction)
    {
        Registers.PC = Registers.HL;
        return 4;
    }

    public byte LD_ptr_a16_A(Instruction instruction)
    {
        bus.WriteWord(instruction.N16, Registers.A);
        return 16;
    }

    public byte ILLEGAL_EB(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte ILLEGAL_EC(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte ILLEGAL_ED(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte XOR_A_d8(Instruction instruction)
    {
        return (byte)(XOR(instruction.N8) + 4);
    }

    public byte RST_28(Instruction instruction)
    {
        return RST(0x28);
    }

    public byte LDH_A_ptr_a8(Instruction instruction)
    {
        Registers.A = bus.Read((ushort)(0xFF00 + instruction.N8));
        return 12;
    }

    public byte POP_AF(Instruction instruction)
    {
        Registers.AF = POP();
        return 12;
    }

    public byte LDH_A_ptr_C(Instruction instruction)
    {
        Registers.A = bus.Read((ushort)(0xFF00 + Registers.C));
        return 8;
    }

    public byte DI(Instruction instruction)
    {
        _ime = false;
        return 4;
    }

    public byte ILLEGAL_F4(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte PUSH_AF(Instruction instruction)
    {
        PUSH(Registers.AF);
        return 16;
    }

    public byte OR_A_d8(Instruction instruction)
    {
        return (byte)(OR(instruction.N8) + 4);
    }

    public byte RST_30(Instruction instruction)
    {
        return RST(0x30);
    }

    public byte LD_HL_SP_e8(Instruction instruction)
    {
        Registers.Flags.Z = false;
        Registers.Flags.N = false;
        Registers.Flags.SetH((byte)Registers.SP, instruction.N8);
        Registers.Flags.SetC((byte)Registers.SP + instruction.N8);
        Registers.HL = (ushort)(Registers.SP + instruction.E8);

        return 12;
    }

    public byte LD_SP_HL(Instruction instruction)
    {
        Registers.SP = Registers.HL;
        return 8;
    }

    public byte LD_A_ptr_a16(Instruction instruction)
    {
        Registers.A = bus.Read(instruction.N16);
        return 16;
    }

    public byte EI(Instruction instruction)
    {
        _imeLatch = true;
        return 4;
    }

    public byte ILLEGAL_FC(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte ILLEGAL_FD(Instruction instruction)
    {
        throw new InvalidOperationException(instruction.ToString());
    }

    public byte CP_A_d8(Instruction instruction)
    {
        return (byte)(CP(instruction.N8) + 4);
    }

    public byte RST_38(Instruction instruction)
    {
        return RST(0x38);
    }
}
