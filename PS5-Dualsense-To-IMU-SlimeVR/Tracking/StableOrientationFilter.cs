using System.Numerics;

public class StableOrientationFilter {
    // State variables
    private Quaternion _state = Quaternion.Identity;
    private Matrix4x4 _covariance = Matrix4x4.Identity * 0.1f;
    private DateTime _lastUpdateTime = DateTime.Now;

    // Process noise (system dynamics)
    private readonly Matrix4x4 _Q = Matrix4x4.Identity * 0.0001f;

    // Measurement noise (accelerometer)
    private readonly Matrix4x4 _R = Matrix4x4.Identity * 0.05f;

    // Gyro processor with proper scaling
    private readonly GyroPreprocessor _gyro = new GyroPreprocessor();

    public GyroPreprocessor Gyro => _gyro;

    public Quaternion Update(Quaternion accelOrientation, short gyroX, short gyroY, short gyroZ) {
        float deltaTime = (float)(DateTime.Now - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = DateTime.Now;

        if (deltaTime <= 0 || deltaTime > 0.1f) // Handle large gaps
            return _state;

        // 1. Process gyro data (with proper scaling and filtering)
        Vector3 gyroRadSec = _gyro.ProcessRawGyro(gyroX, gyroY, gyroZ);

        // 2. Prediction Step (Gyro Integration)
        Quaternion gyroDelta = Quaternion.CreateFromYawPitchRoll(
            gyroRadSec.Y * deltaTime,
            gyroRadSec.X * deltaTime,
            gyroRadSec.Z * deltaTime);

        _state = Quaternion.Normalize(_state * gyroDelta);
        _covariance += _Q * deltaTime;

        // 3. Update Step (Accelerometer Correction)
        if (accelOrientation != Quaternion.Identity) {
            Matrix4x4 H = CalculateMeasurementJacobian();
            Matrix4x4 S = H * _covariance * Matrix4x4.Transpose(H) + _R;
            Matrix4x4.Invert(S, out var value);
            Matrix4x4 K = _covariance * Matrix4x4.Transpose(H) * value;

            // Quaternion error between measurement and state
            Quaternion error = accelOrientation * Quaternion.Inverse(_state);

            // Apply Kalman gain
            Vector4 correction = Vector4.Transform(
                new Vector4(error.X, error.Y, error.Z, error.W),
                K * 0.5f); // Reduced gain for stability

            _state = Quaternion.Normalize(_state *
                    new Quaternion(correction.X, correction.Y, correction.Z, correction.W));

            // Update covariance
            _covariance = (Matrix4x4.Identity - K * H) * _covariance;
        }

        return _state;
    }

    private Matrix4x4 CalculateMeasurementJacobian() {
        // Simplified Jacobian - assumes direct measurement of orientation
        // For a more accurate implementation, linearize the quaternion space
        return Matrix4x4.Identity;
    }

    public void Reset() {
        _state = Quaternion.Identity;
        _covariance = Matrix4x4.Identity * 0.1f;
        _lastUpdateTime = DateTime.Now;
    }

}