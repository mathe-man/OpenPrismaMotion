namespace FlowResolver;

using OpenCvSharp;

public static class FlowResolver
{
    static VideoCapture OpenVideoSource(string filePath)
    {
        return new VideoCapture(filePath);
    }

    static VideoWriter OpenVideoOutput(string filePath, VideoCapture blueprint)
    {
        return new VideoWriter(
            filePath, 
            FourCC.FromFourChars('m', 'p', '4', 'v'), 
            blueprint.Fps, 
            new Size(blueprint.FrameWidth, blueprint.FrameHeight));
    }
}
