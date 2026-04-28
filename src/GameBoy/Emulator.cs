using System.Diagnostics;
using System.Threading;

namespace GameBoy;

[Service(ServiceLifetime.Scoped)]
public sealed class Emulator(
    Cpu cpu,
    Bus bus,
    Ppu ppu,
    Joypad joypad,
    IEmulatorRuntime runtime,
    ILogger<Emulator> logger,
    IEnumerable<IEmulatorStepObserver> observers)
{
    private static readonly TimeSpan s_spinThreshold = TimeSpan.FromMilliseconds(1);
    private bool _isPaused = false;
    private readonly IEmulatorStepObserver[] _observers = observers.ToArray();
    public Bus Bus => bus;

    public FrameRunResult RunFrame(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targetFrame = ppu.CompletedFrames + 1;
        joypad.Update(runtime.PollJoypad());

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
        runtime.PresentFrame(frame);
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

                var frameDurationTimestampDelta = ToStopwatchTicks(runtime.TargetFrameDuration);
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

    private static long ToStopwatchTicks(TimeSpan duration)
        => duration <= TimeSpan.Zero
            ? 0L
            : Math.Max(1L, duration.Ticks * Stopwatch.Frequency / TimeSpan.TicksPerSecond);

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
