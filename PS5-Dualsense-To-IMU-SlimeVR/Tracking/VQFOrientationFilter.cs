using System.Diagnostics;
using System.Numerics;

public class VQFOrientationFilter {
    Stopwatch stopwatch = new Stopwatch();
    // Filter states
    private Quaternion _state = Quaternion.Identity;
    private Vector3 _bias = Vector3.Zero;

    // Filter parameters
    private float _tauAcc = 0.5f;    // Accelerometer time constant
    private float _tauMag = 1.0f;    // Not used for Wii Remote
    private float _tauBias = 200.0f; // Bias estimation time constant

    // Timing
    private DateTime _lastUpdate = DateTime.Now;

    // Gyro processor with proper scaling
    private readonly GyroPreprocessor _gyro = new GyroPreprocessor();

    public GyroPreprocessor Gyro => _gyro;

    public Quaternion Update(Vector3 accel, short gyroX, short gyroY, short gyroZ) {
        float deltaTime = (float)(stopwatch.Elapsed).TotalSeconds;
        stopwatch.Restart();

        if (deltaTime <= 0 || deltaTime > 0.1f)
            return _state;

        // 1. Process gyro data
        Vector3 gyroRadSec = _gyro.ProcessRawGyro(gyroX, gyroY, gyroZ);

        // 2. Remove estimated bias
        Vector3 correctedGyro = gyroRadSec - _bias;

        // 3. Integrate gyro into quaternion properly
        float angle = correctedGyro.Length() * deltaTime;
        if (angle > 0) {
            Vector3 axis = Vector3.Normalize(correctedGyro);
            Quaternion deltaRotation = Quaternion.CreateFromAxisAngle(axis, angle);
            _state = Quaternion.Normalize(_state * deltaRotation);
        }

        // 4. Accelerometer correction
        if (accel.Length() > 0.5f && accel.Length() < 1.5f) {
            Vector3 normAccel = Vector3.Normalize(accel);

            // Flip Z if gravity is down
            Vector3 estimatedGravity = Vector3.Normalize(RotateVector(-Vector3.UnitZ, _state));

            // Correction axis and angle
            Vector3 correctionAxis = Vector3.Cross(estimatedGravity, normAccel);
            float dot = Math.Clamp(Vector3.Dot(estimatedGravity, normAccel), -1f, 1f);
            float correctionAngle = MathF.Acos(dot);

            if (correctionAxis.LengthSquared() > 0.00001f && correctionAngle > 0.0001f) {
                correctionAxis = Vector3.Normalize(correctionAxis);
                float gain = _tauAcc / (_tauAcc + deltaTime);
                Quaternion correction = Quaternion.CreateFromAxisAngle(correctionAxis, correctionAngle * gain);
                _state = Quaternion.Normalize(correction * _state);
            }

            // Optional: temporarily disable this to isolate error source
            Vector3 residualGyro = gyroRadSec - correctedGyro;
            _bias += residualGyro * (deltaTime / _tauBias);
        }


        return _state;
    }

    private Vector3 RotateVector(Vector3 v, Quaternion q) {
        Vector3 u = new Vector3(q.X, q.Y, q.Z);
        float s = q.W;

        return 2.0f * Vector3.Dot(u, v) * u
            + (s * s - Vector3.Dot(u, u)) * v
            + 2.0f * s * Vector3.Cross(u, v);
    }

    public void Reset() {
        _state = Quaternion.Identity;
        _bias = Vector3.Zero;
        _lastUpdate = DateTime.Now;
    }
}