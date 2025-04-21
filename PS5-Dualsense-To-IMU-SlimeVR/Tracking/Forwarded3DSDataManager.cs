using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    public class Forwarded3DSDataManager {
        const int listenPort = 9305;
        private static ConcurrentDictionary<string, JSL.MOTION_STATE> _deviceMap = new ConcurrentDictionary<string, JSL.MOTION_STATE>();

        public static ConcurrentDictionary<string, JSL.MOTION_STATE> DeviceMap { get => _deviceMap; set => _deviceMap = value; }
        static Stopwatch calibrationTimer = new Stopwatch();
        public Forwarded3DSDataManager() {
            Task.Run(() => {
                Initialize();
            });
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ImuPacket {
            public short ax, ay, az;
            public short gx, gy, gz;
        }

        const float accelDeadzone = 1.5f; // Play with this, maybe 0.1–0.3
        const float gyroDeadzone = 1.0f;

        static float ApplyDeadzone(float value, float threshold) {
            return Math.Abs(value) < threshold ? 0 : value;
        }
        public static Quaternion GetOrientationFromGravity(Vector3 gravity) {
            gravity = Vector3.Normalize(gravity);
            float pitch = MathF.Asin(-gravity.X);
            float roll = MathF.Atan2(gravity.Y, gravity.Z);
            Quaternion qPitch = Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
            Quaternion qRoll = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, roll);
            return Quaternion.Normalize(Quaternion.Concatenate(qRoll, qPitch));
        }
        static async void Initialize() {
            UdpClient udpClient = new UdpClient(listenPort); // Match port from 3DS
            _deviceMap = new ConcurrentDictionary<string, JSL.MOTION_STATE>();
            int nextId = 1;

            Console.WriteLine("Listening for IMU data...");
            float accumulatedY = 0;
            int calibrationSamples = 0;
            float divisionValue = 0;
            calibrationTimer.Restart();
            while (true) {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref remoteEP);

                // Identify by IP
                string ip = remoteEP.Address.ToString();

                if (data.Length == Marshal.SizeOf(typeof(ImuPacket))) {
                    GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    var value = (ImuPacket)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ImuPacket));
                    var y = Math.Abs(value.ay);
                    if (calibrationTimer.ElapsedMilliseconds < 10000) {
                        accumulatedY += y;
                        calibrationSamples++;
                        divisionValue = 9.8f / (accumulatedY / calibrationSamples);
                    }
                    var gravity = new Vector3(ApplyDeadzone((float)value.ax * divisionValue, accelDeadzone), 
                        ApplyDeadzone((float)value.ay * divisionValue, accelDeadzone), 
                        ApplyDeadzone((float)value.az * divisionValue, accelDeadzone));
                    var orientation = GetOrientationFromGravity(gravity);
                    _deviceMap[ip] = new JSL.MOTION_STATE {
                        gravX = gravity.X, 
                        gravY = gravity.Y, 
                        gravZ = gravity.Z,
                        accelX = value.gx, accelY = value.gy, accelZ = value.gz,
                        quatX = orientation.X, quatY = orientation.Y, quatZ = orientation.Z, quatW = orientation.W
                    };
                    handle.Free();
                }
            }
        }
    }
}