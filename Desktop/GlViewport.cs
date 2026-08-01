
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using PrismaViz;
using Silk.NET.OpenGL;

namespace Desktop;

public class GlViewport : OpenGlControlBase
{
    private Renderer? _renderer;

    protected override void OnOpenGlInit(GlInterface gl)
    {
        var glApi = GL.GetApi(gl.GetProcAddress);

        var profile = new GraphicsProfile(
            IsOpenGLES: GlVersion.Type == GlProfileType.OpenGLES,
            MajorVersion: GlVersion.Major,
            MinorVersion: GlVersion.Minor);

        _renderer = new Renderer(glApi, profile);

        var image = ImageQuad.FromFile(glApi, "ressources/Textures/leopard.jpg");
        _renderer.AddObject(image);

        var gizmo = AxisGizmo.Create(glApi);
        _renderer.AddObject(gizmo);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        // Resolve High DPI screens scaling
        double scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1.0;

        uint pixelWidth = (uint)(Bounds.Width * scaling);
        uint pixelHeight = (uint)(Bounds.Height * scaling);


        _renderer!.Resize(pixelWidth, pixelHeight);
        _renderer.BeginFrame((uint)fb);
        _renderer.Draw();
        _renderer.EndFrame();

        RequestNextFrameRendering();
    }

    protected override void OnOpenGlDeinit(GlInterface gl) => _renderer?.Dispose();
}