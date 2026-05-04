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
