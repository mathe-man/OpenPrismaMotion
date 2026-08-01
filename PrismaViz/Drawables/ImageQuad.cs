using PrismaViz.Core;
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Resources;

namespace PrismaViz.Drawables;

public sealed class ImageQuad : IDrawable
{
    public Texture2D Texture { get; }
    public Mesh Mesh { get; }
    // Position of the center of the Quad
    public Vector3 Position { get; set; } = Vector3.Zero;

    private static SharedResources _resources;

    public static ImageQuad FromFile(GL gl, SharedResources resources, string path)
    {
        var texture = Texture2D.FromFile(gl, path);
        var mesh = Mesh.CreateQuad(gl, texture.Width, texture.Height);

        return new ImageQuad(texture, mesh, resources);
    }

    private ImageQuad(Texture2D texture, Mesh mesh, SharedResources resources)
    {
        Texture = texture; Mesh = mesh; _resources = resources;
    }

    public void Draw(Camera camera, uint viewportWidth, uint viewportHeight)
    {
        _resources.UnlitShader.Use();

        var model = Matrix4x4.CreateTranslation(Position);
        var mvp = model * camera.GetViewMatrix() * camera.GetProjectionMatrix(viewportWidth, viewportHeight);
        _resources.UnlitShader.SetUniform("uMvp", mvp);

        Texture.Bind();
        _resources.UnlitShader.SetUniform("uTexture", 0);

        Mesh.Draw();
    }

    public void Dispose()
    {
        Texture.Dispose();
        Mesh.Dispose();
    }
}
