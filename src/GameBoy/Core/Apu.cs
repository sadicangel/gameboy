using GameBoy.Runtime;

namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Apu
{
    public const int AudioSampleRate = 48_000;
    public const int AudioChannelCount = 2;

    private const int CpuFrequency = 4_194_304;
    private const int FrameSequencerPeriod = 8_192;
    private const float MixedChannelScale = 0.25f;

    private static readonly double[] s_squareDuty =
    [
        0.125,
        0.25,
        0.5,
        0.75,
    ];

    private readonly byte[] _waveRam = new byte[16];
    private readonly List<float> _samples = [];

    private long _sampleClockAccumulator;
    private int _frameSequencerAccumulator;
    private int _frameSequencerStep;

    private bool _powered = true;
    private bool _channel1Enabled;
    private bool _channel2Enabled;
    private bool _channel3Enabled;
    private bool _channel4Enabled;

    private int _channel1Length;
    private int _channel2Length;
    private int _channel3Length;
    private int _channel4Length;

    private double _channel1Phase;
    private double _channel2Phase;
    private double _channel3Phase;
    private double _channel4Phase;
    private float _leftHighPassInput;
    private float _leftHighPassOutput;
    private float _rightHighPassInput;
    private float _rightHighPassOutput;
    private ushort _channel4Lfsr = 0x7FFF;

    private int _channel1Volume = 15;
    private int _channel2Volume;
    private int _channel4Volume;
    private int _channel1EnvelopeTimer;
    private int _channel2EnvelopeTimer;
    private int _channel4EnvelopeTimer;

    private ushort _channel1SweepShadowFrequency;
    private int _channel1SweepTimer;
    private bool _channel1SweepEnabled;
    private bool _channel1SweepNegated;

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
        GenerateSamples(cycles);

        if (!_powered)
        {
            return;
        }

        _frameSequencerAccumulator += (int)cycles;
        while (_frameSequencerAccumulator >= FrameSequencerPeriod)
        {
            _frameSequencerAccumulator -= FrameSequencerPeriod;
            ClockFrameSequencer();
        }
    }

    public AudioBuffer DrainAudioBuffer()
    {
        var buffer = _samples.ToArray();
        _samples.Clear();
        return new AudioBuffer(buffer, AudioChannelCount, AudioSampleRate);
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
            0xFF26 => ReadNR52(),
            >= 0xFF30 and <= 0xFF3F => _waveRam[address - 0xFF30],
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

        if (address is >= 0xFF30 and <= 0xFF3F)
        {
            _waveRam[address - 0xFF30] = value;
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
                if (_channel1SweepNegated && !SweepIsNegate)
                {
                    _channel1Enabled = false;
                }

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
                if (!Channel3DacEnabled)
                {
                    _channel3Enabled = false;
                }

                break;
            case 0xFF1B:
                _nr31 = value;
                _channel3Length = 256 - value;
                break;
            case 0xFF1C:
                _nr32 = value;
                break;
            case 0xFF1D:
                _nr33 = value;
                break;
            case 0xFF1E:
                _nr34 = value;
                if ((value & 0x80) != 0)
                {
                    TriggerChannel3();
                }

                break;
            case 0xFF20:
                _nr41 = value;
                _channel4Length = 64 - (value & 0x3F);
                break;
            case 0xFF21:
                _nr42 = value;
                if (!Channel4DacEnabled)
                {
                    _channel4Enabled = false;
                }

                break;
            case 0xFF22:
                _nr43 = value;
                break;
            case 0xFF23:
                _nr44 = value;
                if ((value & 0x80) != 0)
                {
                    TriggerChannel4();
                }

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
    private bool Channel3DacEnabled => (_nr30 & 0x80) != 0;
    private bool Channel4DacEnabled => (_nr42 & 0xF8) != 0;
    private ushort Channel1Frequency => (ushort)(((_nr14 & 0x07) << 8) | _nr13);
    private ushort Channel2Frequency => (ushort)(((_nr24 & 0x07) << 8) | _nr23);
    private ushort Channel3Frequency => (ushort)(((_nr34 & 0x07) << 8) | _nr33);
    private bool SweepIsNegate => (_nr10 & 0x08) != 0;

    private byte ReadNR52()
        => (byte)((_powered ? 0x80 : 0x00)
            | 0x70
            | (_channel4Enabled ? 0x08 : 0x00)
            | (_channel3Enabled ? 0x04 : 0x00)
            | (_channel2Enabled ? 0x02 : 0x00)
            | (_channel1Enabled ? 0x01 : 0x00));

    private void WriteNR52(byte value)
    {
        if ((value & 0x80) == 0)
        {
            PowerOff();
            return;
        }

        if (!_powered)
        {
            _powered = true;
            _frameSequencerAccumulator = 0;
            _frameSequencerStep = 0;
            _sampleClockAccumulator = 0;
        }
    }

    private void PowerOff()
    {
        _powered = false;
        _channel1Enabled = false;
        _channel2Enabled = false;
        _channel3Enabled = false;
        _channel4Enabled = false;
        _channel1Length = 0;
        _channel2Length = 0;
        _channel3Length = 0;
        _channel4Length = 0;
        _channel1Phase = 0;
        _channel2Phase = 0;
        _channel3Phase = 0;
        _channel4Phase = 0;
        _leftHighPassInput = 0;
        _leftHighPassOutput = 0;
        _rightHighPassInput = 0;
        _rightHighPassOutput = 0;
        _channel4Lfsr = 0x7FFF;
        _channel1Volume = 0;
        _channel2Volume = 0;
        _channel4Volume = 0;
        _channel1EnvelopeTimer = 0;
        _channel2EnvelopeTimer = 0;
        _channel4EnvelopeTimer = 0;
        _channel1SweepShadowFrequency = 0;
        _channel1SweepTimer = 0;
        _channel1SweepEnabled = false;
        _channel1SweepNegated = false;
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
    }

    private void TriggerChannel1()
    {
        if (_channel1Length == 0)
        {
            _channel1Length = 64;
        }

        _channel1Phase = 0;
        _channel1Volume = InitialEnvelopeVolume(_nr12);
        _channel1EnvelopeTimer = EnvelopePeriodOrReload(_nr12);
        _channel1SweepShadowFrequency = Channel1Frequency;
        _channel1SweepTimer = SweepPeriodOrReload();
        _channel1SweepEnabled = SweepPeriod() != 0 || SweepShift() != 0;
        _channel1SweepNegated = false;
        _channel1Enabled = Channel1DacEnabled;

        if (SweepShift() != 0)
        {
            _ = CalculateSweepFrequency();
        }
    }

    private void TriggerChannel2()
    {
        if (_channel2Length == 0)
        {
            _channel2Length = 64;
        }

        _channel2Phase = 0;
        _channel2Volume = InitialEnvelopeVolume(_nr22);
        _channel2EnvelopeTimer = EnvelopePeriodOrReload(_nr22);
        _channel2Enabled = Channel2DacEnabled;
    }

    private void TriggerChannel3()
    {
        if (_channel3Length == 0)
        {
            _channel3Length = 256;
        }

        _channel3Phase = 0;
        _channel3Enabled = Channel3DacEnabled;
    }

    private void TriggerChannel4()
    {
        if (_channel4Length == 0)
        {
            _channel4Length = 64;
        }

        _channel4Phase = 0;
        _channel4Lfsr = 0x7FFF;
        _channel4Volume = InitialEnvelopeVolume(_nr42);
        _channel4EnvelopeTimer = EnvelopePeriodOrReload(_nr42);
        _channel4Enabled = Channel4DacEnabled;
    }

    private void ClockFrameSequencer()
    {
        if ((_frameSequencerStep & 1) == 0)
        {
            ClockLengthCounters();
        }

        if (_frameSequencerStep is 2 or 6)
        {
            ClockSweep();
        }

        if (_frameSequencerStep == 7)
        {
            ClockEnvelope(ref _channel1Volume, ref _channel1EnvelopeTimer, _nr12);
            ClockEnvelope(ref _channel2Volume, ref _channel2EnvelopeTimer, _nr22);
            ClockEnvelope(ref _channel4Volume, ref _channel4EnvelopeTimer, _nr42);
        }

        _frameSequencerStep = (_frameSequencerStep + 1) & 0x07;
    }

    private void ClockLengthCounters()
    {
        ClockLengthCounter(ref _channel1Length, ref _channel1Enabled, _nr14, maxLength: 64);
        ClockLengthCounter(ref _channel2Length, ref _channel2Enabled, _nr24, maxLength: 64);
        ClockLengthCounter(ref _channel3Length, ref _channel3Enabled, _nr34, maxLength: 256);
        ClockLengthCounter(ref _channel4Length, ref _channel4Enabled, _nr44, maxLength: 64);
    }

    private static void ClockLengthCounter(ref int length, ref bool enabled, byte triggerRegister, int maxLength)
    {
        _ = maxLength;
        if ((triggerRegister & 0x40) == 0 || length <= 0)
        {
            return;
        }

        length--;
        if (length == 0)
        {
            enabled = false;
        }
    }

    private void ClockSweep()
    {
        if (!_channel1SweepEnabled || --_channel1SweepTimer > 0)
        {
            return;
        }

        _channel1SweepTimer = SweepPeriodOrReload();

        if (SweepPeriod() == 0)
        {
            return;
        }

        var newFrequency = CalculateSweepFrequency();
        if (newFrequency > 2047 || SweepShift() == 0)
        {
            return;
        }

        _channel1SweepShadowFrequency = (ushort)newFrequency;
        _nr13 = (byte)newFrequency;
        _nr14 = (byte)((_nr14 & 0xF8) | ((newFrequency >> 8) & 0x07));
        _ = CalculateSweepFrequency();
    }

    private int CalculateSweepFrequency()
    {
        var delta = _channel1SweepShadowFrequency >> SweepShift();
        var newFrequency = SweepIsNegate
            ? _channel1SweepShadowFrequency - delta
            : _channel1SweepShadowFrequency + delta;

        if (SweepIsNegate)
        {
            _channel1SweepNegated = true;
        }

        if (newFrequency > 2047)
        {
            _channel1Enabled = false;
        }

        return newFrequency;
    }

    private int SweepPeriod() => (_nr10 >> 4) & 0x07;
    private int SweepPeriodOrReload() => SweepPeriod() == 0 ? 8 : SweepPeriod();
    private int SweepShift() => _nr10 & 0x07;

    private static void ClockEnvelope(ref int volume, ref int timer, byte envelopeRegister)
    {
        var period = envelopeRegister & 0x07;
        if (period == 0 || --timer > 0)
        {
            return;
        }

        timer = EnvelopePeriodOrReload(envelopeRegister);
        var nextVolume = IsEnvelopeIncreasing(envelopeRegister) ? volume + 1 : volume - 1;
        if ((uint)nextVolume <= 15)
        {
            volume = nextVolume;
        }
    }

    private static int InitialEnvelopeVolume(byte envelopeRegister) => (envelopeRegister >> 4) & 0x0F;
    private static int EnvelopePeriodOrReload(byte envelopeRegister) => (envelopeRegister & 0x07) == 0 ? 8 : envelopeRegister & 0x07;
    private static bool IsEnvelopeIncreasing(byte envelopeRegister) => (envelopeRegister & 0x08) != 0;

    private void GenerateSamples(uint cycles)
    {
        _sampleClockAccumulator += cycles * AudioSampleRate;
        while (_sampleClockAccumulator >= CpuFrequency)
        {
            _sampleClockAccumulator -= CpuFrequency;
            AppendSample();
        }
    }

    private void AppendSample()
    {
        var channel1 = SampleSquareChannel(_channel1Enabled && _powered, Channel1DacEnabled, _nr11, _channel1Volume, Channel1Frequency, ref _channel1Phase);
        var channel2 = SampleSquareChannel(_channel2Enabled && _powered, Channel2DacEnabled, _nr21, _channel2Volume, Channel2Frequency, ref _channel2Phase);
        var channel3 = SampleWaveChannel();
        var channel4 = SampleNoiseChannel();

        var right = 0f;
        var left = 0f;

        MixChannel(_nr51, routeRightBit: 0, routeLeftBit: 4, channel1, ref left, ref right);
        MixChannel(_nr51, routeRightBit: 1, routeLeftBit: 5, channel2, ref left, ref right);
        MixChannel(_nr51, routeRightBit: 2, routeLeftBit: 6, channel3, ref left, ref right);
        MixChannel(_nr51, routeRightBit: 3, routeLeftBit: 7, channel4, ref left, ref right);

        var rightVolume = ((_nr50 & 0x07) + 1) / 8f;
        var leftVolume = (((_nr50 >> 4) & 0x07) + 1) / 8f;

        if (left == 0f && right == 0f)
        {
            ResetHighPass();
            _samples.Add(0f);
            _samples.Add(0f);
            return;
        }

        _samples.Add(HighPass(left * leftVolume * MixedChannelScale, ref _leftHighPassInput, ref _leftHighPassOutput));
        _samples.Add(HighPass(right * rightVolume * MixedChannelScale, ref _rightHighPassInput, ref _rightHighPassOutput));
    }

    private static void MixChannel(byte routing, int routeRightBit, int routeLeftBit, float sample, ref float left, ref float right)
    {
        if ((routing & (1 << routeRightBit)) != 0)
        {
            right += sample;
        }

        if ((routing & (1 << routeLeftBit)) != 0)
        {
            left += sample;
        }
    }

    private static float SampleSquareChannel(
        bool enabled,
        bool dacEnabled,
        byte dutyRegister,
        int volume,
        ushort frequencyRegister,
        ref double phase)
    {
        var period = 2048 - frequencyRegister;
        if (period <= 0)
        {
            return 0f;
        }

        var frequency = 131_072d / period;
        phase += frequency / AudioSampleRate;
        phase -= Math.Floor(phase);

        if (!enabled || !dacEnabled || volume == 0)
        {
            return 0f;
        }

        var duty = s_squareDuty[(dutyRegister >> 6) & 0x03];
        return DacSample(phase < duty ? volume : 0);
    }

    private float SampleWaveChannel()
    {
        var period = 2048 - Channel3Frequency;
        if (period <= 0)
        {
            return 0f;
        }

        var frequency = 65_536d / period;
        _channel3Phase += frequency / AudioSampleRate;
        _channel3Phase -= Math.Floor(_channel3Phase);

        if (!_powered || !_channel3Enabled || !Channel3DacEnabled)
        {
            return 0f;
        }

        var sampleIndex = (int)(_channel3Phase * 32) & 0x1F;
        var packedSample = _waveRam[sampleIndex >> 1];
        var sample = (sampleIndex & 1) == 0
            ? packedSample >> 4
            : packedSample & 0x0F;

        sample = ((_nr32 >> 5) & 0x03) switch
        {
            0 => 0,
            1 => sample,
            2 => sample >> 1,
            _ => sample >> 2,
        };

        return DacSample(sample);
    }

    private float SampleNoiseChannel()
    {
        var noiseFrequency = NoiseFrequency();
        if (noiseFrequency > 0)
        {
            _channel4Phase += noiseFrequency / AudioSampleRate;
            while (_channel4Phase >= 1d)
            {
                _channel4Phase -= 1d;
                ClockNoiseLfsr();
            }
        }

        if (!_powered || !_channel4Enabled || !Channel4DacEnabled || _channel4Volume == 0)
        {
            return 0f;
        }

        return DacSample((_channel4Lfsr & 0x01) == 0 ? _channel4Volume : 0);
    }

    private double NoiseFrequency()
    {
        var divisorCode = _nr43 & 0x07;
        var divisor = divisorCode == 0 ? 8 : divisorCode * 16;
        var shift = (_nr43 >> 4) & 0x0F;
        var period = divisor << shift;
        return period == 0 ? 0d : (double)CpuFrequency / period;
    }

    private void ClockNoiseLfsr()
    {
        var xor = (_channel4Lfsr ^ (_channel4Lfsr >> 1)) & 0x01;
        _channel4Lfsr = (ushort)((_channel4Lfsr >> 1) | (xor << 14));

        if ((_nr43 & 0x08) != 0)
        {
            _channel4Lfsr = (ushort)((_channel4Lfsr & ~(1 << 6)) | (xor << 6));
        }
    }

    private static float DacSample(int sample)
        => sample / 7.5f - 1f;

    private static float HighPass(float input, ref float previousInput, ref float previousOutput)
    {
        const float chargeFactor = 0.996f;
        var output = input - previousInput + chargeFactor * previousOutput;
        previousInput = input;
        previousOutput = output;
        return output;
    }

    private void ResetHighPass()
    {
        _leftHighPassInput = 0;
        _leftHighPassOutput = 0;
        _rightHighPassInput = 0;
        _rightHighPassOutput = 0;
    }
}
