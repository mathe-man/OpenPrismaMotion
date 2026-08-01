
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using Shader = PrismaViz.Primitives.Shader;

namespace PrismaViz.Core;

public sealed class SharedResources
{
    public Shader UnlitShader { get; }
    public Shader ArrowShader { get; }
    public Texture2D WhiteTexture { get; }

    public SharedResources(GL gl, GraphicsProfile profile)
    {
        Shader.SetProfile(profile);
        UnlitShader = new Shader(gl, "unlit");
        ArrowShader = new Shader(gl, "arrow");
        WhiteTexture = Texture2D.CreateWhite1x1(gl);
    }

    public void Dispose()
    {
        UnlitShader.Dispose();
        ArrowShader.Dispose();
        WhiteTexture.Dispose();
    }
}
