using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Channels;
using System.Diagnostics;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class ForwardedWiimoteManager {
        private static ConcurrentDictionary<string, WiimotePacket> _wiimotes = new();
        private static Dictionary<string, byte[]> _rumbleState = new Dictionary<string, byte[]>();

        private static List<string> _wiimoteIds = new();
        public static EventHandler NewPacketReceived;
        public static EventHandler LegacyClientDetected;
        Stopwatch _timeBetweenRequests = new Stopwatch();

        private const int CalibrationSamples = 100;
        private ConcurrentDictionary<string, List<Vector3>> _calibrationSamples = new();
        private ConcurrentDictionary<string, (Vector3 center, float scale)> _calibrationData = new();

        public ForwardedWiimoteManager() {
            Task.Run(() => StartListener());
            _timeBetweenRequests.Restart();
        }
        public static ConcurrentDictionary<string, WiimotePacket> Wiimotes => _wiimotes;
        public static Dictionary<string, byte[]> RumbleState { get => _rumbleState; set => _rumbleState = value; }

        async Task StartListener() {
            TcpListener listener = new TcpListener(IPAddress.Any, 9909);
            listener.Start();
            Console.WriteLine("TCP Listener started on port 9909...");

            while (true) {
                try {
                    var client = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClient(client));
                } catch (Exception ex) {
                    Console.WriteLine($"Listener error: {ex.Message}");
                }
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

                    //Debug.WriteLine($"Wiimote request gap is {_timeBetweenRequests.ElapsedMilliseconds}ms");
                    _timeBetweenRequests.Restart();

                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);

                    int packetLength = 0;
                    int numPackets = 0;

                    if (data.Length % 38 == 0) {
                        numPackets = data.Length / 38;
                        packetLength = 38;
                    } else if (data.Length % 37 == 0) {
                        numPackets = data.Length / 37;
                        packetLength = 37;
                        Console.WriteLine("⚠️  Legacy client detected.");
                        LegacyClientDetected?.Invoke(this, EventArgs.Empty);
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
                        string key = $"{baseIp}:{packet.Id}";
                        _wiimotes[key] = packet;
                    }

                    await stream.WriteAsync(_rumbleState[baseIp], 0, _rumbleState[baseIp].Length);
                    await stream.WriteAsync(new byte[1] { Configuration.Instance.WiiPollingRate }, 0, 1);
                    NewPacketReceived?.Invoke(this, EventArgs.Empty);

                } catch (Exception ex) {
                    Console.WriteLine($"❌ Handler error from {endpoint}: {ex.Message}");
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
            public float WiimoteQuatW;
            public float WiimoteQuatX;
            public float WiimoteQuatY;
            public float WiimoteQuatZ;
            public float NunchukQuatW;
            public float NunchukQuatX;
            public float NunchukQuatY;
            public float NunchukQuatZ;
            public byte NunchukConnected;
            public byte BatteryLevel;
        }
    }
}