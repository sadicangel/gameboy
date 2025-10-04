using System.ComponentModel;

namespace GameBoy.Core;

public enum Opcode : byte
{
    [Description("NOP")]
    NOP = 0x00,
    [Description("LD BC, n16")]
    LD_BC_d16 = 0x01,
    [Description("LD BC, A")]
    LD_ptr_BC_A = 0x02,
    [Description("INC BC")]
    INC_BC = 0x03,
    [Description("INC B")]
    INC_B = 0x04,
    [Description("DEC B")]
    DEC_B = 0x05,
    [Description("LD B, n8")]
    LD_B_d8 = 0x06,
    [Description("RLCA")]
    RLCA = 0x07,
    [Description("LD a16, SP")]
    LD_ptr_a16_SP = 0x08,
    [Description("ADD HL, BC")]
    ADD_HL_BC = 0x09,
    [Description("LD A, BC")]
    LD_A_ptr_BC = 0x0A,
    [Description("DEC BC")]
    DEC_BC = 0x0B,
    [Description("INC C")]
    INC_C = 0x0C,
    [Description("DEC C")]
    DEC_C = 0x0D,
    [Description("LD C, n8")]
    LD_C_d8 = 0x0E,
    [Description("RRCA")]
    RRCA = 0x0F,
    [Description("STOP n8")]
    STOP_d8 = 0x10,
    [Description("LD DE, n16")]
    LD_DE_d16 = 0x11,
    [Description("LD DE, A")]
    LD_ptr_DE_A = 0x12,
    [Description("INC DE")]
    INC_DE = 0x13,
    [Description("INC D")]
    INC_D = 0x14,
    [Description("DEC D")]
    DEC_D = 0x15,
    [Description("LD D, n8")]
    LD_D_d8 = 0x16,
    [Description("RLA")]
    RLA = 0x17,
    [Description("JR e8")]
    JR_e8 = 0x18,
    [Description("ADD HL, DE")]
    ADD_HL_DE = 0x19,
    [Description("LD A, DE")]
    LD_A_ptr_DE = 0x1A,
    [Description("DEC DE")]
    DEC_DE = 0x1B,
    [Description("INC E")]
    INC_E = 0x1C,
    [Description("DEC E")]
    DEC_E = 0x1D,
    [Description("LD E, n8")]
    LD_E_d8 = 0x1E,
    [Description("RRA")]
    RRA = 0x1F,
    [Description("JR NZ, e8")]
    JR_NZ_e8 = 0x20,
    [Description("LD HL, n16")]
    LD_HL_d16 = 0x21,
    [Description("LD HL, A")]
    LD_HLI_A = 0x22,
    [Description("INC HL")]
    INC_HL = 0x23,
    [Description("INC H")]
    INC_H = 0x24,
    [Description("DEC H")]
    DEC_H = 0x25,
    [Description("LD H, n8")]
    LD_H_d8 = 0x26,
    [Description("DAA")]
    DAA = 0x27,
    [Description("JR Z, e8")]
    JR_Z_e8 = 0x28,
    [Description("ADD HL, HL")]
    ADD_HL_HL = 0x29,
    [Description("LD A, HL")]
    LD_A_HLI = 0x2A,
    [Description("DEC HL")]
    DEC_HL = 0x2B,
    [Description("INC L")]
    INC_L = 0x2C,
    [Description("DEC L")]
    DEC_L = 0x2D,
    [Description("LD L, n8")]
    LD_L_d8 = 0x2E,
    [Description("CPL")]
    CPL = 0x2F,
    [Description("JR NC, e8")]
    JR_NC_e8 = 0x30,
    [Description("LD SP, n16")]
    LD_SP_d16 = 0x31,
    [Description("LD HL, A")]
    LD_HLD_A = 0x32,
    [Description("INC SP")]
    INC_SP = 0x33,
    [Description("INC HL")]
    INC_ptr_HL = 0x34,
    [Description("DEC HL")]
    DEC_ptr_HL = 0x35,
    [Description("LD HL, n8")]
    LD_ptr_HL_d8 = 0x36,
    [Description("SCF")]
    SCF = 0x37,
    [Description("JR C, e8")]
    JR_C_e8 = 0x38,
    [Description("ADD HL, SP")]
    ADD_HL_SP = 0x39,
    [Description("LD A, HL")]
    LD_A_HLD = 0x3A,
    [Description("DEC SP")]
    DEC_SP = 0x3B,
    [Description("INC A")]
    INC_A = 0x3C,
    [Description("DEC A")]
    DEC_A = 0x3D,
    [Description("LD A, n8")]
    LD_A_d8 = 0x3E,
    [Description("CCF")]
    CCF = 0x3F,
    [Description("LD B, B")]
    LD_B_B = 0x40,
    [Description("LD B, C")]
    LD_B_C = 0x41,
    [Description("LD B, D")]
    LD_B_D = 0x42,
    [Description("LD B, E")]
    LD_B_E = 0x43,
    [Description("LD B, H")]
    LD_B_H = 0x44,
    [Description("LD B, L")]
    LD_B_L = 0x45,
    [Description("LD B, HL")]
    LD_B_ptr_HL = 0x46,
    [Description("LD B, A")]
    LD_B_A = 0x47,
    [Description("LD C, B")]
    LD_C_B = 0x48,
    [Description("LD C, C")]
    LD_C_C = 0x49,
    [Description("LD C, D")]
    LD_C_D = 0x4A,
    [Description("LD C, E")]
    LD_C_E = 0x4B,
    [Description("LD C, H")]
    LD_C_H = 0x4C,
    [Description("LD C, L")]
    LD_C_L = 0x4D,
    [Description("LD C, HL")]
    LD_C_ptr_HL = 0x4E,
    [Description("LD C, A")]
    LD_C_A = 0x4F,
    [Description("LD D, B")]
    LD_D_B = 0x50,
    [Description("LD D, C")]
    LD_D_C = 0x51,
    [Description("LD D, D")]
    LD_D_D = 0x52,
    [Description("LD D, E")]
    LD_D_E = 0x53,
    [Description("LD D, H")]
    LD_D_H = 0x54,
    [Description("LD D, L")]
    LD_D_L = 0x55,
    [Description("LD D, HL")]
    LD_D_ptr_HL = 0x56,
    [Description("LD D, A")]
    LD_D_A = 0x57,
    [Description("LD E, B")]
    LD_E_B = 0x58,
    [Description("LD E, C")]
    LD_E_C = 0x59,
    [Description("LD E, D")]
    LD_E_D = 0x5A,
    [Description("LD E, E")]
    LD_E_E = 0x5B,
    [Description("LD E, H")]
    LD_E_H = 0x5C,
    [Description("LD E, L")]
    LD_E_L = 0x5D,
    [Description("LD E, HL")]
    LD_E_ptr_HL = 0x5E,
    [Description("LD E, A")]
    LD_E_A = 0x5F,
    [Description("LD H, B")]
    LD_H_B = 0x60,
    [Description("LD H, C")]
    LD_H_C = 0x61,
    [Description("LD H, D")]
    LD_H_D = 0x62,
    [Description("LD H, E")]
    LD_H_E = 0x63,
    [Description("LD H, H")]
    LD_H_H = 0x64,
    [Description("LD H, L")]
    LD_H_L = 0x65,
    [Description("LD H, HL")]
    LD_H_ptr_HL = 0x66,
    [Description("LD H, A")]
    LD_H_A = 0x67,
    [Description("LD L, B")]
    LD_L_B = 0x68,
    [Description("LD L, C")]
    LD_L_C = 0x69,
    [Description("LD L, D")]
    LD_L_D = 0x6A,
    [Description("LD L, E")]
    LD_L_E = 0x6B,
    [Description("LD L, H")]
    LD_L_H = 0x6C,
    [Description("LD L, L")]
    LD_L_L = 0x6D,
    [Description("LD L, HL")]
    LD_L_ptr_HL = 0x6E,
    [Description("LD L, A")]
    LD_L_A = 0x6F,
    [Description("LD HL, B")]
    LD_ptr_HL_B = 0x70,
    [Description("LD HL, C")]
    LD_ptr_HL_C = 0x71,
    [Description("LD HL, D")]
    LD_ptr_HL_D = 0x72,
    [Description("LD HL, E")]
    LD_ptr_HL_E = 0x73,
    [Description("LD HL, H")]
    LD_ptr_HL_H = 0x74,
    [Description("LD HL, L")]
    LD_ptr_HL_L = 0x75,
    [Description("HALT")]
    HALT = 0x76,
    [Description("LD HL, A")]
    LD_ptr_HL_A = 0x77,
    [Description("LD A, B")]
    LD_A_B = 0x78,
    [Description("LD A, C")]
    LD_A_C = 0x79,
    [Description("LD A, D")]
    LD_A_D = 0x7A,
    [Description("LD A, E")]
    LD_A_E = 0x7B,
    [Description("LD A, H")]
    LD_A_H = 0x7C,
    [Description("LD A, L")]
    LD_A_L = 0x7D,
    [Description("LD A, HL")]
    LD_A_ptr_HL = 0x7E,
    [Description("LD A, A")]
    LD_A_A = 0x7F,
    [Description("ADD A, B")]
    ADD_A_B = 0x80,
    [Description("ADD A, C")]
    ADD_A_C = 0x81,
    [Description("ADD A, D")]
    ADD_A_D = 0x82,
    [Description("ADD A, E")]
    ADD_A_E = 0x83,
    [Description("ADD A, H")]
    ADD_A_H = 0x84,
    [Description("ADD A, L")]
    ADD_A_L = 0x85,
    [Description("ADD A, HL")]
    ADD_A_ptr_HL = 0x86,
    [Description("ADD A, A")]
    ADD_A_A = 0x87,
    [Description("ADC A, B")]
    ADC_A_B = 0x88,
    [Description("ADC A, C")]
    ADC_A_C = 0x89,
    [Description("ADC A, D")]
    ADC_A_D = 0x8A,
    [Description("ADC A, E")]
    ADC_A_E = 0x8B,
    [Description("ADC A, H")]
    ADC_A_H = 0x8C,
    [Description("ADC A, L")]
    ADC_A_L = 0x8D,
    [Description("ADC A, HL")]
    ADC_A_ptr_HL = 0x8E,
    [Description("ADC A, A")]
    ADC_A_A = 0x8F,
    [Description("SUB A, B")]
    SUB_A_B = 0x90,
    [Description("SUB A, C")]
    SUB_A_C = 0x91,
    [Description("SUB A, D")]
    SUB_A_D = 0x92,
    [Description("SUB A, E")]
    SUB_A_E = 0x93,
    [Description("SUB A, H")]
    SUB_A_H = 0x94,
    [Description("SUB A, L")]
    SUB_A_L = 0x95,
    [Description("SUB A, HL")]
    SUB_A_ptr_HL = 0x96,
    [Description("SUB A, A")]
    SUB_A_A = 0x97,
    [Description("SBC A, B")]
    SBC_A_B = 0x98,
    [Description("SBC A, C")]
    SBC_A_C = 0x99,
    [Description("SBC A, D")]
    SBC_A_D = 0x9A,
    [Description("SBC A, E")]
    SBC_A_E = 0x9B,
    [Description("SBC A, H")]
    SBC_A_H = 0x9C,
    [Description("SBC A, L")]
    SBC_A_L = 0x9D,
    [Description("SBC A, HL")]
    SBC_A_ptr_HL = 0x9E,
    [Description("SBC A, A")]
    SBC_A_A = 0x9F,
    [Description("AND A, B")]
    AND_A_B = 0xA0,
    [Description("AND A, C")]
    AND_A_C = 0xA1,
    [Description("AND A, D")]
    AND_A_D = 0xA2,
    [Description("AND A, E")]
    AND_A_E = 0xA3,
    [Description("AND A, H")]
    AND_A_H = 0xA4,
    [Description("AND A, L")]
    AND_A_L = 0xA5,
    [Description("AND A, HL")]
    AND_A_ptr_HL = 0xA6,
    [Description("AND A, A")]
    AND_A_A = 0xA7,
    [Description("XOR A, B")]
    XOR_A_B = 0xA8,
    [Description("XOR A, C")]
    XOR_A_C = 0xA9,
    [Description("XOR A, D")]
    XOR_A_D = 0xAA,
    [Description("XOR A, E")]
    XOR_A_E = 0xAB,
    [Description("XOR A, H")]
    XOR_A_H = 0xAC,
    [Description("XOR A, L")]
    XOR_A_L = 0xAD,
    [Description("XOR A, HL")]
    XOR_A_ptr_HL = 0xAE,
    [Description("XOR A, A")]
    XOR_A_A = 0xAF,
    [Description("OR A, B")]
    OR_A_B = 0xB0,
    [Description("OR A, C")]
    OR_A_C = 0xB1,
    [Description("OR A, D")]
    OR_A_D = 0xB2,
    [Description("OR A, E")]
    OR_A_E = 0xB3,
    [Description("OR A, H")]
    OR_A_H = 0xB4,
    [Description("OR A, L")]
    OR_A_L = 0xB5,
    [Description("OR A, HL")]
    OR_A_ptr_HL = 0xB6,
    [Description("OR A, A")]
    OR_A_A = 0xB7,
    [Description("CP A, B")]
    CP_A_B = 0xB8,
    [Description("CP A, C")]
    CP_A_C = 0xB9,
    [Description("CP A, D")]
    CP_A_D = 0xBA,
    [Description("CP A, E")]
    CP_A_E = 0xBB,
    [Description("CP A, H")]
    CP_A_H = 0xBC,
    [Description("CP A, L")]
    CP_A_L = 0xBD,
    [Description("CP A, HL")]
    CP_A_ptr_HL = 0xBE,
    [Description("CP A, A")]
    CP_A_A = 0xBF,
    [Description("RET NZ")]
    RET_NZ = 0xC0,
    [Description("POP BC")]
    POP_BC = 0xC1,
    [Description("JP NZ, a16")]
    JP_NZ_a16 = 0xC2,
    [Description("JP a16")]
    JP_a16 = 0xC3,
    [Description("CALL NZ, a16")]
    CALL_NZ_a16 = 0xC4,
    [Description("PUSH BC")]
    PUSH_BC = 0xC5,
    [Description("ADD A, n8")]
    ADD_A_d8 = 0xC6,
    [Description("RST $00")]
    RST_00 = 0xC7,
    [Description("RET Z")]
    RET_Z = 0xC8,
    [Description("RET")]
    RET = 0xC9,
    [Description("JP Z, a16")]
    JP_Z_a16 = 0xCA,
    [Description("PREFIX")]
    PREFIX = 0xCB,
    [Description("CALL Z, a16")]
    CALL_Z_a16 = 0xCC,
    [Description("CALL a16")]
    CALL_a16 = 0xCD,
    [Description("ADC A, n8")]
    ADC_A_d8 = 0xCE,
    [Description("RST $08")]
    RST_08 = 0xCF,
    [Description("RET NC")]
    RET_NC = 0xD0,
    [Description("POP DE")]
    POP_DE = 0xD1,
    [Description("JP NC, a16")]
    JP_NC_a16 = 0xD2,
    [Description("ILLEGAL_D3")]
    ILLEGAL_D3 = 0xD3,
    [Description("CALL NC, a16")]
    CALL_NC_a16 = 0xD4,
    [Description("PUSH DE")]
    PUSH_DE = 0xD5,
    [Description("SUB A, n8")]
    SUB_A_d8 = 0xD6,
    [Description("RST $10")]
    RST_10 = 0xD7,
    [Description("RET C")]
    RET_C = 0xD8,
    [Description("RETI")]
    RETI = 0xD9,
    [Description("JP C, a16")]
    JP_C_a16 = 0xDA,
    [Description("ILLEGAL_DB")]
    ILLEGAL_DB = 0xDB,
    [Description("CALL C, a16")]
    CALL_C_a16 = 0xDC,
    [Description("ILLEGAL_DD")]
    ILLEGAL_DD = 0xDD,
    [Description("SBC A, n8")]
    SBC_A_d8 = 0xDE,
    [Description("RST $18")]
    RST_18 = 0xDF,
    [Description("LDH a8, A")]
    LDH_ptr_a8_A = 0xE0,
    [Description("POP HL")]
    POP_HL = 0xE1,
    [Description("LDH C, A")]
    LDH_ptr_C_A = 0xE2,
    [Description("ILLEGAL_E3")]
    ILLEGAL_E3 = 0xE3,
    [Description("ILLEGAL_E4")]
    ILLEGAL_E4 = 0xE4,
    [Description("PUSH HL")]
    PUSH_HL = 0xE5,
    [Description("AND A, n8")]
    AND_A_d8 = 0xE6,
    [Description("RST $20")]
    RST_20 = 0xE7,
    [Description("ADD SP, e8")]
    ADD_SP_e8 = 0xE8,
    [Description("JP HL")]
    JP_HL = 0xE9,
    [Description("LD a16, A")]
    LD_ptr_a16_A = 0xEA,
    [Description("ILLEGAL_EB")]
    ILLEGAL_EB = 0xEB,
    [Description("ILLEGAL_EC")]
    ILLEGAL_EC = 0xEC,
    [Description("ILLEGAL_ED")]
    ILLEGAL_ED = 0xED,
    [Description("XOR A, n8")]
    XOR_A_d8 = 0xEE,
    [Description("RST $28")]
    RST_28 = 0xEF,
    [Description("LDH A, a8")]
    LDH_A_ptr_a8 = 0xF0,
    [Description("POP AF")]
    POP_AF = 0xF1,
    [Description("LDH A, C")]
    LDH_A_ptr_C = 0xF2,
    [Description("DI")]
    DI = 0xF3,
    [Description("ILLEGAL_F4")]
    ILLEGAL_F4 = 0xF4,
    [Description("PUSH AF")]
    PUSH_AF = 0xF5,
    [Description("OR A, n8")]
    OR_A_d8 = 0xF6,
    [Description("RST $30")]
    RST_30 = 0xF7,
    [Description("LD HL, SP, e8")]
    LD_HL_SP_e8 = 0xF8,
    [Description("LD SP, HL")]
    LD_SP_HL = 0xF9,
    [Description("LD A, a16")]
    LD_A_ptr_a16 = 0xFA,
    [Description("EI")]
    EI = 0xFB,
    [Description("ILLEGAL_FC")]
    ILLEGAL_FC = 0xFC,
    [Description("ILLEGAL_FD")]
    ILLEGAL_FD = 0xFD,
    [Description("CP A, n8")]
    CP_A_d8 = 0xFE,
    [Description("RST $38")]
    RST_38 = 0xFF,
}
