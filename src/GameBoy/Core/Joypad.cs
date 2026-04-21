namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Joypad
{
    public JoypadState CurrentState { get; private set; }

    public void Update(JoypadState state) => CurrentState = state;
}
