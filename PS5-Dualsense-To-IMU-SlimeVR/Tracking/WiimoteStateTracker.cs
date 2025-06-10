using static Everything_To_IMU_SlimeVR.Tracking.ForwardedWiimoteManager;
using System.Numerics;

public class WiimoteStateTracker {
    private readonly StableOrientationFilter _filter = new();
    private readonly List<Vector3> _calibrationSamples = new();
    private bool _isCalibrating = false;

    public WiimoteInfo ProcessPacket(WiimotePacket packet) {
        var info = new WiimoteInfo(packet);

        // Collect calibration samples if needed
        if (_isCalibrating) {
            _calibrationSamples.Add(new Vector3(
                info.WiimoteGyroX,
                info.WiimoteGyroY,
                info.WiimoteGyroZ));

            if (_calibrationSamples.Count >= 100) // Enough samples
            {
                _filter.Gyro.Calibrate(_calibrationSamples);
                _isCalibrating = false;
            }
        }

        // Update filter
        info.WiimoteFusedOrientation = _filter.Update(
            info.WiimoteGravityOrientation,
            info.WiimoteGyroX,
            info.WiimoteGyroY,
            info.WiimoteGyroZ);

        return info;
    }

    public void StartCalibration() {
        _calibrationSamples.Clear();
        _isCalibrating = true;
    }
}