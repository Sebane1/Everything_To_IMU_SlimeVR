using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static PS5_Dualsense_To_IMU_SlimeVR.SlimeVR.FirmwareConstants;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static PS5_Dualsense_To_IMU_SlimeVR.Utility.BigEndianExtensions;
using PS5_Dualsense_To_IMU_SlimeVR.Utility;
using Microsoft.VisualBasic.Logging;
namespace PS5_Dualsense_To_IMU_SlimeVR.SlimeVR {
    public class PacketBuilder {
        private string _identifierString = "Dualsense-IMU-Tracker";
        private int _protocolVersion = 19;
        private long _packetId = 0;
        private int _trackerId = 0;

        private byte[] _heartBeat = new byte[0];

        public byte[] HeartBeat { get => _heartBeat; set => _heartBeat = value; }

        public PacketBuilder(string fwString, int trackerId) {
            _identifierString = fwString;
            _trackerId = trackerId;
            _heartBeat = CreateHeartBeat();
        }

        private byte[] CreateHeartBeat() {
            MemoryStream memoryStream = new MemoryStream(new byte[28]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.HEARTBEAT); // header
            writer.Write(_packetId++); // packet counter
            writer.Write((byte)_trackerId); // Tracker Id
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }

        public byte[] BuildHandshakePacket(BoardType boardType, ImuType imuType, McuType mcuType, byte[] macAddress) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.HANDSHAKE); // header
            writer.Write((long)_packetId++); // packet counter
            writer.Write((int)boardType); // Board type
            writer.Write((int)imuType); //IMU type
            writer.Write((int)mcuType); // MCU Type

            writer.Write((int)0); // IMU Info
            writer.Write((int)0); // IMU Info
            writer.Write((int)0); // IMU Info

            writer.Write(_protocolVersion); // Protocol Version
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(_identifierString);
            writer.Write((byte)utf8Bytes.Length); // identifier string
            writer.Write(utf8Bytes); // identifier string
            writer.Write(macAddress); // MAC Address
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }


        public byte[] BuildSensorInfoPacket(ImuType imuType, TrackerPosition trackerPosition, TrackerDataType trackerDataType) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write((int)UDPPackets.SENSOR_INFO); // Packet header
            writer.Write((long)_packetId++); // Packet counter
            writer.Write((byte)_trackerId); // Tracker Id
            writer.Write((byte)0); // Sensor status
            writer.Write((byte)imuType); // imu type
            writer.Write((short)0); // Magnometer support
            writer.Write((byte)trackerPosition); // Tracker Position
            writer.Write((byte)trackerDataType); // Tracker Data Type
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }

        public byte[] BuildRotationPacket(Quaternion rotation) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.ROTATION_DATA); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)_trackerId); // Tracker id
            writer.Write((byte)1); // Data type
            writer.Write(rotation.X); // Quaternion X
            writer.Write(rotation.Y); // Quaternion Y
            writer.Write(rotation.Z); // Quaternion Z
            writer.Write(rotation.W); // Quaternion W
            writer.Write((byte)0); // Calibration Info
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildAccelerationPacket(Vector3 acceleration) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.ACCELERATION); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)_trackerId); // Tracker id
            writer.Write((byte)1); // Data type 
            writer.Write(acceleration.X); // Euler X
            writer.Write(acceleration.Y); // Euler Y
            writer.Write(acceleration.Z); // Euler Z
            writer.Write((byte)0); // Calibration Info
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildGyroPacket(Vector3 gyro) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.GYRO); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)_trackerId); // Tracker id
            writer.Write((byte)1); // Data type 
            writer.Write(gyro.X); // Euler X
            writer.Write(gyro.Y); // Euler Y
            writer.Write(gyro.Z); // Euler Z
            writer.Write((byte)0); // Calibration Info
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildFlexDataPacket(float flexData) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.FLEX_DATA_PACKET); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)_trackerId); // Tracker id
            writer.Write(flexData); // Flex data
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildButtonPushedPacket() {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.BUTTON_PUSHED); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)_trackerId); // Tracker id
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
        public byte[] BuildBatteryLevelPacket(byte battery) {
            MemoryStream memoryStream = new MemoryStream(new byte[128]);
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(memoryStream);
            memoryStream.Position = 0;
            writer.Write(UDPPackets.BATTERY_LEVEL); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)_trackerId); // Tracker id
            writer.Write((byte)battery); // Battery data
            memoryStream.Position = 0;
            var data = memoryStream.ToArray();
            return data;
        }
    }
}
