using OpenCvSharp;

namespace PrismaFlow;

public class Video
{
    public static VideoCapture OpenVideoSource(string filePath)
    {
        return new VideoCapture(filePath);
    }

    public static VideoWriter OpenVideoOutput(string filePath, VideoCapture blueprint)
    {
        return new VideoWriter(
            filePath,
            FourCC.FromFourChars('m', 'p', '4', 'v'),
            blueprint.Fps,
            new Size(blueprint.FrameWidth, blueprint.FrameHeight));
    }
}