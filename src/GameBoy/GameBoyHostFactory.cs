namespace GameBoy;

public static class GameBoyHostFactory
{
    public static IHost Create(
        string fileName,
        Action<ILoggingBuilder>? configureLogging = null,
        Action<IServiceCollection>? configureServices = null)
    {
        var builder = Host.CreateApplicationBuilder(["--rom", fileName]);
        configureLogging?.Invoke(builder.Logging);

        builder.Services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.WithAttribute<SingletonAttribute>()).AsSelf().WithSingletonLifetime()
            .AddClasses(classes => classes.WithAttribute<ScopedAttribute>()).AsSelf().WithScopedLifetime()
            .AddClasses(classes => classes.WithAttribute<TransientAttribute>()).AsSelf().WithTransientLifetime());
        builder.Services.AddSingleton<IEmulatorRunObserver>(provider => provider.GetRequiredService<EmulatorLineLogger>());
        configureServices?.Invoke(builder.Services);

        return builder.Build();
    }
}
