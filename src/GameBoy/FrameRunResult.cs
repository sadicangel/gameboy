namespace GameBoy;

public readonly record struct FrameRunResult(
    uint FrameNumber,
    int CpuCyclesExecuted);
