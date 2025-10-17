using System.Collections;
using System.Diagnostics;
using System.Numerics;

namespace GameBoy.Core;

[Singleton]
public sealed partial class Cpu(Bus bus, InterruptController interrupts)
{
    public CpuRegisters Registers = new()
    {
        AF = 0x01B0,
        BC = 0x0013,
        DE = 0x00D8,
        HL = 0x014d,
        SP = 0xFFFE,
        PC = 0x0100,
    };

    private bool _ime;
    private bool _imeLatch;
    private bool _halted;
    private bool _haltBug;

    private readonly ExecutionResultBuffer _executionResults = new(Capacity: 200);

    public IEnumerable<ExecutionResult> ExecutionResults => _executionResults;

    public byte Step()
    {
        if (_halted)
        {
            if (!interrupts.HasPending)
            {
                return 4;
            }

            _halted = false;

            if (_ime)
            {
                return ServiceInterrupts();
            }

            if (_imeLatch)
            {
                _ime = true;
                _imeLatch = false;
            }
        }

        var pc = Registers.PC;
        var instruction = FetchInstruction();

        var cycles = instruction.Exec(this);

        _executionResults.Add(new ExecutionResult(pc, instruction, cycles, Registers));

        if (_ime)
        {
            cycles += ServiceInterrupts();
        }

        if (_imeLatch)
        {
            _ime = true;
            _imeLatch = false;
        }

        return cycles;
    }

    private Instruction FetchInstruction()
    {
        var opcode = (Opcode)bus.Read(Registers.PC);

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
                Registers.PC++;
                break;
            case 2:
                instruction.N16 = bus.ReadWord(Registers.PC);
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

        PUSH(Registers.PC);
        Registers.PC = (ushort)(0x40 + BitOperations.TrailingZeroCount((byte)highestPriority) * 8);

        return 20;
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
