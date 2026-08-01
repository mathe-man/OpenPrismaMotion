
using System.Numerics;
using System.Runtime.InteropServices;

namespace PrismaViz.Primitives;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector3 Position;
    public Vector4 Color; // RGBA, 0-1
    public Vector2 UV;
}


[StructLayout(LayoutKind.Sequential)]
public struct ArrowVertex
{
    public Vector2 Local;
}

[StructLayout(LayoutKind.Sequential)]
public struct ArrowInstance
{
    public Vector4 StartEnd; // xy = start, zw = end
}