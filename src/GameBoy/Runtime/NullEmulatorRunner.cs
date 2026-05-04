using System.Threading;

namespace GameBoy.Runtime;

[Service(ServiceLifetime.Singleton, typeof(IEmulatorRunner))]
public sealed class NullEmulatorRunner : IEmulatorRunner, IJoypadInput, IVideoOutput, IAudioOutput
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
