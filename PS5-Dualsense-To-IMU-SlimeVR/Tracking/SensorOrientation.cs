using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Wujek_Dualsense_API;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    internal class SensorOrientation {
        private Quaternion currentOrientation = Quaternion.Identity;
        private Vector3 accelerometerData = Vector3.Zero;
        private Vector3 gyroData = Vector3.Zero;
        private float _deltaTime = 0.02f; // Example time step (e.g., 50Hz)
        private float _previousTime;

        // Complementary filter parameters
        private float alpha = 0.98f; // Weighting factor for blending
        private Dualsense _dualsense;
        private Vector3 _accellerometerVectorCalibration;
        private Vector3 _gyroVectorCalibration;
        Stopwatch stopwatch = new Stopwatch();
        bool _calibratedRotation = false;
        public Quaternion CurrentOrientation { get => currentOrientation; set => currentOrientation = value; }
        public Vector3 AccelerometerData { get => accelerometerData; set => accelerometerData = value; }
        public Vector3 GyroData { get => gyroData; set => gyroData = value; }

        public SensorOrientation(Dualsense dualsense) {
            _dualsense = dualsense;
            _accellerometerVectorCalibration = -(new Vector3(
                _dualsense.ButtonState.accelerometer.X,
                _dualsense.ButtonState.accelerometer.Y,
                _dualsense.ButtonState.accelerometer.Z) / 1000);
            _gyroVectorCalibration = -(new Vector3(
                _dualsense.ButtonState.gyro.X,
                _dualsense.ButtonState.gyro.Y,
                _dualsense.ButtonState.gyro.Z) / 1000);
            stopwatch.Start();
            Task.Run(() => {
                while (true) {
                    Update();
                    Thread.Sleep(16);
                }
            });
        }

        // Update method to simulate gyroscope and accelerometer data fusion
        public void Update() {
            float currentTime = (float)stopwatch.Elapsed.TotalSeconds;

            // Calculate deltaTime (the difference between current and previous time)
            _deltaTime = currentTime - _previousTime;

            // Update previousTime for the next frame
            _previousTime = currentTime;

            // Accelerometer data
            accelerometerData = (new Vector3(
                _dualsense.ButtonState.accelerometer.X,
                _dualsense.ButtonState.accelerometer.Y,
                _dualsense.ButtonState.accelerometer.Z) / 1000) + _accellerometerVectorCalibration;

            // Gyroscope data
            gyroData = (new Vector3(
                _dualsense.ButtonState.gyro.X,
                _dualsense.ButtonState.gyro.Y,
                _dualsense.ButtonState.gyro.Z) / 1000) + _gyroVectorCalibration;

            // Step 1: Calculate pitch and roll from accelerometer data
            Quaternion accelerometerOrientation = GetOrientationFromAccelerometer(accelerometerData);

            // Step 2: Calculate delta rotation from gyroscope data
            Quaternion gyroDeltaRotation = GetDeltaRotationFromGyroscope(gyroData, _deltaTime);

            // Step 3: Fuse accelerometer and gyroscope data using complementary filter
            currentOrientation = Quaternion.Slerp(accelerometerOrientation, currentOrientation * gyroDeltaRotation, alpha);

            // Normalize the quaternion to prevent drift
            currentOrientation = Quaternion.Normalize(currentOrientation);

            if (!IsValid(currentOrientation)) {
                currentOrientation = Quaternion.Identity;
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

                return new Quaternion(axis.X * sinHalfAngle, axis.Y * sinHalfAngle, axis.Z * sinHalfAngle, cosHalfAngle);
            } else {
                return Quaternion.Identity;
            }
        }
    }
}
