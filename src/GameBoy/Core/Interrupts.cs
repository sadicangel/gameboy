namespace GameBoy.Core;

[Flags]
public enum Interrupts : byte
{
    None = 0x0000,
    VBlank = 0x01, // V-Blank Interrupt
    LCDStat = 0x02, // LCD Status Interrupt
    Timer = 0x04, // Timer Interrupt
    Serial = 0x08, // Serial Interrupt
}

[Flags]
public enum InterruptSwitches : byte
{
    None = 0x0000,
    VBlankEnabled = 0x01, // V-Blank Interrupt Enabled
    LCDStatEnabled = 0x02, // LCD Status Interrupt Enabled
    TimerEnabled = 0x04, // Timer Interrupt Enabled
    SerialEnabled = 0x08, // Serial Interrupt Enabled
}
