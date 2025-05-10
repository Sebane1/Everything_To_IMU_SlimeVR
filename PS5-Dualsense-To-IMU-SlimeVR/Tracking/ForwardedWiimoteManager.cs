using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Numerics;
using System.Linq;

namespace Everything_To_IMU_SlimeVR.Tracking {
    internal class ForwardedWiimoteManager {
        private static ConcurrentDictionary<string, WiimotePacket> _wiimotes = new ConcurrentDictionary<string, WiimotePacket>();
        private static List<string> _wiimoteIds = new List<string>();
        public static EventHandler NewPacketReceived;
        public static EventHandler LegacyClientDetected;
        // Calibration tracking
        private const int CalibrationSamples = 100;
        private ConcurrentDictionary<string, List<Vector3>> _calibrationSamples = new();
        private ConcurrentDictionary<string, (Vector3 center, float scale)> _calibrationData = new();

        public ForwardedWiimoteManager() {
            Task.Run(() => Initialize());
        }

        public static ConcurrentDictionary<string, WiimotePacket> Wiimotes { get => _wiimotes; set => _wiimotes = value; }

        async void Initialize() {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://+:9909/");
            listener.Start();
            Console.WriteLine("HTTP Listener started on port 9909...");

            while (true) {
                try {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;

                    if (request.HttpMethod != "POST") {
                        context.Response.StatusCode = 405;
                        context.Response.Close();
                        continue;
                    }

                    using var bodyStream = request.InputStream;
                    using var ms = new MemoryStream();
                    await bodyStream.CopyToAsync(ms);
                    byte[] data = ms.ToArray();

                    int numPackets = 0;
                    int packetLength = 0;
                    if (data.Length % 38 == 0) {
                        // Current correct format
                        numPackets = data.Length / 38;
                        packetLength = 38;
                        // proceed normally...
                    } else if (data.Length % 37 == 0) {
                        // Old format detected
                        Console.WriteLine("⚠️  Legacy client detected: using old 37-byte packet format.");
                        numPackets = data.Length / 37;
                        packetLength = 37;
                        LegacyClientDetected?.Invoke(this, EventArgs.Empty);
                    } else {
                        // Invalid or corrupted data
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        LegacyClientDetected?.Invoke(this, EventArgs.Empty);
                        continue;
                    }

                    string clientIp = context.Request.RemoteEndPoint?.Address.ToString() ?? "Unknown";

                    for (int i = 0; i < numPackets; i++) {
                        byte[] packetBytes = new byte[packetLength];
                        Buffer.BlockCopy(data, i * packetLength, packetBytes, 0, packetLength);

                        WiimotePacket packet = ParsePacket(packetBytes);
                        string key = $"{clientIp}:{packet.Id}";

                        _wiimotes[key] = packet;
                    }

                    context.Response.StatusCode = 200;
                    context.Response.OutputStream.Write(new byte[] { }, 0, 0);
                    context.Response.Close();
                    NewPacketReceived?.Invoke(this, EventArgs.Empty);
                } catch (Exception ex) {
                    Console.WriteLine($"Listener error: {ex.Message}");
                }
            }
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
