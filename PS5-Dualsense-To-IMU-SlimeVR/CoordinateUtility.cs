using System;
using System.Collections.Generic;
using System.Numerics;

namespace AQuestReborn {
    public static class CoordinateUtility {
        public static float ConvertRadiansToDegrees(this float radians) {
            double degrees = (180 / Math.PI) * radians;
            return (float)degrees;
        }
        public static float ConvertDegreesToRadians(this float degrees) {
            double radians = degrees * (Math.PI / 180);
            return (float)radians;
        }
        public static double ConvertRadiansToDegrees(this double radians) {
            double degrees = (180 / Math.PI) * radians;
            return degrees;
        }
        public static double ConvertDegreesToRadians(this double degrees) {
            double radians = degrees * (Math.PI / 180);
            return radians;
        }

        public static Quaternion ToQuaternion(this Vector3 vector3) // roll (x), pitch (y), yaw (z), angles are in radians
        {
            return ToQuaternion(vector3.X, vector3.Y, vector3.Z);
        }
        public static Vector3 VectorDirection(this Quaternion rotation, Vector3 point) {
            float num1 = rotation.X * 2f;
            float num2 = rotation.Y * 2f;
            float num3 = rotation.Z * 2f;
            float num4 = rotation.X * num1;
            float num5 = rotation.Y * num2;
            float num6 = rotation.Z * num3;
            float num7 = rotation.X * num2;
            float num8 = rotation.X * num3;
            float num9 = rotation.Y * num3;
            float num10 = rotation.W * num1;
            float num11 = rotation.W * num2;
            float num12 = rotation.W * num3;
            Vector3 vector3;
            vector3.X = (float)((1.0 - ((double)num5 + (double)num6)) * (double)point.X + ((double)num7 - (double)num12) * (double)point.Y + ((double)num8 + (double)num11) * (double)point.Z);
            vector3.Y = (float)(((double)num7 + (double)num12) * (double)point.X + (1.0 - ((double)num4 + (double)num6)) * (double)point.Y + ((double)num9 - (double)num10) * (double)point.Z);
            vector3.Z = (float)(((double)num8 - (double)num11) * (double)point.X + ((double)num9 + (double)num10) * (double)point.Y + (1.0 - ((double)num4 + (double)num5)) * (double)point.Z);
            return vector3;
        }

        public static Quaternion CalculateRotationQuaternion(Vector3 from, Vector3 to) {
            Vector3 currentNormalized = Vector3.Normalize(from);
            Vector3 targetNormalized = Vector3.Normalize(to);

            // Calculate the rotation axis
            Vector3 rotationAxis = Vector3.Cross(currentNormalized, targetNormalized);

            // Calculate the angle between the vectors
            float angle = (float)Math.Acos(Vector3.Dot(currentNormalized, targetNormalized));

            // Create the quaternion from the axis and angle
            Quaternion rotationQuaternion = Quaternion.CreateFromAxisAngle(rotationAxis, angle);

            return rotationQuaternion;
        }

        public static Quaternion LookAt(Vector3 position, Vector3 target) {
            var value = Vector3.Normalize(new Vector3(target.X, 0, target.Z) - new Vector3(position.X, 0, position.Z));
            Matrix4x4 viewMatrix = Matrix4x4.CreateLookTo(position, new Vector3(value.X, value.Y, value.Z * -1), Vector3.UnitY);
            return Quaternion.CreateFromRotationMatrix(viewMatrix);
        }
        public static Vector3 QuaternionToEuler(this Quaternion q) {
            Vector3 angles = new Vector3();

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1) {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            } else {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            var degrees = new Vector3() {
                X = (float)(180 / Math.PI) * angles.X,
                Y = (float)(180 / Math.PI) * angles.Y,
                Z = (float)(180 / Math.PI) * angles.Z
            };

            return degrees;
        }
        public static Vector3 Transform(ref System.Numerics.Vector3 v, ref System.Numerics.Quaternion rotation) {
            // This operation is an optimized-down version of v' = q * v * q^-1.
            // The expanded form would be to treat v as an 'axis only' quaternion
            // and perform standard quaternion multiplication. Assuming q is normalized,
            // q^-1 can be replaced by a conjugation.
            float x2 = rotation.X + rotation.X;
            float y2 = rotation.Y + rotation.Y;
            float z2 = rotation.Z + rotation.Z;
            float xx2 = rotation.X * x2;
            float xy2 = rotation.X * y2;
            float xz2 = rotation.X * z2;
            float yy2 = rotation.Y * y2;
            float yz2 = rotation.Y * z2;
            float zz2 = rotation.Z * z2;
            float wx2 = rotation.W * x2;
            float wy2 = rotation.W * y2;
            float wz2 = rotation.W * z2;
            // Defer the component setting since they're used in computation.
            float transformedX = v.X * (1f - yy2 - zz2) + v.Y * (xy2 - wz2) + v.Z * (xz2 + wy2);
            float transformedY = v.X * (xy2 + wz2) + v.Y * (1f - xx2 - zz2) + v.Z * (yz2 - wx2);
            float transformedZ = v.X * (xz2 - wy2) + v.Y * (yz2 + wx2) + v.Z * (1f - xx2 - yy2);

            return new Vector3(transformedX, transformedY, transformedZ);
        }
        public static Quaternion GyroAttitude(this Vector3 gyroData) {

            var orientation = Quaternion.Identity;
            // Convert gyro data from the gyroscope's frame of reference to the world frame of reference.

            // Rotate gyroData by orientation quaternion.
            Vector3 gyroWorldData = Transform(ref gyroData, ref orientation);

            // Check which direction the armband is facing.
            Vector3 forwardSource = new Vector3(1, 0, 0);

            Vector3 right = Vector3.Cross(forwardSource, new Vector3(0, 0, -1));
            Vector3 up = new Vector3(0, 1, 0);

            Quaternion yQuat = CreateRotationFromTo(right, up);

            float m = (float)Math.Sqrt(yQuat.W * yQuat.W +
                yQuat.X * yQuat.X +
                yQuat.Y * yQuat.Y +
                yQuat.Z * yQuat.Z);

            var yCompNorm = new Quaternion(yQuat.W / m, yQuat.X / m, yQuat.Y / m, yQuat.Z / m);
            return yCompNorm;
        }

        public static Quaternion CreateRotationFromTo(Vector3 fromDirection, Vector3 toDirection) {
            fromDirection = Vector3.Normalize(fromDirection);
            toDirection = Vector3.Normalize(toDirection);

            float dot = Vector3.Dot(fromDirection, toDirection);
            if (dot < -1 + float.Epsilon) {
                // Vectors are opposite, cannot rotate smoothly.
                return Quaternion.Identity;
            } else if (dot > 1 - float.Epsilon) {
                // Vectors are identical, no rotation needed.
                return Quaternion.Identity;
            }

            Vector3 axis = Vector3.Cross(fromDirection, toDirection);
            float angle = (float)Math.Acos(dot);
            return Quaternion.CreateFromAxisAngle(axis, angle);
        }

        public static Quaternion ToQuaternion(double x, double y, double z) // roll (x), pitch (y), yaw (z), angles are in radians
    {
            // Abbreviations for the various angular functions
            double roll, pitch, yaw;

            roll = ConvertDegreesToRadians(x);
            pitch = ConvertDegreesToRadians(y);
            yaw = ConvertDegreesToRadians(z);

            double cr = Math.Cos(roll * 0.5);
            double sr = Math.Sin(roll * 0.5);
            double cp = Math.Cos(pitch * 0.5);
            double sp = Math.Sin(pitch * 0.5);
            double cy = Math.Cos(yaw * 0.5);
            double sy = Math.Sin(yaw * 0.5);

            Quaternion q = new Quaternion();
            q.W = (float)(cr * cp * cy + sr * sp * sy);
            q.X = (float)(sr * cp * cy - cr * sp * sy);
            q.Y = (float)(cr * sp * cy + sr * cp * sy);
            q.Z = (float)(cr * cp * sy - sr * sp * cy);

            return q;
        }
    }
}
