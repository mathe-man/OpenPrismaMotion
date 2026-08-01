
using Silk.NET.OpenGL;

namespace PrismaViz.Primitives;

internal class InstancedArrowMesh
{
    private readonly GL _gl;
    private readonly uint _vao, _templateVbo, _templateEbo, _instanceVbo;
    private readonly int _templateIndexCount;
    private int _instanceCount;

    public static unsafe InstancedArrowMesh Create(GL gl, float headLength, float headWidthRatio)
    {
        // Arrow template vertices (in local space, 1 unit long)
        // Shaft (0 -> 1-headLength), triangle head (1-headLength -> 1)

        ArrowVertex[] template =
            {
                new() { Local = new(0f, -0.5f) },                  // 0 shaft, bottom start
                new() { Local = new(0f,  0.5f) },                  // 1 shaft, top start
                new() { Local = new(1f - headLength,  0.5f) },     // 2 shaft, top before head
                new() { Local = new(1f - headLength, -0.5f) },     // 3 shaft, bottom before head
                new() { Local = new(1f - headLength,  headWidthRatio) },  // 4 head base, top (wider)
                new() { Local = new(1f - headLength, -headWidthRatio) },  // 5 head base, bottom
                new() { Local = new(1f, 0f) },                     // 6 head tip
            };

        uint[] indices = { 0, 1, 2, 0, 2, 3, 4, 6, 5 }; // shaft (2 tris) + head (1 tri)


        uint vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);

        uint templateVbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, templateVbo);
        fixed (ArrowVertex* v = template)
            gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(template.Length * sizeof(ArrowVertex)), v, BufferUsageARB.StaticDraw);



        // location 0 : local position in the template
        gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, (uint)sizeof(ArrowVertex), (void*)0);
        gl.EnableVertexAttribArray(0);

        uint ebo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        fixed (uint* i = indices)
            gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);


        // Instance buffer, will be filled by UploadInstances
        uint instanceVbo = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, instanceVbo);

        // location 1 : instance start and end positions (vec4)
        gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, (uint)sizeof(ArrowInstance), (void*)0);
        gl.EnableVertexAttribArray(1);

        // Set divisor to 1 so that this attribute advances per instance, not per vertex
        // Enable instacing 
        gl.VertexAttribDivisor(1, 1);


        return new InstancedArrowMesh(gl, vao, templateVbo, ebo, instanceVbo, indices.Length);
    }

    private InstancedArrowMesh(GL gl,
                               uint vao,
                               uint templateVbo,
                               uint ebo,
                               uint instanceVbo,
                               int templateIndexCount)
    {
        _gl = gl; _vao = vao; 
        _templateVbo = templateVbo; _templateEbo = ebo; 
        _instanceVbo = instanceVbo;
        _templateIndexCount = templateIndexCount;
    }


    public unsafe void UploadInstances(ArrowInstance[] instances)
    {
        _instanceCount = instances.Length;
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _instanceVbo);
        fixed (ArrowInstance* p = instances)
            // BufferUsageARB.DynamicDraw instead of  StaticDraw
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(instances.Length * sizeof(ArrowInstance)), p, BufferUsageARB.DynamicDraw);
    }

    public unsafe void Draw()
    {
        if (_instanceCount == 0) return;
        _gl.BindVertexArray(_vao);
        _gl.DrawElementsInstanced(PrimitiveType.Triangles, (uint)_templateIndexCount, DrawElementsType.UnsignedInt, null, (uint)_instanceCount);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_templateVbo);
        _gl.DeleteBuffer(_templateEbo);
        _gl.DeleteBuffer(_instanceVbo);
    }
}
