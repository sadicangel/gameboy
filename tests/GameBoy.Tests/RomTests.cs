namespace GameBoy.Tests;

public sealed class RomTests
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(10);

    public static TheoryData<string> Roms => CreateRoms();

    [Theory]
    [MemberData(nameof(Roms))]
    public async Task Rom_passes(string expectation)
    {
        var romPath = Path.Combine(AppContext.BaseDirectory, "Roms", expectation.Replace('/', Path.DirectorySeparatorChar));
        var result = await BlarggRomRunner.RunAsync(romPath, s_timeout);

        Assert.True(result.Outcome == BlarggRomOutcome.Passed, BuildFailureMessage(expectation, result));
    }

    private static TheoryData<string> CreateRoms()
    {
        var data = new TheoryData<string>();
        var romRoot = Path.Combine(AppContext.BaseDirectory, "Roms");

        foreach (var romPath in Directory.EnumerateFiles(romRoot, "*.gb", SearchOption.AllDirectories).Order())
        {
            var relativePath = NormalizeRelativePath(Path.GetRelativePath(romRoot, romPath));
            data.Add(relativePath);
        }

        return data;
    }

    private static string NormalizeRelativePath(string path)
        => path.Replace(Path.DirectorySeparatorChar, '/');

    private static string BuildFailureMessage(string expectation, BlarggRomResult result)
        => $"""
            ROM: {expectation}
            Expected: Passed
            Actual: {result.Outcome} via {result.CompletionSource} after {result.Elapsed}
            Exit code: {(result.ExitCode is { } exitCode ? $"0x{exitCode:X2}" : "<none>")}
            Output:
            {FormatOutput(result.Output)}
            """;

    private static string FormatOutput(string output)
        => string.IsNullOrWhiteSpace(output) ? "<no output>" : output;
}
