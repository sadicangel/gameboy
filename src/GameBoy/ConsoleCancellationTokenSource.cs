using System.Threading;

namespace GameBoy;

internal struct ConsoleCancellationTokenSource : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _isDisposed;

    public CancellationToken Token => _cancellationTokenSource.Token;

    public ConsoleCancellationTokenSource(CancellationToken cancellationToken)
    {
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.CancelKeyPress += Cancel;
    }

    private void Cancel(object? sender, ConsoleCancelEventArgs args)
    {
        args.Cancel = true;
        Cancel();
    }

    public void Cancel() => _cancellationTokenSource.Cancel();

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Console.CancelKeyPress -= Cancel;
        _cancellationTokenSource.Dispose();
    }
}
