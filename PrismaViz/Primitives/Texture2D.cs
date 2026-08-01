using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PrismaViz.Primitives;

public sealed class Texture2D : IDisposable
{
    private readonly GL _gl;
    private readonly uint _id;

    public int Width { get; }
    public int Height { get; }

    public static Texture2D FromFile(GL gl, string path)
    {
        using var image = Image.Load<Rgba32>(path);

        // CopyPixelDataTo copy pixels in a contiguous memory array
        var pixels = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixels);

        return new Texture2D(gl, pixels, image.Width, image.Height);
    }
    public static Texture2D CreateWhite1x1(GL gl)
    {
        byte[] pixel = { 255, 255, 255, 255 };
        return new Texture2D(gl, pixel, 1, 1);
    }

    private unsafe Texture2D(GL gl, byte[] pixels, int width, int height)
    {
        _gl = gl;
        Width = width;
        Height = height;

        _id = _gl.GenTexture();

        
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _id);

        fixed (byte* data = pixels)
        {
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8,
                (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, data);
        }

        // Linear filter
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        // Clamp edges (if the texture is too big)
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
    }

    public void Bind()
    {
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _id);
    }

    public void Dispose() =>
        _gl.DeleteTexture(_id);
}