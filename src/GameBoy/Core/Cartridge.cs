using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace GameBoy.Core;

[Singleton]
public sealed class Cartridge
{
    private readonly byte[] _data;
    private readonly ILogger<Cartridge> _logger;
    private CartridgeHeader _header;

    public string FileName { get; }
    public ReadOnlySpan<byte> Data => _data;
    public uint Size => (uint)_data.Length;
    public ref CartridgeHeader Header => ref _header;

    public Cartridge(IConfiguration configuration, ILogger<Cartridge> logger)
    {
        _logger = logger;

        FileName = configuration.GetRequiredSection("rom").Value
            ?? throw new InvalidOperationException("Game rom not provided");

        _data = File.ReadAllBytes(FileName);
        _header = MemoryMarshal.Read<CartridgeHeader>(Data[0x100..]);
        _header.Title[15] = 0;

        var checksumSucceeded = Checksum(_data);

        _logger.LogInformation("""
            Cartridge loaded:
                Title.........: {Title}.
                Type..........: {TypeValue} ({TypeName})
                ROM size......: {RomSize} kB
                RAM size......: {RamSize} kB
                Licensee......: {LicenseeCode} ({LicenseeName})
                ROM version...: {RomVersion}
                Checksum......: {HeaderChecksum} ({HeaderChecksumTest})
            """,
            Encoding.ASCII.GetString(_header.Title),
            (byte)_header.CartridgeType, _header.CartridgeType,
            32 << _header.RomSize,
            _header.RamSize,
            (byte)_header.NewLicenseeCode, _header.NewLicenseeCode,
            _header.RomVersion,
            _header.HeaderChecksum, (checksumSucceeded ? "PASS" : "FAIL"));

        if (checksumSucceeded is false)
        {
            throw new InvalidOperationException($"Checksum failed for cartridge '{FileName}'");
        }

        static bool Checksum(ReadOnlySpan<byte> data)
        {
            ushort x = 0;
            for (ushort i = 0x0134; i <= 0x014C; ++i)
                x = (ushort)(x - data[i] - 1);
            return (x & 0xFF) != 0;
        }
    }

    public byte Read(ushort address)
    {
        var value = _data[address];
        _logger.LogDebug("'{Address:X4}' -> '{Value:X2}'", address, value);
        return value;
    }


    public void Write(ushort address, byte value)
    {
        _logger.LogDebug("'{Address:X4}' <- '{Value:X2}'", address, value);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct CartridgeHeader
{
    [FieldOffset(0x00)]
    public EntryPoint EntryPoint;

    [FieldOffset(0x04)]
    public NintendoLogo Logo;

    [FieldOffset(0x34)]
    public AsciiTitle Title;

    [FieldOffset(0x3F)]
    public ManufacturerCode ManufacturerCode;

    [FieldOffset(0x43)]
    public byte CgbFlag;

    [FieldOffset(0x44)]
    public NewLicenseeCode NewLicenseeCode;

    [FieldOffset(0x46)]
    public byte SgbFlag;

    [FieldOffset(0x47)]
    public CartridgeType CartridgeType;

    [FieldOffset(0x48)]
    public byte RomSize;

    [FieldOffset(0x49)]
    public byte RamSize;

    [FieldOffset(0x4A)]
    public byte DestinationCode;

    [FieldOffset(0x4B)]
    public byte OldLicenseeCode;

    [FieldOffset(0x4C)]
    public byte RomVersion;

    [FieldOffset(0x4D)]
    public byte HeaderChecksum;

    [FieldOffset(0x4E)]
    public ushort GlobalChecksum;
}

[InlineArray(4)] public struct EntryPoint { public byte E0; }
[InlineArray(48)] public struct NintendoLogo { public byte E0; }
[InlineArray(16)] public struct AsciiTitle { public byte E0; }
[InlineArray(16)] public struct ManufacturerCode { public byte E0; }


public enum CartridgeType : byte
{
    ROM_ONLY = 0x00,
    MBC1 = 0x01,
    MBC1_RAM = 0x02,
    MBC1_RAM_BATTERY = 0x03,
    MBC2 = 0x05,
    MBC2_BATTERY = 0x06,
    ROM_RAM = 0x08,
    ROM_RAM_BATTERY = 0x09,
    MMM01 = 0x0B,
    MMM01_RAM = 0x0C,
    MMM01_RAM_BATTERY = 0x0D,
    MBC3_TIMER_BATTERY = 0x0F,
    MBC3_TIMER_RAM_BATTERY = 0x10,
    MBC3 = 0x11,
    MBC3_RAM = 0x12,
    MBC3_RAM_BATTERY = 0x13,
    MBC5 = 0x19,
    MBC5_RAM = 0x1A,
    MBC5_RAM_BATTERY = 0x1B,
    MBC5_RUMBLE = 0x1C,
    MBC5_RUMBLE_RAM = 0x1D,
    MBC5_RUMBLE_RAM_BATTERY = 0x1E,
    MBC6 = 0x20,
    MBC7_SENSOR_RUMBLE_RAM_BATTERY = 0x22,
    POCKET_CAMERA = 0xFC,
    BANDAI_TAMA5 = 0xFD,
    HuC3 = 0xFE,
    HuC1_RAM_BATTERY = 0xFF,
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
