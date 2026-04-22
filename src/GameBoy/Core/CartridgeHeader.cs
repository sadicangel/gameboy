using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace GameBoy.Core;

[StructLayout(LayoutKind.Explicit)]
public struct CartridgeHeader
{
    [FieldOffset(0x00)] public EntryPoint EntryPoint;

    [FieldOffset(0x04)] public NintendoLogo Logo;

    [FieldOffset(0x34)] private AsciiTitle _title;

    public readonly string Title =>
        Encoding.ASCII.GetString(MemoryMarshal.CreateReadOnlySpan(in _title.E0, CgbFlag is 0x80 or 0xC0 ? 11 : 16).TrimEnd("\0 "u8));

    [FieldOffset(0x3F)] public ManufacturerCode ManufacturerCode;

    [FieldOffset(0x43)] public byte CgbFlag;

    [FieldOffset(0x44)] public NewLicenseeCode NewLicenseeCode;

    [FieldOffset(0x46)] public byte SgbFlag;

    [FieldOffset(0x47)] public CartridgeType CartridgeType;

    [FieldOffset(0x48)] public byte RomSize;

    [FieldOffset(0x49)] public byte RamSize;

    [FieldOffset(0x4A)] public byte DestinationCode;

    [FieldOffset(0x4B)] public byte OldLicenseeCode;

    [FieldOffset(0x4C)] public byte RomVersion;

    [FieldOffset(0x4D)] public byte HeaderChecksum;

    [FieldOffset(0x4E)] private byte _globalChecksum0;
    [FieldOffset(0x4F)] private byte _globalChecksum1;
    public readonly ushort GlobalChecksum => BinaryPrimitives.ReadUInt16BigEndian([_globalChecksum0, _globalChecksum1]);
}

[InlineArray(4)]
public struct EntryPoint
{
    public byte E0;
}

[InlineArray(48)]
public struct NintendoLogo
{
    public byte E0;
}

[InlineArray(16)]
public struct AsciiTitle
{
    public byte E0;
}

[InlineArray(16)]
public struct ManufacturerCode
{
    public byte E0;
}

public enum NewLicenseeCode : ushort
{
    None = 0x00,
    NintendoResearchAndDevelopment1 = 0x01,
    Capcom = 0x08,
    EA = 0x13,
    HudsonSoft = 0x18,
    BAI = 0x19,
    KSS = 0x20,
    PlanningOfficeWADA = 0x22,
    PCMComplete = 0x24,
    SanX = 0x25,
    Kemco = 0x28,
    SETACorporation = 0x29,
    Viacom = 0x30,
    Nintendo = 0x31,
    Bandai = 0x32,
    OceanSoftwareAcclaimEntertainment = 0x33,
    Konami = 0x34,
    HectorSoft = 0x35,
    Taito = 0x37,
    HudsonSoft2 = 0x38,
    Banpresto = 0x39,
    UbiSoft = 0x41,
    Atlus = 0x42,
    MalibuInteractive = 0x44,
    Angel = 0x46,
    BulletProofSoftware = 0x47,
    Irem = 0x49,
    Absolute = 0x50,
    AcclaimEntertainment = 0x51,
    Activision = 0x52,
    SammyUSA = 0x53,
    Konami2 = 0x54,
    HiTechExpressions = 0x55,
    LJN = 0x56,
    Matchbox = 0x57,
    Mattel = 0x58,
    MiltonBradleyCompany = 0x59,
    TitusInteractive = 0x60,
    VirginGamesLtd = 0x61,
    LucasfilmGames = 0x64,
    OceanSoftware = 0x67,
    EA2 = 0x69,
    Infogrames = 0x70,
    InterplayEntertainment = 0x71,
    Broderbund = 0x72,
    SculpturedSoftware = 0x73,
    TheSalesCurveLimited = 0x75,
    THQ = 0x78,
    Accolade = 0x79,
    MisawaEntertainment = 0x80,
    lozc = 0x83,
    TokumaShoten = 0x86,
    TsukudaOriginal = 0x87,
    ChunsoftCo = 0x91,
    VideoSystem = 0x92,
    OceanSoftwareAcclaimEntertainment2 = 0x93,
    Varie = 0x95,
    YonezawaSpal = 0x96,
    Kaneko = 0x97,
    PackInVideo = 0x99,
    BottomUp = 0x9A, // 9H is not a valid hex, using 0x9A
    KonamiYuGiOh = 0xA4,
    MTO = 0xB1, // BL is not a valid hex, using 0xB1
    Kodansha = 0xD0 // DK is not a valid hex, using 0xD0
}
