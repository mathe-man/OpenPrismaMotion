using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrismaFlow;

public static class Generator
{
    
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
                    var end = new Point((int)(x + dx), (int)(y + dy));

                    double magnitude = Math.Sqrt(dx * dx + dy * dy);
                    if (magnitude < minMagnitude && magnitude > 0) continue; // ignore smallest movement


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


    public static VideoWriter CreateOpticalFlowVideo(string sourcePath, string outputPath, int step = 8, float minMagnitude = 0.3f, bool drawOverFrame = false, float farnebackScaling = 0.25f, int frameCount = -1, IProgress<float>? progress = null)
    {
        var source = Video.OpenVideoSource(sourcePath);
        var output = Video.OpenVideoOutput(outputPath, source);

        CreateOpticalFlowVideo(source, output, step, minMagnitude, drawOverFrame, farnebackScaling, frameCount, progress);

        source.Release();
        output.Release();

        return output;
    }

    public static void CreateOpticalFlowVideo(VideoCapture source, VideoWriter output, int step = 8, float minMagnitude = 0.3f, bool drawOverFrame = false, float farnebackScaling = 0.25f, int frameCount = -1, IProgress<float>? progress = null)
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
        FlowResolver.GetGray(prevFrame, prevGray);

        // Allocate mat memory
        var frame = new Mat();
        var gray = new Mat();
        var flow = new Mat();


        var i = 0;
        int max = source.FrameCount;    // Number of frame to create

        if (frameCount > 0)
            max = frameCount;

        while (i++ < max && source.Read(frame))
        {
            if (frame.Empty()) break;

            FlowResolver.GetGray(frame, gray);
            // Get the flow between the two frame
            FlowResolver.OpticalFlowScaled(prevGray, gray, flow, farnebackScaling);

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
