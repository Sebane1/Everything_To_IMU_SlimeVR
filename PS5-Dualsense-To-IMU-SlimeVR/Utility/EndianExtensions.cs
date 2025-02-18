using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS5_Dualsense_To_IMU_SlimeVR.Utility {
    public static class EndianExtensions {
        public static int ReverseEndian(this int val) {
            byte[] intAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(intAsBytes);
            return BitConverter.ToInt32(intAsBytes, 0);
        }
        public static long ReverseEndian(this long val) {
            byte[] longAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(longAsBytes);
            return BitConverter.ToInt64(longAsBytes, 0);
        }
        public static float ReverseEndian(this float val) {
            byte[] floatAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(floatAsBytes);
            return BitConverter.ToSingle(floatAsBytes, 0);
        }
        public static short ReverseEndian(this short val) {
            byte[] shortAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(shortAsBytes);
            return BitConverter.ToInt16(shortAsBytes, 0);
        }
        public static double ReverseEndian(this double val) {
            byte[] doubleAsBytes = BitConverter.GetBytes(val);
            Array.Reverse(doubleAsBytes);
            return BitConverter.ToDouble(doubleAsBytes, 0);

        }
        public static byte[] ReverseEndian(this byte[] val) {
            Array.Reverse(val);
            return val;
        }
    }
}
