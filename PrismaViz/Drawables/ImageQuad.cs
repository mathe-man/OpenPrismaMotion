using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using System.Numerics;

namespace PrismaViz.Drawables;

public sealed class ImageQuad : IDrawable, ITextured
{
    public Texture2D Texture { get; }

    public Mesh Mesh { get; }

    // Position of the center of the Quad
    public Vector3 Position { get; set; } = Vector3.Zero;


    public static ImageQuad FromFile(GL gl, string path)
    {
        var texture = Texture2D.FromFile(gl, path);

        // The quad is scaled to the texture size
        var mesh = Mesh.CreateQuad(gl, texture.Width, texture.Height);
        return new ImageQuad(texture, mesh);
    }

    public ImageQuad(Texture2D texture, Mesh mesh)
    {
        Texture = texture;
        Mesh = mesh;
    }

    public void Dispose()
    {
        Texture.Dispose();
        Mesh.Dispose();
    }
}
