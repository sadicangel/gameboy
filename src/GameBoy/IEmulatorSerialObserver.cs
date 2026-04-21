namespace GameBoy;

public interface IEmulatorSerialObserver
{
    void OnSerialLineReceived(string line);
}
