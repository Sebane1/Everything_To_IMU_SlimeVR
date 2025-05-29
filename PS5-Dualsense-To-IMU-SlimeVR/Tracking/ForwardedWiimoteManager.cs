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
        private const float MinAccelDelta = 0.03f; // Tune this (e.g. ~3% of 1g)
        private ConcurrentDictionary<string, Vector3Short> _prevAccelVectors = new();
        private static ConcurrentDictionary<string, byte[]> _rumbleState = new ConcurrentDictionary<string, byte[]>();

        private static List<string> _wiimoteIds = new();
        public static EventHandler NewPacketReceived;
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

        async Task StartListener() {
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
        }

        async Task HandleClient(TcpClient client) {
            var endpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
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

                    if (data.Length % 19 == 0) {
                        numPackets = data.Length / 19;
                        packetLength = 19;
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
                        if (packet.Id != uint.MaxValue) {
                            string key = $"{baseIp}:{packet.Id}";
                            _wiimotes[key] = new WiimoteInfo(packet);
                        }
                    }

                    await stream.WriteAsync(_rumbleState[baseIp], 0, _rumbleState[baseIp].Length);
                    await stream.WriteAsync(new byte[1] { Configuration.Instance.WiiPollingRate }, 0, 1);
                    NewPacketReceived?.Invoke(this, EventArgs.Empty);

                } catch (Exception ex) {
                    Console.WriteLine($"❌ Handler error from {endpoint}: {ex.Message}");
                    await stream.WriteAsync(_rumbleState[baseIp], 0, _rumbleState[baseIp].Length);
                    await stream.WriteAsync(new byte[1] { Configuration.Instance.WiiPollingRate }, 0, 1);
                    LegacyClientDetected?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }

            client.Close();
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
            public uint Id;
            public short WiimoteAccelX;
            public short WiimoteAccelY;
            public short WiimoteAccelZ;
            public short NunchukAccelX;
            public short NunchukAccelY;
            public short NunchukAccelZ;
            public byte NunchukConnected;
            public byte BatteryLevel;
            public bool ButtonUp;
        }
        public struct WiimoteInfo {
            public uint Id;
            public Quaternion WiimoteOrientation;
            public Quaternion NunchuckOrientation;
            public byte NunchukConnected;
            public byte BatteryLevel;
            public bool ButtonUp;

            public short WiimoteAccelX;
            public short WiimoteAccelY;
            public short WiimoteAccelZ;
            public short NunchukAccelX;
            public short NunchukAccelY;
            public short NunchukAccelZ;
            public WiimoteInfo(WiimotePacket wiimotePacket) {
                short wiimoteOffset = 512;
                short nunchuckOffset = 512;
                Id = wiimotePacket.Id;

                WiimoteOrientation = QuaternionUtils.QuatFromGravity(
                    wiimotePacket.WiimoteAccelX, wiimotePacket.WiimoteAccelY, wiimotePacket.WiimoteAccelZ,
                    wiimoteOffset, wiimoteOffset, wiimoteOffset, 200f);

                NunchuckOrientation = QuaternionUtils.QuatFromGravity(
                    wiimotePacket.NunchukAccelX, wiimotePacket.NunchukAccelY, wiimotePacket.NunchukAccelZ,
                    nunchuckOffset, nunchuckOffset, nunchuckOffset, 200f);

                WiimoteAccelX = wiimotePacket.WiimoteAccelX;
                WiimoteAccelY = wiimotePacket.WiimoteAccelY;
                WiimoteAccelZ = wiimotePacket.WiimoteAccelZ;
                NunchukAccelX = wiimotePacket.NunchukAccelX;
                NunchukAccelY = wiimotePacket.NunchukAccelY;
                NunchukAccelZ = wiimotePacket.NunchukAccelZ;

                NunchukConnected = wiimotePacket.NunchukConnected;
                BatteryLevel = wiimotePacket.BatteryLevel;
                ButtonUp = wiimotePacket.ButtonUp;
            }
        }
    }
}