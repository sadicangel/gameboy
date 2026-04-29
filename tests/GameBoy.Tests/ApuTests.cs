namespace GameBoy.Tests;

public sealed class ApuTests
{
    [Fact]
    public void Tick_generates_stereo_samples_at_configured_sample_rate()
    {
        var apu = new Apu();

        apu.Tick(154u * 456u);

        var audio = apu.DrainAudioBuffer();

        Assert.Equal(Apu.AudioChannelCount, audio.ChannelCount);
        Assert.Equal(Apu.AudioSampleRate, audio.SampleRate);
        Assert.Equal(1_606, audio.Samples.Length);
    }

    [Fact]
    public void New_apu_emits_silence_until_a_channel_is_triggered()
    {
        var apu = new Apu();

        apu.Tick(154u * 456u);

        var audio = apu.DrainAudioBuffer();

        Assert.Equal(0xF0, apu.Read(0xFF26));
        Assert.NotEmpty(audio.Samples.ToArray());
        Assert.All(audio.Samples.ToArray(), sample => Assert.Equal(0f, sample));
    }

    [Fact]
    public void DrainAudioBuffer_clears_pending_samples()
    {
        var apu = new Apu();

        apu.Tick(154u * 456u);
        _ = apu.DrainAudioBuffer();

        Assert.Empty(apu.DrainAudioBuffer().Samples.ToArray());
    }

    [Fact]
    public void Powered_off_apu_still_emits_silent_timed_samples()
    {
        var apu = new Apu();

        apu.Write(0xFF26, 0x00);
        apu.Tick(154u * 456u);

        var audio = apu.DrainAudioBuffer();

        Assert.NotEmpty(audio.Samples.ToArray());
        Assert.All(audio.Samples.ToArray(), sample => Assert.Equal(0f, sample));
    }

    [Fact]
    public void Square_channel_respects_stereo_routing()
    {
        var apu = new Apu();

        apu.Write(0xFF24, 0x77);
        apu.Write(0xFF25, 0x10);
        apu.Write(0xFF11, 0x80);
        apu.Write(0xFF12, 0xF0);
        apu.Write(0xFF13, 0x00);
        apu.Write(0xFF14, 0x84);

        apu.Tick(154u * 456u);

        var samples = apu.DrainAudioBuffer().Samples.ToArray();
        var sawLeftSignal = false;

        for (var i = 0; i < samples.Length; i += 2)
        {
            sawLeftSignal |= samples[i] != 0f;
            Assert.Equal(0f, samples[i + 1]);
        }

        Assert.True(sawLeftSignal);
    }

    [Fact]
    public void Wave_channel_uses_wave_ram_and_stereo_routing()
    {
        var apu = new Apu();

        apu.Write(0xFF30, 0xF0);
        apu.Write(0xFF24, 0x77);
        apu.Write(0xFF25, 0x40);
        apu.Write(0xFF1A, 0x80);
        apu.Write(0xFF1C, 0x20);
        apu.Write(0xFF1D, 0x00);
        apu.Write(0xFF1E, 0x80);

        apu.Tick(154u * 456u);

        var samples = apu.DrainAudioBuffer().Samples.ToArray();
        var sawLeftSignal = false;

        for (var i = 0; i < samples.Length; i += 2)
        {
            sawLeftSignal |= samples[i] != 0f;
            Assert.Equal(0f, samples[i + 1]);
        }

        Assert.True(sawLeftSignal);
    }

    [Fact]
    public void Noise_channel_generates_signal()
    {
        var apu = new Apu();

        apu.Write(0xFF24, 0x77);
        apu.Write(0xFF25, 0x80);
        apu.Write(0xFF21, 0xF0);
        apu.Write(0xFF22, 0x03);
        apu.Write(0xFF23, 0x80);

        apu.Tick(154u * 456u);

        var samples = apu.DrainAudioBuffer().Samples.ToArray();

        Assert.Contains(samples.Where((_, index) => index % 2 == 0), sample => sample != 0f);
        Assert.All(samples.Where((_, index) => index % 2 == 1), sample => Assert.Equal(0f, sample));
    }

    [Fact]
    public void Noise_channel_becomes_silent_when_envelope_reaches_zero()
    {
        var apu = new Apu();

        apu.Write(0xFF24, 0x77);
        apu.Write(0xFF25, 0x80);
        apu.Write(0xFF21, 0xF1);
        apu.Write(0xFF22, 0x03);
        apu.Write(0xFF23, 0x80);

        apu.Tick(16u * 65_536u);
        _ = apu.DrainAudioBuffer();

        apu.Tick(154u * 456u);

        var samples = apu.DrainAudioBuffer().Samples.ToArray();

        Assert.All(samples, sample => Assert.Equal(0f, sample, tolerance: 0.0001f));
    }

    [Fact]
    public void Length_counter_clears_channel_status()
    {
        var apu = new Apu();

        apu.Write(0xFF16, 0x3F);
        apu.Write(0xFF17, 0xF0);
        apu.Write(0xFF19, 0xC0);

        Assert.NotEqual(0, apu.Read(0xFF26) & 0x02);

        apu.Tick(8_192);

        Assert.Equal(0, apu.Read(0xFF26) & 0x02);
    }
}
