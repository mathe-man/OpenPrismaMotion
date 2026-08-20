
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using Shader = PrismaViz.Primitives.Shader;

namespace PrismaViz.Core;

public static class SharedResources
{
    public static Shader UnlitShader { get; private set; }
    public static Shader ArrowShader { get; private set; }
    public static Texture2D WhiteTexture { get; private set; }

    
    public static void Init(GL gl, GraphicsProfile profile)
    {
        Shader.SetProfile(profile);
        UnlitShader = new Shader(gl, "unlit");
        ArrowShader = new Shader(gl, "arrow");
        WhiteTexture = Texture2D.CreateWhite1x1(gl);
    }

    public static void Dispose()
    {
        UnlitShader.Dispose();
        ArrowShader.Dispose();
        WhiteTexture.Dispose();
    }
}
