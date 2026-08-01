using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using PrismaViz;
using PrismaViz.Drawables;
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
        var leopard= ImageQuad.FromFile(glApi, "ressources/Textures/leopard.jpg");
        _renderer.AddObject(leopard);

        var purple = ImageQuad.FromFile(glApi, "ressources/Textures/purple.jpg");

        purple.Position += new Vector3(0, 0, -300);
        _renderer.AddObject(purple);

        var gizmo = AxisGizmo.Create(glApi);
        _renderer.AddObject(gizmo);
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