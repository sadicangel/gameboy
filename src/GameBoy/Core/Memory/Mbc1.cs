namespace GameBoy.Core.Memory;

public sealed class Mbc1(byte[] rom, int ramBankCount, bool hasRam) : IMbc
{
    private readonly byte[] _rom = rom;
    private readonly byte[] _ram = ramBankCount > 0 ? new byte[ramBankCount * 0x2000] : hasRam ? new byte[0x0800] : [];
    private readonly int _romBankCount = rom.Length / 0x4000;
    private byte _romBankLow = 1;
    private byte _bankHigh;
    private bool _ramEnabled = false;
    private BankingMode _bankingMode = BankingMode.Rom;
    private readonly int _ramAddressMask = ramBankCount == 0 && hasRam ? 0x07FF : 0x1FFF;

    private enum BankingMode : byte { Rom, Ram }

    public byte Read(ushort address) => address switch
    {
        < 0x4000 => _rom[GetFixedRomBank() * 0x4000 + address],
        < 0x8000 => _rom[GetSwitchableRomBank() * 0x4000 + (address & 0x3FFF)],
        < 0xC000 when _ramEnabled && _ram.Length > 0 => _ram[GetRamOffset(address)],
        _ => 0xFF,
    };

    public void Write(ushort address, byte value)
    {
        switch (address)
        {
            case < 0x2000:
                // RAM Enable
                _ramEnabled = (value & 0x0F) == 0x0A;
                break;

            case < 0x4000:
                // ROM Bank Number (lower 5 bits)
                _romBankLow = (byte)(value & 0x1F);
                if (_romBankLow is 0)
                    _romBankLow = 1; // Bank 00 maps to 01 in the switchable area.
                break;

            case < 0x6000:
                _bankHigh = (byte)(value & 0x03);
                break;

            case < 0x8000:
                _bankingMode = (BankingMode)(value & 0x01);
                break;

            case < 0xC000 when _ramEnabled && _ram.Length > 0:
                _ram[GetRamOffset(address)] = value;
                break;

            default:
                // Do nothing
                break;
        }
    }

    private int GetFixedRomBank()
    {
        var bank = _bankingMode is BankingMode.Ram ? _bankHigh << 5 : 0;
        return WrapRomBank(bank);
    }

    private int GetSwitchableRomBank()
    {
        var bank = (_bankHigh << 5) | _romBankLow;
        return WrapRomBank(bank);
    }

    private int GetRamOffset(ushort address)
    {
        var bank = _bankingMode is BankingMode.Ram && ramBankCount > 0 ? _bankHigh % ramBankCount : 0;
        return bank * 0x2000 + (address & _ramAddressMask);
    }

    private int WrapRomBank(int bank) => _romBankCount == 0 ? 0 : bank % _romBankCount;
}
