namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Apu
{
    private const int LengthClockPeriod = 16_384;

    private int _lengthClockAccumulator;
    private bool _powered = true;
    private bool _channel1Enabled = true;
    private bool _channel2Enabled;
    private int _channel1Length = 1;
    private int _channel2Length;

    private byte _nr10 = 0x80;
    private byte _nr11 = 0xBF;
    private byte _nr12 = 0xF3;
    private byte _nr13 = 0xFF;
    private byte _nr14 = 0xBF;
    private byte _nr21 = 0x3F;
    private byte _nr22;
    private byte _nr23 = 0xFF;
    private byte _nr24 = 0xBF;
    private byte _nr30 = 0x7F;
    private byte _nr31 = 0xFF;
    private byte _nr32 = 0x9F;
    private byte _nr33 = 0xFF;
    private byte _nr34 = 0xBF;
    private byte _nr41 = 0xFF;
    private byte _nr42;
    private byte _nr43;
    private byte _nr44;
    private byte _nr50 = 0x77;
    private byte _nr51 = 0xF3;

    public void Tick(uint cycles)
    {
        if (!_powered)
        {
            return;
        }

        _lengthClockAccumulator += (int)cycles;
        while (_lengthClockAccumulator >= LengthClockPeriod)
        {
            _lengthClockAccumulator -= LengthClockPeriod;
            ClockLengthCounters();
        }
    }

    public byte Read(ushort address)
    {
        return address switch
        {
            0xFF10 => (byte)(_nr10 | 0x80),
            0xFF11 => (byte)(_nr11 | 0x3F),
            0xFF12 => _nr12,
            0xFF13 => 0xFF,
            0xFF14 => (byte)(_nr14 | 0xBF),
            0xFF15 => 0xFF,
            0xFF16 => (byte)(_nr21 | 0x3F),
            0xFF17 => _nr22,
            0xFF18 => 0xFF,
            0xFF19 => (byte)(_nr24 | 0xBF),
            0xFF1A => (byte)(_nr30 | 0x7F),
            0xFF1B => 0xFF,
            0xFF1C => (byte)(_nr32 | 0x9F),
            0xFF1D => 0xFF,
            0xFF1E => (byte)(_nr34 | 0xBF),
            0xFF1F => 0xFF,
            0xFF20 => 0xFF,
            0xFF21 => _nr42,
            0xFF22 => _nr43,
            0xFF23 => (byte)(_nr44 | 0xBF),
            0xFF24 => _nr50,
            0xFF25 => _nr51,
            0xFF26 => (byte)((_powered ? 0x80 : 0x00) | 0x70 | (_channel2Enabled ? 0x02 : 0x00) | (_channel1Enabled ? 0x01 : 0x00)),
            _ => 0xFF,
        };
    }

    public void Write(ushort address, byte value)
    {
        if (address == 0xFF26)
        {
            WriteNR52(value);
            return;
        }

        if (!_powered)
        {
            return;
        }

        switch (address)
        {
            case 0xFF10:
                _nr10 = value;
                break;
            case 0xFF11:
                _nr11 = value;
                _channel1Length = 64 - (value & 0x3F);
                break;
            case 0xFF12:
                _nr12 = value;
                if (!Channel1DacEnabled)
                {
                    _channel1Enabled = false;
                }

                break;
            case 0xFF13:
                _nr13 = value;
                break;
            case 0xFF14:
                _nr14 = value;
                if ((value & 0x80) != 0)
                {
                    TriggerChannel1();
                }

                break;
            case 0xFF16:
                _nr21 = value;
                _channel2Length = 64 - (value & 0x3F);
                break;
            case 0xFF17:
                _nr22 = value;
                if (!Channel2DacEnabled)
                {
                    _channel2Enabled = false;
                }

                break;
            case 0xFF18:
                _nr23 = value;
                break;
            case 0xFF19:
                _nr24 = value;
                if ((value & 0x80) != 0)
                {
                    TriggerChannel2();
                }

                break;
            case 0xFF1A:
                _nr30 = value;
                break;
            case 0xFF1B:
                _nr31 = value;
                break;
            case 0xFF1C:
                _nr32 = value;
                break;
            case 0xFF1D:
                _nr33 = value;
                break;
            case 0xFF1E:
                _nr34 = value;
                break;
            case 0xFF20:
                _nr41 = value;
                break;
            case 0xFF21:
                _nr42 = value;
                break;
            case 0xFF22:
                _nr43 = value;
                break;
            case 0xFF23:
                _nr44 = value;
                break;
            case 0xFF24:
                _nr50 = value;
                break;
            case 0xFF25:
                _nr51 = value;
                break;
        }
    }

    private bool Channel1DacEnabled => (_nr12 & 0xF8) != 0;
    private bool Channel2DacEnabled => (_nr22 & 0xF8) != 0;

    private void WriteNR52(byte value)
    {
        if ((value & 0x80) == 0)
        {
            _powered = false;
            _channel1Enabled = false;
            _channel2Enabled = false;
            _channel1Length = 0;
            _channel2Length = 0;
            _nr10 = 0;
            _nr11 = 0;
            _nr12 = 0;
            _nr13 = 0;
            _nr14 = 0;
            _nr21 = 0;
            _nr22 = 0;
            _nr23 = 0;
            _nr24 = 0;
            _nr30 = 0;
            _nr31 = 0;
            _nr32 = 0;
            _nr33 = 0;
            _nr34 = 0;
            _nr41 = 0;
            _nr42 = 0;
            _nr43 = 0;
            _nr44 = 0;
            _nr50 = 0;
            _nr51 = 0;
            return;
        }

        if (!_powered)
        {
            _powered = true;
            _lengthClockAccumulator = 0;
        }
    }

    private void TriggerChannel1()
    {
        if (_channel1Length == 0)
        {
            _channel1Length = 64;
        }

        _channel1Enabled = Channel1DacEnabled;
    }

    private void TriggerChannel2()
    {
        if (_channel2Length == 0)
        {
            _channel2Length = 64;
        }

        _channel2Enabled = Channel2DacEnabled;
    }

    private void ClockLengthCounters()
    {
        if ((_nr14 & 0x40) != 0 && _channel1Length > 0)
        {
            _channel1Length--;
            if (_channel1Length == 0)
            {
                _channel1Enabled = false;
            }
        }

        if ((_nr24 & 0x40) != 0 && _channel2Length > 0)
        {
            _channel2Length--;
            if (_channel2Length == 0)
            {
                _channel2Enabled = false;
            }
        }
    }
}
