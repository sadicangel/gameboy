namespace GameBoy.Runtime;

public interface IAudioOutput
{
    void SubmitAudio(AudioBuffer audio);
}
