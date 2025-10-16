//foreach (var file in Directory.EnumerateFiles(@"D:\Development\gb-test-roms\cpu_instrs\individual"))
//    await Emulator.RunAsync(file);
await Emulator.RunAsync(@"D:\Development\gb-test-roms\instr_timing\instr_timing.gb");
