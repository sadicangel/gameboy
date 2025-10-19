namespace GameBoy.Core.Mbcs;

public sealed class Mbc2(byte[] rom) : IMbc
{
    private readonly byte[] _rom = rom;
    private readonly byte[] _ram = new byte[0x0200]; // 512 x 4 bits
    private bool _ramEnabled;
    private int _romBank = 1;

    public byte Read(ushort address)
    {
        return address switch
        {
            < 0x4000 => _rom[address],
            < 0x8000 => _rom[(_romBank * 0x4000) + (address & 0x3FFF)],
            < 0xA000 when _ramEnabled => _ram[address & 0x1FF],
            _ => 0xFF,
        };

    }

    public void Write(ushort address, byte value)
    {
        switch (address)
        {
            case < 0x2000:
                // RAM Enable
                _ramEnabled = (value & 0x1) == 0x0;
                break;

            case < 0x4000:
                // ROM Bank Number (only lower 4 bits used)
                _romBank = int.Max(1, value & 0xF);
                break;

            case < 0xC000 when _ramEnabled:
                _ram[address & 0x1FF] = value;
                break;

            default:
                // Do nothing
                break;
        }
    }
}
