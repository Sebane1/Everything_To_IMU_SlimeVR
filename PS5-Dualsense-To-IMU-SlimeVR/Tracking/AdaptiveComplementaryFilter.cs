using System.Diagnostics;
using System.Numerics;

public class AdaptiveComplementaryFilter {
    Stopwatch stopwatch = new Stopwatch();
    // Filter parameters
    private const float MinAlpha = 0.8f;  // Minimum gyro trust (slow movements)
    private const float MaxAlpha = 0.99f;  // Maximum gyro trust (fast movements)
    private const float GyroThreshold = 5f; // Degrees/sec for motion detection

    // State
    private Quaternion _orientation = Quaternion.Identity;
    private DateTime _lastUpdateTime = DateTime.Now;
    private Vector3 _lastGyro = Vector3.Zero;

    public Quaternion Update(Quaternion accelOrientation, Vector3 gyroRates) {
        // Calculate time delta
        var now = DateTime.Now;
        float deltaTime = (float)(stopwatch.Elapsed).TotalSeconds;
        stopwatch.Restart();

        if (deltaTime <= 0)
            return _orientation;

        // Calculate dynamic alpha based on motion
        float alpha = CalculateDynamicAlpha(gyroRates, _lastGyro);
        _lastGyro = gyroRates;

        // Convert gyro rates to rotation quaternion
        Vector3 rotationRad = gyroRates * (MathF.PI / 180f) * deltaTime;
        Quaternion gyroDelta = Quaternion.CreateFromYawPitchRoll(
            rotationRad.Y,
            rotationRad.X,
            rotationRad.Z);

        // Apply gyro integration
        Quaternion gyroOrientation = _orientation * gyroDelta;

        // Complementary blend
        _orientation = Quaternion.Slerp(
            gyroOrientation,
            accelOrientation,
            1.0f - alpha);

        return _orientation;
    }

    private float CalculateDynamicAlpha(Vector3 currentGyro, Vector3 lastGyro) {
        // Detect motion magnitude
        float gyroChange = (currentGyro - lastGyro).Length();

        // Simple adaptive logic:
        // - More gyro trust (higher alpha) during fast/consistent motion
        // - Less gyro trust during slow/erratic motion
        float motionFactor = Math.Clamp(gyroChange / GyroThreshold, 0f, 1f);

        return MinAlpha + (MaxAlpha - MinAlpha) * motionFactor;
    }
}