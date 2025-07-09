using System.Numerics;
using System.Collections.Generic;
using Everything_To_IMU_SlimeVR.Tracking;
using Everything_To_IMU_SlimeVR;
using static Everything_To_IMU_SlimeVR.Tracking.Forwarded3DSDataManager;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
public class ThreeDsStateTracker {
    private VQFWrapper _vqf; // or your desired timestep
    private readonly GyroPreprocessor _gyroPreprocessor = new GyroPreprocessor();
    private readonly List<Vector3> _calibrationSamples = new();
    private bool _isCalibrating = false;
    private int accumulatedY;
    private int gravityCalibrationSamples;
    private float divisionValue;
    private readonly List<long> _timestamps = new();
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private bool _vqfInitialized = false;
    public ThreeDsStateTracker() {
        StartCalibration();
    }

    public ThreeDSState ProcessPacket(ImuPacket packet) {
        var value = packet;

        if (!_vqfInitialized) {
            long now = _stopwatch.ElapsedTicks;
            _timestamps.Add(now);

            if (_timestamps.Count >= 1000) {
                // Calculate average dt in seconds
                double ticksPerSecond = Stopwatch.Frequency;
                double totalDt = 0;
                for (int i = 1; i < _timestamps.Count; i++) {
                    totalDt += (_timestamps[i] - _timestamps[i - 1]) / ticksPerSecond;
                }

                double averageDt = totalDt / (_timestamps.Count - 1);

                Console.WriteLine($"Avg dt: {averageDt * 1000.0:F2} ms ({1.0 / averageDt:F2} Hz)");

                _vqf = new VQFWrapper((float)averageDt);
                _vqfInitialized = true;
            }
        } else {

            var y = Math.Abs(packet.ay);
            if (gravityCalibrationSamples < 100) {
                accumulatedY += y;
                gravityCalibrationSamples++;
                divisionValue = 9.8f / (accumulatedY / gravityCalibrationSamples);
            }
            var accel = new Vector3(((float)value.ax * divisionValue),
                ((float)value.az * divisionValue),
                ((float)value.ay * divisionValue));

            if (_isCalibrating) {
                if (_calibrationSamples.Count >= 100) {
                    _gyroPreprocessor.Calibrate(_calibrationSamples);
                    _isCalibrating = false;
                } else {
                    _calibrationSamples.Add(new Vector3(
        packet.gx,
        packet.gy,
        packet.gz));
                }
            } else {

                // Process gyro and convert degrees/sec to radians/sec
                Vector3 gyroCalibrated = _gyroPreprocessor.ProcessRawGyro(packet.gx, packet.gy, packet.gz, 0.00125f);
                Vector3 gyroRad = new Vector3(
                    -gyroCalibrated.X,
                    -gyroCalibrated.Y,
                    -gyroCalibrated.Z);

                // Update VQF filter
                _vqf.Update(gyroRad.ToVQFDoubleArray(), accel.ToVQFDoubleArray());
                var quatData = _vqf.GetQuat6D();
                var orientation = new Quaternion(
                    (float)quatData[1], (float)quatData[2], (float)quatData[3], (float)quatData[0]);
                return new ThreeDSState {
                    accelX = value.ax,
                    accelY = value.ay,
                    accelZ = value.az,
                    gyroX = value.gx, gyroY = value.gy, gyroZ = value.gz,
                    quatX = orientation.X, quatY = orientation.Y, quatZ = orientation.Z, quatW = orientation.W
                };
            }
        }
        return new ThreeDSState {
            accelX = value.ax,
            accelY = value.ay,
            accelZ = value.az,
            gyroX = value.gx, gyroY = value.gy, gyroZ = value.gz
        };
    }

    public void StartCalibration() {
        _calibrationSamples.Clear();
        _isCalibrating = true;
    }
}
