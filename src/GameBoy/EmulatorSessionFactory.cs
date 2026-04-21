namespace GameBoy;

[Service(ServiceLifetime.Singleton)]
public sealed class EmulatorSessionFactory(IServiceScopeFactory scopeFactory)
{
    public EmulatorSession LoadRom(string romPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);

        var scope = scopeFactory.CreateAsyncScope();
        try
        {
            scope.ServiceProvider.GetRequiredService<EmulatorSessionState>().RomPath = romPath;
            return new EmulatorSession(scope);
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }
}

[Service(ServiceLifetime.Scoped)]
public sealed class EmulatorSessionState
{
    public string RomPath { get; set; } = string.Empty;
}
