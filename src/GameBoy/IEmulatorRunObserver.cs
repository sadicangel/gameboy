namespace GameBoy;

public interface IEmulatorRunObserver
{
    void OnSerialLineReceived(string line);
    void OnStepCompleted(Bus bus);
}
