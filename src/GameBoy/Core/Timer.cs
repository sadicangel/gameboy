namespace GameBoy.Core;

[Singleton]
public sealed class Timer(InterruptController interrupts)
{
    private uint _divAcc;
    private byte _divReg;
    private int _timaReloadDelay = -1;
    private byte _tac;

    public byte DIV { get => _divReg; set { _divAcc = 0; _divReg = 0; } }
    public byte TIMA { get; set; }
    public byte TMA { get; set; }
    public byte TAC { get => (byte)(_tac | 0xF8); set => _tac = (byte)(value & 0x07); }

    public void Tick(uint cycles)
    {
        var prevDiv = _divAcc;
        _divAcc += cycles;
        _divReg = (byte)(_divAcc >> 8);

        if (_timaReloadDelay >= 0)
        {
            _timaReloadDelay -= (int)cycles;
            if (_timaReloadDelay <= 0)
            {
                TIMA = TMA;
                interrupts.Request(Interrupts.Timer);
                _timaReloadDelay = -1;
            }
        }

        if ((_tac & 0x04) == 0)
        {
            return;
        }

        var bit = (_tac & 0x03) switch
        {
            0 => 9,
            1 => 3,
            2 => 5,
            _ => 7
        };
        var period = 1 << (bit + 1);
        var edges = (int)(_divAcc / (uint)period) - (int)(prevDiv / (uint)period);

        for (var i = 0; i < edges; i++)
        {
            if (_timaReloadDelay >= 0)
            {
                break;
            }

            if (TIMA == 0xFF)
            {
                TIMA = 0x00;
                _timaReloadDelay = 4;
            }
            else
            {
                TIMA++;
            }
        }
    }
}
