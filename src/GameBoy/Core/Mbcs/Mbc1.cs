namespace GameBoy.Core.Mbcs;

public sealed class Mbc1(byte[] rom, int ramBankCount, bool hasRam) : IMbc
{
    private readonly byte[] _rom = rom;
    private readonly byte[] _ram = ramBankCount > 0 ? new byte[ramBankCount * 0x2000] : hasRam ? new byte[0x0800] : [];
    private int _romBank = 1;
    private int _ramBank = 0;
    private bool _ramEnabled = false;
    private BankingMode _bankingMode = BankingMode.Rom;
    private int _physicalOffset = 0x1FFF & (hasRam ? 0x07FF : 0x1FFF);

    private enum BankingMode : byte { Rom, Ram }

    public byte Read(ushort address) => address switch
    {
        < 0x4000 => _rom[address],
        < 0x8000 => _rom[(_romBank * 0x4000) + (address & 0x3FFF)],
        < 0xC000 when _ramEnabled => _ram[(_ramBank * 0x2000) + (address & _physicalOffset)],
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
                _romBank = value & 0x1F;
                if (_romBank is 0x00 or 0x20 or 0x40 or 0x60)
                    _romBank++; // Skip invalid banks
                break;

            case < 0x6000 when _bankingMode is BankingMode.Rom:
                _romBank |= value & 0x03;
                if (_romBank is 0x00 or 0x20 or 0x40 or 0x60)
                    _romBank++; // Skip invalid banks
                break;

            case < 0x6000:
                _ramBank = value & 0x03;
                break;

            case < 0x8000:
                _bankingMode = (BankingMode)(value & 0x01);
                break;

            case < 0xC000 when _ramEnabled:
                _ram[(_ramBank * 0x2000) + (address & _physicalOffset)] = value;
                break;

            default:
                // Do nothing
                break;
        }
    }
}
