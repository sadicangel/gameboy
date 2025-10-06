using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace GameBoy.Core;

[Singleton]
public sealed class Bus(Cartridge cartridge)
{
    private readonly VRam _vram;
    private readonly WRam _wram;
    private readonly ORam _oram;
    private readonly HReg _hreg;
    private readonly HRam _hram;

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
                IF |= Interrupts.Serial;
                _outputBuilder.Append((char)SB);
                if ((char)SB is '\n' or '\r' or '\0')
                {
                    Output?.Invoke(_outputBuilder.ToString());
                    _outputBuilder.Clear();
                }
            }
            _hreg.Write(0xFF02, value);
        }
    }
    public byte DIV { get => _hreg.Read(0xFF04); set => _hreg.Write(0xFF04, value); }
    public byte TIMA { get => _hreg.Read(0xFF05); set => _hreg.Write(0xFF05, value); }
    public byte TMA { get => _hreg.Read(0xFF06); set => _hreg.Write(0xFF06, value); }
    public byte TAC { get => _hreg.Read(0xFF07); set => _hreg.Write(0xFF07, value); }

    public Interrupts IF { get => (Interrupts)(_hreg.Read(0xFF0F) | 0xE0); set => _hreg.Write(0xFF0F, (byte)((byte)value & 0x1F)); }
    public Interrupts IE { get; set; }

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
        < 0xFF80 => _hreg.Read(address),
        < 0xFFFF => _hram.Read(address),
        _ => (byte)IE,
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
                IE = (Interrupts)value;
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
                DIV = 0;
                break;
            case 0xFF05:
                TIMA = value;
                break;
            case 0xFF06:
                TMA = value;
                break;
            case 0xFF07:
                TAC = value;
                break;
            case 0xFF0F:
                IF = (Interrupts)value;
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

[Flags]
public enum Interrupts : byte
{
    None = 0,
    VBlank = 1 << 0,
    LCD = 1 << 1,
    Timer = 1 << 2,
    Serial = 1 << 3,
    Joypad = 1 << 4,
}
