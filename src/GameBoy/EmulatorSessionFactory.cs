namespace GameBoy;

[Service(ServiceLifetime.Singleton)]
public sealed class EmulatorSessionFactory(IServiceScopeFactory scopeFactory)
{
    public EmulatorSession LoadRom(string romPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);

        return new EmulatorSession(scopeFactory.CreateAsyncScope(), romPath);
    }
}

[Service(ServiceLifetime.Scoped)]
public sealed class EmulatorSessionState
{
    public string RomPath
    {
        get => string.IsNullOrWhiteSpace(field) ? throw new InvalidOperationException("ROM path has not been set.") : field;
        set;
    }
}
