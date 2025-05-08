using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Everything_To_IMU_SlimeVR.SlimeVR.FirmwareConstants;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Everything_To_IMU_SlimeVR.Utility.BigEndianExtensions;
using Everything_To_IMU_SlimeVR.Utility;
using Microsoft.VisualBasic.Logging;
namespace Everything_To_IMU_SlimeVR.SlimeVR {
    public class PacketBuilder {
        private string _identifierString = "Dualsense-IMU-Tracker";
        private int _protocolVersion = 19;
        private long _packetId = 0;

        private byte[] _heartBeat = new byte[0];
        private MemoryStream heartbeatStream;
        private MemoryStream handshakeStream;
        private MemoryStream sensorInfoStream;
        private MemoryStream rotationPacketStream;
        private MemoryStream accellerationPacketStream;
        private MemoryStream gyroPacketStream;
        private MemoryStream flexdataPacketStream;
        private MemoryStream buttonPushPacketStream;
        private MemoryStream batteryLevelPacketStream;
        private BigEndianBinaryWriter _handshakeWriter;
        private BigEndianBinaryWriter _sensorInfoWriter;
        private BigEndianBinaryWriter _sensorRotationPacketWriter;
        private BigEndianBinaryWriter _rotationPacketWriter;
        private BigEndianBinaryWriter _accelerationPacketWriter;
        private BigEndianBinaryWriter _gyroPacketWriter;
        private BigEndianBinaryWriter _flexDataPacketWriter;
        private BigEndianBinaryWriter _buttonPushPacketWriter;
        private BigEndianBinaryWriter _batteryLevelPacketWriter;

        public byte[] HeartBeat { get => _heartBeat; set => _heartBeat = value; }

        public PacketBuilder(string fwString) {
            _identifierString = fwString;
            heartbeatStream = new MemoryStream(new byte[28]);
            handshakeStream = new MemoryStream(new byte[128]);
            sensorInfoStream = new MemoryStream(new byte[128]);
            rotationPacketStream = new MemoryStream(new byte[128]);
            accellerationPacketStream = new MemoryStream(new byte[128]);
            gyroPacketStream = new MemoryStream(new byte[128]);
            flexdataPacketStream = new MemoryStream(new byte[128]);
            buttonPushPacketStream = new MemoryStream(new byte[128]);
            batteryLevelPacketStream = new MemoryStream(new byte[128]);


            _handshakeWriter = new BigEndianBinaryWriter(handshakeStream);
            _sensorInfoWriter = new BigEndianBinaryWriter(sensorInfoStream);
            _rotationPacketWriter = new BigEndianBinaryWriter(rotationPacketStream);
            _accelerationPacketWriter = new BigEndianBinaryWriter(accellerationPacketStream);
            _gyroPacketWriter = new BigEndianBinaryWriter(gyroPacketStream);
            _flexDataPacketWriter = new BigEndianBinaryWriter(flexdataPacketStream);
            _buttonPushPacketWriter = new BigEndianBinaryWriter(buttonPushPacketStream);
            _batteryLevelPacketWriter = new BigEndianBinaryWriter(batteryLevelPacketStream);

            _heartBeat = CreateHeartBeat();
        }

        private byte[] CreateHeartBeat() {
            BigEndianBinaryWriter writer = new BigEndianBinaryWriter(heartbeatStream);
            heartbeatStream.Position = 0;
            writer.Write(UDPPackets.HEARTBEAT); // header
            writer.Write(_packetId++); // packet counter
            writer.Write((byte)0); // Tracker Id
            heartbeatStream.Position = 0;
            var data = heartbeatStream.ToArray();
            heartbeatStream?.Dispose();
            return data;
        }

        public byte[] BuildHandshakePacket(BoardType boardType, ImuType imuType, McuType mcuType, byte[] macAddress) {
            BigEndianBinaryWriter writer = _handshakeWriter;
            handshakeStream.Position = 0;
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
            handshakeStream.Position = 0;
            var data = handshakeStream.ToArray();
            return data;
        }


        public byte[] BuildSensorInfoPacket(ImuType imuType, TrackerPosition trackerPosition, TrackerDataType trackerDataType, byte trackerId) {
            BigEndianBinaryWriter writer = _sensorInfoWriter;
            sensorInfoStream.Position = 0;
            writer.Write((int)UDPPackets.SENSOR_INFO); // Packet header
            writer.Write((long)_packetId++); // Packet counter
            writer.Write((byte)trackerId); // Tracker Id
            writer.Write((byte)0); // Sensor status
            writer.Write((byte)imuType); // imu type
            writer.Write((short)0); // Magnetometer support
            writer.Write((byte)trackerPosition); // Tracker Position
            writer.Write((byte)trackerDataType); // Tracker Data Type
            sensorInfoStream.Position = 0;
            var data = sensorInfoStream.ToArray();
            return data;
        }

        public byte[] BuildRotationPacket(Quaternion rotation, byte trackerId) {
            BigEndianBinaryWriter writer = _rotationPacketWriter;
            rotationPacketStream.Position = 0;
            writer.Write(UDPPackets.ROTATION_DATA); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)trackerId); // Tracker id
            writer.Write((byte)1); // Data type
            writer.Write(rotation.X); // Quaternion X
            writer.Write(rotation.Y); // Quaternion Y
            writer.Write(rotation.Z); // Quaternion Z
            writer.Write(rotation.W); // Quaternion W
            writer.Write((byte)0); // Calibration Info
            rotationPacketStream.Position = 0;
            var data = rotationPacketStream.ToArray();
            return data;
        }
        public byte[] BuildAccelerationPacket(Vector3 acceleration, byte trackerId) {
            BigEndianBinaryWriter writer = _accelerationPacketWriter;
            accellerationPacketStream.Position = 0;
            writer.Write(UDPPackets.ACCELERATION); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)trackerId); // Tracker id
            writer.Write((byte)1); // Data type 
            writer.Write(acceleration.X); // Euler X
            writer.Write(acceleration.Y); // Euler Y
            writer.Write(acceleration.Z); // Euler Z
            writer.Write((byte)0); // Calibration Info
            accellerationPacketStream.Position = 0;
            var data = accellerationPacketStream.ToArray();
            return data;
        }
        public byte[] BuildGyroPacket(Vector3 gyro, byte trackerId) {
            BigEndianBinaryWriter writer = _gyroPacketWriter;
            gyroPacketStream.Position = 0;
            writer.Write(UDPPackets.GYRO); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)trackerId); // Tracker id
            writer.Write((byte)1); // Data type 
            writer.Write(gyro.X); // Euler X
            writer.Write(gyro.Y); // Euler Y
            writer.Write(gyro.Z); // Euler Z
            writer.Write((byte)0); // Calibration Info
            gyroPacketStream.Position = 0;
            var data = gyroPacketStream.ToArray();
            return data;
        }
        public byte[] BuildFlexDataPacket(float flexData, byte trackerId) {
            BigEndianBinaryWriter writer = _flexDataPacketWriter;
            flexdataPacketStream.Position = 0;
            writer.Write(UDPPackets.FLEX_DATA_PACKET); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)trackerId); // Tracker id
            writer.Write(flexData); // Flex data
            flexdataPacketStream.Position = 0;
            var data = flexdataPacketStream.ToArray();
            return data;
        }
        public byte[] BuildButtonPushedPacket() {
            BigEndianBinaryWriter writer = _buttonPushPacketWriter;
            buttonPushPacketStream.Position = 0;
            writer.Write(UDPPackets.BUTTON_PUSHED); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write((byte)0); // Tracker id
            buttonPushPacketStream.Position = 0;
            var data = buttonPushPacketStream.ToArray();
            return data;
        }
        public byte[] BuildBatteryLevelPacket(float battery) {
            BigEndianBinaryWriter writer = _batteryLevelPacketWriter;
            batteryLevelPacketStream.Position = 0;
            writer.Write(UDPPackets.BATTERY_LEVEL); // Header
            writer.Write(_packetId++); // Packet counter
            writer.Write(battery); // Battery data
            writer.Write(battery); // Battery data
            batteryLevelPacketStream.Position = 0;
            var data = batteryLevelPacketStream.ToArray();
            return data;
        }
    }
}
