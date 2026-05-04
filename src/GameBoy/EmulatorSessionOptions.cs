namespace GameBoy;

[Service(ServiceLifetime.Scoped)]
public sealed class EmulatorSessionOptions
{
    public string RomPath
    {
        get => string.IsNullOrWhiteSpace(field) ? throw new InvalidOperationException("ROM path has not been set.") : field;
        set;
    }
}
