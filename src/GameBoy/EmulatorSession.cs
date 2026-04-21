namespace GameBoy;

public sealed class EmulatorSession(AsyncServiceScope scope) : IDisposable, IAsyncDisposable
{
    internal IServiceProvider Services { get; } = scope.ServiceProvider;
    public Emulator Emulator { get; } = scope.ServiceProvider.GetRequiredService<Emulator>();

    public void Dispose() => scope.Dispose();

    public ValueTask DisposeAsync() => scope.DisposeAsync();
}
