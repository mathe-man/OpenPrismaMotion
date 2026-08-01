using System.Numerics;

namespace PrismaViz;

public sealed class CameraController
{
    private readonly Camera _camera;

    public float OrbitSpeed = 0.005f;
    public float PanSpeed = 0.0015f;
    public float ZoomSpeed = 0.1f;

    public CameraController(Camera camera) => _camera = camera;


    public void Orbit(float deltaX, float deltaY)
    {
        Vector3 offset = _camera.Position - _camera.Target;
        float distance = offset.Length();

        // Build yaw and pitch from actual camera possition, then apply deltaX and deltaY to them.
        float yaw = MathF.Atan2(offset.X, offset.Z);
        float pitch = MathF.Asin(offset.Y / distance);

        yaw -= deltaX * OrbitSpeed;
        pitch += deltaY * OrbitSpeed;

        // Avoid gimbal lock by clamping pitch to just below 90 degrees.
        float limit = MathF.PI / 2f - 0.02f;
        pitch = Math.Clamp(pitch, -limit, limit);

        var newOffset = new Vector3(
            distance * MathF.Cos(pitch) * MathF.Sin(yaw),
            distance * MathF.Sin(pitch),
            distance * MathF.Cos(pitch) * MathF.Cos(yaw));

        _camera.Position = _camera.Target + newOffset;
    }


    public void Pan(float deltaX, float deltaY)
    {
        Vector3 forward = Vector3.Normalize(_camera.Target - _camera.Position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        Vector3 up = Vector3.Cross(right, forward);

        // The pan is scaled by the distance to the target to make it feel more natural.
        float distance = Vector3.Distance(_camera.Position, _camera.Target);
        Vector3 pan = (-deltaX * right + deltaY * up) * PanSpeed * distance;

        _camera.Position += pan;
        _camera.Target += pan;
    }

    public void Zoom(float wheelDelta)
    {
        Vector3 offset = _camera.Position - _camera.Target;
        float distance = offset.Length();
        float newDistance = MathF.Max(1f, distance * (1f - wheelDelta * ZoomSpeed));

        _camera.Position = _camera.Target + Vector3.Normalize(offset) * newDistance;
    }
}
