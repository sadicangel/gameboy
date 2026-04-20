if (args is not [var romPath])
{
    Console.Error.WriteLine("Usage: GameBoy <rom-path>");
    Environment.ExitCode = 1;
    return;
}

await Emulator.RunAsync(romPath, System.Threading.CancellationToken.None);
