namespace GameBoy;

[Singleton]
public sealed class EmulatorLineLogger(ILogger<Emulator> logger) : IEmulatorRunObserver
{
    public void OnSerialLineReceived(string line) => logger.LogInformation("{line}", line);

    public void OnStepCompleted(Bus bus)
    {
    }
}
