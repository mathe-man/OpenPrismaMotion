using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using PrismaViz;
using PrismaViz.Core;
using PrismaViz.Drawables;
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using System.Numerics;

namespace Desktop.Views;

public partial class ViewportView : UserControl
{
    public ViewportView()
        => InitializeComponent();
}


public class GlViewport : OpenGlControlBase, ICustomHitTest // Handle mouse events for camera control
{
   

    private Renderer? _renderer;

    // Camera management
    private CameraController? _cameraController;
    private Point? _lastPointerPosition;
    private bool _isOrbiting, _isPanning;


    public bool HitTest(Point point) => true; // Alow mouse hits on the viewport area


    protected override void OnOpenGlInit(GlInterface gl)
    {
        var glApi = GL.GetApi(gl.GetProcAddress);

        var profile = new GraphicsProfile(
            IsOpenGLES: GlVersion.Type == GlProfileType.OpenGLES,
            MajorVersion: GlVersion.Major,
            MinorVersion: GlVersion.Minor);

        _renderer = new Renderer(glApi, profile);

        _cameraController = new CameraController(_renderer.Camera);

        // Subscribe to Avalonia mouse related events for camera control
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerWheelChanged += OnPointerWheelChanged;

        // Load some objects to render
        var leopard= ImageQuad.FromFile(glApi, _renderer.Resources, "ressources/Textures/leopard.jpg");
        leopard.Position = new Vector3(0, 0, -500);
        _renderer.AddObject(leopard);

        var purple = Texture2D.FromFile(glApi, "ressources/Textures/purple.jpg");

        var gizmo = AxisGizmo.Create(glApi, _renderer.Resources);
        _renderer.AddObject(gizmo);


        // Timeline flow arrows
        var testArrows = new ArrowInstance[]
        {
            new() { StartEnd = new Vector4(0, 0, 0, 0) },
            new() { StartEnd = new Vector4( 
                - purple.Width/2, 
                - purple.Height/2,
                purple.Width / 2,
                purple.Height/2
                ) 
            },
        };

        var scene = new TimelineScene(glApi, _renderer);
        scene.Load(
            framePaths: new[] { "ressources/Textures/purple.jpg" }, // une seule frame pour l'instant
            flowsBetweenFrames: new[] { testArrows }
        );
    }


    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        _isOrbiting = point.Properties.IsLeftButtonPressed;
        _isPanning = point.Properties.IsMiddleButtonPressed;
        _lastPointerPosition = point.Position;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_lastPointerPosition is not { } last) return;

        var current = e.GetCurrentPoint(this).Position;
        var delta = current - last;
        _lastPointerPosition = current;

        if (_isOrbiting) _cameraController!.Orbit((float)delta.X, (float)delta.Y);
        else if (_isPanning) _cameraController!.Pan((float)delta.X, (float)delta.Y);
        else return;

        RequestNextFrameRendering();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isOrbiting = false;
        _isPanning = false;
        _lastPointerPosition = null;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _cameraController!.Zoom((float)e.Delta.Y);
        RequestNextFrameRendering();
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

;
    }

    protected override void OnOpenGlDeinit(GlInterface gl) => _renderer?.Dispose();
}