namespace GameBoy.Tests;

public sealed class PpuTests
{
    [Fact]
    public void Tick_advances_visible_scanline_modes_in_order()
    {
        var (ppu, _) = CreateSeedablePpu();
        ppu.LCDC = 0x91;

        Assert.Equal(0x02, ppu.STAT & 0x03);
        Assert.Equal((byte)0, ppu.LY);

        ppu.Tick(80);
        Assert.Equal(0x03, ppu.STAT & 0x03);
        Assert.Equal((byte)0, ppu.LY);

        ppu.Tick(172);
        Assert.Equal(0x00, ppu.STAT & 0x03);
        Assert.Equal((byte)0, ppu.LY);

        ppu.Tick(204);
        Assert.Equal(0x02, ppu.STAT & 0x03);
        Assert.Equal((byte)1, ppu.LY);
    }

    [Fact]
    public void Tick_enters_vblank_completes_a_frame_and_wraps_after_line_153()
    {
        var (ppu, interrupts) = CreateSeedablePpu();
        ppu.LCDC = 0x91;

        ppu.Tick(456u * 144u);

        Assert.Equal(1u, ppu.CompletedFrames);
        Assert.Equal((byte)144, ppu.LY);
        Assert.Equal(0x01, ppu.STAT & 0x03);
        Assert.NotEqual(0, interrupts.ReadIF() & (byte)Interrupts.VBlank);

        ppu.Tick(456u * 10u);

        Assert.Equal((byte)0, ppu.LY);
        Assert.Equal(0x02, ppu.STAT & 0x03);
    }

    [Fact]
    public void LatestFrame_updates_only_when_vblank_swaps_the_buffers()
    {
        var (ppu, _) = CreateSeedablePpu();
        FillTile(ppu, tileIndex: 0, low: 0xFF, high: 0xFF);
        SetBackgroundTile(ppu, tileX: 0, tileY: 0, tileIndex: 0);
        ppu.LCDC = 0x91;

        Assert.Equal(0u, ppu.LatestFrame.FrameNumber);
        Assert.Equal((byte)0xFF, ppu.LatestFrame.Pixels.Span[0]);

        ppu.Tick(252);

        Assert.Equal(0u, ppu.CompletedFrames);
        Assert.Equal((byte)0xFF, ppu.LatestFrame.Pixels.Span[0]);

        ppu.Tick(204u + 456u * 143u);

        Assert.Equal(1u, ppu.LatestFrame.FrameNumber);
        Assert.Equal((byte)0x00, ppu.LatestFrame.Pixels.Span[0]);
    }

    [Fact]
    public void LYC_updates_coincidence_immediately_and_interrupts_only_on_rising_edges()
    {
        var (ppu, interrupts) = CreateSeedablePpu();
        ppu.LCDC = 0x91;
        ppu.LYC = 1;
        interrupts.WriteIF(0);

        ppu.STAT = 0x40;
        Assert.Equal(0, interrupts.ReadIF() & (byte)Interrupts.LCD);
        Assert.Equal(0x00, ppu.STAT & 0x04);

        ppu.LYC = 0;
        Assert.Equal(0x04, ppu.STAT & 0x04);
        Assert.NotEqual(0, interrupts.ReadIF() & (byte)Interrupts.LCD);

        interrupts.WriteIF(0);
        ppu.Tick(4);
        Assert.Equal(0, interrupts.ReadIF() & (byte)Interrupts.LCD);

        ppu.LYC = 1;
        Assert.Equal(0x00, ppu.STAT & 0x04);

        ppu.LYC = 0;
        Assert.NotEqual(0, interrupts.ReadIF() & (byte)Interrupts.LCD);
    }

    [Fact]
    public void Stat_mode_2_interrupts_are_edge_triggered()
    {
        var (ppu, interrupts) = CreateSeedablePpu();
        ppu.LCDC = 0x91;
        interrupts.WriteIF(0);

        ppu.STAT = 0x20;
        Assert.NotEqual(0, interrupts.ReadIF() & (byte)Interrupts.LCD);

        interrupts.WriteIF(0);
        ppu.Tick(4);
        Assert.Equal(0, interrupts.ReadIF() & (byte)Interrupts.LCD);

        ppu.Tick(452);
        Assert.NotEqual(0, interrupts.ReadIF() & (byte)Interrupts.LCD);
    }

    [Fact]
    public void Stat_hblank_interrupts_fire_when_entering_hblank()
    {
        var (ppu, interrupts) = CreateSeedablePpu();
        ppu.LCDC = 0x91;
        interrupts.WriteIF(0);

        ppu.STAT = 0x08;
        Assert.Equal(0, interrupts.ReadIF() & (byte)Interrupts.LCD);

        ppu.Tick(252);
        Assert.NotEqual(0, interrupts.ReadIF() & (byte)Interrupts.LCD);

        interrupts.WriteIF(0);
        ppu.Tick(4);
        Assert.Equal(0, interrupts.ReadIF() & (byte)Interrupts.LCD);
    }

    [Fact]
    public void Renders_background_using_scroll_and_palette_mapping()
    {
        var (ppu, _) = CreateSeedablePpu();
        FillTile(ppu, tileIndex: 1, low: 0xFF, high: 0x00);
        FillTile(ppu, tileIndex: 2, low: 0x00, high: 0xFF);
        SetBackgroundTile(ppu, tileX: 0, tileY: 0, tileIndex: 1);
        SetBackgroundTile(ppu, tileX: 1, tileY: 0, tileIndex: 2);
        ppu.SCX = 4;
        ppu.LCDC = 0x91;

        RunUntilFrameReady(ppu);
        var pixels = ppu.LatestFrame.Pixels.Span;

        Assert.Equal((byte)0xAA, pixels[0]);
        Assert.Equal((byte)0xAA, pixels[3]);
        Assert.Equal((byte)0x55, pixels[4]);
        Assert.Equal((byte)0x55, pixels[11]);
    }

    [Fact]
    public void Window_overrides_background_when_visible()
    {
        var (ppu, _) = CreateSeedablePpu();
        FillTile(ppu, tileIndex: 1, low: 0xFF, high: 0x00);
        FillTile(ppu, tileIndex: 2, low: 0x00, high: 0xFF);
        SetBackgroundTile(ppu, tileX: 0, tileY: 0, tileIndex: 1);
        SetBackgroundTile(ppu, tileX: 0, tileY: 0, tileIndex: 2, upperMap: true);
        ppu.WY = 0;
        ppu.WX = 10;
        ppu.LCDC = 0xF1;

        RunUntilFrameReady(ppu);
        var pixels = ppu.LatestFrame.Pixels.Span;

        Assert.Equal((byte)0xAA, pixels[0]);
        Assert.Equal((byte)0xAA, pixels[2]);
        Assert.Equal((byte)0x55, pixels[3]);
        Assert.Equal((byte)0x55, pixels[7]);
    }

    [Fact]
    public void Sprites_apply_flip_and_palette_rules()
    {
        var (ppu, _) = CreateSeedablePpu();
        ppu.OBP1 = 0x6C;
        WriteTileRow(ppu, tileIndex: 1, row: 7, low: 0xF0, high: 0x0F);
        SetSprite(ppu, spriteIndex: 0, y: 16, x: 8, tileIndex: 1, flags: 0x70);
        ppu.LCDC = 0x93;

        RunUntilFrameReady(ppu);
        var pixels = ppu.LatestFrame.Pixels.Span;

        Assert.Equal((byte)0x55, pixels[0]);
        Assert.Equal((byte)0x55, pixels[3]);
        Assert.Equal((byte)0x00, pixels[4]);
        Assert.Equal((byte)0x00, pixels[7]);
    }

    [Fact]
    public void Sprite_priority_allows_bg_color_0_but_hides_behind_non_zero_bg_pixels()
    {
        var (ppu, _) = CreateSeedablePpu();
        WriteTileRow(ppu, tileIndex: 1, row: 0, low: 0x40, high: 0x00);
        FillTile(ppu, tileIndex: 2, low: 0x00, high: 0xFF);
        SetBackgroundTile(ppu, tileX: 0, tileY: 0, tileIndex: 1);
        SetSprite(ppu, spriteIndex: 0, y: 16, x: 8, tileIndex: 2, flags: 0x80);
        ppu.LCDC = 0x93;

        RunUntilFrameReady(ppu);
        var pixels = ppu.LatestFrame.Pixels.Span;

        Assert.Equal((byte)0x55, pixels[0]);
        Assert.Equal((byte)0xAA, pixels[1]);
    }

    private static (Ppu Ppu, InterruptController Interrupts) CreateSeedablePpu()
    {
        var interrupts = new InterruptController();
        var ppu = new Ppu(interrupts)
        {
            LCDC = 0x00,
            BGP = 0xE4,
            OBP0 = 0xE4,
            OBP1 = 0xE4,
        };

        return (ppu, interrupts);
    }

    private static void RunUntilFrameReady(Ppu ppu)
        => ppu.Tick(456u * (uint)VideoFrame.Height);

    private static void FillTile(Ppu ppu, int tileIndex, byte low, byte high)
    {
        for (var row = 0; row < 8; row++)
        {
            WriteTileRow(ppu, tileIndex, row, low, high);
        }
    }

    private static void WriteTileRow(Ppu ppu, int tileIndex, int row, byte low, byte high)
    {
        var address = (ushort)(0x8000 + tileIndex * 16 + row * 2);
        ppu.WriteVideoRam(address, low);
        ppu.WriteVideoRam((ushort)(address + 1), high);
    }

    private static void SetBackgroundTile(Ppu ppu, int tileX, int tileY, byte tileIndex, bool upperMap = false)
    {
        var mapBase = upperMap ? 0x9C00 : 0x9800;
        ppu.WriteVideoRam((ushort)(mapBase + tileY * 32 + tileX), tileIndex);
    }

    private static void SetSprite(Ppu ppu, int spriteIndex, byte y, byte x, byte tileIndex, byte flags)
    {
        var address = (ushort)(0xFE00 + spriteIndex * 4);
        ppu.WriteObjectAttributeMemory(address, y);
        ppu.WriteObjectAttributeMemory((ushort)(address + 1), x);
        ppu.WriteObjectAttributeMemory((ushort)(address + 2), tileIndex);
        ppu.WriteObjectAttributeMemory((ushort)(address + 3), flags);
    }
}
