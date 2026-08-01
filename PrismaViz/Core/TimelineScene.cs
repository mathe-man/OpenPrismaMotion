
using PrismaViz.Drawables;
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using System.Numerics;

namespace PrismaViz.Core;

public sealed class TimelineScene
{
    private readonly GL _gl;
    private readonly Renderer _renderer;

    private readonly List<ImageQuad> _frames = new();
    private readonly List<ArrowField> _arrowFields = new();

    private float _frameSpacing = 200f;
    public float FrameSpacing
    {
        get => _frameSpacing;
        set { _frameSpacing = value; ApplySpacing(); }
    }

    public TimelineScene(GL gl, Renderer renderer)
    {
        _gl = gl;
        _renderer = renderer;
    }

    public void Load(IReadOnlyList<string> framePaths, IReadOnlyList<ArrowInstance[]> flowsBetweenFrames)
    {
        Clear();

        for (int i = 0; i < framePaths.Count; i++)
        {
            var quad = ImageQuad.FromFile(_gl, _renderer.Resources, framePaths[i]);
            quad.Position = new Vector3(0, 0, i * _frameSpacing);
            _frames.Add(quad);
            _renderer.AddObject(quad);
        }

        for (int i = 0; i < flowsBetweenFrames.Count; i++)
        {
            var field = ArrowField.Create(_gl, _renderer.Resources, flowsBetweenFrames[i]);
            field.ZStart = i * _frameSpacing;
            field.ZEnd = (i + 1) * _frameSpacing;
            _arrowFields.Add(field);
            _renderer.AddObject(field);
        }
    }

    private void ApplySpacing()
    {
        for (int i = 0; i < _frames.Count; i++)
            _frames[i].Position = new Vector3(0, 0, i * _frameSpacing);

        for (int i = 0; i < _arrowFields.Count; i++)
        {
            _arrowFields[i].ZStart = i * _frameSpacing;
            _arrowFields[i].ZEnd = (i + 1) * _frameSpacing;
        }
    }

    public void Clear()
    {
        foreach (var f in _frames) _renderer.RemoveObject(f);
        foreach (var a in _arrowFields) _renderer.RemoveObject(a);
        _frames.Clear();
        _arrowFields.Clear();
    }
}
