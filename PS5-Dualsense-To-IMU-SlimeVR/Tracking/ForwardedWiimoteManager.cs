using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Channels;
using System.Diagnostics;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class ForwardedWiimoteManager {
        private static ConcurrentDictionary<string, WiimoteInfo> _wiimotes = new();
        private static ConcurrentDictionary<string, byte[]> _rumbleState = new ConcurrentDictionary<string, byte[]>();
        private static ConcurrentDictionary<string, WiimoteStateTracker> _wiimoteTrackers = new();


        private static List<string> _wiimoteIds = new();
        public static EventHandler<string> NewPacketReceived;
        public static EventHandler LegacyClientDetected;
        Stopwatch _timeBetweenRequests = new Stopwatch();
        Stopwatch _memoryWipeTimer = new Stopwatch();

        private const int CalibrationSamples = 100;
        private ConcurrentDictionary<string, List<Vector3>> _calibrationSamples = new();
        private ConcurrentDictionary<string, (Vector3 center, float scale)> _calibrationData = new();
        private long _wiiRequestGap;

        public ForwardedWiimoteManager() {
            Task.Run(() => StartListener());
            _timeBetweenRequests.Restart();
        }
        public static ConcurrentDictionary<string, WiimoteInfo> Wiimotes => _wiimotes;
        public static ConcurrentDictionary<string, byte[]> RumbleState { get => _rumbleState; set => _rumbleState = value; }
        public static ConcurrentDictionary<string, WiimoteStateTracker> WiimoteTrackers { get => _wiimoteTrackers; set => _wiimoteTrackers = value; }

        async Task StartListener() {
            try {
                _memoryWipeTimer.Start();
                while (true) {
                    TcpListener listener = new TcpListener(IPAddress.Any, 9909);
                    listener.Start();
                    Console.WriteLine("TCP Listener started on port 9909...");

                    while (_memoryWipeTimer.ElapsedMilliseconds < 1200000) {
                        try {
                            var client = await listener.AcceptTcpClientAsync();
                            _ = Task.Run(() => HandleClient(client));
                        } catch (Exception ex) {
                            Console.WriteLine($"Listener error: {ex.Message}");
                        }
                    }
                    listener?.Stop();
                }
            } catch {

            }
        }

        async Task HandleClient(TcpClient client) {
            try {
                var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[4096];
                int maxWiimotePacket = 17;
                string baseIp = endpoint.Split(":")[0];
                if (!_rumbleState.ContainsKey(baseIp)) {
                    _rumbleState[baseIp] = new byte[4] { 0, 0, 0, 0 };
                }
                while (client.Connected) {
                    try {
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead <= 0) break;

                        _wiiRequestGap = _timeBetweenRequests.ElapsedMilliseconds;
                        _timeBetweenRequests.Restart();

                        byte[] data = new byte[bytesRead];
                        Array.Copy(buffer, data, bytesRead);

                        int packetLength = 0;
                        int numPackets = 0;

                        if (data.Length % maxWiimotePacket == 0) {
                            numPackets = data.Length / maxWiimotePacket;
                            packetLength = maxWiimotePacket;
                        } else {
                            Console.WriteLine($"❌ Malformed packet from {endpoint} (size={data.Length})");
                            await stream.WriteAsync(_rumbleState[baseIp], 0, _rumbleState[baseIp].Length);
                            await stream.WriteAsync(new byte[1] { Configuration.Instance.WiiPollingRate }, 0, 1);
                            LegacyClientDetected?.Invoke(this, EventArgs.Empty);
                            continue;
                        }

                        for (int i = 0; i < numPackets; i++) {
                            byte[] packetBytes = new byte[packetLength];
                            Buffer.BlockCopy(data, i * packetLength, packetBytes, 0, packetLength);
                            WiimotePacket packet = ParsePacket(packetBytes);
                            if (packet.Id != byte.MaxValue) {
                                string key = $"{baseIp}:{packet.Id}";
                                ProcessIncomingPacket(key, packet);
                            }
                        }

                        await stream.WriteAsync(_rumbleState[baseIp], 0, _rumbleState[baseIp].Length);
                        await stream.WriteAsync(new byte[1] { Configuration.Instance.WiiPollingRate }, 0, 1);
                        NewPacketReceived?.Invoke(this, baseIp);
                    } catch (Exception ex) {
                        Console.WriteLine($"❌ Handler error from {endpoint}: {ex.Message}");
                        await stream.WriteAsync(_rumbleState[baseIp], 0, _rumbleState[baseIp].Length);
                        await stream.WriteAsync(new byte[1] { Configuration.Instance.WiiPollingRate }, 0, 1);
                        LegacyClientDetected?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                }

                client.Close();
            } catch {

            }
        }
        private void ProcessIncomingPacket(string key, WiimotePacket packet) {
            // Get or create tracker for this wiimote
            if (!_wiimoteTrackers.TryGetValue(key, out var tracker)) {
                tracker = new WiimoteStateTracker();
                _wiimoteTrackers[key] = tracker;
            }

            // Process packet and update wiimote info
            _wiimotes[key] = tracker.ProcessPacket(packet);
        }
        WiimotePacket ParsePacket(byte[] data) {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try {
                return Marshal.PtrToStructure<WiimotePacket>(handle.AddrOfPinnedObject());
            } finally {
                handle.Free();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WiimotePacket {
            public byte Id;
            public short WiimoteAccelX;
            public short WiimoteAccelY;
            public short WiimoteAccelZ;
            public short WiimoteDataX;
            public short WiimoteDataY;
            public short WiimoteDataZ;
            public byte NunchukConnected;
            public byte MotionPlusSupport;
            public byte BatteryLevel;
            public bool ButtonUp;
        }
        public struct WiimoteInfo {
            public byte Id;
            public Quaternion WiimoteGravityOrientation;
            public Quaternion NunchuckOrientation;
            public byte NunchukConnected;
            public byte MotionPlusSupport;
            public byte BatteryLevel;
            public bool ButtonUp;

            public short WiimoteAccelX;
            public short WiimoteAccelY;
            public short WiimoteAccelZ;
            public short NunchukAccelX;
            public short NunchukAccelY;
            public short NunchukAccelZ;
            public short WiimoteGyroX;
            public short WiimoteGyroY;
            public short WiimoteGyroZ;
            public short GyroDeadzone = 100;
            public float GyroFilterFactor = 0.3f;
            public float GyroRateX { get; private set; }
            public float GyroRateY { get; private set; }
            public float GyroRateZ { get; private set; }

            public WiimoteInfo(WiimotePacket wiimotePacket) {
                short wiimoteOffset = 512;
                short nunchuckOffset = 512;
                Id = wiimotePacket.Id;

                WiimoteAccelX = wiimotePacket.WiimoteAccelX;
                WiimoteAccelY = wiimotePacket.WiimoteAccelY;
                WiimoteAccelZ = wiimotePacket.WiimoteAccelZ;

                WiimoteGravityOrientation = QuaternionUtils.QuatFromGravity(
                    wiimotePacket.WiimoteAccelX, wiimotePacket.WiimoteAccelY, wiimotePacket.WiimoteAccelZ,
                    wiimoteOffset, wiimoteOffset, wiimoteOffset, 200f);

                if (wiimotePacket.NunchukConnected != 0) {
                    NunchukAccelX = wiimotePacket.WiimoteDataX;
                    NunchukAccelY = wiimotePacket.WiimoteDataY;
                    NunchukAccelZ = wiimotePacket.WiimoteDataZ;

                    NunchuckOrientation = QuaternionUtils.QuatFromGravity(
                        wiimotePacket.WiimoteDataX, wiimotePacket.WiimoteDataY, wiimotePacket.WiimoteDataZ,
                        nunchuckOffset, nunchuckOffset, nunchuckOffset, 200f);
                } else {
                    WiimoteGyroX = ((short)(wiimotePacket.WiimoteDataX - 8192));
                    WiimoteGyroY = ((short)(wiimotePacket.WiimoteDataY - 8192));
                    WiimoteGyroZ = ((short)(wiimotePacket.WiimoteDataZ - 8192));

                    GyroRateX = (wiimotePacket.WiimoteDataX - 8192) * 0.07f;
                    GyroRateY = (wiimotePacket.WiimoteDataY - 8192) * 0.07f;
                    GyroRateZ = (wiimotePacket.WiimoteDataZ - 8192) * 0.07f;
                }

                NunchukConnected = wiimotePacket.NunchukConnected;
                MotionPlusSupport = wiimotePacket.MotionPlusSupport;

                BatteryLevel = wiimotePacket.BatteryLevel;
                ButtonUp = wiimotePacket.ButtonUp;
            }

            public Quaternion WiimoteFusedOrientation { get; internal set; }
        }
    }
}