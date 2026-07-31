using Silk.NET.OpenGL;
using System.Numerics;
using System.Runtime.InteropServices;


[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector3 Position;
    public Vector4 Color; // RGBA, 0-1
    public Vector2 UV;
}

public sealed class Mesh : IDisposable
{
    private readonly GL _gl;
    private readonly uint _vao, _vbo, _ebo;
    private readonly int _indexCount;

    private Mesh(GL gl, uint vao, uint vbo, uint ebo, int indexCount)
    {
        _gl = gl;
        _vao = vao;
        _vbo = vbo;
        _ebo = ebo;
        _indexCount = indexCount;
    }

    public static unsafe Mesh Create(GL gl, Vertex[] vertices, uint[] indices)
    {
        uint vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        // generate VBO
        uint vbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
        fixed (Vertex* v = vertices)
            gl.BufferData(BufferTargetARB.ArrayBuffer,
                (nuint)(vertices.Length * sizeof(Vertex)), v, BufferUsageARB.StaticDraw);

        // generate EBO
        uint ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (uint* i = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer,
                (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);

        // Vertices attributes

        int stride = sizeof(Vertex);

        // location 0 : position (3 floats, offset 0)
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)stride, (void*)0);
        gl.EnableVertexAttribArray(0);

        // location 1 : color (4 floats, offset after position)
        gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)stride, (void*)sizeof(Vector3));
        gl.EnableVertexAttribArray(1);

        // location 2 : UV (2 floats, offset after Position+Color)
        gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, (uint)stride, (void*)(sizeof(Vector3) + sizeof(Vector4)));
        gl.EnableVertexAttribArray(2);

        return new Mesh(gl, vao, vbo, ebo, indices.Length);
    }

    public static Mesh CreateQuad(GL gl, float width, float height, Vector3 center, Vector3 facing, Vector3 up)
    {

        float hw = width / 2f, hh = height / 2f;
        var white = new Vector4(1, 1, 1, 1);

        Vector3 normal = Vector3.Normalize(facing);


        Vector3 upReference = up;
        if (MathF.Abs(Vector3.Dot(Vector3.Normalize(up), normal)) > 0.999f)
            upReference = MathF.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.999f ? Vector3.UnitZ : Vector3.UnitX;

        Vector3 right = Vector3.Normalize(Vector3.Cross(upReference, normal));

        Vector3 realUp = Vector3.Cross(normal, right);



        Vertex[] vertices =
        {
            new() { Position = center - right * hw + up * hh, Color = white, UV = new(0, 0) }, // Upper-Left
            new() { Position = center + right * hw + up * hh, Color = white, UV = new(1, 0) }, // Upper-Rigtht
            new() { Position = center + right * hw - up * hh, Color = white, UV = new(1, 1) }, // Lower-Right
            new() { Position = center - right * hw - up * hh, Color = white, UV = new(0, 1) }, // Lower-Left
        };
        uint[] indices = { 0, 1, 2, 0, 2, 3 };

        return Create(gl, vertices, indices);
    }

    public static Mesh CreateQuad(GL gl, float width, float height, Vector3 center, Vector3 facing)
        => CreateQuad(gl, width, height, center, facing, Vector3.UnitY);

    public static Mesh CreateQuad(GL gl, float width, float height,  Vector3 center)
        => CreateQuad(gl, width, height, center, Vector3.UnitZ, Vector3.UnitY);

    public static Mesh CreateQuad(GL gl, float width, float height)
        => CreateQuad(gl, width, height, Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);




    public unsafe void Draw()
    {
        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, null);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _gl.DeleteBuffer(_ebo);
    }
}