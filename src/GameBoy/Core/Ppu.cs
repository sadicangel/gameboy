namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Ppu
{
    private const ushort VramOffset = 0x8000;
    private const ushort OamOffset = 0xFE00;

    private const byte LcdEnableMask = 0x80;
    private const byte WindowTileMapMask = 0x40;
    private const byte WindowEnableMask = 0x20;
    private const byte TileDataSelectMask = 0x10;
    private const byte BgTileMapMask = 0x08;
    private const byte ObjSizeMask = 0x04;
    private const byte ObjEnableMask = 0x02;
    private const byte BgEnableMask = 0x01;

    private const byte StatLycInterruptMask = 0x40;
    private const byte StatMode2InterruptMask = 0x20;
    private const byte StatMode1InterruptMask = 0x10;
    private const byte StatMode0InterruptMask = 0x08;

    private const int OamScanCycles = 80;
    private const int DrawingCycles = 172;
    private const int HBlankCycles = 204;
    private const int ScanlineCycles = OamScanCycles + DrawingCycles + HBlankCycles;
    private const int FirstVBlankLine = VideoFrame.Height;
    private const int LastVBlankLine = 153;
    private const int MaxSpritesPerScanline = 10;

    private static readonly byte[] s_dmgShades = [0xFF, 0xAA, 0x55, 0x00];

    private readonly InterruptController _interrupts;
    private readonly byte[] _vram = new byte[0x2000];
    private readonly byte[] _oam = new byte[0xA0];
    private readonly byte[] _bgColorIds = new byte[VideoFrame.Width];
    private readonly SpriteCandidate[] _scanlineSprites = new SpriteCandidate[MaxSpritesPerScanline];

    private byte[] _frontBuffer = CreateBlankFrameBuffer();
    private byte[] _backBuffer = CreateBlankFrameBuffer();
    private PpuMode _mode = PpuMode.OamScan;
    private int _modeCycles;
    private int _windowLine;
    private byte _lcdc = 0x91;
    private byte _statSelectBits;
    private byte _ly;
    private byte _lyc;
    private bool _coincidence;
    private bool _statInterruptLine;

    public Ppu(InterruptController interrupts)
    {
        _interrupts = interrupts;
        UpdateCoincidence();
    }

    public uint CompletedFrames { get; private set; }
    public VideoFrame LatestFrame => new(CompletedFrames, _frontBuffer);

    public byte LCDC
    {
        get => _lcdc;
        set
        {
            var wasEnabled = IsLcdEnabled;
            _lcdc = value;
            var isEnabled = IsLcdEnabled;

            if (wasEnabled && !isEnabled)
            {
                DisableLcd();
                return;
            }

            if (!wasEnabled && isEnabled)
            {
                EnableLcd();
            }
        }
    }

    public byte STAT
    {
        get => (byte)(0x80 | _statSelectBits | (_coincidence ? 0x04 : 0x00) | (byte)_mode);
        set
        {
            _statSelectBits = (byte)(value & 0x78);
            UpdateStatInterruptLine();
        }
    }

    public byte SCY { get; set; }
    public byte SCX { get; set; }

    public byte LY
    {
        get => _ly;
        set
        {
            _ = value;
            if (!IsLcdEnabled)
            {
                _ly = 0;
                UpdateCoincidence();
                return;
            }

            _windowLine = 0;
            SetLyAndMode(ly: 0, PpuMode.OamScan);
        }
    }

    public byte LYC
    {
        get => _lyc;
        set
        {
            _lyc = value;
            UpdateCoincidence();
        }
    }

    public byte BGP { get; set; } = 0xFC;
    public byte OBP0 { get; set; } = 0xFF;
    public byte OBP1 { get; set; } = 0xFF;
    public byte WY { get; set; }
    public byte WX { get; set; }

    public byte ReadVideoRam(ushort address)
        => CanAccessVram ? _vram[address - VramOffset] : (byte)0xFF;

    public void WriteVideoRam(ushort address, byte value)
    {
        if (!CanAccessVram)
        {
            return;
        }

        _vram[address - VramOffset] = value;
    }

    public byte ReadObjectAttributeMemory(ushort address)
        => CanAccessOam ? _oam[address - OamOffset] : (byte)0xFF;

    public void WriteObjectAttributeMemory(ushort address, byte value)
    {
        if (!CanAccessOam)
        {
            return;
        }

        _oam[address - OamOffset] = value;
    }

    public void DmaWriteObjectAttributeMemory(ushort address, byte value)
        => _oam[address - OamOffset] = value;

    public void Tick(uint cycles)
    {
        if (!IsLcdEnabled)
        {
            return;
        }

        var remaining = (int)cycles;
        while (remaining > 0)
        {
            var step = Math.Min(remaining, ModeDuration - _modeCycles);
            _modeCycles += step;
            remaining -= step;

            if (_modeCycles < ModeDuration)
            {
                continue;
            }

            AdvanceMode();
        }
    }

    private bool IsLcdEnabled => (_lcdc & LcdEnableMask) != 0;
    private bool CanAccessVram => !IsLcdEnabled || _mode != PpuMode.Drawing;
    private bool CanAccessOam => !IsLcdEnabled || (_mode != PpuMode.OamScan && _mode != PpuMode.Drawing);
    private int ModeDuration => _mode switch
    {
        PpuMode.OamScan => OamScanCycles,
        PpuMode.Drawing => DrawingCycles,
        PpuMode.HBlank => HBlankCycles,
        _ => ScanlineCycles
    };

    private void DisableLcd()
    {
        _mode = PpuMode.HBlank;
        _modeCycles = 0;
        _ly = 0;
        _windowLine = 0;
        _statInterruptLine = false;
        UpdateCoincidence();
    }

    private void EnableLcd()
    {
        _windowLine = 0;
        _statInterruptLine = false;
        SetLyAndMode(ly: 0, PpuMode.OamScan);
    }

    private void AdvanceMode()
    {
        switch (_mode)
        {
            case PpuMode.OamScan:
                SetMode(PpuMode.Drawing);
                break;

            case PpuMode.Drawing:
                RenderVisibleScanline();
                if (WindowDrawsOnCurrentScanline())
                {
                    _windowLine++;
                }

                SetMode(PpuMode.HBlank);
                break;

            case PpuMode.HBlank:
                AdvanceAfterHBlank();
                break;

            case PpuMode.VBlank:
                AdvanceVBlank();
                break;
        }
    }

    private void AdvanceAfterHBlank()
    {
        var nextLine = (byte)(_ly + 1);
        if (nextLine == FirstVBlankLine)
        {
            CompletedFrames++;
            (_frontBuffer, _backBuffer) = (_backBuffer, _frontBuffer);
            _interrupts.Request(Interrupts.VBlank);
            _windowLine = 0;
            SetLyAndMode(nextLine, PpuMode.VBlank);
            return;
        }

        SetLyAndMode(nextLine, PpuMode.OamScan);
    }

    private void AdvanceVBlank()
    {
        if (_ly >= LastVBlankLine)
        {
            _windowLine = 0;
            SetLyAndMode(ly: 0, PpuMode.OamScan);
            return;
        }

        _ly++;
        _modeCycles = 0;
        UpdateCoincidence();
    }

    private void SetMode(PpuMode mode)
    {
        _mode = mode;
        _modeCycles = 0;
        UpdateStatInterruptLine();
    }

    private void SetLyAndMode(byte ly, PpuMode mode)
    {
        _ly = ly;
        _mode = mode;
        _modeCycles = 0;
        UpdateCoincidence();
    }

    private void UpdateCoincidence()
    {
        _coincidence = _ly == _lyc;
        UpdateStatInterruptLine();
    }

    private void UpdateStatInterruptLine()
    {
        var nextInterruptLine =
            IsLcdEnabled
            && ((_coincidence && (_statSelectBits & StatLycInterruptMask) != 0)
                || (_mode == PpuMode.OamScan && (_statSelectBits & StatMode2InterruptMask) != 0)
                || (_mode == PpuMode.VBlank && (_statSelectBits & StatMode1InterruptMask) != 0)
                || (_mode == PpuMode.HBlank && (_statSelectBits & StatMode0InterruptMask) != 0));

        if (nextInterruptLine && !_statInterruptLine)
        {
            _interrupts.Request(Interrupts.LCD);
        }

        _statInterruptLine = nextInterruptLine;
    }

    private void RenderVisibleScanline()
    {
        if (_ly >= VideoFrame.Height)
        {
            return;
        }

        var lineOffset = _ly * VideoFrame.Width;
        var line = _backBuffer.AsSpan(lineOffset, VideoFrame.Width);
        var bgColorIds = _bgColorIds.AsSpan();
        bgColorIds.Fill(0);

        if ((_lcdc & BgEnableMask) != 0)
        {
            RenderBackground(line, bgColorIds);

            if (WindowDrawsOnCurrentScanline())
            {
                RenderWindow(line, bgColorIds);
            }
        }
        else
        {
            line.Fill(s_dmgShades[0]);
        }

        if ((_lcdc & ObjEnableMask) != 0)
        {
            RenderSprites(line, bgColorIds);
        }
    }

    private bool WindowDrawsOnCurrentScanline()
        => (_lcdc & (BgEnableMask | WindowEnableMask)) == (BgEnableMask | WindowEnableMask)
            && _ly >= WY
            && WY < VideoFrame.Height
            && WX <= 166;

    private void RenderBackground(Span<byte> line, Span<byte> bgColorIds)
    {
        var tileMapBase = (_lcdc & BgTileMapMask) != 0 ? 0x1C00 : 0x1800;
        var scrolledY = (SCY + _ly) & 0xFF;
        var tileY = scrolledY >> 3;
        var tileRow = scrolledY & 0x07;

        for (var screenX = 0; screenX < VideoFrame.Width; screenX++)
        {
            var scrolledX = (SCX + screenX) & 0xFF;
            var tileX = scrolledX >> 3;
            var tileNumber = _vram[tileMapBase + tileY * 32 + tileX];
            var tileAddress = BackgroundTileDataAddress(tileNumber);
            var colorId = ReadTileColor(tileAddress, tileRow, 7 - (scrolledX & 0x07));

            bgColorIds[screenX] = (byte)colorId;
            line[screenX] = MapPalette(BGP, colorId);
        }
    }

    private void RenderWindow(Span<byte> line, Span<byte> bgColorIds)
    {
        var windowStartX = WX - 7;
        var firstVisibleX = Math.Max(windowStartX, 0);
        var tileMapBase = (_lcdc & WindowTileMapMask) != 0 ? 0x1C00 : 0x1800;
        var tileY = _windowLine >> 3;
        var tileRow = _windowLine & 0x07;

        for (var screenX = firstVisibleX; screenX < VideoFrame.Width; screenX++)
        {
            var windowX = screenX - windowStartX;
            var tileX = windowX >> 3;
            var tileNumber = _vram[tileMapBase + tileY * 32 + tileX];
            var tileAddress = BackgroundTileDataAddress(tileNumber);
            var colorId = ReadTileColor(tileAddress, tileRow, 7 - (windowX & 0x07));

            bgColorIds[screenX] = (byte)colorId;
            line[screenX] = MapPalette(BGP, colorId);
        }
    }

    private void RenderSprites(Span<byte> line, Span<byte> bgColorIds)
    {
        var sprites = _scanlineSprites.AsSpan();
        var spriteCount = CollectVisibleSprites(sprites);

        for (var spriteIndex = spriteCount - 1; spriteIndex >= 0; spriteIndex--)
        {
            var sprite = sprites[spriteIndex];
            var spriteHeight = (_lcdc & ObjSizeMask) != 0 ? 16 : 8;
            var spriteRow = _ly - sprite.ScreenY;
            if ((sprite.Flags & 0x40) != 0)
            {
                spriteRow = spriteHeight - 1 - spriteRow;
            }

            var tileNumber = sprite.TileNumber;
            if (spriteHeight == 16)
            {
                tileNumber &= 0xFE;
                if (spriteRow >= 8)
                {
                    tileNumber++;
                    spriteRow -= 8;
                }
            }

            var tileAddress = tileNumber * 16 + spriteRow * 2;
            var low = _vram[tileAddress];
            var high = _vram[tileAddress + 1];
            var palette = (sprite.Flags & 0x10) != 0 ? OBP1 : OBP0;

            for (var pixel = 0; pixel < 8; pixel++)
            {
                var screenX = sprite.ScreenX + pixel;
                if ((uint)screenX >= VideoFrame.Width)
                {
                    continue;
                }

                var bit = (sprite.Flags & 0x20) != 0 ? pixel : 7 - pixel;
                var colorId = ColorId(low, high, bit);
                if (colorId == 0)
                {
                    continue;
                }

                if ((sprite.Flags & 0x80) != 0 && bgColorIds[screenX] != 0)
                {
                    continue;
                }

                line[screenX] = MapPalette(palette, colorId);
            }
        }
    }

    private int CollectVisibleSprites(Span<SpriteCandidate> sprites)
    {
        var spriteHeight = (_lcdc & ObjSizeMask) != 0 ? 16 : 8;
        var count = 0;

        for (var index = 0; index < 40 && count < MaxSpritesPerScanline; index++)
        {
            var oamOffset = index * 4;
            var screenY = _oam[oamOffset] - 16;
            if (_ly < screenY || _ly >= screenY + spriteHeight)
            {
                continue;
            }

            sprites[count++] = new SpriteCandidate(
                OamIndex: index,
                ScreenX: _oam[oamOffset + 1] - 8,
                ScreenY: screenY,
                TileNumber: _oam[oamOffset + 2],
                Flags: _oam[oamOffset + 3]);
        }

        for (var i = 1; i < count; i++)
        {
            var sprite = sprites[i];
            var insertionIndex = i - 1;

            while (insertionIndex >= 0 && SpritePrecedes(sprite, sprites[insertionIndex]))
            {
                sprites[insertionIndex + 1] = sprites[insertionIndex];
                insertionIndex--;
            }

            sprites[insertionIndex + 1] = sprite;
        }

        return count;
    }

    private static bool SpritePrecedes(in SpriteCandidate left, in SpriteCandidate right)
        => left.ScreenX < right.ScreenX || (left.ScreenX == right.ScreenX && left.OamIndex < right.OamIndex);

    private int BackgroundTileDataAddress(byte tileNumber)
        => (_lcdc & TileDataSelectMask) != 0
            ? tileNumber * 16
            : 0x1000 + unchecked((sbyte)tileNumber) * 16;

    private int ReadTileColor(int tileAddress, int tileRow, int bit)
    {
        var rowAddress = tileAddress + tileRow * 2;
        return ColorId(_vram[rowAddress], _vram[rowAddress + 1], bit);
    }

    private static int ColorId(byte low, byte high, int bit)
        => ((high >> bit) & 0x01) << 1 | ((low >> bit) & 0x01);

    private static byte MapPalette(byte palette, int colorId)
        => s_dmgShades[(palette >> (colorId * 2)) & 0x03];

    private static byte[] CreateBlankFrameBuffer()
    {
        var buffer = new byte[VideoFrame.Width * VideoFrame.Height];
        Array.Fill(buffer, s_dmgShades[0]);
        return buffer;
    }

    private enum PpuMode : byte
    {
        HBlank = 0,
        VBlank = 1,
        OamScan = 2,
        Drawing = 3,
    }

    private readonly record struct SpriteCandidate(int OamIndex, int ScreenX, int ScreenY, byte TileNumber, byte Flags);
}
