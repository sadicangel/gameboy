using GameBoy.Core;

namespace GameBoy;
public sealed class Emulator
{
    public Cpu Cpu;
    public Memory Memory;
    private int _timerCounter;
    private int _dividerCounter;
    private bool _isInterruptsEnabled;

    public Emulator()
    {
        Cpu = new();
        Memory = new();
        Memory.TimerFrequencyChanged += ResetTimerCount;
        _timerCounter = 1024;
        _dividerCounter = 0;
    }

    private void ResetTimerCount(TimerFrequency frequency)
    {
        _timerCounter = frequency switch
        {
            TimerFrequency.Hz4096 => 1024,
            TimerFrequency.Hz262144 => 16,
            TimerFrequency.Hz65536 => 64,
            TimerFrequency.Hz16384 => 256,
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null),
        };
    }

    public void Update()
    {
        const int MaxCycles = 69905;
        var cyclesThisUpdate = 0;

        while (cyclesThisUpdate < MaxCycles)
        {
            var cycles = Cpu.ExecuteNextInstruction();
            UpdateTimers(cycles);
            UpdateGraphics(cycles);
            DoInterrupts();
            cyclesThisUpdate += cycles;
        }

        Render();
    }

    private void Render() => throw new NotImplementedException();

    private void DoInterrupts()
    {
        if (!_isInterruptsEnabled)
        {
            return;
        }

        var interrupts = Memory.Interrupts;
        if (interrupts is Interrupts.None)
        {
            return;
        }

        var interruptSwitches = Memory.InterruptSwitches;
        if (interruptSwitches is InterruptSwitches.None)
        {
            return;
        }

        ReadOnlySpan<Interrupts> checks = [Interrupts.VBlank, Interrupts.LCDStat, Interrupts.Timer, Interrupts.Serial];
        foreach (var check in checks)
        {
            if (interrupts.HasFlag(check) && interruptSwitches.HasFlag((InterruptSwitches)check))
            {
                _isInterruptsEnabled = false;
                Memory.Interrupts &= ~check; // Clear the interrupt

                StackPush(Cpu.Registers.PC); // Push the current PC onto the stack

                Cpu.Registers.PC = check switch
                {
                    Interrupts.VBlank => (Register)0x0040,// VBlank interrupt handler address
                    Interrupts.LCDStat => (Register)0x0048,// LCD Stat interrupt handler address
                    Interrupts.Timer => (Register)0x0050,// Timer interrupt handler address
                    Interrupts.Serial => (Register)0x0058,// Serial interrupt handler address
                    _ => throw new ArgumentOutOfRangeException(nameof(check), check, null),
                };
            }
        }
    }
    private void UpdateGraphics(int cycles) => throw new NotImplementedException();
    private void UpdateTimers(int cycles)
    {
        _dividerCounter += cycles;
        if (_dividerCounter >= 255)
        {
            _dividerCounter = 0;
            Memory.DIV++;
        }

        // the clock must be enabled to update the clock
        if (!Memory.IsTimerEnabled)
        {
            return;
        }

        _timerCounter -= cycles;

        // enough cpu clock cycles have happened to update the timer
        if (_timerCounter <= 0)
        {
            // reset m_TimerTracer to the correct value
            ResetTimerCount(Memory.TimerFrequency);

            // timer about to overflow
            if (Memory.TIMA == 255)
            {
                Memory.TIMA = Memory.TMA;
                Memory.Interrupts |= Interrupts.Timer;
            }
            else
            {
                Memory.TIMA += 1;
            }
        }
    }
}
