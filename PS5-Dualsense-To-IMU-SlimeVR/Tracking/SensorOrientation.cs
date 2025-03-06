using PS5_Dualsense_To_IMU_SlimeVR.Utility;
using System.Diagnostics;
using System.Numerics;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class SensorOrientation : IDisposable {
        private Quaternion currentOrientation = Quaternion.Identity;
        private Vector3 accelerometerData = Vector3.Zero;
        private Vector3 gyroData = Vector3.Zero;
        private float _deltaTime = 0.02f; // Example time step (e.g., 50Hz)
        private float _previousTime;

        // Complementary filter parameters
        private float alpha = 0.98f; // Weighting factor for blending
        private IBodyTracker _bodyTracker;
        private int _index;
        private Vector3 _accellerometerVectorCalibration;
        private Vector3 _gyroVectorCalibration;
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

        public Quaternion CurrentOrientation { get => currentOrientation; set => currentOrientation = value; }
        public Vector3 AccelerometerData { get => accelerometerData; set => accelerometerData = value; }
        public Vector3 GyroData { get => gyroData; set => gyroData = value; }
        public float YawRadians { get => yawRadians; set => yawRadians = value; }
        public float YawDegrees { get => yawDegrees; set => yawDegrees = value; }

        public SensorOrientation(int index) {
            _index = index;
            RefreshSensorData();
            _accellerometerVectorCalibration = -(_accelerometer);

            _gyroVectorCalibration = -(_gyro);
            stopwatch.Start();
            Task.Run(() => {
                while (!disposed) {
                    Update();
                    Thread.Sleep(16);
                }
            });
        }

        private void RefreshSensorData() {
            var sensorData = JSL.JslGetMotionState(_index);
            _accelerometer = new Vector3(sensorData.gravX, sensorData.gravY, sensorData.gravZ);
            _gyro = new Vector3(sensorData.accelX, sensorData.accelY, sensorData.accelZ);
        }
        // Update method to simulate gyroscope and accelerometer data fusion
        public void Update() {
            if (!disposed) {
                RefreshSensorData();
                float currentTime = (float)stopwatch.Elapsed.TotalSeconds;

                // Calculate deltaTime (the difference between current and previous time)
                _deltaTime = currentTime - _previousTime;

                // Update previousTime for the next frame
                _previousTime = currentTime;

                // Accelerometer data
                accelerometerData = (_accelerometer + _accellerometerVectorCalibration) * 10;

                // Gyroscope data
                gyroData = (_gyro + _gyroVectorCalibration);

                // Step 1: Calculate pitch and roll from accelerometer data
                Quaternion accelerometerOrientation = GetOrientationFromAccelerometer(accelerometerData);

                // Step 2: Calculate delta rotation from gyroscope data
                Quaternion gyroDeltaRotation = GetDeltaRotationFromGyroscope(gyroData, _deltaTime);

                // Step 3: Fuse accelerometer and gyroscope data using complementary filter
                currentOrientation = accelerometerOrientation;

                // Normalize the quaternion to prevent drift
                currentOrientation = Quaternion.Normalize(currentOrientation);

                if (!IsValid(currentOrientation)) {
                    currentOrientation = Quaternion.Identity;
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
