using PrismaViz.Core;
using Silk.NET.OpenGL;
using System.Numerics;

namespace PrismaViz.Drawables;

public sealed class ArrowField : IDrawable
{
    public Vector3 Position => Vector3.Zero;
    private readonly GL _gl;
    private readonly uint _vao, _vbo;
    private readonly uint vertexCount;

    public float ZStart { get; set; }
    public float ZEnd { get; set; }
    public float MinMagnitude { get; set; }

    public float Thickness { get; set; } = 1f;
    public Vector4 Color { get; set; } = new(1f, 0f, 0f, 1f);

    

    private static float[] GlReadyFlows(float[] flows)
    {
        var result = new float[flows.Length * 2];

        int j = 0;

        for (int i = 0; i <= flows.Length - 4; i += 4)
        {
            float x = flows[i];
            float y = flows[i + 1];

            float endX = flows[i + 2];
            float endY = flows[i + 3];

            float dx = endX - x;
            float dy = endY - y;

            // Start
            result[j++] = x;
            result[j++] = y;
            result[j++] = dx;
            result[j++] = dy;

            // End
            result[j++] = endX;
            result[j++] = endY;
            result[j++] = dx;
            result[j++] = dy;
        }

        return result;
    }
    public unsafe ArrowField(GL gl, float[] flows, float zStart, float zEnd, float minMagnitude)
    {
        ZStart = zStart;
        ZEnd = zEnd;
        MinMagnitude = minMagnitude;

        _gl = gl;
        var bufferFlows = GlReadyFlows(flows);
        vertexCount = (uint)bufferFlows.Length / 4;


        _vao = _gl.GenVertexArray();
        gl.BindVertexArray(_vao);

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* f = bufferFlows)
            _gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(bufferFlows.Length * sizeof(float)), f, BufferUsageARB.DynamicDraw);


        uint stride = sizeof(float) * 4;

        // location 0 : position (x, y)
        _gl.VertexAttribPointer(
            0,
            2,
            VertexAttribPointerType.Float,
            false,
            stride,
            (void*)0
        );
        _gl.EnableVertexAttribArray(0);

        // location 1 : flow (dx, dy)
        _gl.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            stride,
            (void*)(sizeof(float) * 2)
        );
        _gl.EnableVertexAttribArray(1);
    }


    public void Draw(Camera camera, uint viewportWidth, uint viewportHeight)
    {
        SharedResources.ArrowShader.Use();
        var mvp =
            camera.GetViewMatrix() *
            camera.GetProjectionMatrix(viewportWidth, viewportHeight);

        SharedResources.ArrowShader.SetUniform("uMvp", mvp);
        SharedResources.ArrowShader.SetUniform("uZStart", ZStart);
        SharedResources.ArrowShader.SetUniform("uZEnd", ZEnd);
        SharedResources.ArrowShader.SetUniform("uColor", Color);

        SharedResources.ArrowShader.SetUniform("uMinMagnitude", MinMagnitude);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(
            PrimitiveType.Lines,
            0,
            vertexCount
        );
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
    }
}