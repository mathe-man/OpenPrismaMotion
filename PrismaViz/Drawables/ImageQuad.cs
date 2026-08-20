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


    public static ImageQuad FromFile(GL gl, string path)
    {
        var texture = Texture2D.FromFile(gl, path);
        var mesh = Mesh.CreateQuad(gl, texture.Width, texture.Height);

        return new ImageQuad(texture, mesh);
    }

    private ImageQuad(Texture2D texture, Mesh mesh)
    {
        Texture = texture; Mesh = mesh;
    }

    public void Draw(Camera camera, uint viewportWidth, uint viewportHeight)
    {
        SharedResources.UnlitShader.Use();

        var model = Matrix4x4.CreateTranslation(Position);
        var mvp = model * camera.GetViewMatrix() * camera.GetProjectionMatrix(viewportWidth, viewportHeight);
        SharedResources.UnlitShader.SetUniform("uMvp", mvp);

        Texture.Bind();
        SharedResources.UnlitShader.SetUniform("uTexture", 0);

        Mesh.Draw();
    }

    public void Dispose()
    {
        Texture.Dispose();
        Mesh.Dispose();
    }
}
