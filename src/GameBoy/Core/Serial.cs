using System.Text;

namespace GameBoy.Core;

[Service(ServiceLifetime.Scoped)]
public sealed class Serial(InterruptController interrupts, IEnumerable<IEmulatorSerialObserver> observers)
{
    private readonly StringBuilder _lineBuilder = new();
    private readonly IEmulatorSerialObserver[] _observers = observers.ToArray();

    // public event Action<char>? CharReceived;
    // public event Action<string>? LineReceived;

    public byte SB { get; set; }

    public byte SC
    {
        get => field;
        set
        {
            if ((value & 0x81) == 0x81)
            {
                value = (byte)(value & ~0x80);
                interrupts.Request(Interrupts.Serial);
                var @char = (char)SB;
                _lineBuilder.Append(@char);
                //CharReceived?.Invoke(@char);
                if (@char is '\n' or '\r' or '\0')
                {
                    var line = _lineBuilder.ToString().TrimEnd('\n', '\r', '\0');
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        foreach (var observer in _observers)
                        {
                            observer.OnSerialLineReceived(line);
                        }

                        //LineReceived?.Invoke(line);
                    }

                    _lineBuilder.Clear();
                }
            }

            field = value;
        }
    }
}
