using Microsoft.Extensions.Logging.Abstractions;

namespace GameBoy.Tests;

public sealed class BusMmioTests
{
    [Fact]
    public void FF00_reads_selected_button_group()
    {
        var hardware = CreateHardware();
        hardware.Joypad.Update(new JoypadState(
            Right: true,
            Left: false,
            Up: false,
            Down: true,
            A: true,
            B: false,
            Select: false,
            Start: true));

        hardware.Bus.Write(0xFF00, 0x20);
        Assert.Equal(0xE6, hardware.Bus.Read(0xFF00));

        hardware.Bus.Write(0xFF00, 0x10);
        Assert.Equal(0xD6, hardware.Bus.Read(0xFF00));

        hardware.Bus.Write(0xFF00, 0x30);
        Assert.Equal(0xFF, hardware.Bus.Read(0xFF00));
    }

    [Fact]
    public void Joypad_interrupt_requires_a_new_selected_press()
    {
        var hardware = CreateHardware();
        hardware.Bus.Write(0xFF00, 0x20);

        hardware.Interrupts.WriteIF(0);
        hardware.Joypad.Update(new JoypadState(
            Right: false,
            Left: false,
            Up: false,
            Down: false,
            A: true,
            B: false,
            Select: false,
            Start: false));
        Assert.Equal(0, hardware.Interrupts.ReadIF() & (byte)Interrupts.Joypad);

        hardware.Joypad.Update(new JoypadState(
            Right: true,
            Left: false,
            Up: false,
            Down: false,
            A: true,
            B: false,
            Select: false,
            Start: false));
        Assert.NotEqual(0, hardware.Interrupts.ReadIF() & (byte)Interrupts.Joypad);

        hardware.Interrupts.WriteIF(0);
        hardware.Joypad.Update(new JoypadState(
            Right: true,
            Left: false,
            Up: false,
            Down: false,
            A: true,
            B: false,
            Select: false,
            Start: false));
        Assert.Equal(0, hardware.Interrupts.ReadIF() & (byte)Interrupts.Joypad);
    }

    [Fact]
    public void FF46_copies_a_full_oam_page()
    {
        var hardware = CreateHardware();
        hardware.Bus.Write(0xFF40, 0x00);

        for (ushort offset = 0; offset < 0xA0; offset++)
        {
            hardware.Bus.Write((ushort)(0xC000 + offset), (byte)(offset ^ 0x5A));
        }

        hardware.Bus.Write(0xFF46, 0xC0);

        Assert.Equal(0xC0, hardware.Bus.Read(0xFF46));
        for (ushort offset = 0; offset < 0xA0; offset++)
        {
            Assert.Equal((byte)(offset ^ 0x5A), hardware.Bus.Read((ushort)(0xFE00 + offset)));
        }
    }

    [Fact]
    public void Vram_is_blocked_during_mode_3_and_accessible_in_hblank()
    {
        var hardware = CreateHardware();
        hardware.Bus.Write(0xFF40, 0x00);
        hardware.Bus.Write(0x8000, 0x12);
        hardware.Bus.Write(0xFF40, 0x91);

        hardware.Ppu.Tick(80);

        hardware.Bus.Write(0x8000, 0x34);
        Assert.Equal(0xFF, hardware.Bus.Read(0x8000));

        hardware.Ppu.Tick(172);

        Assert.Equal(0x12, hardware.Bus.Read(0x8000));
    }

    [Fact]
    public void Oam_is_blocked_in_mode_2_and_mode_3_but_dma_still_populates_memory()
    {
        var hardware = CreateHardware();
        hardware.Bus.Write(0xFF40, 0x00);
        hardware.Bus.Write(0xFE00, 0x12);
        hardware.Bus.Write(0xC000, 0x99);
        hardware.Bus.Write(0xFF40, 0x91);

        Assert.Equal(0xFF, hardware.Bus.Read(0xFE00));
        hardware.Bus.Write(0xFE00, 0x34);
        hardware.Bus.Write(0xFF46, 0xC0);

        hardware.Ppu.Tick(80);
        Assert.Equal(0xFF, hardware.Bus.Read(0xFE00));

        hardware.Ppu.Tick(172);
        Assert.Equal(0x99, hardware.Bus.Read(0xFE00));
    }

    [Fact]
    public void Oam_dma_stalls_cpu_until_transfer_window_has_elapsed()
    {
        var hardware = CreateHardware();
        hardware.Bus.Write(0xC000, 0x00);
        hardware.Cpu.Registers.PC = 0xC000;

        hardware.Bus.Write(0xFF46, 0xC0);

        for (var i = 0; i < 160; i++)
        {
            Assert.Equal(4, hardware.Cpu.Step());
            Assert.Equal(0xC000, hardware.Cpu.Registers.PC);
        }

        Assert.Equal(4, hardware.Cpu.Step());
        Assert.Equal(0xC001, hardware.Cpu.Registers.PC);
    }

    [Theory]
    [InlineData((ushort)0xFEA0)]
    [InlineData((ushort)0xFEFF)]
    public void Unusable_oam_range_reads_back_as_ff(ushort address)
    {
        var hardware = CreateHardware();

        hardware.Bus.Write(address, 0x12);

        Assert.Equal(0xFF, hardware.Bus.Read(address));
    }

    [Fact]
    public void Wave_ram_is_mapped_to_apu()
    {
        var hardware = CreateHardware();

        hardware.Bus.Write(0xFF30, 0xAB);

        Assert.Equal(0xAB, hardware.Bus.Read(0xFF30));
    }

    private static TestHardware CreateHardware()
    {
        var state = new EmulatorSessionState
        {
            RomPath = Path.Combine(AppContext.BaseDirectory, "Roms", "halt_bug.gb")
        };
        var cartridge = new Cartridge(state, NullLogger<Cartridge>.Instance);
        var interrupts = new InterruptController();
        var joypad = new Joypad(interrupts);
        var speedController = new SpeedController(cartridge);
        var timer = new GameBoy.Core.Timer(interrupts, speedController);
        var ppu = new Ppu(interrupts);
        var apu = new Apu();
        var serial = new Serial(interrupts, Array.Empty<IEmulatorSerialObserver>());
        var bus = new Bus(cartridge, serial, timer, ppu, apu, speedController, interrupts, joypad);
        var cpu = new Cpu(bus, interrupts, timer, ppu, apu, speedController, cartridge);

        return new TestHardware(bus, cpu, ppu, joypad, interrupts);
    }

    private sealed record TestHardware(Bus Bus, Cpu Cpu, Ppu Ppu, Joypad Joypad, InterruptController Interrupts);
}
