using Thread = System.Threading.Thread;

namespace GameBoy;

[Singleton]
public sealed class Emulator(Cpu cpu, Serial serial, Timer timer, ILogger<Emulator> logger)
{
    private bool _isRunning = true;
    private bool _isPaused = false;
    private ulong _totalCycles;
    private ulong _ticks;

    private void Run()
    {
        serial.LineReceived += line =>
        {
            logger.LogInformation("{line}", line);
            if (line.StartsWith("Passed"))
                _isRunning = false;
        };

        try
        {
            while (_isRunning)
            {
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
            logger.LogError(ex, "{message}", ex.Message);
        }
    }

    public static async Task RunAsync(string fileName)
    {
        var builder = Host.CreateApplicationBuilder(["--rom", fileName]);

        builder.Logging.ClearProviders().AddSerilog(
            new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Hour)
            .CreateLogger());

        builder.Services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.WithAttribute<SingletonAttribute>()).AsSelf().WithSingletonLifetime()
            .AddClasses(classes => classes.WithAttribute<ScopedAttribute>()).AsSelf().WithScopedLifetime()
            .AddClasses(classes => classes.WithAttribute<TransientAttribute>()).AsSelf().WithTransientLifetime());

        var app = builder.Build();

        await app.StartAsync();
        var emulator = app.Services.GetRequiredService<Emulator>();
        emulator.Run();
        await app.StopAsync();
    }
}
