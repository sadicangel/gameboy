namespace GameBoy;

[Service(ServiceLifetime.Singleton, typeof(IEmulatorSerialObserver))]
public sealed class EmulatorLineLogger(ILogger<Emulator> logger) : IEmulatorSerialObserver
{
    public void OnSerialLineReceived(string line) => logger.LogInformation("{line}", line);
}
