using System.Numerics;
using System.Drawing;

public sealed class Camera
{
    public Vector3 Target = Vector3.Zero;
    public Vector3 Position = new Vector3(0.5f, 0.5f, 1f) * 1000;

    public float FieldOfViewDegrees = 60f;

    public float NearPlane = 0.1f;      // Near clamping distance
    public float FarPlane = 100000f;    // Far clamping distance

    public Color backgroundColor = Color.Black;
 
    public Matrix4x4 GetViewMatrix() =>
        Matrix4x4.CreateLookAt(Position, Target, Vector3.UnitY);

    public Matrix4x4 GetProjectionMatrix(float viewportWidth, float viewportHeight)
    {
        float fovRadians = FieldOfViewDegrees * MathF.PI / 180f;
        return Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, viewportWidth / viewportHeight, NearPlane, FarPlane);
    }
}