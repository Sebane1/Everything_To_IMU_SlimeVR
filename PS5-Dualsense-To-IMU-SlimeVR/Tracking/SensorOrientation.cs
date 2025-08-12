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
            if (e.Item1 == _index) {
                _accelerometer = new Vector3(e.Item4.accelX, e.Item4.accelY, e.Item4.accelZ) * 10 /* 9.80665f*/;
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
        }

        static void OnControllerEvent(int deviceId, JSL.JOY_SHOCK_STATE state, JSL.JOY_SHOCK_STATE state2, JSL.IMU_STATE imuState, JSL.IMU_STATE imuState2, float delta) {
            OnNewJSLData?.Invoke(new object(), new Tuple<int, JOY_SHOCK_STATE, JOY_SHOCK_STATE, IMU_STATE, IMU_STATE, float>(deviceId, state, state2, imuState, imuState2, delta));
        }
        //private JSL.IMU_STATE GetReleventMotionState() {
        //    switch (_sensorType) {
        //        case SensorType.Bluetooth:
        //            return JSL.JslGetIMUState(_index);
        //        case SensorType.ThreeDs:
        //            return Forwarded3DSDataManager.DeviceMap.ElementAt(_index).Value;
        //    }
        //    return new JSL.IMU_STATE();
        //}
        public enum SensorType {
            Bluetooth = 0,
            ThreeDs = 1,
            Wiimote = 2,
            Nunchuck = 3
        }
        // Update method to simulate gyroscope and accelerometer data fusion
        public async void Update() {
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
        }

        // Get orientation from accelerometer data (pitch and roll)
        private Quaternion GetOrientationFromAccelerometer(Vector3 accelData) {
            // Normalize the accelerometer data (gravity vector)
            accelData = Vector3.Normalize(accelData);

            // Assuming gravity vector points along the Z-axis
            float pitch = (float)Math.Atan2(accelData.Y, accelData.Z);
            float roll = (float)Math.Atan2(-accelData.X, Math.Sqrt(accelData.Y * accelData.Y + accelData.Z * accelData.Z));

            // Create a quaternion based on the pitch and roll (ignore yaw for now)
            Quaternion orientation = Quaternion.CreateFromYawPitchRoll(0, pitch, roll);

            return orientation;
        }

        public bool IsValid(Quaternion quaternion) {
            bool isNaN = float.IsNaN(quaternion.X + quaternion.Y + quaternion.Z + quaternion.W);
            bool isZero = quaternion.X == 0 && quaternion.Y == 0 && quaternion.Z == 0 && quaternion.W == 0;
            return !(isNaN || isZero);
        }

        // Get delta rotation from gyroscope data
        private Quaternion GetDeltaRotationFromGyroscope(Vector3 gyroData, float deltaTime) {
            // Calculate the angle from the gyroscope angular velocity
            float angle = gyroData.Length() * deltaTime;

            if (angle > 0) {
                // Normalize the gyro data to get the rotation axis
                Vector3 axis = Vector3.Normalize(gyroData);

                // Calculate quaternion from the axis-angle representation
                float halfAngle = angle / 2.0f;
                float sinHalfAngle = (float)Math.Sin(halfAngle);
                float cosHalfAngle = (float)Math.Cos(halfAngle);

                gyroYawRate += gyroData.Z * deltaTime;
                yawRadians = gyroDriftCompensation * (yawRadians + gyroData.Z * deltaTime) + (1 - gyroDriftCompensation) * gyroYawRate;
                if (yawRadians > Math.PI) {
                    yawRadians -= (float)(2 * Math.PI);
                } else if (yawRadians < -Math.PI) {
                    yawRadians += (float)(2 * Math.PI);
                }
                yawDegrees = yawRadians.ConvertRadiansToDegrees();
                return new Quaternion(axis.X * sinHalfAngle, axis.Y * sinHalfAngle, axis.Z * sinHalfAngle, cosHalfAngle);
            } else {
                return Quaternion.Identity;
            }
        }

        public void Dispose() {
            disposed = true;
        }
    }
}
