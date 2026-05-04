namespace GameBoy.Runtime;

public interface IVideoOutput
{
    void PresentFrame(VideoFrame frame);
}
