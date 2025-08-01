using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameBoy.Core;

[InlineArray(0x2000)]
public struct RamBank { byte _element0; }

public readonly struct Ram
{
    private readonly byte[] _bytes = new byte[0x8000];

    public ref RamBank this[int bank]
    {
        get
        {
            Debug.Assert(bank is >= 0 and <= 4, "Bank must be between 0 and 4.");

            return ref MemoryMarshal.AsRef<RamBank>(_bytes.AsSpan(bank * 0x2000, 0x2000));
        }
    }
    public Ram() => _bytes.AsSpan().Clear();
}
