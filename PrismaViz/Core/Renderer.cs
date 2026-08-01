using PrismaViz.Core;
using PrismaViz.Drawables;
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using System.Numerics;

namespace PrismaViz;

public readonly record struct GraphicsProfile(bool IsOpenGLES, int MajorVersion, int MinorVersion);


public sealed class Renderer : IDisposable
{
    private readonly GL _gl;
    public Camera Camera { get; } = new();
    public SharedResources Resources { get; }


    private uint _width = 1, _height = 1;
    private readonly List<IDrawable> _objects = new();



    public Renderer(GL gl, GraphicsProfile profile)
    {
        _gl = gl;

        // Enable depth testing for proper 3D rendering
        _gl.Enable(EnableCap.DepthTest);

        Resources = new SharedResources(gl, profile);
    }

    

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
    }


    public void AddObject(IDrawable obj)
        => _objects.Add(obj);

    public void RemoveObject(IDrawable obj)
    {
        _objects.Remove(obj);
        obj.Dispose();
    }

    public void ClearObjects()
    {
        foreach (var obj in _objects) obj.Dispose();
        _objects.Clear();
    }


    public void Draw()
    {
        foreach (IDrawable obj in _objects)
        {
            obj.Draw(Camera, _width, _height);
        }
    }

    public void EndFrame() { }

    public void Dispose()
    {
        ClearObjects();
        Resources.Dispose();
    }
}