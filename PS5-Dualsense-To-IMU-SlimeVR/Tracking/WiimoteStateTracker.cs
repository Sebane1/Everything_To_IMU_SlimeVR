using static Everything_To_IMU_SlimeVR.Tracking.ForwardedWiimoteManager;
using System.Numerics;
using System.Collections.Generic;
using Everything_To_IMU_SlimeVR.Tracking;
using Everything_To_IMU_SlimeVR;
public class WiimoteStateTracker {
    private VQFWrapper _vqf; // or your desired timestep
    private readonly GyroPreprocessor _gyroPreprocessor = new GyroPreprocessor();
    private readonly List<Vector3> _calibrationSamples = new();
    private bool _isCalibrating = false;

    public WiimoteInfo ProcessPacket(WiimotePacket packet) {
        var info = new WiimoteInfo(packet);
        if (_vqf == null) {
            _vqf = new VQFWrapper(Configuration.Instance.WiiPollingRate / 1000f);
        }
        if (_isCalibrating) {
            _calibrationSamples.Add(new Vector3(
                info.WiimoteGyroX,
                info.WiimoteGyroY,
                info.WiimoteGyroZ));
            if (_calibrationSamples.Count >= 100) {
                _gyroPreprocessor.Calibrate(_calibrationSamples);
                _isCalibrating = false;
            }
        }

        // Convert raw accel (assuming 0–1024 scale) to m/s²
        Vector3 accel = new Vector3(
            (info.WiimoteAccelX - 512) / 200.0f,
            (info.WiimoteAccelY - 512) / 200.0f,
            (info.WiimoteAccelZ - 512) / 200.0f) * 9.80665f;

        // Process gyro and convert degrees/sec to radians/sec
        Vector3 gyroRaw = new Vector3(info.WiimoteGyroX, info.WiimoteGyroY, info.WiimoteGyroZ);
        Vector3 gyroCalibrated = _gyroPreprocessor.ProcessRawGyro(info.WiimoteGyroX, info.WiimoteGyroY, info.WiimoteGyroZ);
        Vector3 gyroRad = new Vector3(
            gyroCalibrated.X,
            gyroCalibrated.Y,
            gyroCalibrated.Z);

        // Update VQF filter
        _vqf.Update((gyroRad).ToVQFDoubleArray(), accel.ToVQFDoubleArray());
        var quatData = _vqf.GetQuat6D();
        info.WiimoteFusedOrientation = new Quaternion(
            (float)quatData[1], (float)quatData[2], (float)quatData[3], (float)quatData[0]);

        return info;
    }

    public void StartCalibration() {
        _calibrationSamples.Clear();
        _isCalibrating = true;
    }
}
