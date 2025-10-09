using Thread = System.Threading.Thread;

namespace GameBoy;

[Singleton]
public sealed class Emulator(Cpu cpu, Timer timer, ILogger<Emulator> logger)
{
    private bool _isRunning = true;
    private bool _isPaused = false;
    private ulong _totalCycles;
    private ulong _ticks;

    private void Run()
    {
        cpu.Output += line => logger.LogWarning("{line}", line);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("""
                PC: 0x{PC:X4}, SP: 0x{SP:X4},
                A: 0x{A:X2}, B: 0x{B:X2}, D: 0x{D:X2}, H: 0x{H:X2}
                F: 0x{F:X2}, C: 0x{C:X2}, E: 0x{E:X2}, L: 0x{L:X2}
                Z:    {Z}, N:    {N}, H:    {H}, C:    {C}
                """,
            cpu.Registers.PC, cpu.Registers.SP,
            cpu.Registers.A, cpu.Registers.B, cpu.Registers.D, cpu.Registers.H,
            cpu.Registers.F, cpu.Registers.C, cpu.Registers.E, cpu.Registers.L,
            Convert.ToByte(cpu.Registers.Flags.Z), Convert.ToByte(cpu.Registers.Flags.N), Convert.ToByte(cpu.Registers.Flags.H), Convert.ToByte(cpu.Registers.Flags.C));
        }

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
