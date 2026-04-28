using GameBoy.Core.Memory;

namespace GameBoy.Tests;

public sealed class Mbc1Tests
{
    [Fact]
    public void Rom_bank_high_bits_are_shifted_into_bits_5_and_6()
    {
        var mbc = new Mbc1(CreateBankedRom(128), ramBankCount: 0, hasRam: false);

        mbc.Write(0x2000, 0x03);
        mbc.Write(0x4000, 0x02);

        Assert.Equal(0x43, mbc.Read(0x4000));
    }

    [Fact]
    public void Rom_bank_zero_maps_to_one_in_switchable_window()
    {
        var mbc = new Mbc1(CreateBankedRom(128), ramBankCount: 0, hasRam: false);

        mbc.Write(0x2000, 0x00);

        Assert.Equal(0x01, mbc.Read(0x4000));
    }

    [Fact]
    public void Ram_banking_mode_uses_high_bits_for_fixed_rom_window()
    {
        var mbc = new Mbc1(CreateBankedRom(128), ramBankCount: 0, hasRam: false);

        mbc.Write(0x4000, 0x02);
        mbc.Write(0x6000, 0x01);

        Assert.Equal(0x40, mbc.Read(0x0000));
        Assert.Equal(0x41, mbc.Read(0x4000));
    }

    [Fact]
    public void Eight_kib_ram_is_not_mirrored_as_two_kib_ram()
    {
        var mbc = new Mbc1(CreateBankedRom(2), ramBankCount: 1, hasRam: true);

        mbc.Write(0x0000, 0x0A);
        mbc.Write(0xA000, 0x12);
        mbc.Write(0xA800, 0x34);

        Assert.Equal(0x12, mbc.Read(0xA000));
        Assert.Equal(0x34, mbc.Read(0xA800));
    }

    private static byte[] CreateBankedRom(int bankCount)
    {
        var rom = new byte[bankCount * 0x4000];
        for (var bank = 0; bank < bankCount; bank++)
        {
            Array.Fill(rom, (byte)bank, bank * 0x4000, 0x4000);
        }

        return rom;
    }
}
