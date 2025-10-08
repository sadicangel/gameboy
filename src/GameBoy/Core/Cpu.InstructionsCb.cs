namespace GameBoy.Core;

partial class Cpu
{
    private byte RLC(ref byte r)
    {
        var result = (byte)((r << 1) | (r >> 7));
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = (r & 0x80) != 0;

        r = result;
        return 8;
    }

    private byte RRC(ref byte r)
    {
        var result = (byte)((r >> 1) | (r << 7));
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = (r & 0x1) != 0;

        r = result;
        return 8;
    }

    private byte RL(ref byte r)
    {
        var prevC = Registers.Flags.C;
        var result = (byte)((r << 1) | (prevC ? 1 : 0));
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = (r & 0x80) != 0;

        r = result;
        return 8;
    }

    private byte RR(ref byte r)
    {
        var prevC = Registers.Flags.C;
        var result = (byte)((r >> 1) | (prevC ? 0x80 : 0));
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = (r & 0x1) != 0;

        r = result;
        return 8;
    }

    private byte SLA(ref byte r)
    {
        var result = (byte)(r << 1);
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = (r & 0x80) != 0;

        r = result;
        return 8;
    }

    private byte SRA(ref byte r)
    {
        var result = (byte)((r >> 1) | (r & 0x80));
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = (r & 0x1) != 0;

        r = result;
        return 8;
    }

    private byte SWAP(ref byte r)
    {
        var result = (byte)((r & 0xF0) >> 4 | (r & 0x0F) << 4);
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = false;

        r = result;
        return 8;
    }

    private byte SRL(ref byte r)
    {
        var result = (byte)(r >> 1);
        Registers.Flags.SetZ(result);
        Registers.Flags.N = false;
        Registers.Flags.H = false;
        Registers.Flags.C = (r & 0x1) != 0;

        r = result;
        return 8;
    }

    private byte BIT(byte r, byte b)
    {
        Registers.Flags.Z = (r & b) == 0;
        Registers.Flags.N = false;
        Registers.Flags.H = true;

        return 8;
    }
    private byte RES(ref byte r, int b)
    {
        r &= (byte)~b;
        return 8;
    }

    private byte SET(ref byte r, byte b)
    {
        r |= b;
        return 8;
    }

    public byte RLC_B() => RLC(ref Registers.B);
    public byte RLC_C() => RLC(ref Registers.C);
    public byte RLC_D() => RLC(ref Registers.D);
    public byte RLC_E() => RLC(ref Registers.E);
    public byte RLC_H() => RLC(ref Registers.H);
    public byte RLC_L() => RLC(ref Registers.L);
    public byte RLC_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RLC(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RLC_A() => RLC(ref Registers.A);

    public byte RRC_B() => RRC(ref Registers.B);
    public byte RRC_C() => RRC(ref Registers.C);
    public byte RRC_D() => RRC(ref Registers.D);
    public byte RRC_E() => RRC(ref Registers.E);
    public byte RRC_H() => RRC(ref Registers.H);
    public byte RRC_L() => RRC(ref Registers.L);
    public byte RRC_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RRC(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RRC_A() => RRC(ref Registers.A);

    public byte RL_B() => RL(ref Registers.B);
    public byte RL_C() => RL(ref Registers.C);
    public byte RL_D() => RL(ref Registers.D);
    public byte RL_E() => RL(ref Registers.E);
    public byte RL_H() => RL(ref Registers.H);
    public byte RL_L() => RL(ref Registers.L);
    public byte RL_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RL(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RL_A() => RL(ref Registers.A);

    public byte RR_B() => RR(ref Registers.B);
    public byte RR_C() => RR(ref Registers.C);
    public byte RR_D() => RR(ref Registers.D);
    public byte RR_E() => RR(ref Registers.E);
    public byte RR_H() => RR(ref Registers.H);
    public byte RR_L() => RR(ref Registers.L);
    public byte RR_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RR(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RR_A() => RR(ref Registers.A);

    public byte SLA_B() => SLA(ref Registers.B);
    public byte SLA_C() => SLA(ref Registers.C);
    public byte SLA_D() => SLA(ref Registers.D);
    public byte SLA_E() => SLA(ref Registers.E);
    public byte SLA_H() => SLA(ref Registers.H);
    public byte SLA_L() => SLA(ref Registers.L);
    public byte SLA_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SLA(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SLA_A() => SLA(ref Registers.A);

    public byte SRA_B() => SRA(ref Registers.B);
    public byte SRA_C() => SRA(ref Registers.C);
    public byte SRA_D() => SRA(ref Registers.D);
    public byte SRA_E() => SRA(ref Registers.E);
    public byte SRA_H() => SRA(ref Registers.H);
    public byte SRA_L() => SRA(ref Registers.L);
    public byte SRA_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SRA(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SRA_A() => SRA(ref Registers.A);

    public byte SWAP_B() => SWAP(ref Registers.B);
    public byte SWAP_C() => SWAP(ref Registers.C);
    public byte SWAP_D() => SWAP(ref Registers.D);
    public byte SWAP_E() => SWAP(ref Registers.E);
    public byte SWAP_H() => SWAP(ref Registers.H);
    public byte SWAP_L() => SWAP(ref Registers.L);
    public byte SWAP_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SWAP(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SWAP_A() => SWAP(ref Registers.A);

    public byte SRL_B() => SRL(ref Registers.B);
    public byte SRL_C() => SRL(ref Registers.C);
    public byte SRL_D() => SRL(ref Registers.D);
    public byte SRL_E() => SRL(ref Registers.E);
    public byte SRL_H() => SRL(ref Registers.H);
    public byte SRL_L() => SRL(ref Registers.L);
    public byte SRL_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SRL(ref value);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SRL_A() => SRL(ref Registers.A);

    public byte BIT_0_B() => BIT(Registers.B, 1 << 0);
    public byte BIT_0_C() => BIT(Registers.C, 1 << 0);
    public byte BIT_0_D() => BIT(Registers.D, 1 << 0);
    public byte BIT_0_E() => BIT(Registers.E, 1 << 0);
    public byte BIT_0_H() => BIT(Registers.H, 1 << 0);
    public byte BIT_0_L() => BIT(Registers.L, 1 << 0);
    public byte BIT_0_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 0) + 4);
    public byte BIT_0_A() => BIT(Registers.A, 1 << 0);

    public byte BIT_1_B() => BIT(Registers.B, 1 << 1);
    public byte BIT_1_C() => BIT(Registers.C, 1 << 1);
    public byte BIT_1_D() => BIT(Registers.D, 1 << 1);
    public byte BIT_1_E() => BIT(Registers.E, 1 << 1);
    public byte BIT_1_H() => BIT(Registers.H, 1 << 1);
    public byte BIT_1_L() => BIT(Registers.L, 1 << 1);
    public byte BIT_1_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 1) + 4);
    public byte BIT_1_A() => BIT(Registers.A, 1 << 1);

    public byte BIT_2_B() => BIT(Registers.B, 1 << 2);
    public byte BIT_2_C() => BIT(Registers.C, 1 << 2);
    public byte BIT_2_D() => BIT(Registers.D, 1 << 2);
    public byte BIT_2_E() => BIT(Registers.E, 1 << 2);
    public byte BIT_2_H() => BIT(Registers.H, 1 << 2);
    public byte BIT_2_L() => BIT(Registers.L, 1 << 2);
    public byte BIT_2_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 2) + 4);
    public byte BIT_2_A() => BIT(Registers.A, 1 << 2);

    public byte BIT_3_B() => BIT(Registers.B, 1 << 3);
    public byte BIT_3_C() => BIT(Registers.C, 1 << 3);
    public byte BIT_3_D() => BIT(Registers.D, 1 << 3);
    public byte BIT_3_E() => BIT(Registers.E, 1 << 3);
    public byte BIT_3_H() => BIT(Registers.H, 1 << 3);
    public byte BIT_3_L() => BIT(Registers.L, 1 << 3);
    public byte BIT_3_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 3) + 4);
    public byte BIT_3_A() => BIT(Registers.A, 1 << 3);

    public byte BIT_4_B() => BIT(Registers.B, 1 << 4);
    public byte BIT_4_C() => BIT(Registers.C, 1 << 4);
    public byte BIT_4_D() => BIT(Registers.D, 1 << 4);
    public byte BIT_4_E() => BIT(Registers.E, 1 << 4);
    public byte BIT_4_H() => BIT(Registers.H, 1 << 4);
    public byte BIT_4_L() => BIT(Registers.L, 1 << 4);
    public byte BIT_4_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 4) + 4);
    public byte BIT_4_A() => BIT(Registers.A, 1 << 4);

    public byte BIT_5_B() => BIT(Registers.B, 1 << 5);
    public byte BIT_5_C() => BIT(Registers.C, 1 << 5);
    public byte BIT_5_D() => BIT(Registers.D, 1 << 5);
    public byte BIT_5_E() => BIT(Registers.E, 1 << 5);
    public byte BIT_5_H() => BIT(Registers.H, 1 << 5);
    public byte BIT_5_L() => BIT(Registers.L, 1 << 5);
    public byte BIT_5_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 5) + 4);
    public byte BIT_5_A() => BIT(Registers.A, 1 << 5);

    public byte BIT_6_B() => BIT(Registers.B, 1 << 6);
    public byte BIT_6_C() => BIT(Registers.C, 1 << 6);
    public byte BIT_6_D() => BIT(Registers.D, 1 << 6);
    public byte BIT_6_E() => BIT(Registers.E, 1 << 6);
    public byte BIT_6_H() => BIT(Registers.H, 1 << 6);
    public byte BIT_6_L() => BIT(Registers.L, 1 << 6);
    public byte BIT_6_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 6) + 4);
    public byte BIT_6_A() => BIT(Registers.A, 1 << 6);

    public byte BIT_7_B() => BIT(Registers.B, 1 << 7);
    public byte BIT_7_C() => BIT(Registers.C, 1 << 7);
    public byte BIT_7_D() => BIT(Registers.D, 1 << 7);
    public byte BIT_7_E() => BIT(Registers.E, 1 << 7);
    public byte BIT_7_H() => BIT(Registers.H, 1 << 7);
    public byte BIT_7_L() => BIT(Registers.L, 1 << 7);
    public byte BIT_7_ptr_HL() => (byte)(BIT(bus.Read(Registers.HL), 1 << 7) + 4);
    public byte BIT_7_A() => BIT(Registers.A, 1 << 7);

    public byte RES_0_B() => RES(ref Registers.B, 1 << 0);
    public byte RES_0_C() => RES(ref Registers.C, 1 << 0);
    public byte RES_0_D() => RES(ref Registers.D, 1 << 0);
    public byte RES_0_E() => RES(ref Registers.E, 1 << 0);
    public byte RES_0_H() => RES(ref Registers.H, 1 << 0);
    public byte RES_0_L() => RES(ref Registers.L, 1 << 0);
    public byte RES_0_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 0);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_0_A() => RES(ref Registers.A, 1 << 0);

    public byte RES_1_B() => RES(ref Registers.B, 1 << 1);
    public byte RES_1_C() => RES(ref Registers.C, 1 << 1);
    public byte RES_1_D() => RES(ref Registers.D, 1 << 1);
    public byte RES_1_E() => RES(ref Registers.E, 1 << 1);
    public byte RES_1_H() => RES(ref Registers.H, 1 << 1);
    public byte RES_1_L() => RES(ref Registers.L, 1 << 1);
    public byte RES_1_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 1);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_1_A() => RES(ref Registers.A, 1 << 1);

    public byte RES_2_B() => RES(ref Registers.B, 1 << 2);
    public byte RES_2_C() => RES(ref Registers.C, 1 << 2);
    public byte RES_2_D() => RES(ref Registers.D, 1 << 2);
    public byte RES_2_E() => RES(ref Registers.E, 1 << 2);
    public byte RES_2_H() => RES(ref Registers.H, 1 << 2);
    public byte RES_2_L() => RES(ref Registers.L, 1 << 2);
    public byte RES_2_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 2);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_2_A() => RES(ref Registers.A, 1 << 2);

    public byte RES_3_B() => RES(ref Registers.B, 1 << 3);
    public byte RES_3_C() => RES(ref Registers.C, 1 << 3);
    public byte RES_3_D() => RES(ref Registers.D, 1 << 3);
    public byte RES_3_E() => RES(ref Registers.E, 1 << 3);
    public byte RES_3_H() => RES(ref Registers.H, 1 << 3);
    public byte RES_3_L() => RES(ref Registers.L, 1 << 3);
    public byte RES_3_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 3);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_3_A() => RES(ref Registers.A, 1 << 3);

    public byte RES_4_B() => RES(ref Registers.B, 1 << 4);
    public byte RES_4_C() => RES(ref Registers.C, 1 << 4);
    public byte RES_4_D() => RES(ref Registers.D, 1 << 4);
    public byte RES_4_E() => RES(ref Registers.E, 1 << 4);
    public byte RES_4_H() => RES(ref Registers.H, 1 << 4);
    public byte RES_4_L() => RES(ref Registers.L, 1 << 4);
    public byte RES_4_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 4);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_4_A() => RES(ref Registers.A, 1 << 4);

    public byte RES_5_B() => RES(ref Registers.B, 1 << 5);
    public byte RES_5_C() => RES(ref Registers.C, 1 << 5);
    public byte RES_5_D() => RES(ref Registers.D, 1 << 5);
    public byte RES_5_E() => RES(ref Registers.E, 1 << 5);
    public byte RES_5_H() => RES(ref Registers.H, 1 << 5);
    public byte RES_5_L() => RES(ref Registers.L, 1 << 5);
    public byte RES_5_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 5);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_5_A() => RES(ref Registers.A, 1 << 5);

    public byte RES_6_B() => RES(ref Registers.B, 1 << 6);
    public byte RES_6_C() => RES(ref Registers.C, 1 << 6);
    public byte RES_6_D() => RES(ref Registers.D, 1 << 6);
    public byte RES_6_E() => RES(ref Registers.E, 1 << 6);
    public byte RES_6_H() => RES(ref Registers.H, 1 << 6);
    public byte RES_6_L() => RES(ref Registers.L, 1 << 6);
    public byte RES_6_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 6);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_6_A() => RES(ref Registers.A, 1 << 6);

    public byte RES_7_B() => RES(ref Registers.B, 1 << 7);
    public byte RES_7_C() => RES(ref Registers.C, 1 << 7);
    public byte RES_7_D() => RES(ref Registers.D, 1 << 7);
    public byte RES_7_E() => RES(ref Registers.E, 1 << 7);
    public byte RES_7_H() => RES(ref Registers.H, 1 << 7);
    public byte RES_7_L() => RES(ref Registers.L, 1 << 7);
    public byte RES_7_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        RES(ref value, 1 << 7);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte RES_7_A() => RES(ref Registers.A, 1 << 7);

    public byte SET_0_B() => SET(ref Registers.B, 1 << 0);
    public byte SET_0_C() => SET(ref Registers.C, 1 << 0);
    public byte SET_0_D() => SET(ref Registers.D, 1 << 0);
    public byte SET_0_E() => SET(ref Registers.E, 1 << 0);
    public byte SET_0_H() => SET(ref Registers.H, 1 << 0);
    public byte SET_0_L() => SET(ref Registers.L, 1 << 0);
    public byte SET_0_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 0);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_0_A() => SET(ref Registers.A, 1 << 0);

    public byte SET_1_B() => SET(ref Registers.B, 1 << 1);
    public byte SET_1_C() => SET(ref Registers.C, 1 << 1);
    public byte SET_1_D() => SET(ref Registers.D, 1 << 1);
    public byte SET_1_E() => SET(ref Registers.E, 1 << 1);
    public byte SET_1_H() => SET(ref Registers.H, 1 << 1);
    public byte SET_1_L() => SET(ref Registers.L, 1 << 1);
    public byte SET_1_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 1);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_1_A() => SET(ref Registers.A, 1 << 1);

    public byte SET_2_B() => SET(ref Registers.B, 1 << 2);
    public byte SET_2_C() => SET(ref Registers.C, 1 << 2);
    public byte SET_2_D() => SET(ref Registers.D, 1 << 2);
    public byte SET_2_E() => SET(ref Registers.E, 1 << 2);
    public byte SET_2_H() => SET(ref Registers.H, 1 << 2);
    public byte SET_2_L() => SET(ref Registers.L, 1 << 2);
    public byte SET_2_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 2);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_2_A() => SET(ref Registers.A, 1 << 2);

    public byte SET_3_B() => SET(ref Registers.B, 1 << 3);
    public byte SET_3_C() => SET(ref Registers.C, 1 << 3);
    public byte SET_3_D() => SET(ref Registers.D, 1 << 3);
    public byte SET_3_E() => SET(ref Registers.E, 1 << 3);
    public byte SET_3_H() => SET(ref Registers.H, 1 << 3);
    public byte SET_3_L() => SET(ref Registers.L, 1 << 3);
    public byte SET_3_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 3);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_3_A() => SET(ref Registers.A, 1 << 3);

    public byte SET_4_B() => SET(ref Registers.B, 1 << 4);
    public byte SET_4_C() => SET(ref Registers.C, 1 << 4);
    public byte SET_4_D() => SET(ref Registers.D, 1 << 4);
    public byte SET_4_E() => SET(ref Registers.E, 1 << 4);
    public byte SET_4_H() => SET(ref Registers.H, 1 << 4);
    public byte SET_4_L() => SET(ref Registers.L, 1 << 4);
    public byte SET_4_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 4);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_4_A() => SET(ref Registers.A, 1 << 4);

    public byte SET_5_B() => SET(ref Registers.B, 1 << 5);
    public byte SET_5_C() => SET(ref Registers.C, 1 << 5);
    public byte SET_5_D() => SET(ref Registers.D, 1 << 5);
    public byte SET_5_E() => SET(ref Registers.E, 1 << 5);
    public byte SET_5_H() => SET(ref Registers.H, 1 << 5);
    public byte SET_5_L() => SET(ref Registers.L, 1 << 5);
    public byte SET_5_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 5);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_5_A() => SET(ref Registers.A, 1 << 5);

    public byte SET_6_B() => SET(ref Registers.B, 1 << 6);
    public byte SET_6_C() => SET(ref Registers.C, 1 << 6);
    public byte SET_6_D() => SET(ref Registers.D, 1 << 6);
    public byte SET_6_E() => SET(ref Registers.E, 1 << 6);
    public byte SET_6_H() => SET(ref Registers.H, 1 << 6);
    public byte SET_6_L() => SET(ref Registers.L, 1 << 6);
    public byte SET_6_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 6);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_6_A() => SET(ref Registers.A, 1 << 6);

    public byte SET_7_B() => SET(ref Registers.B, 1 << 7);
    public byte SET_7_C() => SET(ref Registers.C, 1 << 7);
    public byte SET_7_D() => SET(ref Registers.D, 1 << 7);
    public byte SET_7_E() => SET(ref Registers.E, 1 << 7);
    public byte SET_7_H() => SET(ref Registers.H, 1 << 7);
    public byte SET_7_L() => SET(ref Registers.L, 1 << 7);
    public byte SET_7_ptr_HL()
    {
        var value = bus.Read(Registers.HL);
        SET(ref value, 1 << 7);
        bus.Write(Registers.HL, value);
        return 16;
    }
    public byte SET_7_A() => SET(ref Registers.A, 1 << 7);
}
