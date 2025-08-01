namespace GameBoy.Core;

public struct Cpu
{
    public const int ClockSpeed = 4194304;

    public Registers Registers = new();
    public Cpu() { }

    public int ExecuteNextInstruction()
    {
        return 1;
    }
}
