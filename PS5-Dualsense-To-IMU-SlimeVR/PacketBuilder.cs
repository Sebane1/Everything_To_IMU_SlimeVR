using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static PS5_Dualsense_To_IMU_SlimeVR.FirmwareConstants;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static PS5_Dualsense_To_IMU_SlimeVR.EndianExtensions;
namespace PS5_Dualsense_To_IMU_SlimeVR {
    public class PacketBuilder {
        private string _identifierString = "Dualsense-IMU-Tracker";
        private int _protocolVersion = 19;
        private long _packetId = 0;
        private int _trackerId = 0;

        private byte[] _heartBeat = new byte[0];

        public byte[] HeartBeat { get => _heartBeat; set => _heartBeat = value; }

        public PacketBuilder(string fwString, int trackerId) {
            this._identifierString = fwString;
            _trackerId = trackerId;
            _heartBeat = CreateHeartBeat();
        }

        private byte[] CreateHeartBeat() {
            MemoryStream memoryStream = new MemoryStream(new byte[28]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.HEARTBEAT);
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }

        public byte[] BuildHandshakePacket(BoardType boardType, McuType mcuType, byte[] macAddress) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.HANDSHAKE);
            writer.Write((long)0);
            writer.Write((int)boardType);
            writer.Write((long)0);
            writer.Write((int)mcuType);
            for (int i = 0; i < 3; i++) {
                writer.Write(0);
            }
            writer.Write((int)_protocolVersion);
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(_identifierString);
            writer.Write((byte)utf8Bytes.Length);
            writer.Write(utf8Bytes);
            writer.Write(macAddress);
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }


        public byte[] BuildSensorInfoPacket(ImuType imuType, TrackerPosition trackerPosition, TrackerDataType trackerDataType) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.SENSOR_INFO);
            writer.Write((long)_packetId++);
            writer.Write((byte)_trackerId);
            writer.Write((byte)0);
            writer.Write((byte)imuType);
            writer.Write((short)0);
            writer.Write((byte)trackerPosition);
            writer.Write((byte)trackerDataType);
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }

        public byte[] BuildRotationPacket(Quaternion rotation) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.ROTATION_DATA);
            writer.Write((long)_packetId++);
            writer.Write((byte)_trackerId);
            writer.Write((byte)1);
            writer.Write((float)rotation.X);
            writer.Write((float)rotation.Y);
            writer.Write((float)rotation.Z);
            writer.Write((float)rotation.W);
            writer.Write((byte)0);
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildAccellerationPacket(Vector3 acceleration) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.ACCELERATION);
            writer.Write((long)_packetId++);
            writer.Write((byte)_trackerId);
            writer.Write((byte)1);
            writer.Write((float)acceleration.X);
            writer.Write((float)acceleration.Y);
            writer.Write((float)acceleration.Z);
            writer.Write((byte)0);
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildGyroPacket(Vector3 gyro) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.GYRO);
            writer.Write((long)_packetId++);
            writer.Write((byte)_trackerId);
            writer.Write((byte)1);
            writer.Write((float)gyro.X);
            writer.Write((float)gyro.Y);
            writer.Write((float)gyro.Z);
            writer.Write((byte)0);
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildFlexDataPacket(float flexData) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.FLEX_DATA_PACKET);
            writer.Write((long)_packetId++);
            writer.Write((byte)_trackerId);
            writer.Write((byte)0);
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildBatteryLevelPacket(float battery) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)FirmwareConstants.UDPPackets.BATTERY_LEVEL);
            writer.Write((long)_packetId++);
            writer.Write((byte)_trackerId);
            writer.Write((float)battery); 
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
    }
}
