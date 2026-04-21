namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class SpeedController(Cartridge cartridge)
{
    private bool _speedSwitchArmed;

    public bool IsCgbMode => cartridge.Header.CgbFlag == 0xC0;
    public bool IsDoubleSpeed { get; private set; }

    public byte KEY1
        => !IsCgbMode
            ? (byte)0xFF
            : (byte)(0x7E | (IsDoubleSpeed ? 0x80 : 0x00) | (_speedSwitchArmed ? 0x01 : 0x00));

    public void WriteKEY1(byte value)
    {
        if (!IsCgbMode)
        {
            return;
        }

        _speedSwitchArmed = (value & 0x01) != 0;
    }

    public bool TryToggleSpeed()
    {
        if (!IsCgbMode || !_speedSwitchArmed)
        {
            return false;
        }

        IsDoubleSpeed = !IsDoubleSpeed;
        _speedSwitchArmed = false;
        return true;
    }

    public uint ToNormalCycles(uint cpuCycles)
        => IsDoubleSpeed
            ? cpuCycles / 2
            : cpuCycles;
}
