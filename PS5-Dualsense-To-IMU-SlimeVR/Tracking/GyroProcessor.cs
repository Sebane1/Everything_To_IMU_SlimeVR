using System.Numerics;

public class GyroProcessor {
    // Calibration offsets (should be set during calibration)
    private Vector3 _gyroOffsets = Vector3.Zero;

    // Conversion factors
    private const float RawToDegPerSec = 0.07f; // Wii Remote specific scaling
    private const float DegToRad = MathF.PI / 180f;

    // Noise thresholds
    private const float DeadzoneThreshold = 0.5f; // deg/sec

    public Vector3 ProcessRawGyro(short x, short y, short z) {
        // 1. Apply calibration offsets
        Vector3 calibrated = new Vector3(
            x - _gyroOffsets.X,
            y - _gyroOffsets.Y,
            z - _gyroOffsets.Z);

        // 2. Convert to degrees/sec
        Vector3 degPerSec = calibrated * RawToDegPerSec;

        // 3. Apply deadzone
        degPerSec = ApplyDeadzone(degPerSec);

        // 4. Convert to radians/sec (for quaternion math)
        return degPerSec * DegToRad;
    }

    private Vector3 ApplyDeadzone(Vector3 input) {
        return new Vector3(
            Math.Abs(input.X) < DeadzoneThreshold ? 0 : input.X,
            Math.Abs(input.Y) < DeadzoneThreshold ? 0 : input.Y,
            Math.Abs(input.Z) < DeadzoneThreshold ? 0 : input.Z);
    }

    public void Calibrate(IEnumerable<Vector3> samples) {
        // Calculate mean of samples
        _gyroOffsets = new Vector3(
            samples.Average(s => s.X),
            samples.Average(s => s.Y),
            samples.Average(s => s.Z));
    }
}