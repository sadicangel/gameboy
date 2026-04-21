namespace GameBoy;

public interface IEmulatorStepObserver
{
    void OnStepCompleted(Bus bus);
}
