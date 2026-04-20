namespace GameBoy.Core;

[Singleton]
public sealed class Ppu(InterruptController interrupts)
{
    private int _lineCycles;
    private byte _ly;
    private byte _stat;

    public byte LCDC { get; set; } = 0x91;
    public byte STAT
    {
        get => (byte)((_stat & 0x78) | CurrentMode | (_ly == LYC ? 0x04 : 0x00));
        set => _stat = (byte)(value & 0x78);
    }

    public byte SCY { get; set; }
    public byte SCX { get; set; }
    public byte LY
    {
        get => _ly;
        set
        {
            _ly = 0;
            _lineCycles = 0;
        }
    }
    public byte LYC { get; set; }
    public byte BGP { get; set; } = 0xFC;
    public byte OBP0 { get; set; } = 0xFF;
    public byte OBP1 { get; set; } = 0xFF;
    public byte WY { get; set; }
    public byte WX { get; set; }

    public void Tick(uint cycles)
    {
        if ((LCDC & 0x80) == 0)
        {
            _ly = 0;
            _lineCycles = 0;
            return;
        }

        var remaining = (int)cycles;
        while (remaining > 0)
        {
            var step = Math.Min(remaining, 456 - _lineCycles);
            _lineCycles += step;
            remaining -= step;

            if (_lineCycles < 456)
            {
                continue;
            }

            _lineCycles = 0;
            _ly++;

            if (_ly == 144)
            {
                interrupts.Request(Interrupts.VBlank);
            }
            else if (_ly > 153)
            {
                _ly = 0;
            }
        }
    }

    private byte CurrentMode
        => _ly >= 144
            ? (byte)0x01
            : _lineCycles < 80
                ? (byte)0x02
                : _lineCycles < 252
                    ? (byte)0x03
                    : (byte)0x00;
}
