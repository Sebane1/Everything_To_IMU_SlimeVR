using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS5_Dualsense_To_IMU_SlimeVR.Utility {
    public class BigEndianBinaryWriter : BinaryWriter {
        public BigEndianBinaryWriter(Stream stream) : base(stream) { }
        public void Write(float value) {
            base.Write(value.CorrectEndian());
        }
        public void Write(int value) {
            base.Write(value.CorrectEndian());
        }
        public void Write(long value) {
            base.Write(value.CorrectEndian());
        }
        public void Write(short value) {
            base.Write(value.CorrectEndian());
        }
        public void Write(double value) {
            base.Write(value.CorrectEndian());
        }
        public void Write(byte[] value) {
            base.Write(value.CorrectEndian());
        }
    }
}
