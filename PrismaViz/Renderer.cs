using Silk;
using Silk.NET.OpenGL;
using System.Numerics;

namespace PrismaViz;

public readonly record struct GraphicsProfile(bool IsOpenGLES, int MajorVersion, int MinorVersion);

public interface IDrawable : IDisposable
{
    Vector3 Position {  get; }
    Mesh Mesh { get; }
}
public interface ITextured
{
    Texture2D Texture { get; }
}


public sealed class Renderer : IDisposable
{
    private readonly GL _gl;

    public Camera Camera { get; } = new();

    private readonly Shader _unlitShader;
    private readonly Texture2D _whiteTexture;

    private readonly GraphicsProfile _profile;


    private List<IDrawable> _objects = new();
    public void AddObject(IDrawable obj)
        => _objects.Add(obj);
    public void RemoveObject(IDrawable obj)
    {
        _objects.Remove(obj);
        obj.Dispose();
    }


    public Renderer(GL gl, GraphicsProfile profile)
    {
        _gl = gl;
        _profile = profile;

        // Load shader
        _unlitShader = new Shader(_gl, "unlit", profile);
        // Load blank texture for non textured objects
        _whiteTexture = Texture2D.CreateWhite1x1(_gl);
    }

    

    private uint _width = 1, _height = 1;
    public void Resize(uint width, uint height)
    {
        _width = width; _height = height;
        _gl.Viewport(0, 0, width, height);
    }

    public void BeginFrame(uint framebuffer)
    {
        // Clear framebuffer before drawing
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        _gl.ClearColor(Camera.backgroundColor);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        // Enable our shader
        _unlitShader.Use();
    }


    public void Draw()
    {
        // Loop over each object to draw

        foreach (IDrawable obj in _objects)
        {
            // Calculate matrices
            var model = Matrix4x4.CreateTranslation(obj.Position);
            var mvp = model * Camera.GetViewMatrix() * Camera.GetProjectionMatrix(_width, _height);

            // Set the uniform for the shader
            _unlitShader.SetUniform("uMvp", mvp);

            // Use the object texture avaible otherwise a blank one will do the job
            if (obj is ITextured textured)
                textured.Texture.Bind();
            else
                _whiteTexture.Bind();

            _unlitShader.SetUniform("uTexture", 0);

            obj.Mesh.Draw();
        }
    }

    public void EndFrame() { }

    public void Dispose()
    {
        foreach (IDrawable obj in _objects)
            obj.Dispose();

        _unlitShader.Dispose();
        _whiteTexture.Dispose();
    }
}