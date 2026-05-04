using GameBoy.Runtime;

namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Joypad(InterruptController interrupts)
{
    private byte _selection = 0x30;

    public JoypadState CurrentState { get; private set; }

    public byte P1 => (byte)(0xC0 | _selection | ReadSelectedLines(CurrentState, _selection));

    public void Update(JoypadState state)
    {
        var previous = P1;
        CurrentState = state;
        RequestInterruptOnFallingEdge(previous, P1);
    }

    public void WriteP1(byte value)
    {
        var previous = P1;
        _selection = (byte)(value & 0x30);
        RequestInterruptOnFallingEdge(previous, P1);
    }

    private void RequestInterruptOnFallingEdge(byte previous, byte current)
    {
        if ((previous & ~current & 0x0F) != 0)
        {
            interrupts.Request(Interrupts.Joypad);
        }
    }

    private static byte ReadSelectedLines(JoypadState state, byte selection)
    {
        var lines = 0x0F;

        if ((selection & 0x10) == 0)
        {
            if (state.Right) lines &= 0x0E;
            if (state.Left) lines &= 0x0D;
            if (state.Up) lines &= 0x0B;
            if (state.Down) lines &= 0x07;
        }

        if ((selection & 0x20) == 0)
        {
            if (state.A) lines &= 0x0E;
            if (state.B) lines &= 0x0D;
            if (state.Select) lines &= 0x0B;
            if (state.Start) lines &= 0x07;
        }

        return (byte)lines;
    }
}
