using System.Runtime.ExceptionServices;
using System.Threading;

namespace GameBoy;

public sealed class EmulatorSession(AsyncServiceScope serviceScope, string romPath) : IAsyncDisposable, IDisposable
{
    private Thread? _thread;
    private ExceptionDispatchInfo? _exceptionDispatchInfo;
    private CancellationTokenSource? _cts;
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private volatile bool _isPaused;
#pragma warning restore CS0414 // Field is assigned but its value is never used

    public ValueTask DisposeAsync() => serviceScope.DisposeAsync();

    public void Dispose() => serviceScope.Dispose();

    public Emulator Emulator
    {
        get
        {
            serviceScope.ServiceProvider.GetRequiredService<EmulatorSessionState>().RomPath = romPath;
            return serviceScope.ServiceProvider.GetRequiredService<Emulator>();
        }
    }

    public void Start()
    {
        if (_thread is not null)
            throw new InvalidOperationException("Session already started.");

        _cts = new CancellationTokenSource();

        _thread = new Thread(() => RunEmulator(_cts.Token))
        {
            IsBackground = true,
            Name = "GameBoy emulator"
        };

        _thread.Start();
    }

    internal Emulator RunEmulator(CancellationToken cancellationToken)
    {
        try
        {
            var emulator = Emulator;
            emulator.Run(cancellationToken);
            return emulator;
        }
        catch (Exception ex)
        {
            _exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
            _exceptionDispatchInfo.Throw();
            throw;
        }
    }


    public void Pause() => _isPaused = true;

    public void Resume() => _isPaused = false;

    public void Stop()
    {
        var cts = _cts;
        var thread = _thread;

        if (cts is null || thread is null)
            return;

        cts.Cancel();
        thread.Join();

        cts.Dispose();
        _cts = null;
        _thread = null;
    }
}
