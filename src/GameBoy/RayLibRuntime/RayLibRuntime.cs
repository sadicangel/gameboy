using System.Threading;
using RayLibNet.Interop;

namespace GameBoy.RayLibRuntime;

internal sealed class RayLibRuntime : IEmulatorRuntime
{
    private static readonly TimeSpan s_targetFrameDuration = TimeSpan.FromSeconds(154d * 456d / 4_194_304d);
    private static readonly TimeSpan s_turboFrameDuration = TimeSpan.FromTicks(Math.Max(1L, s_targetFrameDuration.Ticks / 3));
    private readonly Lock _lock = new();

    private JoypadState _joypad = default;
    private VideoFrame _frame;
    private AudioBuffer _audio;
    private bool _isTurboRequested;


    private Texture _texture;
    private readonly byte[] _rgbaBuffer = new byte[VideoFrame.Width * VideoFrame.Height * 4];

    public TimeSpan TargetFrameDuration
    {
        get
        {
            lock (_lock)
            {
                return GetTargetFrameDuration(_isTurboRequested);
            }
        }
    }

    /// <inheritdoc />
    public JoypadState PollJoypad()
    {
        lock (_lock)
        {
            return _joypad;
        }
    }

    /// <inheritdoc />
    public void PresentFrame(VideoFrame frame)
    {
        lock (_lock)
        {
            _frame = frame;
        }
    }

    /// <inheritdoc />
    public void SubmitAudio(AudioBuffer audio)
    {
        lock (_lock)
        {
            _audio = audio;
        }
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        Load("GameBoy"u8);
        try
        {
            while (!cancellationToken.IsCancellationRequested && RayLib.WindowShouldClose() == 0)
            {
                ReadInput();
                Draw();
            }
        }
        finally
        {
            Unload();
        }

        return Task.CompletedTask;
    }

    private void Load(ReadOnlySpan<byte> title)
    {
        unsafe
        {
            fixed (byte* titlePtr = title)
                RayLib.InitWindow(160 * 4, 144 * 4, titlePtr);
        }

        RayLib.SetTargetFPS(60);

        var image = RayLib.GenImageColor(VideoFrame.Width, VideoFrame.Height, Color.Black);
        try
        {
            _texture = RayLib.LoadTextureFromImage(image);
        }
        finally
        {
            RayLib.UnloadImage(image);
        }
    }

    private void Unload()
    {
        if (RayLib.IsWindowReady() == 0) return;
        RayLib.CloseWindow();
        RayLib.UnloadTexture(_texture);
    }

    private void ReadInput()
    {
        lock (_lock)
        {
            _isTurboRequested = RayLib.IsKeyDown((int)KeyboardKey.KEY_SPACE) != 0;
            _joypad = new JoypadState(
                A: RayLib.IsKeyDown((int)KeyboardKey.KEY_Z) != 0,
                B: RayLib.IsKeyDown((int)KeyboardKey.KEY_X) != 0,
                Start: RayLib.IsKeyDown((int)KeyboardKey.KEY_ENTER) != 0,
                Select: RayLib.IsKeyDown((int)KeyboardKey.KEY_BACKSPACE) != 0,
                Up: RayLib.IsKeyDown((int)KeyboardKey.KEY_UP) != 0,
                Down: RayLib.IsKeyDown((int)KeyboardKey.KEY_DOWN) != 0,
                Left: RayLib.IsKeyDown((int)KeyboardKey.KEY_LEFT) != 0,
                Right: RayLib.IsKeyDown((int)KeyboardKey.KEY_RIGHT) != 0);
        }
    }

    private void Draw()
    {
        VideoFrame frame;
        lock (_lock)
        {
            frame = _frame;
        }

        RayLib.BeginDrawing();
        RayLib.ClearBackground(Color.Black);

        ConvertFrameToRgba(frame, _rgbaBuffer);
        UpdateTexture(_texture, _rgbaBuffer);

        RayLib.DrawTexturePro(
            _texture,
            new Rectangle
            {
                x = 0,
                y = 0,
                width = VideoFrame.Width,
                height = VideoFrame.Height
            },
            new Rectangle
            {
                x = 0,
                y = 0,
                width = VideoFrame.Width * 4,
                height = VideoFrame.Height * 4
            },
            new Vector2
            {
                x = 0,
                y = 0
            },
            0f,
            Color.White);
        RayLib.EndDrawing();
    }

    internal static void ConvertFrameToRgba(VideoFrame frame, Span<byte> destination)
    {
        var source = frame.Pixels.Span;

        if (source.Length != VideoFrame.Width * VideoFrame.Height)
            throw new InvalidOperationException("Unexpected frame size.");

        if (destination.Length != source.Length * 4)
            throw new InvalidOperationException("Unexpected RGBA buffer size.");

        for (var i = 0; i < source.Length; i++)
        {
            var shade = source[i];

            var dst = i * 4;
            destination[dst + 0] = shade;
            destination[dst + 1] = shade;
            destination[dst + 2] = shade;
            destination[dst + 3] = 255;
        }
    }

    internal static TimeSpan GetTargetFrameDuration(bool isTurboRequested)
        => isTurboRequested ? s_turboFrameDuration : s_targetFrameDuration;

    private static unsafe void UpdateTexture(Texture texture, Span<byte> rgba)
    {
        fixed (byte* p = rgba)
        {
            RayLib.UpdateTexture(texture, p);
        }
    }
}
