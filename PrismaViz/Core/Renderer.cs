using PrismaViz.Core;
using PrismaViz.Drawables;
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using System.Numerics;

namespace PrismaViz;

public readonly record struct GraphicsProfile(bool IsOpenGLES, int MajorVersion, int MinorVersion);


public static class Renderer
{
    public static GL _gl {  get; private set; }
    public static Camera Camera { get; } = new();


    private static uint _width = 1, _height = 1;
    private static readonly List<IDrawable> _objects = new();



    public static void Init(GL gl, GraphicsProfile profile)
    {
        _gl = gl;

        // Enable depth testing for proper 3D rendering
        _gl.Disable(EnableCap.DepthTest);

        SharedResources.Init(gl, profile);
    }

    

    public static void Resize(uint width, uint height)
    {
        _width = width; _height = height;
        _gl.Viewport(0, 0, width, height);
    }

    public static void BeginFrame(uint framebuffer)
    {
        // Clear framebuffer before drawing
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
        _gl.ClearColor(Camera.backgroundColor);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }


    public static void AddObject(IDrawable obj)
        => _objects.Add(obj);

    public static void RemoveObject(IDrawable obj)
    {
        _objects.Remove(obj);
        obj.Dispose();
    }

    public static void ClearObjects()
    {
        foreach (var obj in _objects) obj.Dispose();
        _objects.Clear();
    }


    public static void Draw()
    {
        foreach (IDrawable obj in _objects)
        {
            obj.Draw(Camera, _width, _height);
        }
    }

    public static void EndFrame() { }

    public static void Dispose()
    {
        ClearObjects();
        SharedResources.Dispose();
    }
}