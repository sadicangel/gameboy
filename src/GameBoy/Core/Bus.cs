using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Bus(Cartridge cartridge, Serial serial, Timer timer, Ppu ppu, Apu apu, SpeedController speedController, InterruptController interrupts, Joypad joypad)
{
    private const int OamDmaCpuCycles = 160 * 4;

    private WRam _wram = new();
    private HReg _hreg = new();
    private HRam _hram = new();
    private byte _oamDmaRegister;
    private int _oamDmaCyclesRemaining;

    // 0x0000 - 0x3FFF : ROM Bank 0
    // 0x4000 - 0x7FFF : ROM Bank 1 - Switchable
    // 0x8000 - 0x97FF : CHR RAM
    // 0x9800 - 0x9BFF : BG Map 1
    // 0x9C00 - 0x9FFF : BG Map 2
    // 0xA000 - 0xBFFF : Cartridge RAM
    // 0xC000 - 0xCFFF : RAM Bank 0
    // 0xD000 - 0xDFFF : RAM Bank 1-7 - switchable - Color only
    // 0xE000 - 0xFDFF : Reserved - Echo RAM
    // 0xFE00 - 0xFE9F : Object Attribute Memory
    // 0xFEA0 - 0xFEFF : Reserved - Unusable
    // 0xFF00 - 0xFF7F : I/O Registers
    // 0xFF80 - 0xFFFE : Zero Page

    public byte Read(ushort address) => ReadCore(address);

    public ushort ReadWord(ushort address) => BinaryPrimitives.ReadUInt16LittleEndian([Read(address), Read((ushort)(address + 1))]);

    public void Write(ushort address, byte value) => WriteCore(address, value);

    public void WriteWord(ushort address, ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        Write(address, buffer[0]);
        Write((ushort)(address + 1), buffer[1]);
    }

    public byte ConsumeOamDmaStallCycles()
    {
        if (_oamDmaCyclesRemaining <= 0)
        {
            return 0;
        }

        const byte stepCycles = 4;
        _oamDmaCyclesRemaining -= stepCycles;
        return stepCycles;
    }

    private byte ReadCore(ushort address) => address switch
    {
        < 0x8000 => cartridge.Read(address),
        < 0xA000 => ppu.ReadVideoRam(address),
        < 0xC000 => cartridge.Read(address),
        < 0xE000 => _wram.Read(address),
        < 0xFE00 => _wram.Read((ushort)(address - 0x1E00)),
        < 0xFEA0 => ppu.ReadObjectAttributeMemory(address),
        < 0xFF00 => 0xFF,
        < 0xFF80 => ReadHReg(address),
        < 0xFFFF => _hram.Read(address),
        _ => interrupts.ReadIE(),
    };

    private void WriteCore(ushort address, byte value)
    {
        switch (address)
        {
            case < 0x8000:
                cartridge.Write(address, value);
                return;

            case < 0xA000:
                ppu.WriteVideoRam(address, value);
                return;

            case < 0xC000:
                cartridge.Write(address, value);
                return;

            case < 0xE000:
                _wram.Write(address, value);
                return;

            case < 0xFE00:
                _wram.Write((ushort)(address - 0x1E00), value);
                return;

            case < 0xFEA0:
                ppu.WriteObjectAttributeMemory(address, value);
                return;

            case < 0xFF00:
                return;

            case < 0xFF80:
                WriteHReg(address, value);
                return;

            case < 0xFFFF:
                _hram.Write(address, value);
                return;

            default:
                interrupts.WriteIE(value);
                return;
        }
    }

    private byte ReadHReg(ushort address)
    {
        return address switch
        {
            0xFF00 => joypad.P1,
            0xFF01 => serial.SB,
            0xFF02 => serial.SC,
            0xFF04 => timer.DIV,
            0xFF05 => timer.TIMA,
            0xFF06 => timer.TMA,
            0xFF07 => timer.TAC,
            0xFF0F => interrupts.ReadIF(),
            >= 0xFF10 and <= 0xFF26 => apu.Read(address),
            0xFF40 => ppu.LCDC,
            0xFF41 => ppu.STAT,
            0xFF42 => ppu.SCY,
            0xFF43 => ppu.SCX,
            0xFF44 => ppu.LY,
            0xFF45 => ppu.LYC,
            0xFF46 => _oamDmaRegister,
            0xFF47 => ppu.BGP,
            0xFF48 => ppu.OBP0,
            0xFF49 => ppu.OBP1,
            0xFF4A => ppu.WY,
            0xFF4B => ppu.WX,
            0xFF4D => speedController.KEY1,
            _ => _hreg.Read(address),
        };
    }

    private void WriteHReg(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF00:
                joypad.WriteP1(value);
                break;
            case 0xFF01:
                serial.SB = value;
                break;
            case 0xFF02:
                serial.SC = value;
                break;
            case 0xFF04:
                timer.DIV = 0;
                break;
            case 0xFF05:
                timer.TIMA = value;
                break;
            case 0xFF06:
                timer.TMA = value;
                break;
            case 0xFF07:
                timer.TAC = value;
                break;
            case >= 0xFF10 and <= 0xFF26:
                apu.Write(address, value);
                break;
            case 0xFF40:
                ppu.LCDC = value;
                break;
            case 0xFF41:
                ppu.STAT = value;
                break;
            case 0xFF42:
                ppu.SCY = value;
                break;
            case 0xFF43:
                ppu.SCX = value;
                break;
            case 0xFF44:
                ppu.LY = value;
                break;
            case 0xFF45:
                ppu.LYC = value;
                break;
            case 0xFF46:
                StartOamDma(value);
                break;
            case 0xFF47:
                ppu.BGP = value;
                break;
            case 0xFF48:
                ppu.OBP0 = value;
                break;
            case 0xFF49:
                ppu.OBP1 = value;
                break;
            case 0xFF4A:
                ppu.WY = value;
                break;
            case 0xFF4B:
                ppu.WX = value;
                break;
            case 0xFF4D:
                speedController.WriteKEY1(value);
                break;
            case 0xFF0F:
                interrupts.WriteIF(value);
                break;
            default:
                _hreg.Write(address, value);
                break;
        }
    }

    private void StartOamDma(byte value)
    {
        _oamDmaRegister = value;

        var source = (ushort)(value << 8);
        for (ushort offset = 0; offset < 0xA0; offset++)
        {
            ppu.DmaWriteObjectAttributeMemory((ushort)(0xFE00 + offset), ReadCore((ushort)(source + offset)));
        }

        // DMG OAM DMA occupies the CPU for 160 M-cycles; we model that as
        // an immediate copy plus a short CPU stall so games don't race past it.
        _oamDmaCyclesRemaining = OamDmaCpuCycles;
    }

    [InlineArray(0x2000)]
    private struct WRam
    {
        private const ushort Offset = 0xC000;
        public byte E0;

        public readonly byte Read(ushort address) => this[address - Offset];
        public void Write(ushort address, byte value) => this[address - Offset] = value;
    }

    [InlineArray(0x0080)]
    private struct HReg
    {
        private const ushort Offset = 0xFF00;
        public byte E0;

        public HReg()
        {
            this[0x10] = 0x80;
            this[0x11] = 0xBF;
            this[0x12] = 0xF3;
            this[0x14] = 0xBF;
            this[0x16] = 0x3F;
            this[0x19] = 0xBF;
            this[0x1A] = 0x7F;
            this[0x1B] = 0xFF;
            this[0x1C] = 0x9F;
            this[0x1E] = 0xBF;
            this[0x20] = 0xFF;
            this[0x23] = 0xBF;
            this[0x24] = 0x77;
            this[0x25] = 0xF3;
            this[0x26] = 0xF1;
            this[0x40] = 0x91;
            this[0x47] = 0xFC;
            this[0x48] = 0xFF;
            this[0x49] = 0xFF;
            this[0x4D] = 0xFF;
        }

        public readonly byte Read(ushort address) => this[address - Offset];
        public void Write(ushort address, byte value) => this[address - Offset] = value;
    }

    [InlineArray(0x007F)]
    private struct HRam
    {
        private const ushort Offset = 0xFF80;
        public byte E0;

        public readonly byte Read(ushort address) => this[address - Offset];
        public void Write(ushort address, byte value) => this[address - Offset] = value;
    }
}
