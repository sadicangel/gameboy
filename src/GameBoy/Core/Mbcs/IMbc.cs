namespace GameBoy.Core.Mbcs;

public interface IMbc
{
    byte Read(ushort address);
    void Write(ushort address, byte value);
}
