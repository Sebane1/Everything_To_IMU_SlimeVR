using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everything_To_IMU_SlimeVR {
    public struct Vector3Short {
        short _x;
        short _y;
        short _z;

        public Vector3Short(short x, short y, short z) {
            _x = x;
            _y = y;
            _z = z;
        }

        public short X { get => _x; set => _x = value; }
        public short Y { get => _y; set => _y = value; }
        public short Z { get => _z; set => _z = value; }

        // Returns squared Euclidean distance between this and another Vector3Short
        public static float SquaredDistance(Vector3Short first, Vector3Short other) {
            float dx = first.X - other.X;
            float dy = first.Y - other.Y;
            float dz = first.Z - other.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        // Optional: Get float distance if needed (expensive)
        public static float Distance(Vector3Short first, Vector3Short other) {
            return MathF.Sqrt(SquaredDistance(first, other));
        }

        public override string ToString() => $"({X}, {Y}, {Z})";

    }
}
