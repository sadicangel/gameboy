namespace GameBoy;

[Singleton]
public sealed class Emulator(Cpu cpu, ILogger<Emulator> logger)
{
    private bool _isRunning = true;
    private bool _isPaused = false;
    private ulong _ticks;

    private void Run()
    {
        try
        {
            while (_isRunning)
            {
                if (_isPaused)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (!cpu.Step())
                {
                    throw new InvalidOperationException("CPU stopped");
                }

                ++_ticks;
            }
        }
        catch (Exception ex)
        {
            logger.LogError("{message}", ex.Message);
        }
    }

    public static async Task RunAsync(string fileName)
    {
        var builder = Host.CreateApplicationBuilder(["--rom", fileName]);

        builder.Logging.AddSerilog();

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
