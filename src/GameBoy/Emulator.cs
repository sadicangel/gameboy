using CancellationToken = System.Threading.CancellationToken;
using CancellationTokenSource = System.Threading.CancellationTokenSource;
using Thread = System.Threading.Thread;

namespace GameBoy;

[Singleton]
public sealed class Emulator(Cpu cpu, Serial serial, Timer timer, ILogger<Emulator> logger)
{
    private bool _isRunning = true;
    private bool _isPaused = false;
    private ulong _totalCycles;
    private ulong _ticks;

    private void Run(CancellationToken cancellationToken)
    {
        serial.CharReceived += @char =>
        {
            logger.LogInformation("{char}", @char);
        };
        serial.LineReceived += line =>
        {
            logger.LogInformation("{line}", line);
            if (line.StartsWith("Passed") || line.StartsWith("Failed"))
            {
                _isRunning = false;
            }
            if (line.StartsWith("Failed"))
            {
                logger.LogWarning("Last Results:{NewLine}{Results}", Environment.NewLine, string.Join(Environment.NewLine, cpu.ExecutionResults.Select(x => new
                {
                    Instruction = $"{x.PC:X4}: {x.Instruction} ({x.Cycles:D2} cycles)",
                    AF = $"{x.Registers.AF:X4}",
                    BC = $"{x.Registers.BC:X4}",
                    DE = $"{x.Registers.DE:X4}",
                    HL = $"{x.Registers.HL:X4}",
                    SP = $"{x.Registers.SP:X4}",
                    PC = $"{x.Registers.PC:X4}",
                })));
            }

        };

        try
        {
            logger.LogInformation("{@Registers}", new
            {
                AF = $"{cpu.Registers.AF:X4}",
                BC = $"{cpu.Registers.BC:X4}",
                DE = $"{cpu.Registers.DE:X4}",
                HL = $"{cpu.Registers.HL:X4}",
                SP = $"{cpu.Registers.SP:X4}",
                PC = $"{cpu.Registers.PC:X4}",
            });

            while (_isRunning)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_isPaused)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var cycles = cpu.Step();

                timer.Tick(cycles);

                _totalCycles += cycles;
                ++_ticks;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{message}{NewLine}Last Results:{NewLine}{Results}", ex.Message, Environment.NewLine, Environment.NewLine, string.Join(Environment.NewLine, cpu.ExecutionResults.Select(x => new
            {
                Instruction = $"{x.PC:X4}: {x.Instruction} ({x.Cycles:D2} cycles)",
                AF = $"{x.Registers.AF:X4}",
                BC = $"{x.Registers.BC:X4}",
                DE = $"{x.Registers.DE:X4}",
                HL = $"{x.Registers.HL:X4}",
                SP = $"{x.Registers.SP:X4}",
                PC = $"{x.Registers.PC:X4}",
            })));
        }
    }

    public static async Task RunAsync(string fileName)
    {
        var builder = Host.CreateApplicationBuilder(["--rom", fileName]);

        builder.Logging.ClearProviders().AddSerilog(
            new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            //.WriteTo.File("log.txt", rollingInterval: RollingInterval.Hour)
            .CreateLogger());

        builder.Services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.WithAttribute<SingletonAttribute>()).AsSelf().WithSingletonLifetime()
            .AddClasses(classes => classes.WithAttribute<ScopedAttribute>()).AsSelf().WithScopedLifetime()
            .AddClasses(classes => classes.WithAttribute<TransientAttribute>()).AsSelf().WithTransientLifetime());

        var app = builder.Build();

        await app.StartAsync();
        var emulator = app.Services.GetRequiredService<Emulator>();
        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };
        emulator.Run(cancellationTokenSource.Token);
        await app.StopAsync();
    }
}
