using System.Net.Sockets;
using System.Numerics;
using System.Text;
using static Everything_To_IMU_SlimeVR.SlimeVR.FirmwareConstants;

namespace Everything_To_IMU_SlimeVR.SlimeVR {
    public class UDPHandler {
        private byte[] _macAddress;
        private PacketBuilder packetBuilder;
        private int slimevrPort = 6969;
        UdpClient udpClient;
        int handshakeCount = 1000;
        bool _active = false;
        private bool _shouldSendHeartbeat = true;

        public bool Active { get => _active; set => _active = value; }
        public bool ShouldSendHeartbeat { get => _shouldSendHeartbeat; set => _shouldSendHeartbeat = value; }

        public UDPHandler(string trackerLabel, int trackerId, byte[] macAddress) {
            _macAddress = macAddress;
            packetBuilder = new PacketBuilder(trackerLabel, trackerId);
            udpClient = new UdpClient();
            udpClient.Connect("localhost", 6969);
            Task.Run(() => {
                while (true) {
                    if (_active) {
                        Initialize();
                        Thread.Sleep(1000);
                        handshakeCount += 1;
                        if (handshakeCount > 10) {
                            break;
                        }
                    } else {
                        Thread.Sleep(5000);
                    }
                }
            });
        }

        public void Initialize() {
            Handshake(BoardType.UNKNOWN, ImuType.UNKNOWN, McuType.UNKNOWN, _macAddress);
            AddImu(ImuType.UNKNOWN, TrackerPosition.NONE, TrackerDataType.ROTATION);
        }

        public void Heartbeat() {
            Task.Run(async () => {
                while (_shouldSendHeartbeat) {
                    if (_active) {
                        await udpClient.SendAsync(packetBuilder.HeartBeat);
                    }
                    await Task.Delay(900); // At least 1 time per second (<1000ms)
                }
            });
        }

        public async void AddImu(ImuType imuType, TrackerPosition trackerPosition, TrackerDataType trackerDataType) {
            await udpClient.SendAsync(packetBuilder.BuildSensorInfoPacket(imuType, trackerPosition, trackerDataType));
        }

        public async void Handshake(BoardType boardType, ImuType imuType, McuType mcuType, byte[] macAddress) {
            await udpClient.SendAsync(packetBuilder.BuildHandshakePacket(boardType, imuType, mcuType, macAddress));
            await Task.Delay(500);
            Heartbeat();
        }

        public async Task<bool> SetSensorRotation(Quaternion rotation) {
            await udpClient.SendAsync(packetBuilder.BuildRotationPacket(rotation));
            _shouldSendHeartbeat = false;
            return true;
        }
        public async Task<bool> SetSensorAcceleration(Vector3 acceleration) {
            await udpClient.SendAsync(packetBuilder.BuildAccelerationPacket(acceleration));
            _shouldSendHeartbeat = false;
            return true;
        }
        public async Task<bool> SetSensorGyro(Vector3 gyro) {
            await udpClient.SendAsync(packetBuilder.BuildGyroPacket(gyro));
            _shouldSendHeartbeat = false;
            return true;
        }
        public async Task<bool> SetSensorFlexData(float flexResistance) {
            await udpClient.SendAsync(packetBuilder.BuildFlexDataPacket(flexResistance));
            _shouldSendHeartbeat = false;
            return true;
        }

        public async Task<bool> SendButton() {
            await udpClient.SendAsync(packetBuilder.BuildButtonPushedPacket());
            _shouldSendHeartbeat = false;
            return true;
        }

        public async Task<bool> SendPacket(byte[] packet) {
            await udpClient.SendAsync(packet);
            _shouldSendHeartbeat = false;
            return true;
        }

        public async void ListenForHandshake() {
            try {
                var data = await udpClient.ReceiveAsync();
                string value = Encoding.UTF8.GetString(data.Buffer);
                if (value.Contains("Hey OVR =D 5")) {
                    udpClient.Connect(data.RemoteEndPoint.Address.ToString(), 6969);
                }
            } catch {

            }
        }

        public async Task<bool> SetSensorBattery(float battery) {
            _shouldSendHeartbeat = false;
            await udpClient.SendAsync(packetBuilder.BuildBatteryLevelPacket(battery));
            return true;
        }
    }
}
