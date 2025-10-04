using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace GameBoy.Core;

[Singleton]
public sealed class Mmu(Cartridge cartridge)
{
    private readonly WRam _wram;
    private readonly HRam _hram;

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
        < 0xA000 => throw new NotImplementedException("CHR RAM"),
        < 0xC000 => cartridge.Read(address),
        < 0xE000 => _wram.Read(address),
        < 0xFE00 => _wram.Read(address),
        < 0xFEA0 => throw new NotImplementedException("Object Attribute RAM"),
        < 0xFF00 => 0x00,
        < 0xFF80 => throw new NotImplementedException("I/O Registers"),
        < 0xFFFF => _hram.Read(address),
        _ => throw new NotImplementedException("CPU set enable register"),
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
                throw new NotImplementedException("CHR RAM");

            case < 0xC000:
                cartridge.Write(address, value);
                return;

            case < 0xE000:
                _wram.Write(address, value);
                return;

            case < 0xFE00:
                _wram.Write(address, value);
                return;

            case < 0xFEA0:
                throw new NotImplementedException("Object Attribute RAM");

            case < 0xFF00:
                // Reserved
                return;

            case < 0xFF80:
                throw new NotImplementedException("I/O Registers");

            case < 0xFFFF:
                _hram.Write(address, value);
                return;

            default:
                throw new NotImplementedException("CPU set enable register");
        }
    }

    public void WriteWord(ushort address, ushort value)
    {
        Span<byte> buffer = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        Write(address, buffer[0]);
        Write((ushort)(address + 1), buffer[1]);
    }

    [InlineArray(0x2000)]
    private struct WRam
    {
        private const ushort Offset = 0xC000;
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
