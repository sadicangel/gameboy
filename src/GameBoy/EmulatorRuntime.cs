namespace GameBoy;

public interface IEmulatorRuntime
{
    JoypadState PollJoypad();
    void PresentFrame(VideoFrame frame);
    void SubmitAudio(AudioBuffer audio);
}

public readonly record struct JoypadState(
    bool Right,
    bool Left,
    bool Up,
    bool Down,
    bool A,
    bool B,
    bool Select,
    bool Start);

public readonly record struct VideoFrame(
    uint FrameNumber,
    ReadOnlyMemory<byte> Pixels)
{
    public const int Width = 160;
    public const int Height = 144;
}

public readonly record struct AudioBuffer(
    ReadOnlyMemory<float> Samples,
    int ChannelCount,
    int SampleRate);

public readonly record struct FrameRunResult(
    uint FrameNumber,
    int CpuCyclesExecuted);

[Service(ServiceLifetime.Singleton, typeof(IEmulatorRuntime))]
public sealed class NullEmulatorRuntime : IEmulatorRuntime
{
    public JoypadState PollJoypad() => default;

    public void PresentFrame(VideoFrame frame) { }

    public void SubmitAudio(AudioBuffer audio) { }
}
