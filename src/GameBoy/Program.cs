using System.Threading;
using GameBoy.RayLibRuntime;
using GameBoy.Runtime;

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

builder.Services.AddSingleton<RayLibRunner>();
builder.Services.AddSingleton<IEmulatorRunner>(provider => provider.GetRequiredService<RayLibRunner>());
builder.Services.AddSingleton<IJoypadInput>(provider => provider.GetRequiredService<RayLibRunner>());
builder.Services.AddSingleton<IVideoOutput>(provider => provider.GetRequiredService<RayLibRunner>());
builder.Services.AddSingleton<IAudioOutput>(provider => provider.GetRequiredService<RayLibRunner>());

using var app = builder.Build();
using var cancellationTokenSource = new ConsoleCancellationTokenSource(CancellationToken.None);
await app.StartAsync(cancellationTokenSource.Token);

var runtime = app.Services.GetRequiredService<IEmulatorRunner>();
// TODO: This should be the only call here. Starting a session should be done inside the runner.
// await runner.RunAsync(cancellationTokenSource.Token);

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
