namespace GameBoy.Core;

public record struct CpuFlags
{
    public byte _0;

    public bool Z { readonly get => _0.HasBitSet(7); set => _0.SetBit(7, value); }
    public bool N { readonly get => _0.HasBitSet(6); set => _0.SetBit(6, value); }
    public bool H { readonly get => _0.HasBitSet(5); set => _0.SetBit(5, value); }
    public bool C { readonly get => _0.HasBitSet(4); set => _0.SetBit(4, value); }


    public void SetZ(int b) => Z = (byte)b == 0;

    public void SetC(int i) => C = (i >> 8) != 0;

    public void SetH(byte b1, byte b2) => H = ((b1 & 0xF) + (b2 & 0xF)) > 0xF;

    public void SetH(ushort w1, ushort w2) => H = ((w1 & 0xFFF) + (w2 & 0xFFF)) > 0xFFF;

    public void SetHCarry(byte b1, byte b2) => H = ((b1 & 0xF) + (b2 & 0xF)) >= 0xF;

    public void SetHSub(byte b1, byte b2) => H = (b1 & 0xF) < (b2 & 0xF);

    public void SetHSubCarry(byte b1, byte b2) => H = (b1 & 0xF) < ((b2 & 0xF) + (C ? 1 : 0));
}
