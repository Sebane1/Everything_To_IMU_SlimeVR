using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    using System;
    using System.Diagnostics;
    using System.Numerics;
    using Wujek_Dualsense_API;

    class GyroToQuaternion {
        private Quaternion _currentOrientation = Quaternion.Identity;
        private Vector3 _gyroVector = Vector3.Zero; // Example gyroscope input
        private float _deltaTime = 0.02f; // Example timestep, 50Hz
        private float _previousTime = 0;
        private Dualsense _dualSense;
        Stopwatch stopwatch = new Stopwatch();
        public GyroToQuaternion(Dualsense dualsense) {
            _dualSense = dualsense;
            Task.Run(async () => {
                while (true) {
                    Update();
                    await Task.Delay(16);
                }
            });
        }

        public Quaternion CurrentOrientation { get => _currentOrientation; set => _currentOrientation = value; }

        // Update method to simulate gyroscope data conversion to quaternion attitude
        public void Update() {
            float currentTime = (float)stopwatch.Elapsed.TotalSeconds;

            // Calculate deltaTime (the difference between current and previous time)
            _deltaTime = currentTime - _previousTime;

            // Update previousTime for the next frame
            _previousTime = currentTime;
            stopwatch.Reset();
            stopwatch.Start();
            // Replace with your actual gyroscope data input
            _gyroVector = new Vector3(_dualSense.ButtonState.gyro.X, _dualSense.ButtonState.gyro.Y, _dualSense.ButtonState.gyro.Z);

            // Convert angular velocity to quaternion change
            Quaternion deltaRotation = QuaternionFromAngularVelocity(_gyroVector, _deltaTime);

            // Apply the delta to the current orientation
            _currentOrientation = Quaternion.Multiply(_currentOrientation, deltaRotation);

            // Normalize the quaternion to prevent drift (optional but recommended)
            _currentOrientation = Quaternion.Normalize(_currentOrientation);
        }

        // Convert angular velocity vector to a quaternion
        private Quaternion QuaternionFromAngularVelocity(Vector3 angularVelocity, float deltaTime) {
            // Convert angular velocity to radians over the timestep
            float angle = angularVelocity.Length() * deltaTime;

            if (angle > 0) {
                // Axis of rotation is the normalized angular velocity vector
                Vector3 axis = Vector3.Normalize(angularVelocity);

                // Convert to quaternion (axis-angle representation)
                float halfAngle = angle / 2.0f;
                float sinHalfAngle = (float)Math.Sin(halfAngle);
                float cosHalfAngle = (float)Math.Cos(halfAngle);

                return new Quaternion(axis.X * sinHalfAngle, axis.Y * sinHalfAngle, axis.Z * sinHalfAngle, cosHalfAngle);
            } else {
                // No rotation, return identity quaternion
                return Quaternion.Identity;
            }
        }
    }
}
