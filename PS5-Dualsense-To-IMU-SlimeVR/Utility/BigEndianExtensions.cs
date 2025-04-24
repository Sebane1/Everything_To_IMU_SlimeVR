namespace Everything_To_IMU_SlimeVR.Utility {
    public static class BigEndianExtensions {
        public static int CorrectEndian(this int val) {
            if (BitConverter.IsLittleEndian) {
                byte[] intAsBytes = BitConverter.GetBytes(val);
                Array.Reverse(intAsBytes);
                return BitConverter.ToInt32(intAsBytes, 0);
            }
            return val;
        }
        public static long CorrectEndian(this long val) {
            if (BitConverter.IsLittleEndian) {
                byte[] longAsBytes = BitConverter.GetBytes(val);
                Array.Reverse(longAsBytes);
                return BitConverter.ToInt64(longAsBytes, 0);
            }
            return val;
        }
        public static float CorrectEndian(this float val) {
            if (BitConverter.IsLittleEndian) {
                byte[] floatAsBytes = BitConverter.GetBytes(val);
                Array.Reverse(floatAsBytes);
                return BitConverter.ToSingle(floatAsBytes, 0);
            }
            return val;
        }
        public static short CorrectEndian(this short val) {
            if (BitConverter.IsLittleEndian) {
                byte[] shortAsBytes = BitConverter.GetBytes(val);
                Array.Reverse(shortAsBytes);
                return BitConverter.ToInt16(shortAsBytes, 0);
            }
            return val;
        }
        public static double CorrectEndian(this double val) {
            if (BitConverter.IsLittleEndian) {
                byte[] doubleAsBytes = BitConverter.GetBytes(val);
                Array.Reverse(doubleAsBytes);
                return BitConverter.ToDouble(doubleAsBytes, 0);
            }
            return val;
        }
        public static byte[] CorrectEndian(this byte[] val) {
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(val);
            }
            return val;
        }
    }
}
