namespace GameBoy.Tests;

public sealed class FrameRuntimeBoundaryTests
{
    [Fact]
    public async Task RunFrame_polls_joypad_once_and_presents_one_frame()
    {
        var runtime = new FakeRuntime();
        using var host = CreateHost(services => services.AddSingleton<IEmulatorRuntime>(runtime));

        await host.StartAsync();
        using var session = CreateSession(host, "halt_bug.gb");
        try
        {
            var emulator = session.Emulator;

            var result = emulator.RunFrame(CancellationToken.None);

            var frame = Assert.Single(runtime.PresentedFrames);
            Assert.Equal(1, runtime.PollCount);
            Assert.Equal(result.FrameNumber, frame.FrameNumber);
            Assert.Equal(VideoFrame.Width * VideoFrame.Height, frame.Pixels.Length);
            Assert.True(result.CpuCyclesExecuted > 0);

            var audio = Assert.Single(runtime.SubmittedAudio);
            Assert.Equal(Apu.AudioChannelCount, audio.ChannelCount);
            Assert.Equal(Apu.AudioSampleRate, audio.SampleRate);
            Assert.NotEmpty(audio.Samples.ToArray());
            Assert.Equal(0, audio.Samples.Length % Apu.AudioChannelCount);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task RunFrame_increments_frame_numbers()
    {
        var runtime = new FakeRuntime();
        using var host = CreateHost(services => services.AddSingleton<IEmulatorRuntime>(runtime));

        await host.StartAsync();
        using var session = CreateSession(host, "halt_bug.gb");
        try
        {
            var emulator = session.Emulator;

            var first = emulator.RunFrame(CancellationToken.None);
            var second = emulator.RunFrame(CancellationToken.None);

            Assert.Equal(1u, first.FrameNumber);
            Assert.Equal(2u, second.FrameNumber);
            Assert.Equal(2, runtime.PollCount);
            Assert.Equal([1u, 2u], runtime.PresentedFrames.Select(static frame => frame.FrameNumber).ToArray());
            Assert.Equal(2, runtime.SubmittedAudio.Count);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Run_works_in_headless_mode_without_custom_runtime()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var observer = new CancellingObserver(cancellationTokenSource, stepLimit: 64);
        using var host = CreateHost(services => services.AddSingleton<IEmulatorStepObserver>(observer));

        await host.StartAsync();
        using var session = CreateSession(host, "halt_bug.gb");
        try
        {
            var emulator = session.Emulator;

            emulator.Run(cancellationTokenSource.Token);

            Assert.True(cancellationTokenSource.IsCancellationRequested);
            Assert.True(observer.StepCount >= 64);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Host_can_create_a_fresh_session_without_rebuilding()
    {
        using var host = CreateHost();

        await host.StartAsync();
        try
        {
            FrameRunResult first;
            using (var firstSession = CreateSession(host, "halt_bug.gb"))
            {
                first = firstSession.Emulator.RunFrame(CancellationToken.None);
            }

            FrameRunResult second;
            using (var secondSession = CreateSession(host, "halt_bug.gb"))
            {
                second = secondSession.Emulator.RunFrame(CancellationToken.None);
            }

            Assert.Equal(1u, first.FrameNumber);
            Assert.Equal(1u, second.FrameNumber);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    private static IHost CreateHost(Action<IServiceCollection>? configureServices = null)
    {
        var builder = GameBoyHost.CreateBuilder();
        builder.Logging.ClearProviders();
        configureServices?.Invoke(builder.Services);
        return builder.Build();
    }

    private static EmulatorSession CreateSession(IHost host, string romPath)
        => host.Services.GetRequiredService<EmulatorSessionFactory>().LoadRom(GetRomPath(romPath));

    private static string GetRomPath(string relativePath)
        => Path.Combine(AppContext.BaseDirectory, "Roms", relativePath.Replace('/', Path.DirectorySeparatorChar));

    private sealed class FakeRuntime : IEmulatorRuntime
    {
        private readonly List<VideoFrame> _presentedFrames = [];
        private readonly List<AudioBuffer> _submittedAudio = [];

        public int PollCount { get; private set; }
        public JoypadState JoypadState { get; set; }
        public IReadOnlyList<VideoFrame> PresentedFrames => _presentedFrames;
        public IReadOnlyList<AudioBuffer> SubmittedAudio => _submittedAudio;

        public JoypadState PollJoypad()
        {
            PollCount++;
            return JoypadState;
        }

        public void PresentFrame(VideoFrame frame) => _presentedFrames.Add(frame);

        public void SubmitAudio(AudioBuffer audio) => _submittedAudio.Add(audio);

        public Task RunAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class CancellingObserver(CancellationTokenSource cancellationTokenSource, int stepLimit) : IEmulatorStepObserver
    {
        public int StepCount { get; private set; }

        public void OnStepCompleted(Bus bus)
        {
            StepCount++;

            if (StepCount >= stepLimit)
            {
                cancellationTokenSource.Cancel();
            }
        }
    }
}
