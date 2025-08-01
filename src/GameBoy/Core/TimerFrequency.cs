
namespace GameBoy.Core;

public enum TimerFrequency : byte
{
    Hz4096 = 0b00,
    Hz262144 = 0b01,
    Hz65536 = 0b10,
    Hz16384 = 0b11,
}
