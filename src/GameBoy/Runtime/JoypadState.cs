namespace GameBoy.Runtime;

public readonly record struct JoypadState(JoypadButtons Buttons)
{
    public static readonly JoypadState None = new(JoypadButtons.None);

    public bool A => (Buttons & JoypadButtons.A) != 0;
    public bool B => (Buttons & JoypadButtons.B) != 0;
    public bool Start => (Buttons & JoypadButtons.Start) != 0;
    public bool Select => (Buttons & JoypadButtons.Select) != 0;
    public bool Up => (Buttons & JoypadButtons.Up) != 0;
    public bool Down => (Buttons & JoypadButtons.Down) != 0;
    public bool Left => (Buttons & JoypadButtons.Left) != 0;
    public bool Right => (Buttons & JoypadButtons.Right) != 0;
    public bool Turbo => (Buttons & JoypadButtons.Turbo) != 0;
}

[Flags]
public enum JoypadButtons
{
    None = 0,
    A = 1 << 0,
    B = 1 << 1,
    Start = 1 << 2,
    Select = 1 << 3,
    Up = 1 << 4,
    Down = 1 << 5,
    Left = 1 << 6,
    Right = 1 << 7,
    Turbo = 1 << 8,
}
