using GameBoy.RayLibRuntime;

namespace GameBoy.Tests;

public sealed class RayLibRunnerTests
{
    [Fact]
    public void ConvertFrameToRgba_preserves_grayscale_pixel_values()
    {
        var pixels = new byte[VideoFrame.Width * VideoFrame.Height];
        pixels[0] = 0xFF;
        pixels[1] = 0xAA;
        pixels[2] = 0x55;
        pixels[3] = 0x00;

        var rgba = new byte[pixels.Length * 4];

        RayLibRunner.ConvertFrameToRgba(new VideoFrame(1, pixels), rgba);

        Assert.Equal([0xFF, 0xFF, 0xFF, 0xFF], rgba[..4]);
        Assert.Equal([0xAA, 0xAA, 0xAA, 0xFF], rgba[4..8]);
        Assert.Equal([0x55, 0x55, 0x55, 0xFF], rgba[8..12]);
        Assert.Equal([0x00, 0x00, 0x00, 0xFF], rgba[12..16]);
    }

    [Fact]
    public void GetTargetFrameDuration_uses_one_third_duration_when_turbo_is_requested()
    {
        var normalDuration = RayLibRunner.GetTargetFrameDuration(isTurboRequested: false);
        var turboDuration = RayLibRunner.GetTargetFrameDuration(isTurboRequested: true);

        Assert.Equal(TimeSpan.FromTicks(Math.Max(1L, normalDuration.Ticks / 3)), turboDuration);
    }
}
