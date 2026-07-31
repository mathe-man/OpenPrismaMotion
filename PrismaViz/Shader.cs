using Silk;
using Silk.NET.OpenGL;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PrismaViz;

public sealed class Shader : IDisposable
{
    private readonly GL _gl;
    private readonly uint _programId;

    public Shader(GL gl, string name, GraphicsProfile profile)
    {
        _gl = gl;

        uint vs = Compile(ShaderType.VertexShader, BuildSource(name, "vert", profile));
        uint fs = Compile(ShaderType.FragmentShader, BuildSource(name, "frag", profile));


        // Create shader program
        _programId = _gl.CreateProgram();
        _gl.AttachShader(_programId, vs);
        _gl.AttachShader(_programId, fs);
        _gl.LinkProgram(_programId);

        // Free shaders once the program is created
        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);
    }

    private static string BuildSource(string name, string extension, GraphicsProfile profile)
    {
        string body = File.ReadAllText($"ressources/Shaders/{name}.{extension}");

        string header = profile.IsOpenGLES ? "#version 300 es\n" : "#version 330 core\n";
        if (profile.IsOpenGLES && extension == "frag")
            header += "precision mediump float;\n"; // obligatoire en ES pour les fragment shaders

        return header + body;
    }

    private uint Compile(ShaderType type, string source)
    {
        uint id = _gl.CreateShader(type);
        _gl.ShaderSource(id, source);
        _gl.CompileShader(id);

        string log = _gl.GetShaderInfoLog(id);
        if (!string.IsNullOrWhiteSpace(log))
            throw new Exception($"Failed to compile shader ({type}) : {log}");

        return id;
    }

    public void Use() =>
        _gl.UseProgram(_programId);

    public void SetUniform(string name, Matrix4x4 matrix)
    {
        int location = _gl.GetUniformLocation(_programId, name);

        unsafe
        {
            _gl.UniformMatrix4(location, 1, false, (float*)&matrix);
        }
    }

    public void SetUniform(string name, int value)
    {
        int location = _gl.GetUniformLocation(_programId, name);
        _gl.Uniform1(location, value);
    }

    public void Dispose() =>
        _gl.DeleteProgram(_programId);
}
