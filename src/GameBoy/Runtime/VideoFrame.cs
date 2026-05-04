namespace GameBoy.Runtime;

public readonly record struct VideoFrame(
    uint FrameNumber,
    ReadOnlyMemory<byte> Pixels)
{
    public const int Width = 160;
    public const int Height = 144;
}
