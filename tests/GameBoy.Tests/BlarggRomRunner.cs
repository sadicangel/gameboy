using System.Diagnostics;

namespace GameBoy.Tests;

internal enum BlarggRomOutcome
{
    Passed,
    Failed,
    TimedOut
}

internal enum BlarggRomCompletionSource
{
    Serial,
    Shell,
    Timeout
}

internal sealed record BlarggRomResult(
    BlarggRomOutcome Outcome,
    BlarggRomCompletionSource CompletionSource,
    string Output,
    byte? ExitCode,
    TimeSpan Elapsed);

internal static class BlarggRomRunner
{
    private const int ShellOutputLength = 0x3C;

    public static async Task<BlarggRomResult> RunAsync(string romPath, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();
        using var runCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        runCancellationTokenSource.CancelAfter(timeout);
        var observer = new BlarggRomObserver(stopwatch, runCancellationTokenSource);
        var builder = GameBoyHost.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<IEmulatorSerialObserver>(observer);
        builder.Services.AddSingleton<IEmulatorStepObserver>(observer);
        using var host = builder.Build();

        try
        {
            await host.StartAsync(cancellationToken);
            await using var session = host.Services.GetRequiredService<EmulatorSessionFactory>().LoadRom(romPath);
            stopwatch.Start();

            Emulator emulator;
            Bus bus = null!;

            try
            {
                emulator = session.RunEmulator(runCancellationTokenSource.Token);
                bus = emulator.Bus;
            }
            catch (OperationCanceledException) when (runCancellationTokenSource.IsCancellationRequested) { }

            cancellationToken.ThrowIfCancellationRequested();

            if (observer.TryGetResult(out var result))
            {
                return result;
            }

            var timeoutOutput = TryReadShellOutput(bus, out var partialShellOutput)
                ? partialShellOutput
                : observer.GetSerialOutput();

            return new BlarggRomResult(
                BlarggRomOutcome.TimedOut,
                BlarggRomCompletionSource.Timeout,
                timeoutOutput,
                ExitCode: null,
                stopwatch.Elapsed);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    private static string BuildSerialOutput(IEnumerable<string> outputLines)
        => string.Join(Environment.NewLine, outputLines);

    private static bool TryReadShellExit(Bus bus, out byte exitCode, out string output)
    {
        output = string.Empty;
        exitCode = 0x80;

        if (!TryReadShellOutput(bus, out output))
        {
            return false;
        }

        exitCode = bus.Read(0xA000);
        return exitCode != 0x80;
    }

    private static bool TryReadShellOutput(Bus bus, out string output)
    {
        output = string.Empty;

        if (bus.Read(0xA001) != 0xDE || bus.Read(0xA002) != 0xB0 || bus.Read(0xA003) != 0x61)
        {
            return false;
        }

        output = new string(
                Enumerable.Range(0, ShellOutputLength)
                    .Select(offset => (char)bus.Read((ushort)(0xA004 + offset)))
                    .TakeWhile(@char => @char != '\0')
                    .ToArray())
            .TrimEnd();

        return true;
    }

    private sealed class BlarggRomObserver(Stopwatch stopwatch, CancellationTokenSource runCancellationTokenSource)
        : IEmulatorSerialObserver, IEmulatorStepObserver
    {
        private readonly List<string> _outputLines = [];
        private BlarggRomResult? _result;

        public void OnSerialLineReceived(string line)
        {
            _outputLines.Add(line);

            if (line.StartsWith("Passed", StringComparison.Ordinal))
            {
                TryComplete(
                    new BlarggRomResult(
                        BlarggRomOutcome.Passed,
                        BlarggRomCompletionSource.Serial,
                        BuildSerialOutput(_outputLines),
                        ExitCode: null,
                        stopwatch.Elapsed));
            }
            else if (line.StartsWith("Failed", StringComparison.Ordinal))
            {
                TryComplete(
                    new BlarggRomResult(
                        BlarggRomOutcome.Failed,
                        BlarggRomCompletionSource.Serial,
                        BuildSerialOutput(_outputLines),
                        ExitCode: null,
                        stopwatch.Elapsed));
            }
        }

        public void OnStepCompleted(Bus bus)
        {
            if (_result is not null)
            {
                return;
            }

            if (TryReadShellExit(bus, out var exitCode, out var shellOutput))
            {
                TryComplete(
                    new BlarggRomResult(
                        exitCode == 0x00 ? BlarggRomOutcome.Passed : BlarggRomOutcome.Failed,
                        BlarggRomCompletionSource.Shell,
                        string.IsNullOrWhiteSpace(shellOutput) ? BuildSerialOutput(_outputLines) : shellOutput,
                        exitCode,
                        stopwatch.Elapsed));
            }
        }

        public string GetSerialOutput() => BuildSerialOutput(_outputLines);

        public bool TryGetResult(out BlarggRomResult result)
        {
            result = _result!;
            return _result is not null;
        }

        private void TryComplete(BlarggRomResult result)
        {
            if (_result is not null)
            {
                return;
            }

            _result = result;
            runCancellationTokenSource.Cancel();
        }
    }
}
