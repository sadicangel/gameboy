namespace GameBoy.Core.Mbcs;

public sealed class Mbc5(byte[] rom, int ramBankCount) : IMbc
{
    private readonly byte[] _rom = rom;
    private readonly byte[] _ram = ramBankCount > 0 ? new byte[ramBankCount * 0x2000] : [];
    private bool _ramEnabled = false;
    private int _romBankHi = 0;
    private int _romBankLo = 1;
    private int _ramBank = 0;

    public byte Read(ushort address) => address switch
    {
        < 0x4000 => _rom[address],
        < 0x8000 => _rom[(_romBankHi + _romBankLo) * 0x4000 + (address & 0x3FFF)],
        < 0xC000 when _ramEnabled => _ram[(_ramBank * 0x2000) + (address & 0x1FFF)],
        _ => 0xFF,
    };

    public void Write(ushort address, byte value)
    {
        switch (address)
        {
            case < 0x2000:
                _ramEnabled = (value & 0x0F) == 0x0A;
                break;

            case < 0x3000:
                _romBankLo = value;
                break;

            case < 0x4000:
                _romBankHi = (value & 0x01) << 8;
                break;

            case < 0x6000:
                _ramBank = value & 0x0F;
                break;

            case < 0xC000 when _ramEnabled:
                _ram[(_ramBank * 0x2000) + (address & 0x1FFF)] = value;
                break;
        }
    }
}
