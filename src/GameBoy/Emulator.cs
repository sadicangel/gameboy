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
                    continue;
                }

                RunFrame(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{message}", ex.Message);
        }
    }

    public static async Task RunAsync(string fileName, CancellationToken cancellationToken)
    {
        var builder = GameBoyHost.CreateBuilder();
        builder.Logging.ClearProviders().AddSerilog(
            new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger());

        using var app = builder.Build();
        await app.StartAsync(cancellationToken);
        await using var session = app.Services.GetRequiredService<EmulatorSessionFactory>().LoadRom(fileName);
        using var cancellationTokenSource = new ConsoleCancellationTokenSource(cancellationToken);
        session.Emulator.Run(cancellationTokenSource.Token);

        await app.StopAsync(CancellationToken.None);
    }

    private struct ConsoleCancellationTokenSource : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _isDisposed;

        public CancellationToken Token => _cancellationTokenSource.Token;

        public ConsoleCancellationTokenSource(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Console.CancelKeyPress += Cancel;
        }

        private void Cancel(object? sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            Console.CancelKeyPress -= Cancel;
            _cancellationTokenSource.Dispose();
        }
    }
}
