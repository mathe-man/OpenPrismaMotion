
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
        var results = new List<FlowVector>();
       

        var flowPtr = (float*)flow.DataPointer;
        int flowStep = (int)(flow.Step() / sizeof(float));

        int height = flow.Height;
        int width = flow.Width;

        for (int y = 0; y < height; y += step)
            for (int x = 0; x < width; x += step)
            {
                float dx = flowPtr[y * flowStep + x * 2];
                float dy = flowPtr[y * flowStep + x * 2 + 1];


                results.Add(new FlowVector(new Vector2(x, y), new Vector2(x + dx, y + dy)));
            }

        return results.ToArray();
    }
    
    public static float[] ExtractToBuffer(Mat flow, int step = 8)
    {
        return ExtractToBuffer(ExtractFlowVectors(flow, step));
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
