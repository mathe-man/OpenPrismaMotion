using Silk.NET.OpenGL;
using System.Numerics;

using PrismaViz.Primitives;

namespace PrismaViz.Drawables;

public sealed class AxisGizmo : IDrawable
{
    public Vector3 Position => Vector3.Zero;
    public Mesh Mesh { get; }

    private static readonly Vector4 Red = new(1f, 0.2f, 0.2f, 1f);
    private static readonly Vector4 Green = new(0.2f, 1f, 0.2f, 1f);
    private static readonly Vector4 Blue = new(0.3f, 0.5f, 1f, 1f);
    private static readonly Vector4 White = new(1f, 1f, 1f, 1f);

    public static AxisGizmo Create(GL gl, Vector3 position, uint length = 1000, uint thickness = 2) =>
        new(gl, position, length, thickness);

    public static AxisGizmo Create(GL gl, uint length = 1000, uint thickness = 2) =>
        new(gl, Vector3.Zero, length, thickness);

    private AxisGizmo(GL gl, Vector3 position, uint length, uint thickness)
    {
        float half = thickness / 2f;

        var vertices = new List<Vertex>();
        var indices = new List<uint>();

        AppendArm(vertices, indices, position, axis: Vector3.UnitX, perpA: Vector3.UnitY, perpB: Vector3.UnitZ, length, half, Red);
        AppendArm(vertices, indices, position, axis: Vector3.UnitY, perpA: Vector3.UnitZ, perpB: Vector3.UnitX, length, half, Green);
        AppendArm(vertices, indices, position, axis: Vector3.UnitZ, perpA: Vector3.UnitX, perpB: Vector3.UnitY, length, half, Blue);

        AppendCube(vertices, indices, position, half, White);


        Mesh = Mesh.Create(gl, vertices.ToArray(), indices.ToArray());
    }

    private static void AppendArm(List<Vertex> vertices, List<uint> indices, Vector3 center,
        Vector3 axis, Vector3 perpA, Vector3 perpB, float length, float halfThickness, Vector4 color)
    {
        Vector3 near = center;
        Vector3 far = center + axis * length;

        Vector3[] offset =
        {
            -perpA * halfThickness - perpB * halfThickness,
             perpA * halfThickness - perpB * halfThickness,
             perpA * halfThickness + perpB * halfThickness,
            -perpA * halfThickness + perpB * halfThickness,
        };

        Vector3[] nearCorners = { near + offset[0], near + offset[1], near + offset[2], near + offset[3] };
        Vector3[] farCorners = { far + offset[0], far + offset[1], far + offset[2], far + offset[3] };

        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) % 4;
            AddQuad(vertices, indices, nearCorners[i], nearCorners[j], farCorners[j], farCorners[i], color);
        }

        AddQuad(vertices, indices, farCorners[0], farCorners[1], farCorners[2], farCorners[3], color);
    }

    private static void AppendCube(List<Vertex> vertices, List<uint> indices, Vector3 center, float half, Vector4 color)
    {
        Vector3[] c =
        {
            center + new Vector3(-half, -half, -half),
            center + new Vector3( half, -half, -half),
            center + new Vector3( half,  half, -half),
            center + new Vector3(-half,  half, -half),
            center + new Vector3(-half, -half,  half),
            center + new Vector3( half, -half,  half),
            center + new Vector3( half,  half,  half),
            center + new Vector3(-half,  half,  half),
        };

        AddQuad(vertices, indices, c[0], c[1], c[2], c[3], color);
        AddQuad(vertices, indices, c[5], c[4], c[7], c[6], color);
        AddQuad(vertices, indices, c[4], c[0], c[3], c[7], color);
        AddQuad(vertices, indices, c[1], c[5], c[6], c[2], color);
        AddQuad(vertices, indices, c[4], c[5], c[1], c[0], color);
        AddQuad(vertices, indices, c[3], c[2], c[6], c[7], color);
    }

    private static void AddQuad(List<Vertex> vertices, List<uint> indices, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector4 color)
    {
        uint start = (uint)vertices.Count;
        vertices.Add(new Vertex { Position = a, Color = color, UV = Vector2.Zero });
        vertices.Add(new Vertex { Position = b, Color = color, UV = Vector2.Zero });
        vertices.Add(new Vertex { Position = c, Color = color, UV = Vector2.Zero });
        vertices.Add(new Vertex { Position = d, Color = color, UV = Vector2.Zero });

        indices.Add(start); indices.Add(start + 1); indices.Add(start + 2);
        indices.Add(start); indices.Add(start + 2); indices.Add(start + 3);
    }

    public void Dispose() => Mesh.Dispose();

}
