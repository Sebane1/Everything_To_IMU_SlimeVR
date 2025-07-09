using System.Numerics;

public class GyroPreprocessor {
    // Calibration data
    private Vector3 _offsets = Vector3.Zero;
    private bool _isCalibrated = false;

    // Wii Remote specific constants
    private const int GyroCenter = 0; // Raw zero-rate level

    public Vector3 ProcessRawGyro(short x, short y, short z, float scaleFactor) {
        // 1. Remove offsets and center
        Vector3 calibrated = new Vector3(
            x - GyroCenter - _offsets.X,
            y - GyroCenter - _offsets.Y,
            z - GyroCenter - _offsets.Z);

        // 2. Scale to radians/sec
        return calibrated * scaleFactor;
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