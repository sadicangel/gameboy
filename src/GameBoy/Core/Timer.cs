using System.Diagnostics;

namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Timer(InterruptController interrupts, SpeedController speedController)
{
    private uint _systemCounter;
    private byte _divReg;
    private int _timaReloadDelay = -1;
    private byte _tac;

    public byte DIV
    {
        get => _divReg;
        set
        {
            _ = value;
            var wasObserved = ObservedInput(_systemCounter, _tac);
            _systemCounter = 0;
            _divReg = 0;
            var nowObserved = ObservedInput(_systemCounter, _tac);

            if (wasObserved && !nowObserved)
            {
                ApplyOneFallingEdgeIfAllowed();
            }
        }
    }

    public byte TIMA { get; set; }
    public byte TMA { get; set; }

    public byte TAC
    {
        get => (byte)(_tac | 0xF8);
        set
        {
            var previousTac = _tac;
            var wasObserved = ObservedInput(_systemCounter, previousTac);
            _tac = (byte)(value & 0x07);
            var nowObserved = ObservedInput(_systemCounter, _tac);

            var disabledTimerOnCgb = speedController.IsCgbMode
                && (previousTac & 0x04) != 0
                && (_tac & 0x04) == 0;

            if (wasObserved && !nowObserved && !disabledTimerOnCgb)
            {
                ApplyOneFallingEdgeIfAllowed();
            }
        }
    }

    public void Tick(uint cycles)
    {
        Debug.Assert((cycles & 0x03) == 0, "Timer expects cycle counts aligned to M-cycles");

        var mCycles = (int)(cycles >> 2);
        for (var i = 0; i < mCycles; i++)
        {
            if (_timaReloadDelay == 0)
            {
                TIMA = TMA;
                interrupts.Request(Interrupts.Timer);
                _timaReloadDelay = -1;
            }

            var wasObserved = ObservedInput(_systemCounter, _tac);
            _systemCounter++;
            _divReg = (byte)(_systemCounter >> 6);
            var nowObserved = ObservedInput(_systemCounter, _tac);

            if (wasObserved && !nowObserved)
            {
                ApplyOneFallingEdgeIfAllowed();
            }

            if (_timaReloadDelay > 0)
            {
                _timaReloadDelay--;
            }
        }
    }

    private static int SelectBitFromTAC(byte tac)
    {
        return (tac & 0x03) switch
        {
            0 => 7,
            1 => 1,
            2 => 3,
            _ => 5
        };
    }

    private static bool ObservedInput(uint divCounterSnapshot, byte tacSnapshot)
    {
        if ((tacSnapshot & 0x04) == 0) return false; // disabled => 0
        var bit = SelectBitFromTAC(tacSnapshot);
        return ((divCounterSnapshot >> bit) & 1) != 0;
    }

    private void ApplyOneFallingEdgeIfAllowed()
    {
        if (_timaReloadDelay >= 0)
        {
            return;
        }

        if (TIMA == 0xFF)
        {
            TIMA = 0x00;
            _timaReloadDelay = 1;
        }
        else
        {
            TIMA++;
        }
    }
}
