using SlimeImuProtocol.Utility;
using System.Diagnostics;
using System.Numerics;
using static JSL;

namespace Everything_To_IMU_SlimeVR.Tracking {
    internal class SensorOrientation : IDisposable {
        private Quaternion currentOrientation = Quaternion.Identity;
        private float _deltaTime = 0.02f; // Example time step (e.g., 50Hz)
        private float _previousTime;
        private bool _useCustomFusion;

        // Complementary filter parameters
        private float alpha = 0.98f; // Weighting factor for blending
        private IBodyTracker _bodyTracker;
        private int _index;
        private SensorType _sensorType;
        Stopwatch stopwatch = new Stopwatch();
        bool _calibratedRotation = false;
        private bool disposed;
        private Vector3 _magnetometer;
        private float gyroYawRate;
        private float gyroDriftCompensation;
        private float yawRadians;
        private float yawDegrees;
        private Vector3 _accelerometer;
        private Vector3 _gyro;
        private byte _battery;
        private VQFWrapper _vqf;
        private JSL.EventCallback _callback;
        List<float> averageSampleTicks = new List<float>();
        private static bool jslHandlerSet = false;

        public Quaternion CurrentOrientation { get => currentOrientation; set => currentOrientation = value; }
        public float YawRadians { get => yawRadians; set => yawRadians = value; }
        public float YawDegrees { get => yawDegrees; set => yawDegrees = value; }
        public Quaternion AXES_OFFSET { get; internal set; }
        public Vector3 Accelerometer { get => _accelerometer; set => _accelerometer = value; }
        public Vector3 Gyro { get => _gyro; set => _gyro = value; }
        public event EventHandler NewData;

        public static event EventHandler<Tuple<int, JSL.JOY_SHOCK_STATE, JSL.JOY_SHOCK_STATE, JSL.IMU_STATE, JSL.IMU_STATE, float>> OnNewJSLData;
        public event EventHandler<string> OnExceptionMessage;

        public SensorOrientation(int index, SensorType sensorType) {
            // Apply AXES_OFFSET * rot
            float angle = -MathF.PI / 2;

            AXES_OFFSET = Quaternion.CreateFromAxisAngle(Vector3.UnitX, angle);
            _index = index;
            _sensorType = sensorType;
            stopwatch.Start();
            if (!jslHandlerSet) {
                _callback = new JSL.EventCallback(OnControllerEvent);
                jslHandlerSet = true;
                JSL.JslSetCallback(_callback);
            }
            OnNewJSLData += SensorOrientation_OnNewJSLData;
        }

        private void SensorOrientation_OnNewJSLData(object? sender, Tuple<int, JSL.JOY_SHOCK_STATE, JSL.JOY_SHOCK_STATE, JSL.IMU_STATE, JSL.IMU_STATE, float> e) {
            try {
                if (e.Item1 == _index) {
                    _accelerometer = new Vector3(e.Item4.accelX, e.Item4.accelY, e.Item4.accelZ) * 10;
                    _gyro = (new Vector3(e.Item4.gyroX, e.Item4.gyroY, e.Item4.gyroZ)).ConvertDegreesToRadians();
                    if (_vqf == null) {
                        if (averageSampleTicks.Count < 1000) {
                            averageSampleTicks.Add(e.Item6);
                        } else {
                            _vqf = new VQFWrapper(averageSampleTicks.Average());
                            averageSampleTicks.Clear();
                        }
                    } else {
                        Update();
                    }
                }
            } catch (Exception ex) {
                OnExceptionMessage?.Invoke(this, ex.Message + ":" + ex.StackTrace);
            }
        }

        static void OnControllerEvent(int deviceId, JSL.JOY_SHOCK_STATE state, JSL.JOY_SHOCK_STATE state2, JSL.IMU_STATE imuState, JSL.IMU_STATE imuState2, float delta) {
            try {
                OnNewJSLData?.Invoke(new object(), new Tuple<int, JOY_SHOCK_STATE, JOY_SHOCK_STATE, IMU_STATE, IMU_STATE, float>(deviceId, state, state2, imuState, imuState2, delta));
            } catch (Exception ex) {
            }
        }

        public enum SensorType {
            Bluetooth = 0,
            ThreeDs = 1,
            Wiimote = 2,
            Nunchuck = 3
        }
        // Update method to simulate gyroscope and accelerometer data fusion
        public async void Update() {
            try {
                if (!disposed) {
                    switch (_sensorType) {
                        case SensorType.Bluetooth:
                            _vqf.Update(_gyro.ToVQFDoubleArray(), _accelerometer.ToVQFDoubleArray());
                            var vfqData = _vqf.GetQuat6D();
                            currentOrientation = new Quaternion((float)vfqData[1], (float)vfqData[2], (float)vfqData[3], (float)vfqData[0]);
                            NewData?.Invoke(this, EventArgs.Empty);
                            break;
                    }
                }
            } catch (Exception ex) {
                OnExceptionMessage?.Invoke(this, ex.Message + ":" + ex.StackTrace);
            }
        }

        public void Dispose() {
            disposed = true;
        }
    }
}
