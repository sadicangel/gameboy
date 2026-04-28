using System.Threading;

namespace GameBoy;

public interface IEmulatorRuntime
{
    TimeSpan TargetFrameDuration => TimeSpan.Zero;
    JoypadState PollJoypad();
    void PresentFrame(VideoFrame frame);
    void SubmitAudio(AudioBuffer audio);
    Task RunAsync(CancellationToken cancellationToken);
}

public readonly record struct JoypadState(
    bool A,
    bool B,
    bool Start,
    bool Select,
    bool Up,
    bool Down,
    bool Left,
    bool Right);

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

    public void PresentFrame(VideoFrame frame)
    {
        var p0 = frame.Pixels.Span[0];
        foreach (var pixel in frame.Pixels.Span)
        {
            if (pixel != p0) break;
        }
    }

    public void SubmitAudio(AudioBuffer audio) { }

    public Task RunAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
