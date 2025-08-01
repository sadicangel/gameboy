using System.Diagnostics;

namespace GameBoy.Core;

public struct Memory
{
    public Cartridge Cartridge = new();
    public Ram Ram = new();
    public Rom Rom = new();
    public bool IsRamEnabled = false;
    public byte CurrentRamBank = 0;
    public byte CurrentRomBank = 1;
    public bool IsRomBanking;

    /// <summary>Divider Register (DIV)</summary>
    public byte DIV { readonly get => Rom[0xFF04]; set => Rom[0xFF04] = value; }
    /// <summary>Timer Counter Register (TIMA)</summary>
    public byte TIMA { readonly get => Rom[0xFF05]; set => Rom[0xFF05] = value; }
    /// <summary>Timer Modulo Register (TMA)</summary>
    public byte TMA { readonly get => Rom[0xFF06]; set => Rom[0xFF06] = value; }
    /// <summary>Timer Control Register (TMC)</summary>
    public byte TMC
    {
        readonly get => Rom[0xFF07];
        set
        {
            if (Rom[0xFF07] != value)
            {
                Rom[0xFF07] = value;
                TimerFrequencyChanged?.Invoke(TimerFrequency);
            }
        }
    }
    public readonly bool IsTimerEnabled => TMC.HasBitSet(2);
    public readonly TimerFrequency TimerFrequency => (TMC & 0x03) switch
    {
        0b00 => TimerFrequency.Hz4096,
        0b01 => TimerFrequency.Hz262144,
        0b10 => TimerFrequency.Hz65536,
        0b11 => TimerFrequency.Hz16384,
        _ => throw new UnreachableException()
    };

    public Interrupts Interrupts { readonly get => (Interrupts)Rom[0xFF0F]; set => Rom[0xFF0F] = (byte)value; }
    public InterruptSwitches InterruptSwitches { readonly get => (InterruptSwitches)Rom[0xFFFF]; set => Rom[0xFFFF] = (byte)value; }

    public event Action<TimerFrequency>? TimerFrequencyChanged;

    public Memory()
    {

    }

    public readonly byte Read(ushort address)
    {
        return address switch
        {
            >= 0x4000 and <= 0x7FFF => Cartridge[address + (ushort)((CurrentRomBank - 1u) * 0x4000u)],
            >= 0xA000 and <= 0xBFFF => Ram[CurrentRamBank][address - 0xA000],
            _ => Rom[address],
        };
    }

    public void Write(ushort address, byte data)
    {
        switch (address)
        {
            case 0xFF04:
                DIV = 0x00; // Reset Divider Register
                break;

            case 0xFF07:
                TMC = data;
                break;

            case < 0x2000 when Cartridge.IsMbc1 || Cartridge.IsMbc2:
                SetRamEnabled(address, data);
                break;

            case < 0x4000:
                ChangeLoRomBank(data);
                break;

            case < 0x6000:
                if (Cartridge.IsMbc1)
                {
                    if (IsRomBanking)
                    {
                        ChangeHiRomBank(data);
                    }
                    else
                    {
                        ChangeRamBank(data);
                    }
                }
                break;

            case < 0x8000:
                SetRomRamMode(data);
                break;

            case >= 0xE000 and <= 0xFE00:
                Rom[address] = data;
                Write((ushort)(address - 0x2000), data); // Mirror RAM in E000 to C000
                break;
            case >= 0xFEA0 and <= 0xFEFF:
                // Writing to OAM (Object Attribute Memory), which is read-only in this implementation
                break;
            default:
                Rom[address] = data;
                break;
        }
    }

    private void ChangeRamBank(byte data) =>
        CurrentRamBank = (byte)(data & 0x03);

    private void SetRomRamMode(byte data)
    {
        IsRomBanking = (data & 0x1) == 0;
        if (IsRomBanking)
            CurrentRamBank = 0;
    }

    private void SetRamEnabled(ushort address, byte data)
    {
        if (!Cartridge.IsMbc1 && !Cartridge.IsMbc2)
        {
            return; // Only MBC1 and MBC2 support RAM enable/disable
        }

        if (Cartridge.IsMbc2 && address.HasBitSet(4))
        {
            return;
        }

        IsRamEnabled = (data & 0xF) switch
        {
            0x0 => false,
            0xA => true,
            _ => IsRamEnabled // Keep current state
        };
    }

    private void ChangeLoRomBank(byte data)
    {
        if (!Cartridge.IsMbc1 && !Cartridge.IsMbc2)
        {
            return; // Only MBC1 and MBC2 support ROM bank switching
        }

        if (Cartridge.IsMbc2)
        {
            CurrentRomBank = (byte)(data & 0x0F);
        }
        else
        {
            CurrentRomBank &= 0xE0;
            CurrentRomBank |= (byte)(data & 0x1F);
        }

        if (CurrentRomBank == 0)
        {
            CurrentRomBank = 1;
        }
    }

    private void ChangeHiRomBank(byte data)
    {
        CurrentRomBank &= 0x1F;
        CurrentRomBank |= (byte)(data & 0xE0);

        if (CurrentRomBank == 0)
        {
            CurrentRomBank = 1;
        }
    }

    public void SetInterruptFlag(Interrupts flag)
    {
        Interrupts |= flag;
    }
}
