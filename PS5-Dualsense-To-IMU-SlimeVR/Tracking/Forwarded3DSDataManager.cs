using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    public class Forwarded3DSDataManager {
        const int listenPort = 9305;
        private static ConcurrentDictionary<string, string> _deviceMap = new ConcurrentDictionary<string, string>();

        public static ConcurrentDictionary<string, string> DeviceMap { get => _deviceMap; set => _deviceMap = value; }

        public Forwarded3DSDataManager() {
            Task.Run(() => {
                Initialize();
            });
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ImuPacket {
            public short ax, ay, az;
            public short gx, gy, gz;
        }


        static async void Initialize() {
            UdpClient udpClient = new UdpClient(listenPort); // Match port from 3DS
            _deviceMap = new ConcurrentDictionary<string, string>();
            int nextId = 1;

            Console.WriteLine("Listening for IMU data...");

            while (true) {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref remoteEP);

                // Identify by IP
                string ip = remoteEP.Address.ToString();
                if (!_deviceMap.ContainsKey(ip)) {
                    _deviceMap[ip] = $"3DS_{nextId++}";
                }
                string label = _deviceMap[ip];

                if (data.Length == Marshal.SizeOf(typeof(ImuPacket))) {
                    GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    ImuPacket packet = (ImuPacket)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ImuPacket));
                    handle.Free();

                    Console.WriteLine($"{label} ({ip}): " +
                        $"ACC=({packet.ax}, {packet.ay}, {packet.az}) " +
                        $"GYRO=({packet.gx}, {packet.gy}, {packet.gz})");
                } else {
                    Console.WriteLine($"{label} ({ip}): Invalid packet size: {data.Length} bytes");
                }
            }
        }
    }
}