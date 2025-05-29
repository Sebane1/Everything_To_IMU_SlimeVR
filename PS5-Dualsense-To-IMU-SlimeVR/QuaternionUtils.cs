using System;
using System.Numerics;

public static class QuaternionUtils {
    private static Vector3 NormalizeVector(float x, float y, float z) {
        float length = MathF.Sqrt(x * x + y * y + z * z);
        if (length == 0f) return new Vector3(0f, 0f, 0f);
        return new Vector3(x / length, y / length, z / length);
    }

    public static Quaternion QuatFromGravity(
        float x, float y, float z,
        float cx, float cy, float cz,
        float scale
    ) {
        // 1. Scale raw values
        float sx = (x - cx) / scale;
        float sy = (y - cy) / scale;
        float sz = (z - cz) / scale;

        // 2. Clamp values to [-1.5, 1.5]
        const float clamp = 1f;
        sx = Math.Clamp(sx, -clamp, clamp);
        sy = Math.Clamp(sy, -clamp, clamp);
        sz = Math.Clamp(sz, -clamp, clamp);

        // 3. Use custom normalize function
        Vector3 gravity = NormalizeVector(sx, sy, sz);
        Vector3 reference = new Vector3(0f, 0f, -1f);

        // 4. Cross product to find axis
        Vector3 axis = new Vector3(
            gravity.Y * reference.Z - gravity.Z * reference.Y,
            gravity.Z * reference.X - gravity.X * reference.Z,
            gravity.X * reference.Y - gravity.Y * reference.X
        );

        // 5. Dot product and clamp
        float dot = gravity.X * reference.X + gravity.Y * reference.Y + gravity.Z * reference.Z;
        dot = Math.Clamp(dot, -1.0f, 1.0f); // ensure acos is valid

        // 6. Edge cases
        if (MathF.Abs(dot - 1.0f) < 1e-5f) {
            return new Quaternion(0f, 0f, 0f, 1f); // identity
        }
        if (MathF.Abs(dot + 1.0f) < 1e-5f) {
            return new Quaternion(1f, 0f, 0f, 0f); // 180 degrees around X
        }

        // 7. Normalize axis
        axis = NormalizeVector(axis.X, axis.Y, axis.Z);

        // 8. Convert axis-angle to quaternion
        float angle = MathF.Acos(dot);
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
