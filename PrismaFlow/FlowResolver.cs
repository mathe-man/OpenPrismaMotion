
using OpenCvSharp;
using System.Numerics;

namespace PrismaFlow;


public readonly record struct FlowVector(Vector2 Start, Vector2 End);

public static class FlowResolver
{
    public static Mat GetGray(Mat frame, Mat gray)
    {
        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

        return gray;
    }


    // Reusable buffers to avoid re-allocation for each frame
    private static readonly Mat _smallGray1 = new();
    private static readonly Mat _smallGray2 = new();
    private static readonly Mat _smallFlow = new();
    
    public static Mat OpticalFlowScaled(Mat gray1, Mat gray2, Mat flow, float scale = 0.5f)
    {
        if (scale >= 1f)
            return OpticalFlow(gray1, gray2, flow);

        var smallSize = new Size(
            (int)(gray1.Width * scale),
            (int)(gray1.Height * scale));

        Cv2.Resize(gray1, _smallGray1, smallSize, interpolation: InterpolationFlags.Area);
        Cv2.Resize(gray2, _smallGray2, smallSize, interpolation: InterpolationFlags.Area);

        Cv2.CalcOpticalFlowFarneback(
            _smallGray1, _smallGray2, _smallFlow,
            pyrScale: 0.5, levels: 2, winsize: 13,
            iterations: 2, polyN: 5, polySigma: 1.1, flags: 0);

        // Resize to the original size
        Cv2.Resize(_smallFlow, flow, new Size(gray1.Width, gray1.Height),
            interpolation: InterpolationFlags.Linear);

        // Important: the movements were calculated at small scale so they need to be scaled up to match the original size
        Cv2.Multiply(flow, new Scalar(1f / scale, 1f / scale), flow);

        return flow;
    }
    
    public static Mat OpticalFlow(Mat gray1, Mat gray2, Mat flow)
    {
        Cv2.CalcOpticalFlowFarneback(
                gray1, gray2, flow,
                pyrScale: 0.5, levels: 3, winsize: 15,
                iterations: 3, polyN: 5, polySigma: 1.2, flags: 0);

        return flow;
    }

    public static float[] GenerateDebugFlowBuffer(
    int width,
    int height,
    float maxMovement = 20f)
    {
        var result = new float[width * height * 4];

        int i = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                
                float tx = width > 1 ? x / (float)(width - 1) : 0f;
                float ty = height > 1 ? y / (float)(height - 1) : 0f;

                // The movement increase when we move to the corner
                float strength = (tx + ty) * 0.5f;

                float dx = strength * maxMovement;
                float dy = strength * maxMovement;

                // Start
                result[i++] = x;
                result[i++] = y;

                // End
                result[i++] = x + dx;
                result[i++] = y + dy;
            }
        }

        return result;
    }
    
    public static unsafe FlowVector[] ExtractFlowVectors(Mat flow, int step = 8)
    {
        int height = flow.Height;
        int width = flow.Width;

        int countX = (width + step - 1) / step;
        int countY = (height + step - 1) / step;
        var results = new FlowVector[countX * countY];

        IntPtr basePtr = flow.Data;
        int flowStep = (int)(flow.Step() / sizeof(float));

        Parallel.For(0, countY, iy =>
        {
            unsafe
            {
                float* flowPtr = (float*)basePtr;
                int y = iy * step;
                int rowOffset = iy * countX;

                for (int ix = 0; ix < countX; ix++)
                {
                    int x = ix * step;
                    float dx = flowPtr[y * flowStep + x * 2];
                    float dy = flowPtr[y * flowStep + x * 2 + 1];
                    results[rowOffset + ix] = new FlowVector(new Vector2(x, y), new Vector2(x + dx, y + dy));
                }
            }
        });

        return results;
    }
    
    public static float[] ExtractToBuffer(Mat flow, int step = 8)
    {
        int height = flow.Height;
        int width = flow.Width;
        int countX = (width + step - 1) / step;
        int countY = (height + step - 1) / step;

        var result = new float[countX * countY * 4];

        IntPtr basePtr = flow.Data;
        int flowStep = (int)(flow.Step() / sizeof(float));

        Parallel.For(0, countY, iy =>
        {
            unsafe
            {
                float* flowPtr = (float*)basePtr;
                int y = iy * step;
                int rowOffset = (iy * countX) * 4;

                for (int ix = 0; ix < countX; ix++)
                {
                    int x = ix * step;
                    float dx = flowPtr[y * flowStep + x * 2];
                    float dy = flowPtr[y * flowStep + x * 2 + 1];

                    int o = rowOffset + ix * 4;
                    result[o] = x;
                    result[o + 1] = y;
                    result[o + 2] = x + dx;
                    result[o + 3] = y + dy;
                }
            }
        });

        return result;
    }

    public static float[] ExtractToBuffer(FlowVector[] flows)
    {
        float[] result = new float[flows.Length * 4]; // 4 float per flow vector (start.x, start.y, end.x, end.y)

        for (int i = 0; i < flows.Length; i++)
        {
            result[i * 4] = flows[i].Start.X;
            result[i * 4 + 1] = flows[i].Start.Y;
            result[i * 4 + 2] = flows[i].End.X;
            result[i * 4 + 3] = flows[i].End.Y;
        }

        return result;
    }
}
