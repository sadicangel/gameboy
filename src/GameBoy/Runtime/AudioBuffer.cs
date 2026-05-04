namespace GameBoy.Runtime;

public readonly record struct AudioBuffer(
    ReadOnlyMemory<float> Samples,
    int ChannelCount,
    int SampleRate);
