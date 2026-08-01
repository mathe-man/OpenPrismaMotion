using PrismaViz.Core;
using PrismaViz.Primitives;
using Silk.NET.OpenGL;
using System.Numerics;
using Shader = PrismaViz.Primitives.Shader;

namespace PrismaViz.Drawables;

public sealed class ArrowField : IDrawable
{
    public Vector3 Position => Vector3.Zero;

    private readonly SharedResources _resources;
    private readonly InstancedArrowMesh _mesh;


    public float ZStart { get; set; }
    public float ZEnd { get; set; }
    public float Thickness { get; set; } = 4f;
    public Vector4 Color { get; set; } = new(1f, 0.6f, 0.1f, 1f);

    public static ArrowField Create(GL gl, SharedResources resources, ArrowInstance[] arrows)
    {
        var mesh = InstancedArrowMesh.Create(gl, headLength: 0.25f, headWidthRatio: 1.0f);
        mesh.UploadInstances(arrows);
        return new ArrowField(resources, mesh);
    }

    private ArrowField(SharedResources resources, InstancedArrowMesh mesh)
    {
        _resources = resources; _mesh = mesh;
    }

    public void UpdateArrows(ArrowInstance[] arrows) => _mesh.UploadInstances(arrows);

    public void Draw(Camera camera, uint viewportWidth, uint viewportHeight)
    {
        _resources.ArrowShader.Use();
        _resources.ArrowShader.SetUniform("uMvp", camera.GetViewMatrix() * camera.GetProjectionMatrix(viewportWidth, viewportHeight));
        _resources.ArrowShader.SetUniform("uZStart", ZStart);
        _resources.ArrowShader.SetUniform("uZEnd", ZEnd);
        _resources.ArrowShader.SetUniform("uThickness", Thickness);
        _resources.ArrowShader.SetUniform("uColor", Color);
        _mesh.Draw();
    }

    public void Dispose() => _mesh.Dispose();
}