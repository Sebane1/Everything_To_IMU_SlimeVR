using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Numerics;
using System.Threading.Channels;
using System.Diagnostics;

namespace Everything_To_IMU_SlimeVR.Tracking {
    internal class ForwardedWiimoteManager {
        private static ConcurrentDictionary<string, WiimotePacket> _wiimotes = new();
        private static List<string> _wiimoteIds = new();
        public static EventHandler NewPacketReceived;
        public static EventHandler LegacyClientDetected;
        Stopwatch _timeBetweenRequests = new Stopwatch();

        private const int CalibrationSamples = 100;
        private ConcurrentDictionary<string, List<Vector3>> _calibrationSamples = new();
        private ConcurrentDictionary<string, (Vector3 center, float scale)> _calibrationData = new();

        private readonly Channel<HttpListenerContext> _contextQueue = Channel.CreateUnbounded<HttpListenerContext>();

        public ForwardedWiimoteManager() {
            Task.Run(() => StartListener());
            Task.Run(() => ProcessQueue());
            _timeBetweenRequests.Restart();
        }

        public static ConcurrentDictionary<string, WiimotePacket> Wiimotes => _wiimotes;

        async Task StartListener() {
            HttpListener listener = new();
            listener.Prefixes.Add("http://+:9909/");
            listener.Start();
            Console.WriteLine("HTTP Listener started on port 9909...");

            while (true) {
                try {
                    var context = await listener.GetContextAsync();
                    await _contextQueue.Writer.WriteAsync(context);
                } catch (Exception ex) {
                    Console.WriteLine($"Listener error: {ex.Message}");
                }
            }
        }

        async Task ProcessQueue() {
            await foreach (var context in _contextQueue.Reader.ReadAllAsync()) {
                _ = Task.Run(() => HandleRequest(context));
            }
        }

        async Task HandleRequest(HttpListenerContext context) {
            var clientIp = context.Request.RemoteEndPoint?.Address.ToString() ?? "Unknown";
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Debug.WriteLine("Message received, last message received " + _timeBetweenRequests.ElapsedMilliseconds + "ms ago.");
            _timeBetweenRequests.Restart();
            try {
                var request = context.Request;
                if (request.HttpMethod != "POST") {
                    context.Response.StatusCode = 405;
                    context.Response.Close();
                    return;
                }

                if (request.ContentLength64 <= 0 || request.ContentLength64 > 4096) {
                    context.Response.StatusCode = 411;
                    context.Response.Close();
                    return;
                }

                byte[] data = new byte[request.ContentLength64];
                int bytesRead = 0;
                while (bytesRead < data.Length) {
                    int read = await request.InputStream.ReadAsync(data, bytesRead, data.Length - bytesRead);
                    if (read == 0) break;
                    bytesRead += read;
                }

                stopwatch.Stop();
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
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                    LegacyClientDetected?.Invoke(this, EventArgs.Empty);
                    return;
                }

                for (int i = 0; i < numPackets; i++) {
                    byte[] packetBytes = new byte[packetLength];
                    Buffer.BlockCopy(data, i * packetLength, packetBytes, 0, packetLength);
                    WiimotePacket packet = ParsePacket(packetBytes);
                    string key = $"{clientIp}:{packet.Id}";
                    _wiimotes[key] = packet;
                }

                context.Response.StatusCode = 200;
                await context.Response.OutputStream.FlushAsync(); // Just to be safe
                context.Response.Close();
                NewPacketReceived?.Invoke(this, EventArgs.Empty);
            } catch (Exception ex) {
                Console.WriteLine($"❌ Handler error from {clientIp}: {ex.Message}");
                try {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                } catch { /* ignored */ }
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