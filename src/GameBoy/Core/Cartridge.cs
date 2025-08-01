namespace GameBoy.Core;

public readonly struct Cartridge
{
    private readonly byte[] _bytes = new byte[0x200000];

    public readonly byte this[int address] { get => _bytes[address]; set => _bytes[address] = value; }

    public readonly bool IsMbc1 => _bytes[0x147] is 0x01 or 0x02 or 0x03;
    public readonly bool IsMbc2 => _bytes[0x147] is 0x05 or 0x06;
    public readonly int RamBanksCount => _bytes[0x148];

    public Cartridge() { }
}
