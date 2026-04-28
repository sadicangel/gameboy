using System.Threading;
using GameBoy.RayLibRuntime;

if (args is not [var romPath])
{
    Console.Error.WriteLine("Usage: GameBoy <rom-path>");
    Environment.ExitCode = 1;
    return;
}

var builder = GameBoyHost.CreateBuilder();
builder.Logging.ClearProviders().AddSerilog(
    new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger());

builder.Services.AddSingleton<IEmulatorRuntime, RayLibRuntime>();

using var app = builder.Build();
using var cancellationTokenSource = new ConsoleCancellationTokenSource(CancellationToken.None);
await app.StartAsync(cancellationTokenSource.Token);

var runtime = app.Services.GetRequiredService<IEmulatorRuntime>();
// TODO: This should be the only call here. Starting a session should be done inside the runtime.
// await runtime.RunAsync(cancellationTokenSource.Token);

var sessionFactory = app.Services.GetRequiredService<EmulatorSessionFactory>();
await using var session = sessionFactory.LoadRom(romPath);
try
{
    session.Start();
    await runtime.RunAsync(cancellationTokenSource.Token);
}
catch (Exception ex)
{
    app.Services.GetRequiredService<ILogger<Program>>().LogError(ex, "An error occurred while running the emulator.");
}
finally
{
    session.Stop();
}

await app.StopAsync(CancellationToken.None);
