using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Numerics;

namespace Everything_To_IMU_SlimeVR.Tracking {
    internal class ForwardedWiimoteManager {
        private static ConcurrentDictionary<string, JSL.MOTION_STATE> _wiimotes = new ConcurrentDictionary<string, JSL.MOTION_STATE>();
        private static ConcurrentDictionary<string, JSL.MOTION_STATE> _nunchucks = new ConcurrentDictionary<string, JSL.MOTION_STATE>();

        // Calibration tracking
        private const int CalibrationSamples = 100;
        private ConcurrentDictionary<string, List<Vector3>> _calibrationSamples = new();
        private ConcurrentDictionary<string, (Vector3 center, float scale)> _calibrationData = new();

        public ForwardedWiimoteManager() {
            Task.Run(() => Initialize());
        }

        public static ConcurrentDictionary<string, JSL.MOTION_STATE> Wiimotes { get => _wiimotes; set => _wiimotes = value; }
        public static ConcurrentDictionary<string, JSL.MOTION_STATE> Nunchucks { get => _nunchucks; set => _nunchucks = value; }

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

                    if (data.Length != 37) {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                        continue;
                    }


                    WiimotePacket packet = ParsePacket(data);
                    string clientIp = context.Request.RemoteEndPoint?.Address.ToString() ?? "Unknown";
                    string key = $"{clientIp}:{packet.Id}";

                    Quaternion wiimoteQuat = new Quaternion(packet.WiimoteQuatX, packet.WiimoteQuatY, packet.WiimoteQuatZ, packet.WiimoteQuatW);

                    _wiimotes[key] = new JSL.MOTION_STATE {
                        quatW = wiimoteQuat.W,
                        quatX = wiimoteQuat.X,
                        quatY = wiimoteQuat.Y,
                        quatZ = wiimoteQuat.Z
                    };

                    // Calibration (Nunchuk)
                    if (packet.NunchukConnected == 1) {
                        Quaternion nunchukQuat = new Quaternion(packet.NunchukQuatX, packet.NunchukQuatY, packet.NunchukQuatZ, packet.NunchukQuatW);

                        _nunchucks[key] = new JSL.MOTION_STATE {
                            quatW = nunchukQuat.W,
                            quatX = nunchukQuat.X,
                            quatY = nunchukQuat.Y,
                            quatZ = nunchukQuat.Z
                        };
                    }

                    context.Response.StatusCode = 200;
                    context.Response.OutputStream.Write(new byte[] { }, 0, 0);
                    context.Response.Close();
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
        }

    }
}
