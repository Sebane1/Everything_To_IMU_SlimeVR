using System;
using System.Numerics;

public static class QuaternionUtils {
    public static Quaternion QuatFromGravity(
        float x, float y, float z,
        float cx, float cy, float cz,
        float scale
    ) {
        // Scale raw values
        float sx = (x - cx) / scale;
        float sy = (y - cy) / scale;
        float sz = (z - cz) / scale;

        // Clamp scaled acceleration to avoid spikes
        const float clampMax = 1.5f;
        const float clampMin = -1.5f;
        sx = Math.Clamp(sx, clampMin, clampMax);
        sy = Math.Clamp(sy, clampMin, clampMax);
        sz = Math.Clamp(sz, clampMin, clampMax);

        Vector3 gravity = Vector3.Normalize(new Vector3(sx, sy, sz));
        Vector3 reference = new Vector3(0.0f, 0.0f, -1.0f);

        Vector3 axis = Vector3.Cross(gravity, reference);
        float dot = Vector3.Dot(gravity, reference);
        float angle = MathF.Acos(dot);

        if (MathF.Abs(dot - 1.0f) < 1e-5f) {
            return new Quaternion(0f, 0f, 0f, 1f); // Identity quaternion
        } else if (MathF.Abs(dot + 1.0f) < 1e-5f) {
            return new Quaternion(1f, 0f, 0f, 0f); // 180-degree rotation around X
        }

        axis = Vector3.Normalize(axis);
        float halfAngle = angle * 0.5f;
        float sinHalf = MathF.Sin(halfAngle);

        return new Quaternion(
            axis.X * sinHalf,
            axis.Y * sinHalf,
            axis.Z * sinHalf,
            MathF.Cos(halfAngle)
        );
    }
}
