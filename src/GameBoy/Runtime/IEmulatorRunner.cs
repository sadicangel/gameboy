using System.Threading;

namespace GameBoy.Runtime;

public interface IEmulatorRunner
{
    Task RunAsync(CancellationToken cancellationToken);
}
