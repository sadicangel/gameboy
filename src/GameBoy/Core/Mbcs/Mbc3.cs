namespace GameBoy.Core.Mbcs;

public sealed class Mbc3(byte[] rom, int ramBankCount) : IMbc
{
    private readonly byte[] _rom = rom;
    private readonly byte[] _ram = ramBankCount > 0 ? new byte[ramBankCount * 0x2000] : [];
    private int _romBank = 1;
    private int _ramBank = 0;
    private bool _ramEnabled = false;

    private byte _rtcS;  //08h RTC S Seconds   0-59 (0-3Bh)
    private byte _rtcM;  //09h RTC M Minutes   0-59 (0-3Bh)
    private byte _rtcH;  //0Ah RTC H Hours     0-23 (0-17h)
    private byte _rtcDL; //0Bh RTC DL Lower 8 bits of Day Counter(0-FFh)
    private byte _rtcDH; //0Ch RTC DH Upper 1 bit of Day Counter, Carry Bit, Halt Flag
    // private byte _rtc0;  //Bit 0  Most significant bit of Day Counter(Bit 8)
    // private byte _rtc6;  //Bit 6  Halt(0=Active, 1=Stop Timer)
    // private byte _rtc7;  //Bit 7  Day Counter Carry Bit(1=Counter Overflow)

    public byte Read(ushort address) => address switch
    {
        < 0x4000 => _rom[address],
        < 0x8000 => _rom[(_romBank * 0x4000) + (address & 0x3FFF)],
        < 0xC000 when _ramEnabled && _ramBank <= 0x03 => _ram[(_ramBank * 0x2000) + (address & 0x1FFF)],
        < 0xC000 when _ramEnabled && _ramBank == 0x08 => _rtcS,
        < 0xC000 when _ramEnabled && _ramBank == 0x09 => _rtcM,
        < 0xC000 when _ramEnabled && _ramBank == 0x0A => _rtcH,
        < 0xC000 when _ramEnabled && _ramBank == 0x0B => _rtcDL,
        < 0xC000 when _ramEnabled && _ramBank == 0x0C => _rtcDH,
        _ => 0xFF,
    };

    public void Write(ushort address, byte value)
    {
        switch (address)
        {
            case < 0x2000:
                // RAM and RTC Enable
                _ramEnabled = (value & 0x0F) == 0x0A;
                break;

            case < 0x4000:
                // ROM Bank Number (7 bits)
                _romBank = value & 0x7F;
                if (_romBank is 0x00)
                    _romBank++; // Skip invalid bank
                break;

            case < 0x6000 when value is (>= 0x00 and <= 0x03) or (>= 0x08 and <= 0xC0):
                // RAM Bank Number or RTC Register Select
                _ramBank = value;
                break;

            case < 0x8000:
                // Latch Clock Data
                var now = DateTime.Now;
                _rtcS = (byte)now.Second;
                _rtcM = (byte)now.Minute;
                _rtcH = (byte)now.Hour;
                break;

            case < 0xC000 when _ramEnabled && _ramBank <= 0x03:
                _ram[(_ramBank * 0x2000) + (address & 0x1FFF)] = value;
                break;

            case < 0xC000 when _ramEnabled && _ramBank == 0x08:
                _rtcS = value;
                break;

            case < 0xC000 when _ramEnabled && _ramBank == 0x09:
                _rtcM = value;
                break;

            case < 0xC000 when _ramEnabled && _ramBank == 0x0A:
                _rtcH = value;
                break;

            case < 0xC000 when _ramEnabled && _ramBank == 0x0B:
                _rtcDL = value;
                break;

            case < 0xC000 when _ramEnabled && _ramBank == 0x0C:
                _rtcDH = value;
                break;

            default:
                // Do nothing
                break;
        }
    }
}
