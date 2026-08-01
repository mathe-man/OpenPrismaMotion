namespace FlowResolver;

using OpenCvSharp;

public static class FlowResolver
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

    public static Mat GetGray(Mat frame, Mat gray)
    {
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

        return gray;
    }

    public static Mat OpticalFlow(Mat gray1, Mat gray2, Mat flow)
    {
        Cv2.CalcOpticalFlowFarneback(
                gray1, gray2, flow,
                pyrScale: 0.5, levels: 3, winsize: 15,
                iterations: 3, polyN: 5, polySigma: 1.2, flags: 0);

        return flow;
    }

    public static Mat GenerateOpticalFlowFrame(Mat flow, Mat original, Mat output, int step = 8, float minMagnitude = 0.3f)
    {

        // Use memory pointer for significant speed increase
        unsafe
        {
            var flowPtr = (float*)flow.DataPointer;
            int flowStep = (int)(flow.Step() / sizeof(float)); // floats stride

            var colorPtr = (byte*)original.DataPointer;
            int colorStep = (int)original.Step(); // colors stride


            var height = output.Height;
            var width = output.Width;

            if (height < 0 || width < 0)
                return output;

            for (int y = 0; y < height; y += step)
                for (int x = 0; x < width; x += step)
                {
                    float dx = flowPtr[y * flowStep + x * 2];
                    float dy = flowPtr[y * flowStep + x * 2 + 1];

                    var star = new Point(x, y);
                    var end  = new Point((int)(x + dx), (int)(y + dy));

                    double magnitude = Math.Sqrt(dx * dx + dy * dy);
                    if (magnitude < minMagnitude) continue; // ignore smallest movement


                    // Color access using pointers
                    int colorIndex = y * colorStep + x * 3;
                    byte b = colorPtr[colorIndex];
                    byte g = colorPtr[colorIndex + 1];
                    byte r = colorPtr[colorIndex + 2];


                    Scalar arrowColor = new Scalar(b, g, r);

                    Cv2.ArrowedLine(output, star, end, arrowColor, thickness: 1, tipLength: 0.2);

                }

            return output;
        }
    }


    public static VideoWriter GenerateOpticalFlowVideo(string sourcePath, string outputPath, int step = 8, float minMagnitude = 0.3f, bool drawOverFrame = false, int frameCount = -1, IProgress<float>? progress = null)
    {
        var source = OpenVideoSource(sourcePath);
        var output = OpenVideoOutput(outputPath, source);

        GenerateOpticalFlowVideo(source, output, step, minMagnitude, drawOverFrame, frameCount, progress);

        source.Release();
        output.Release();

        return output;
    }

    public static void GenerateOpticalFlowVideo(VideoCapture source, VideoWriter output, int step = 8, float minMagnitude = 0.3f, bool drawOverFrame = false, int frameCount = -1, IProgress<float>? progress = null)
    {
        // If both source and output have the same size
        if (source.FrameWidth != output.FrameSize.Width ||
            source.FrameHeight != output.FrameSize.Height
            )
            throw new Exception("Parameter 'source' and 'output' must have the same frames sizes");


        // Allocate mat memory now for a single allocation
        var prevFrame = new Mat();
        var prevGray = new Mat();

        // Get the first frame and is gray
        source.Read(prevFrame);
        GetGray(prevFrame, prevGray);

        // Allocate mat memory
        var frame = new Mat();
        var gray  = new Mat();
        var flow  = new Mat();

        
        var i = 0;
        int max = source.FrameCount;    // Number of frame to create

        if (frameCount > 0)
            max = frameCount;

        while (i++ < max && source.Read(frame))
        {
            if (frame.Empty()) break;

            GetGray(frame, gray);
            // Get the flow between the two frame
            OpticalFlow(prevGray, gray, flow);

            Mat outFrame;
            // Use the same frame if we have to draw over it
            if (drawOverFrame)
                outFrame = frame.Clone();
            // Otherwise we use an empty frame of the same size
            else
                outFrame = frame.EmptyClone();


            GenerateOpticalFlowFrame(flow, frame, outFrame, step, minMagnitude);

            output.Write(outFrame);

            // Set previous value for next loop
            (prevGray, gray) = (gray, prevGray);
            // The prevFrame is only needed before the loop

            // Update progres
            progress?.Report((i + 1f) / max);
        }

        progress?.Report(1f);
    }
}
