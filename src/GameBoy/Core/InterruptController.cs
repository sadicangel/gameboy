namespace GameBoy.Core;

[Singleton]
public sealed class InterruptController
{
    private Interrupts _if;
    private Interrupts _ie;

    public Interrupts Pending => (_ie & _if & Interrupts.All);
    public bool HasPending => Pending != Interrupts.None;

    public void Request(Interrupts interrupt) => _if |= interrupt;

    public bool TryPopHighestPending(out Interrupts highestPriority)
    {
        highestPriority = Pending.HighestPriority;
        if (highestPriority is Interrupts.None)
        {
            return false;
        }

        if (highestPriority != Interrupts.Timer)
            Console.WriteLine();

        _if &= ~highestPriority;
        return true;
    }

    public byte ReadIF() => (byte)(_if | ~Interrupts.All);
    public void WriteIF(byte v) => _if = (Interrupts)v & Interrupts.All;

    public byte ReadIE() => (byte)(_ie & Interrupts.All);
    public void WriteIE(byte v) => _ie = (Interrupts)v & Interrupts.All;
}
