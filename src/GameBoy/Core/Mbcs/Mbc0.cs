namespace GameBoy.Core.Mbcs;

public sealed class Mbc0(byte[] rom) : IMbc
{
    private readonly byte[] _rom = rom;

    public byte Read(ushort address) => address < 0x8000 ? _rom[address] : (byte)0xFF;

    public void Write(ushort address, byte value)
    {
        // MBC0 does not support writing
    }
}
