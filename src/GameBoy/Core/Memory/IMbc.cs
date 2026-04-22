namespace GameBoy.Core.Memory;

public interface IMbc
{
    byte Read(ushort address);
    void Write(ushort address, byte value);

    public static IMbc Create(in CartridgeHeader header, byte[] rom)
    {
        var ramBankCount = RamInfo.GetRamBankCount(header.RamSize);

        return header.CartridgeType switch
        {
            // No mapper (some carts still have RAM/Battery)
            CartridgeType.ROM_ONLY
                => new Mbc0(rom),

            CartridgeType.ROM_RAM
                => new Mbc0(rom),

            CartridgeType.ROM_RAM_BATTERY
                => new Mbc0(rom),

            // MBC1 family
            CartridgeType.MBC1
                => new Mbc1(rom, ramBankCount, hasRam: false),

            CartridgeType.MBC1_RAM
                => new Mbc1(rom, ramBankCount, hasRam: true),

            CartridgeType.MBC1_RAM_BATTERY
                => new Mbc1(rom, ramBankCount, hasRam: true),

            // MBC2 (fixed internal 512×4-bit RAM; ignore header RAM size)
            CartridgeType.MBC2
                => new Mbc2(rom),

            CartridgeType.MBC2_BATTERY
                => new Mbc2(rom),

            //// MMM01 family
            //CartridgeType.MMM01
            //    => new Mmm01(rom, ramBankCount),

            //CartridgeType.MMM01_RAM
            //    => new Mmm01(rom, ramBankCount),

            //CartridgeType.MMM01_RAM_BATTERY
            //    => new Mmm01(rom, ramBankCount),

            // MBC3 family (some with RTC)
            CartridgeType.MBC3
                => new Mbc3(rom, ramBankCount),

            CartridgeType.MBC3_RAM
                => new Mbc3(rom, ramBankCount),

            CartridgeType.MBC3_RAM_BATTERY
                => new Mbc3(rom, ramBankCount),

            CartridgeType.MBC3_TIMER_BATTERY // RTC, no external RAM
                => new Mbc3(rom, ramBankCount),

            CartridgeType.MBC3_TIMER_RAM_BATTERY // RTC + RAM + Battery
                => new Mbc3(rom, ramBankCount),

            // MBC5 family (some with rumble)
            CartridgeType.MBC5
                => new Mbc5(rom, ramBankCount),

            CartridgeType.MBC5_RAM
                => new Mbc5(rom, ramBankCount),

            CartridgeType.MBC5_RAM_BATTERY
                => new Mbc5(rom, ramBankCount),

            CartridgeType.MBC5_RUMBLE
                => new Mbc5(rom, ramBankCount),

            CartridgeType.MBC5_RUMBLE_RAM
                => new Mbc5(rom, ramBankCount),

            CartridgeType.MBC5_RUMBLE_RAM_BATTERY
                => new Mbc5(rom, ramBankCount),

            //// Rarer mappers / specials
            //CartridgeType.MBC6
            //    => new Mbc6(rom), // handle its SRAM peculiarities inside

            //CartridgeType.MBC7_SENSOR_RUMBLE_RAM_BATTERY
            //    => new Mbc7(rom), // accel + rumble + RAM

            //CartridgeType.POCKET_CAMERA
            //    => new PocketCamera(rom),

            //CartridgeType.BANDAI_TAMA5
            //    => new Tama5(rom),

            //CartridgeType.HuC3
            //    => new HuC3(rom, ramBankCount),

            //CartridgeType.HuC1_RAM_BATTERY
            //    => new HuC1(rom, ramBankCount),

            _ => throw new NotSupportedException($"Unsupported cart type: '{header.CartridgeType}'"),
        };
    }
}

file static class RamInfo
{
    public static int GetRamBankCount(int ramSize)
    {
        var (ramBytes, ramBankCount) = ramSize switch
        {
            0x00 => (0, 0), // no RAM
            0x01 => (2 * 1024, 0), // 2KB (mirror within 8KB window)
            0x02 => (8 * 1024, 1), // 8KB  (1 × 8KB bank)
            0x03 => (32 * 1024, 4), // 32KB (4 × 8KB)
            0x04 => (128 * 1024, 16), // 128KB (16 × 8KB)
            0x05 => (64 * 1024, 8), // 64KB (8 × 8KB)
            _ => (0, 0),
        };

        if (ramBytes != 0 && ramBankCount <= 0 && ramSize != 0x01)
        {
            throw new InvalidOperationException($"Unsupported RAM configuration: {ramBytes} bytes, {ramBankCount} banks, size {ramSize}");
        }

        return ramBankCount;
    }
}
