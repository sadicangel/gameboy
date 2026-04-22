using System.Runtime.InteropServices;
using GameBoy.Core.Memory;

namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Cartridge
{
    private readonly CartridgeHeader _header;
    private readonly IMbc _mbc;

    public ref readonly CartridgeHeader Header => ref _header;

    public Cartridge(EmulatorSessionState state, ILogger<Cartridge> logger)
    {
        var rom = File.ReadAllBytes(state.RomPath);

        var header = MemoryMarshal.Read<CartridgeHeader>(rom.AsSpan(0x100..));

        var checksumSucceeded = GameBoyHeaderChecksum.Validate(rom, header.HeaderChecksum);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                """
                Cartridge loaded:
                    Title.........: {Title}.
                    Type..........: {TypeValue} ({TypeName})
                    ROM size......: {RomSize} kB
                    RAM size......: {RamSize} kB
                    Licensee......: {LicenseeCode} ({LicenseeName})
                    ROM version...: {RomVersion}
                    Checksum......: {HeaderChecksum} ({HeaderChecksumTest})
                """,
                header.Title,
                (byte)header.CartridgeType,
                header.CartridgeType,
                32 << header.RomSize,
                header.RamSize,
                (byte)header.NewLicenseeCode,
                header.NewLicenseeCode,
                header.RomVersion,
                header.HeaderChecksum,
                checksumSucceeded ? "PASS" : "FAIL");
        }

        if (!checksumSucceeded)
        {
            throw new InvalidOperationException($"Checksum failed for cartridge '{state.RomPath}'");
        }

        var mbc = IMbc.Create(in header, rom);

        _header = header;
        _mbc = mbc;
    }

    public byte Read(ushort address) => _mbc.Read(address);
    public void Write(ushort address, byte value) => _mbc.Write(address, value);
}

file static class GameBoyHeaderChecksum
{
    public static bool Validate(ReadOnlySpan<byte> data, byte expectedChecksum)
    {
        byte x = 0;
        for (var i = 0x0134; i <= 0x014C; ++i)
            x = (byte)(x - data[i] - 1);

        return x == expectedChecksum;
    }
}
