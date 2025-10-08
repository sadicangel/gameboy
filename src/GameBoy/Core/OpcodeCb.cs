using System.ComponentModel;

namespace GameBoy.Core;

public enum OpcodeCb : byte
{
    [Description("RLC B")]
    RLC_B = 0x00,
    [Description("RLC C")]
    RLC_C = 0x01,
    [Description("RLC D")]
    RLC_D = 0x02,
    [Description("RLC E")]
    RLC_E = 0x03,
    [Description("RLC H")]
    RLC_H = 0x04,
    [Description("RLC L")]
    RLC_L = 0x05,
    [Description("RLC (HL)")]
    RLC_ptr_HL = 0x06,
    [Description("RLC A")]
    RLC_A = 0x07,
    [Description("RRC B")]
    RRC_B = 0x08,
    [Description("RRC C")]
    RRC_C = 0x09,
    [Description("RRC D")]
    RRC_D = 0x0A,
    [Description("RRC E")]
    RRC_E = 0x0B,
    [Description("RRC H")]
    RRC_H = 0x0C,
    [Description("RRC L")]
    RRC_L = 0x0D,
    [Description("RRC (HL)")]
    RRC_ptr_HL = 0x0E,
    [Description("RRC A")]
    RRC_A = 0x0F,
    [Description("RL B")]
    RL_B = 0x10,
    [Description("RL C")]
    RL_C = 0x11,
    [Description("RL D")]
    RL_D = 0x12,
    [Description("RL E")]
    RL_E = 0x13,
    [Description("RL H")]
    RL_H = 0x14,
    [Description("RL L")]
    RL_L = 0x15,
    [Description("RL (HL)")]
    RL_ptr_HL = 0x16,
    [Description("RL A")]
    RL_A = 0x17,
    [Description("RR B")]
    RR_B = 0x18,
    [Description("RR C")]
    RR_C = 0x19,
    [Description("RR D")]
    RR_D = 0x1A,
    [Description("RR E")]
    RR_E = 0x1B,
    [Description("RR H")]
    RR_H = 0x1C,
    [Description("RR L")]
    RR_L = 0x1D,
    [Description("RR (HL)")]
    RR_ptr_HL = 0x1E,
    [Description("RR A")]
    RR_A = 0x1F,
    [Description("SLA B")]
    SLA_B = 0x20,
    [Description("SLA C")]
    SLA_C = 0x21,
    [Description("SLA D")]
    SLA_D = 0x22,
    [Description("SLA E")]
    SLA_E = 0x23,
    [Description("SLA H")]
    SLA_H = 0x24,
    [Description("SLA L")]
    SLA_L = 0x25,
    [Description("SLA (HL)")]
    SLA_ptr_HL = 0x26,
    [Description("SLA A")]
    SLA_A = 0x27,
    [Description("SRA B")]
    SRA_B = 0x28,
    [Description("SRA C")]
    SRA_C = 0x29,
    [Description("SRA D")]
    SRA_D = 0x2A,
    [Description("SRA E")]
    SRA_E = 0x2B,
    [Description("SRA H")]
    SRA_H = 0x2C,
    [Description("SRA L")]
    SRA_L = 0x2D,
    [Description("SRA (HL)")]
    SRA_ptr_HL = 0x2E,
    [Description("SRA A")]
    SRA_A = 0x2F,
    [Description("SWAP B")]
    SWAP_B = 0x30,
    [Description("SWAP C")]
    SWAP_C = 0x31,
    [Description("SWAP D")]
    SWAP_D = 0x32,
    [Description("SWAP E")]
    SWAP_E = 0x33,
    [Description("SWAP H")]
    SWAP_H = 0x34,
    [Description("SWAP L")]
    SWAP_L = 0x35,
    [Description("SWAP (HL)")]
    SWAP_ptr_HL = 0x36,
    [Description("SWAP A")]
    SWAP_A = 0x37,
    [Description("SRL B")]
    SRL_B = 0x38,
    [Description("SRL C")]
    SRL_C = 0x39,
    [Description("SRL D")]
    SRL_D = 0x3A,
    [Description("SRL E")]
    SRL_E = 0x3B,
    [Description("SRL H")]
    SRL_H = 0x3C,
    [Description("SRL L")]
    SRL_L = 0x3D,
    [Description("SRL (HL)")]
    SRL_ptr_HL = 0x3E,
    [Description("SRL A")]
    SRL_A = 0x3F,
    [Description("BIT 0, B")]
    BIT_0_B = 0x40,
    [Description("BIT 0, C")]
    BIT_0_C = 0x41,
    [Description("BIT 0, D")]
    BIT_0_D = 0x42,
    [Description("BIT 0, E")]
    BIT_0_E = 0x43,
    [Description("BIT 0, H")]
    BIT_0_H = 0x44,
    [Description("BIT 0, L")]
    BIT_0_L = 0x45,
    [Description("BIT 0, (HL)")]
    BIT_0_ptr_HL = 0x46,
    [Description("BIT 0, A")]
    BIT_0_A = 0x47,
    [Description("BIT 1, B")]
    BIT_1_B = 0x48,
    [Description("BIT 1, C")]
    BIT_1_C = 0x49,
    [Description("BIT 1, D")]
    BIT_1_D = 0x4A,
    [Description("BIT 1, E")]
    BIT_1_E = 0x4B,
    [Description("BIT 1, H")]
    BIT_1_H = 0x4C,
    [Description("BIT 1, L")]
    BIT_1_L = 0x4D,
    [Description("BIT 1, (HL)")]
    BIT_1_ptr_HL = 0x4E,
    [Description("BIT 1, A")]
    BIT_1_A = 0x4F,
    [Description("BIT 2, B")]
    BIT_2_B = 0x50,
    [Description("BIT 2, C")]
    BIT_2_C = 0x51,
    [Description("BIT 2, D")]
    BIT_2_D = 0x52,
    [Description("BIT 2, E")]
    BIT_2_E = 0x53,
    [Description("BIT 2, H")]
    BIT_2_H = 0x54,
    [Description("BIT 2, L")]
    BIT_2_L = 0x55,
    [Description("BIT 2, (HL)")]
    BIT_2_ptr_HL = 0x56,
    [Description("BIT 2, A")]
    BIT_2_A = 0x57,
    [Description("BIT 3, B")]
    BIT_3_B = 0x58,
    [Description("BIT 3, C")]
    BIT_3_C = 0x59,
    [Description("BIT 3, D")]
    BIT_3_D = 0x5A,
    [Description("BIT 3, E")]
    BIT_3_E = 0x5B,
    [Description("BIT 3, H")]
    BIT_3_H = 0x5C,
    [Description("BIT 3, L")]
    BIT_3_L = 0x5D,
    [Description("BIT 3, (HL)")]
    BIT_3_ptr_HL = 0x5E,
    [Description("BIT 3, A")]
    BIT_3_A = 0x5F,
    [Description("BIT 4, B")]
    BIT_4_B = 0x60,
    [Description("BIT 4, C")]
    BIT_4_C = 0x61,
    [Description("BIT 4, D")]
    BIT_4_D = 0x62,
    [Description("BIT 4, E")]
    BIT_4_E = 0x63,
    [Description("BIT 4, H")]
    BIT_4_H = 0x64,
    [Description("BIT 4, L")]
    BIT_4_L = 0x65,
    [Description("BIT 4, (HL)")]
    BIT_4_ptr_HL = 0x66,
    [Description("BIT 4, A")]
    BIT_4_A = 0x67,
    [Description("BIT 5, B")]
    BIT_5_B = 0x68,
    [Description("BIT 5, C")]
    BIT_5_C = 0x69,
    [Description("BIT 5, D")]
    BIT_5_D = 0x6A,
    [Description("BIT 5, E")]
    BIT_5_E = 0x6B,
    [Description("BIT 5, H")]
    BIT_5_H = 0x6C,
    [Description("BIT 5, L")]
    BIT_5_L = 0x6D,
    [Description("BIT 5, (HL)")]
    BIT_5_ptr_HL = 0x6E,
    [Description("BIT 5, A")]
    BIT_5_A = 0x6F,
    [Description("BIT 6, B")]
    BIT_6_B = 0x70,
    [Description("BIT 6, C")]
    BIT_6_C = 0x71,
    [Description("BIT 6, D")]
    BIT_6_D = 0x72,
    [Description("BIT 6, E")]
    BIT_6_E = 0x73,
    [Description("BIT 6, H")]
    BIT_6_H = 0x74,
    [Description("BIT 6, L")]
    BIT_6_L = 0x75,
    [Description("BIT 6, (HL)")]
    BIT_6_ptr_HL = 0x76,
    [Description("BIT 6, A")]
    BIT_6_A = 0x77,
    [Description("BIT 7, B")]
    BIT_7_B = 0x78,
    [Description("BIT 7, C")]
    BIT_7_C = 0x79,
    [Description("BIT 7, D")]
    BIT_7_D = 0x7A,
    [Description("BIT 7, E")]
    BIT_7_E = 0x7B,
    [Description("BIT 7, H")]
    BIT_7_H = 0x7C,
    [Description("BIT 7, L")]
    BIT_7_L = 0x7D,
    [Description("BIT 7, (HL)")]
    BIT_7_ptr_HL = 0x7E,
    [Description("BIT 7, A")]
    BIT_7_A = 0x7F,
    [Description("RES 0, B")]
    RES_0_B = 0x80,
    [Description("RES 0, C")]
    RES_0_C = 0x81,
    [Description("RES 0, D")]
    RES_0_D = 0x82,
    [Description("RES 0, E")]
    RES_0_E = 0x83,
    [Description("RES 0, H")]
    RES_0_H = 0x84,
    [Description("RES 0, L")]
    RES_0_L = 0x85,
    [Description("RES 0, (HL)")]
    RES_0_ptr_HL = 0x86,
    [Description("RES 0, A")]
    RES_0_A = 0x87,
    [Description("RES 1, B")]
    RES_1_B = 0x88,
    [Description("RES 1, C")]
    RES_1_C = 0x89,
    [Description("RES 1, D")]
    RES_1_D = 0x8A,
    [Description("RES 1, E")]
    RES_1_E = 0x8B,
    [Description("RES 1, H")]
    RES_1_H = 0x8C,
    [Description("RES 1, L")]
    RES_1_L = 0x8D,
    [Description("RES 1, (HL)")]
    RES_1_ptr_HL = 0x8E,
    [Description("RES 1, A")]
    RES_1_A = 0x8F,
    [Description("RES 2, B")]
    RES_2_B = 0x90,
    [Description("RES 2, C")]
    RES_2_C = 0x91,
    [Description("RES 2, D")]
    RES_2_D = 0x92,
    [Description("RES 2, E")]
    RES_2_E = 0x93,
    [Description("RES 2, H")]
    RES_2_H = 0x94,
    [Description("RES 2, L")]
    RES_2_L = 0x95,
    [Description("RES 2, (HL)")]
    RES_2_ptr_HL = 0x96,
    [Description("RES 2, A")]
    RES_2_A = 0x97,
    [Description("RES 3, B")]
    RES_3_B = 0x98,
    [Description("RES 3, C")]
    RES_3_C = 0x99,
    [Description("RES 3, D")]
    RES_3_D = 0x9A,
    [Description("RES 3, E")]
    RES_3_E = 0x9B,
    [Description("RES 3, H")]
    RES_3_H = 0x9C,
    [Description("RES 3, L")]
    RES_3_L = 0x9D,
    [Description("RES 3, (HL)")]
    RES_3_ptr_HL = 0x9E,
    [Description("RES 3, A")]
    RES_3_A = 0x9F,
    [Description("RES 4, B")]
    RES_4_B = 0xA0,
    [Description("RES 4, C")]
    RES_4_C = 0xA1,
    [Description("RES 4, D")]
    RES_4_D = 0xA2,
    [Description("RES 4, E")]
    RES_4_E = 0xA3,
    [Description("RES 4, H")]
    RES_4_H = 0xA4,
    [Description("RES 4, L")]
    RES_4_L = 0xA5,
    [Description("RES 4, (HL)")]
    RES_4_ptr_HL = 0xA6,
    [Description("RES 4, A")]
    RES_4_A = 0xA7,
    [Description("RES 5, B")]
    RES_5_B = 0xA8,
    [Description("RES 5, C")]
    RES_5_C = 0xA9,
    [Description("RES 5, D")]
    RES_5_D = 0xAA,
    [Description("RES 5, E")]
    RES_5_E = 0xAB,
    [Description("RES 5, H")]
    RES_5_H = 0xAC,
    [Description("RES 5, L")]
    RES_5_L = 0xAD,
    [Description("RES 5, (HL)")]
    RES_5_ptr_HL = 0xAE,
    [Description("RES 5, A")]
    RES_5_A = 0xAF,
    [Description("RES 6, B")]
    RES_6_B = 0xB0,
    [Description("RES 6, C")]
    RES_6_C = 0xB1,
    [Description("RES 6, D")]
    RES_6_D = 0xB2,
    [Description("RES 6, E")]
    RES_6_E = 0xB3,
    [Description("RES 6, H")]
    RES_6_H = 0xB4,
    [Description("RES 6, L")]
    RES_6_L = 0xB5,
    [Description("RES 6, (HL)")]
    RES_6_ptr_HL = 0xB6,
    [Description("RES 6, A")]
    RES_6_A = 0xB7,
    [Description("RES 7, B")]
    RES_7_B = 0xB8,
    [Description("RES 7, C")]
    RES_7_C = 0xB9,
    [Description("RES 7, D")]
    RES_7_D = 0xBA,
    [Description("RES 7, E")]
    RES_7_E = 0xBB,
    [Description("RES 7, H")]
    RES_7_H = 0xBC,
    [Description("RES 7, L")]
    RES_7_L = 0xBD,
    [Description("RES 7, (HL)")]
    RES_7_ptr_HL = 0xBE,
    [Description("RES 7, A")]
    RES_7_A = 0xBF,
    [Description("SET 0, B")]
    SET_0_B = 0xC0,
    [Description("SET 0, C")]
    SET_0_C = 0xC1,
    [Description("SET 0, D")]
    SET_0_D = 0xC2,
    [Description("SET 0, E")]
    SET_0_E = 0xC3,
    [Description("SET 0, H")]
    SET_0_H = 0xC4,
    [Description("SET 0, L")]
    SET_0_L = 0xC5,
    [Description("SET 0, (HL)")]
    SET_0_ptr_HL = 0xC6,
    [Description("SET 0, A")]
    SET_0_A = 0xC7,
    [Description("SET 1, B")]
    SET_1_B = 0xC8,
    [Description("SET 1, C")]
    SET_1_C = 0xC9,
    [Description("SET 1, D")]
    SET_1_D = 0xCA,
    [Description("SET 1, E")]
    SET_1_E = 0xCB,
    [Description("SET 1, H")]
    SET_1_H = 0xCC,
    [Description("SET 1, L")]
    SET_1_L = 0xCD,
    [Description("SET 1, (HL)")]
    SET_1_ptr_HL = 0xCE,
    [Description("SET 1, A")]
    SET_1_A = 0xCF,
    [Description("SET 2, B")]
    SET_2_B = 0xD0,
    [Description("SET 2, C")]
    SET_2_C = 0xD1,
    [Description("SET 2, D")]
    SET_2_D = 0xD2,
    [Description("SET 2, E")]
    SET_2_E = 0xD3,
    [Description("SET 2, H")]
    SET_2_H = 0xD4,
    [Description("SET 2, L")]
    SET_2_L = 0xD5,
    [Description("SET 2, (HL)")]
    SET_2_ptr_HL = 0xD6,
    [Description("SET 2, A")]
    SET_2_A = 0xD7,
    [Description("SET 3, B")]
    SET_3_B = 0xD8,
    [Description("SET 3, C")]
    SET_3_C = 0xD9,
    [Description("SET 3, D")]
    SET_3_D = 0xDA,
    [Description("SET 3, E")]
    SET_3_E = 0xDB,
    [Description("SET 3, H")]
    SET_3_H = 0xDC,
    [Description("SET 3, L")]
    SET_3_L = 0xDD,
    [Description("SET 3, (HL)")]
    SET_3_ptr_HL = 0xDE,
    [Description("SET 3, A")]
    SET_3_A = 0xDF,
    [Description("SET 4, B")]
    SET_4_B = 0xE0,
    [Description("SET 4, C")]
    SET_4_C = 0xE1,
    [Description("SET 4, D")]
    SET_4_D = 0xE2,
    [Description("SET 4, E")]
    SET_4_E = 0xE3,
    [Description("SET 4, H")]
    SET_4_H = 0xE4,
    [Description("SET 4, L")]
    SET_4_L = 0xE5,
    [Description("SET 4, (HL)")]
    SET_4_ptr_HL = 0xE6,
    [Description("SET 4, A")]
    SET_4_A = 0xE7,
    [Description("SET 5, B")]
    SET_5_B = 0xE8,
    [Description("SET 5, C")]
    SET_5_C = 0xE9,
    [Description("SET 5, D")]
    SET_5_D = 0xEA,
    [Description("SET 5, E")]
    SET_5_E = 0xEB,
    [Description("SET 5, H")]
    SET_5_H = 0xEC,
    [Description("SET 5, L")]
    SET_5_L = 0xED,
    [Description("SET 5, (HL)")]
    SET_5_ptr_HL = 0xEE,
    [Description("SET 5, A")]
    SET_5_A = 0xEF,
    [Description("SET 6, B")]
    SET_6_B = 0xF0,
    [Description("SET 6, C")]
    SET_6_C = 0xF1,
    [Description("SET 6, D")]
    SET_6_D = 0xF2,
    [Description("SET 6, E")]
    SET_6_E = 0xF3,
    [Description("SET 6, H")]
    SET_6_H = 0xF4,
    [Description("SET 6, L")]
    SET_6_L = 0xF5,
    [Description("SET 6, (HL)")]
    SET_6_ptr_HL = 0xF6,
    [Description("SET 6, A")]
    SET_6_A = 0xF7,
    [Description("SET 7, B")]
    SET_7_B = 0xF8,
    [Description("SET 7, C")]
    SET_7_C = 0xF9,
    [Description("SET 7, D")]
    SET_7_D = 0xFA,
    [Description("SET 7, E")]
    SET_7_E = 0xFB,
    [Description("SET 7, H")]
    SET_7_H = 0xFC,
    [Description("SET 7, L")]
    SET_7_L = 0xFD,
    [Description("SET 7, (HL)")]
    SET_7_ptr_HL = 0xFE,
    [Description("SET 7, A")]
    SET_7_A = 0xFF,
}
