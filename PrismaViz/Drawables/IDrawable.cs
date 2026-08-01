
using System.Numerics;
using PrismaViz.Primitives;

namespace PrismaViz.Drawables;

public interface IDrawable : IDisposable
{
    Vector3 Position { get; }
    Mesh Mesh { get; }
}