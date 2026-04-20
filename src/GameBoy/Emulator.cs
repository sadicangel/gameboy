using CancellationToken = System.Threading.CancellationToken;
using CancellationTokenSource = System.Threading.CancellationTokenSource;
using Thread = System.Threading.Thread;

namespace GameBoy;

[Singleton]
public sealed class Emulator(Cpu cpu, Bus bus, ILogger<Emulator> logger, IEnumerable<IEmulatorRunObserver> observers)
{
    private bool _isPaused = false;
    private readonly IEmulatorRunObserver[] _observers = observers.ToArray();

    public void Run(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{@Registers}",
            new
            {
                AF = $"{cpu.Registers.AF:X4}",
                BC = $"{cpu.Registers.BC:X4}",
                DE = $"{cpu.Registers.DE:X4}",
                HL = $"{cpu.Registers.HL:X4}",
                SP = $"{cpu.Registers.SP:X4}",
                PC = $"{cpu.Registers.PC:X4}",
            });

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_isPaused)
            {
                Thread.Sleep(10);
                continue;
            }

            cpu.Step();

            foreach (var observer in _observers)
            {
                observer.OnStepCompleted(bus);
            }
        }
    }

    public static async Task RunAsync(string fileName, CancellationToken cancellationToken)
    {
        using var app = GameBoyHostFactory.Create(
            fileName,
            logging => logging.ClearProviders().AddSerilog(
                new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger()));

        await app.StartAsync(cancellationToken);
        var emulator = app.Services.GetRequiredService<Emulator>();
        var logger = app.Services.GetRequiredService<ILogger<Emulator>>();
        using var cancellationTokenSource = new ConsoleCancellationTokenSource(cancellationToken);
        try
        {
            emulator.Run(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException) when (cancellationTokenSource.Token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{message}", ex.Message);
        }

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
