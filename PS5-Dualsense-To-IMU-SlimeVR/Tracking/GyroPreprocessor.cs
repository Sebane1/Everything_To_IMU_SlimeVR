using System.Numerics;

public class GyroPreprocessor {
    // Calibration data
    private Vector3 _offsets = Vector3.Zero;
    private bool _isCalibrated = false;

    // Wii Remote specific constants
    private const int GyroCenter = 0; // Raw zero-rate level
    private const float ScaleFactor = 0.001065f; // Verified scale for MotionPlus

    // IIR Low-pass filter
    private Vector3 _filtered = Vector3.Zero;
    private const float FilterAlpha = 0.15f;

    // Deadzone (in radians/sec)
    private const float Deadzone = 0.0087f; // ~0.5°/sec

    public Vector3 ProcessRawGyro(short x, short y, short z) {
        // 1. Remove offsets and center
        Vector3 calibrated = new Vector3(
            x - GyroCenter - _offsets.X,
            y - GyroCenter - _offsets.Y,
            z - GyroCenter - _offsets.Z);

        // 2. Scale to radians/sec
        Vector3 radSec = calibrated * ScaleFactor;

        // 3. Apply low-pass filter
        _filtered = _filtered * (1 - FilterAlpha) + radSec * FilterAlpha;

        // 4. Apply deadzone
        return ApplyDeadzone(_filtered);
    }

    private Vector3 ApplyDeadzone(Vector3 input) {
        return new Vector3(
            Math.Abs(input.X) < Deadzone ? 0 : input.X,
            Math.Abs(input.Y) < Deadzone ? 0 : input.Y,
            Math.Abs(input.Z) < Deadzone ? 0 : input.Z);
    }

    public void Calibrate(IEnumerable<Vector3> samples) {
        if (samples.Count() < 100) return;

        _offsets = new Vector3(
            samples.Average(s => s.X - GyroCenter),
            samples.Average(s => s.Y - GyroCenter),
            samples.Average(s => s.Z - GyroCenter));

        _isCalibrated = true;
    }
}