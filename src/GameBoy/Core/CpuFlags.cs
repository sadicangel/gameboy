namespace GameBoy.Core;

public record struct CpuFlags
{
    public byte _0;

    public bool Z { readonly get => _0.HasBitSet(7); set => _0.SetBit(7, value); }
    public bool N { readonly get => _0.HasBitSet(6); set => _0.SetBit(6, value); }
    public bool H { readonly get => _0.HasBitSet(5); set => _0.SetBit(5, value); }
    public bool C { readonly get => _0.HasBitSet(4); set => _0.SetBit(4, value); }
}
