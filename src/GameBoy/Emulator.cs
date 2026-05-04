using System.Diagnostics;
using System.Threading;
using GameBoy.Runtime;

namespace GameBoy;

[Service(ServiceLifetime.Scoped)]
public sealed class Emulator(
    Cpu cpu,
    Bus bus,
    Ppu ppu,
    Apu apu,
    Joypad joypad,
    IJoypadInput joypadInput,
    IVideoOutput videoOutput,
    IAudioOutput audioOutput,
    EmulatorOptions options,
    ILogger<Emulator> logger,
    IEnumerable<IEmulatorStepObserver> observers)
{
    private static readonly long s_targetFrameDurationX1 = Math.Max(1L, TimeSpan.FromSeconds(154d * 456d / 4_194_304d).Ticks * Stopwatch.Frequency / TimeSpan.TicksPerSecond);
    private static readonly long s_targetFrameDurationX2 = Math.Max(1L, s_targetFrameDurationX1 / 2);
    private static readonly long s_targetFrameDurationX3 = Math.Max(1L, s_targetFrameDurationX1 / 3);
    private static readonly long s_targetFrameDurationX4 = Math.Max(1L, s_targetFrameDurationX1 / 4);

    private static readonly TimeSpan s_spinThreshold = TimeSpan.FromMilliseconds(1);
    private bool _isPaused = false;
    private readonly IEmulatorStepObserver[] _observers = observers.ToArray();
    public Bus Bus => bus;

    public FrameRunResult RunFrame(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targetFrame = ppu.CompletedFrames + 1;
        joypad.Update(joypadInput.PollJoypad());

        var cpuCyclesExecuted = 0;

        while (ppu.CompletedFrames < targetFrame)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cpuCyclesExecuted += cpu.Step();

            foreach (var observer in _observers)
            {
                observer.OnStepCompleted(bus);
            }
        }

        var frame = ppu.LatestFrame;
        videoOutput.PresentFrame(frame);
        audioOutput.SubmitAudio(apu.DrainAudioBuffer());
        return new FrameRunResult(frame.FrameNumber, cpuCyclesExecuted);
    }

    public void Run(CancellationToken cancellationToken)
    {
        var lastFrameDurationTimestampDelta = 0L;
        var nextFrameDeadlineTimestamp = 0L;

        try
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Registers: {@Registers}",
                    new
                    {
                        AF = $"{cpu.Registers.AF:X4}",
                        BC = $"{cpu.Registers.BC:X4}",
                        DE = $"{cpu.Registers.DE:X4}",
                        HL = $"{cpu.Registers.HL:X4}",
                        SP = $"{cpu.Registers.SP:X4}",
                        PC = $"{cpu.Registers.PC:X4}",
                    });
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                if (_isPaused)
                {
                    Thread.Sleep(10);
                    lastFrameDurationTimestampDelta = 0L;
                    nextFrameDeadlineTimestamp = 0L;
                    continue;
                }

                RunFrame(cancellationToken);

                var frameDurationTimestampDelta = GetFrameDurationTimestampDelta();
                if (frameDurationTimestampDelta == 0)
                {
                    lastFrameDurationTimestampDelta = 0L;
                    nextFrameDeadlineTimestamp = 0L;
                    continue;
                }

                if (frameDurationTimestampDelta != lastFrameDurationTimestampDelta || nextFrameDeadlineTimestamp == 0)
                {
                    nextFrameDeadlineTimestamp = Stopwatch.GetTimestamp() + frameDurationTimestampDelta;
                    lastFrameDurationTimestampDelta = frameDurationTimestampDelta;
                }

                WaitForNextFrame(frameDurationTimestampDelta, ref nextFrameDeadlineTimestamp, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal shutdown
        }
    }

    private long GetFrameDurationTimestampDelta()
    {
        if (!joypad.CurrentState.Turbo) return s_targetFrameDurationX1;

        var multiplier = options.TargetFrameMultiplier;
        return multiplier switch
        {
            1 => s_targetFrameDurationX1,
            2 => s_targetFrameDurationX2,
            3 => s_targetFrameDurationX3,
            4 => s_targetFrameDurationX4,
            _ => throw new ArgumentOutOfRangeException(nameof(multiplier), "TargetFrameMultiplier must be between 1 and 4.")
        };
    }

    private static void WaitForNextFrame(
        long frameDurationTimestampDelta,
        ref long nextFrameDeadlineTimestamp,
        CancellationToken cancellationToken)
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        if (currentTimestamp - nextFrameDeadlineTimestamp > frameDurationTimestampDelta + frameDurationTimestampDelta)
        {
            nextFrameDeadlineTimestamp = currentTimestamp + frameDurationTimestampDelta;
            return;
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            currentTimestamp = Stopwatch.GetTimestamp();
            if (currentTimestamp >= nextFrameDeadlineTimestamp)
            {
                nextFrameDeadlineTimestamp += frameDurationTimestampDelta;
                return;
            }

            var remaining = Stopwatch.GetElapsedTime(currentTimestamp, nextFrameDeadlineTimestamp);
            if (remaining > s_spinThreshold)
            {
                Thread.Sleep(remaining - s_spinThreshold);
                continue;
            }

            Thread.SpinWait(256);
        }
    }
}
