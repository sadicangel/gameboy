using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameBoy.Core;

[Singleton]
public sealed class Bus(Cartridge cartridge, InterruptController interrupts, Timer timer)
{
    private VRam _vram;
    private WRam _wram;
    private ORam _oram;
    private HReg _hreg;
    private HRam _hram;

    private readonly StringBuilder _outputBuilder = new();
    public event Action<string>? Output;

    public byte SB { get => _hreg.Read(0xFF01); set => _hreg.Write(0xFF01, value); }
    public byte SC
    {
        get => _hreg.Read(0xFF02);
        set
        {
            if ((value & 0x81) == 0x81)
            {
                value = (byte)(value & ~0x80);
                interrupts.Request(Interrupts.Serial);
                _outputBuilder.Append((char)SB);
                if ((char)SB is '\n' or '\r' or '\0')
                {
                    var line = _outputBuilder.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                        Output?.Invoke(line);
                    _outputBuilder.Clear();
                }
            }
            _hreg.Write(0xFF02, value);
        }
    }

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

    public byte Read(ushort address) => address switch
    {
        < 0x8000 => cartridge.Read(address),
        < 0xA000 => _vram.Read(address),
        < 0xC000 => cartridge.Read(address),
        < 0xE000 => _wram.Read(address),
        < 0xFE00 => _wram.Read((ushort)(address - 0x1E00)),
        < 0xFEA0 => _oram.Read(address),
        < 0xFF00 => 0xFF,
        < 0xFF80 => ReadHReg(address),
        < 0xFFFF => _hram.Read(address),
        _ => interrupts.ReadIE(),
    };

    public ushort ReadWord(ushort address) => BinaryPrimitives.ReadUInt16LittleEndian([Read(address), Read((ushort)(address + 1))]);

    public void Write(ushort address, byte value)
    {
        switch (address)
        {
            case < 0x8000:
                cartridge.Write(address, value);
                return;

            case < 0xA000:
                _vram.Write(address, value);
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
                _oram.Write(address, value);
                return;

            case < 0xFF00:
                // Reserved
                return;

            case < 0xFF80:
                WriteHReg(address, value);
                return;

            case < 0xFFFF:
                _hram.Write(address, value);
                return;

            default: // 0xFFFF
                interrupts.WriteIE(value);
                return;
        }
    }

    public void WriteWord(ushort address, ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        Write(address, buffer[0]);
        Write((ushort)(address + 1), buffer[1]);
    }

    private byte ReadHReg(ushort address)
    {
        return address switch
        {
            0xFF01 => SB,
            0xFF02 => SC,
            0xFF04 => timer.DIV,
            0xFF05 => timer.TIMA,
            0xFF06 => timer.TMA,
            0xFF07 => timer.TAC,
            0xFF0F => interrupts.ReadIF(),
            _ => _hreg.Read(address),
        };
    }

    private void WriteHReg(ushort address, byte value)
    {
        switch (address)
        {
            case 0xFF01:
                SB = value;
                break;
            case 0xFF02:
                SC = value;
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
            case 0xFF0F:
                interrupts.WriteIF(value);
                break;
            default:
                _hreg.Write(address, value);
                break;
        }
    }

    [InlineArray(0x8000)]
    private struct VRam
    {
        private const ushort Offset = 0x8000;
        public byte E0;

        public readonly byte Read(ushort address) => this[address - Offset];
        public void Write(ushort address, byte value) => this[address - Offset] = value;
    }

    [InlineArray(0x2000)]
    private struct WRam
    {
        private const ushort Offset = 0xC000;
        public byte E0;

        public readonly byte Read(ushort address) => this[address - Offset];
        public void Write(ushort address, byte value) => this[address - Offset] = value;
    }

    [InlineArray(0x000A0)]
    private struct ORam
    {
        private const ushort Offset = 0xFE00;
        public byte E0;

        public readonly byte Read(ushort address) => this[address - Offset];
        public void Write(ushort address, byte value) => this[address - Offset] = value;
    }

    [InlineArray(0x0080)]
    private struct HReg
    {
        private const ushort Offset = 0xFF00;
        public byte E0;

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
