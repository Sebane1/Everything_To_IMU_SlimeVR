using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class ForwardedWiimoteManager {
        public ForwardedWiimoteManager() {
            Task.Run(() => {
                Initialize();
            });
        }

        async void Initialize() {
            var udp = new UdpClient(9930);
            var ep = new IPEndPoint(IPAddress.Any, 9930);

            while (true) {
                byte[] data = udp.Receive(ref ep);

                try {
                    WiimotePacket packet = ParsePacket(data);
                    Console.WriteLine(packet.ToString());
                } catch (Exception ex) {
                    Console.WriteLine($"Error parsing packet: {ex.Message}");
                }
            }

        }
        WiimotePacket ParsePacket(byte[] data) {
            if (data.Length != Marshal.SizeOf<WiimotePacket>())
                throw new ArgumentException("Invalid packet size");

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

            public ushort WiimoteAccelX;
            public ushort WiimoteAccelY;
            public ushort WiimoteAccelZ;

            public ushort NunchukAccelX;
            public ushort NunchukAccelY;
            public ushort NunchukAccelZ;

            public byte NunchukConnected;

            public override string ToString() {
                string nunchukInfo = NunchukConnected == 1
                    ? $"({NunchukAccelX}, {NunchukAccelY}, {NunchukAccelZ})"
                    : "Not connected";

                return $"ID: {Id} | Wiimote Accel: ({WiimoteAccelX}, {WiimoteAccelY}, {WiimoteAccelZ}) | Nunchuk Accel: {nunchukInfo}";
            }
        }
    }
}
