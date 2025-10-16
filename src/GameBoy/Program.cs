foreach (var file in Directory.EnumerateFiles(@"D:\Development\gb-test-roms\cpu_instrs\individual"))
    await Emulator.RunAsync(file);
