using System.Collections;
using System.Diagnostics;
using System.Numerics;

namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed partial class Cpu(Bus bus, InterruptController interrupts, Timer timer, Ppu ppu, Apu apu, SpeedController speedController, Cartridge cartridge)
{
    public CpuRegisters Registers = cartridge.Header.CgbFlag == 0xC0
        ? new CpuRegisters
        {
            AF = 0x1180,
            BC = 0x0000,
            DE = 0xFF56,
            HL = 0x000D,
            SP = 0xFFFE,
            PC = 0x0100,
        }
        : new CpuRegisters
        {
            AF = 0x01B0,
            BC = 0x0013,
            DE = 0x00D8,
            HL = 0x014D,
            SP = 0xFFFE,
            PC = 0x0100,
        };

    private bool _ime;
    private bool _imeLatch;
    private bool _halted;
    private bool _haltBug;
    private bool _interruptReturnsToHalt;

    private ulong _instructionCount = 0;
    private byte _instructionCyclesConsumed = 0;
    private readonly ExecutionResultBuffer _executionResults = new(Capacity: 200);

    public IEnumerable<ExecutionResult> ExecutionResults => _executionResults;

    public byte Step()
    {
        var oamDmaCycles = bus.ConsumeOamDmaStallCycles();
        if (oamDmaCycles != 0)
        {
            TickHardware(oamDmaCycles);
            return oamDmaCycles;
        }

        var enableImeAfterInstruction = _imeLatch;

        if (_halted)
        {
            if (interrupts.HasPending)
            {
                _halted = false;

                if (_ime)
                {
                    return ServiceInterruptsAndTick();
                }
            }

            TickHardware(4);
            if (!interrupts.HasPending)
            {
                return 4;
            }

            _halted = false;

            if (_ime)
            {
                return (byte)(4 + ServiceInterruptsAndTick());
            }

            return 4;
        }

        _instructionCyclesConsumed = 0;
        var pc = Registers.PC;
        var instruction = FetchInstruction();

        var cycles = instruction.Exec(this);
        TickHardware((byte)(cycles - _instructionCyclesConsumed));
        _instructionCount++;
        _executionResults.Add(new ExecutionResult(pc, instruction, cycles, Registers));

        if (enableImeAfterInstruction && _imeLatch)
        {
            _ime = true;
            _imeLatch = false;
        }

        if (_ime)
        {
            cycles += ServiceInterruptsAndTick();
        }

        return cycles;
    }

    private Instruction FetchInstruction()
    {
        var opcode = (Opcode)bus.Read(Registers.PC);
        AdvanceInstruction(4);

        if (_haltBug)
            _haltBug = false;
        else
            Registers.PC++;

        var instruction = new Instruction(opcode);

        switch (opcode.ImmediateByteCount)
        {
            case 0:
                break;
            case 1:
                instruction.N8 = bus.Read(Registers.PC);
                AdvanceInstruction(4);
                Registers.PC++;
                break;
            case 2:
                instruction.N16 = bus.ReadWord(Registers.PC);
                AdvanceInstruction(8);
                Registers.PC++;
                Registers.PC++;
                break;
            default:
                throw new UnreachableException();
        }

        return instruction;
    }

    private byte ServiceInterrupts()
    {
        if (!interrupts.TryPopHighestPending(out var highestPriority))
        {
            return 0;
        }

        _ime = false;

        var returnAddress = Registers.PC;
        if (_interruptReturnsToHalt)
        {
            returnAddress--;
            _interruptReturnsToHalt = false;
            _haltBug = false;
        }

        PUSH(returnAddress);
        Registers.PC = (ushort)(0x40 + BitOperations.TrailingZeroCount((byte)highestPriority) * 8);

        return 20;
    }

    private byte ServiceInterruptsAndTick()
    {
        var cycles = ServiceInterrupts();
        if (cycles != 0)
        {
            TickHardware(cycles);
        }

        return cycles;
    }

    private void TickHardware(byte cycles)
    {
        timer.Tick(cycles);
        var normalCycles = speedController.ToNormalCycles(cycles);
        ppu.Tick(normalCycles);
        apu.Tick(normalCycles);
    }

    private void AdvanceInstruction(byte cycles)
    {
        TickHardware(cycles);
        _instructionCyclesConsumed += cycles;
    }

    private readonly record struct ExecutionResultBuffer(int Capacity) : IEnumerable<ExecutionResult>
    {
        private readonly Queue<ExecutionResult> _buffer = new(Capacity);

        public void Add(ExecutionResult result)
        {
            if (_buffer.Count >= Capacity)
            {
                _buffer.Dequeue();
            }

            _buffer.Enqueue(result);
        }

        public IEnumerator<ExecutionResult> GetEnumerator() => ((IEnumerable<ExecutionResult>)_buffer).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_buffer).GetEnumerator();
    }
}

public readonly record struct ExecutionResult(ushort PC, Instruction Instruction, int Cycles, CpuRegisters Registers);
