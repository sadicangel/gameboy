namespace GameBoy.Core;

[Flags]
public enum Interrupts : byte
{
    None = 0,
    VBlank = 1 << 0,
    LCD = 1 << 1,
    Timer = 1 << 2,
    Serial = 1 << 3,
    Joypad = 1 << 4,

    All = VBlank | LCD | Timer | Serial | Joypad,
}

public static class InterruptExtensions
{
    extension(Interrupts interrupts)
    {
        public bool HasVBlank => (interrupts & Interrupts.VBlank) != Interrupts.None;
        public bool HasLCD => (interrupts & Interrupts.LCD) != Interrupts.None;
        public bool HasTimer => (interrupts & Interrupts.Timer) != Interrupts.None;
        public bool HasSerial => (interrupts & Interrupts.Serial) != Interrupts.None;
        public bool HasJoypad => (interrupts & Interrupts.Joypad) != Interrupts.None;

        public Interrupts HighestPriority
        {
            get
            {
                if (interrupts.HasVBlank) return Interrupts.VBlank;
                if (interrupts.HasLCD) return Interrupts.LCD;
                if (interrupts.HasTimer) return Interrupts.Timer;
                if (interrupts.HasSerial) return Interrupts.Serial;
                if (interrupts.HasJoypad) return Interrupts.Joypad;

                return Interrupts.None;
            }
        }
    }
}
