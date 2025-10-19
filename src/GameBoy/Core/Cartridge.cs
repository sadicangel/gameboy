using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using GameBoy.Core.Mbcs;

namespace GameBoy.Core;

[Singleton]
public sealed class Cartridge
{
    private readonly byte[] _rom;
    private readonly IMbc _mbc;
    private readonly ILogger<Cartridge> _logger;
    private CartridgeHeader _header;

    public string FileName { get; }
    public ref CartridgeHeader Header => ref _header;

    public Cartridge(IConfiguration configuration, ILogger<Cartridge> logger)
    {
        _logger = logger;

        FileName = configuration.GetRequiredSection("rom").Value
            ?? throw new InvalidOperationException("Game rom not provided");

        _rom = File.ReadAllBytes(FileName);
        _header = MemoryMarshal.Read<CartridgeHeader>(_rom.AsSpan(0x100..));

        var checksumSucceeded = Checksum(_rom, _header.HeaderChecksum);

        if (_logger.IsEnabled(LogLevel.Information))
        {
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
                _header.Title,
                (byte)_header.CartridgeType, _header.CartridgeType,
                32 << _header.RomSize,
                _header.RamSize,
                (byte)_header.NewLicenseeCode, _header.NewLicenseeCode,
                _header.RomVersion,
                _header.HeaderChecksum, (checksumSucceeded ? "PASS" : "FAIL"));
        }

        if (checksumSucceeded is false)
        {
            throw new InvalidOperationException($"Checksum failed for cartridge '{FileName}'");
        }

        var (ramBytes, ramBankCount) = DecodeRamSize(_header.RamSize);

        Debug.Assert(ramBankCount != 0 || ramBytes == 0, "RAM bytes without RAM banks not supported");

        _mbc = _header.CartridgeType switch
        {
            // No mapper (some carts still have RAM/Battery)
            CartridgeType.ROM_ONLY
                => new Mbc0(_rom),

            CartridgeType.ROM_RAM
                => new Mbc0(_rom),

            CartridgeType.ROM_RAM_BATTERY
                => new Mbc0(_rom),

            // MBC1 family
            CartridgeType.MBC1
                => new Mbc1(_rom, ramBankCount, hasRam: false),

            CartridgeType.MBC1_RAM
                => new Mbc1(_rom, ramBankCount, hasRam: true),

            CartridgeType.MBC1_RAM_BATTERY
                => new Mbc1(_rom, ramBankCount, hasRam: true),

            // MBC2 (fixed internal 512×4-bit RAM; ignore header RAM size)
            CartridgeType.MBC2
                => new Mbc2(_rom),

            CartridgeType.MBC2_BATTERY
                => new Mbc2(_rom),

            //// MMM01 family
            //CartridgeType.MMM01
            //    => new Mmm01(_rom, ramBankCount),

            //CartridgeType.MMM01_RAM
            //    => new Mmm01(_rom, ramBankCount),

            //CartridgeType.MMM01_RAM_BATTERY
            //    => new Mmm01(_rom, ramBankCount),

            // MBC3 family (some with RTC)
            CartridgeType.MBC3
                => new Mbc3(_rom, ramBankCount),

            CartridgeType.MBC3_RAM
                => new Mbc3(_rom, ramBankCount),

            CartridgeType.MBC3_RAM_BATTERY
                => new Mbc3(_rom, ramBankCount),

            CartridgeType.MBC3_TIMER_BATTERY              // RTC, no external RAM
                => new Mbc3(_rom, ramBankCount),

            CartridgeType.MBC3_TIMER_RAM_BATTERY          // RTC + RAM + Battery
                => new Mbc3(_rom, ramBankCount),

            // MBC5 family (some with rumble)
            CartridgeType.MBC5
                => new Mbc5(_rom, ramBankCount),

            CartridgeType.MBC5_RAM
                => new Mbc5(_rom, ramBankCount),

            CartridgeType.MBC5_RAM_BATTERY
                => new Mbc5(_rom, ramBankCount),

            CartridgeType.MBC5_RUMBLE
                => new Mbc5(_rom, ramBankCount),

            CartridgeType.MBC5_RUMBLE_RAM
                => new Mbc5(_rom, ramBankCount),

            CartridgeType.MBC5_RUMBLE_RAM_BATTERY
                => new Mbc5(_rom, ramBankCount),

            //// Rarer mappers / specials
            //CartridgeType.MBC6
            //    => new Mbc6(_rom), // handle its SRAM peculiarities inside

            //CartridgeType.MBC7_SENSOR_RUMBLE_RAM_BATTERY
            //    => new Mbc7(_rom), // accel + rumble + RAM

            //CartridgeType.POCKET_CAMERA
            //    => new PocketCamera(_rom),

            //CartridgeType.BANDAI_TAMA5
            //    => new Tama5(_rom),

            //CartridgeType.HuC3
            //    => new HuC3(_rom, ramBankCount),

            //CartridgeType.HuC1_RAM_BATTERY
            //    => new HuC1(_rom, ramBankCount),

            _ => throw new NotSupportedException($"Unsupported cart type: {_header.CartridgeType}"),
        };

        static bool Checksum(ReadOnlySpan<byte> data, ushort expectedChecksum)
        {
            ushort x = 0;
            for (ushort i = 0x0134; i <= 0x014C; ++i)
                x = (ushort)(x - data[i] - 1);
            return (x & 0xFF) == expectedChecksum;
        }

        static (int ramBytes, int ramBankCount) DecodeRamSize(byte code) => code switch
        {
            0x00 => (0, 0),   // no RAM
            0x01 => (2 * 1024, 0),   // 2KB (mirror within 8KB window)
            0x02 => (8 * 1024, 1),   // 8KB  (1 × 8KB bank)
            0x03 => (32 * 1024, 4),   // 32KB (4 × 8KB)
            0x04 => (128 * 1024, 16),   // 128KB (16 × 8KB)
            0x05 => (64 * 1024, 8),   // 64KB (8 × 8KB)
            _ => (0, 0),
        };
    }

    public byte Read(ushort address) => _mbc.Read(address);
    public void Write(ushort address, byte value) => _mbc.Write(address, value);
}

[StructLayout(LayoutKind.Explicit)]
public struct CartridgeHeader
{
    [FieldOffset(0x00)]
    public EntryPoint EntryPoint;

    [FieldOffset(0x04)]
    public NintendoLogo Logo;

    [FieldOffset(0x34)]
    private AsciiTitle _title;
    public readonly string Title =>
        Encoding.ASCII.GetString(MemoryMarshal.CreateReadOnlySpan(in _title.E0, (CgbFlag is 0x80 or 0xC0) ? 11 : 16).TrimEnd((ReadOnlySpan<byte>)[0, (byte)' ']));

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
    private byte _globalChecksum0;
    [FieldOffset(0x4F)]
    private byte _globalChecksum1;
    public readonly ushort GlobalChecksum => BinaryPrimitives.ReadUInt16BigEndian([_globalChecksum0, _globalChecksum1]);
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
