using System.Threading;

namespace GameBoy;

[Service(ServiceLifetime.Singleton)]
public sealed class EmulatorOptions
{
    private readonly Lock _lock = new();

    public int TargetFrameMultiplier
    {
        get
        {
            lock (_lock)
            {
                return field;
            }
        }
        set
        {
            lock (_lock)
            {
                field = value;
            }
        }
    } = 3;
}
