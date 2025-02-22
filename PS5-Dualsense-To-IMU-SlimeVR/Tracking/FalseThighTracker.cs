using PS5_Dualsense_To_IMU_SlimeVR.SlimeVR;
using PS5_Dualsense_To_IMU_SlimeVR.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Wujek_Dualsense_API;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class FalseThighTracker {
        private DualsenseTracker _dualSenseTracker;
        private string _macSpoof;
        private UDPHandler udpHandler;
        private bool _ready;
        private string _debug;

        public string Debug { get => _debug; set => _debug = value; }

        public FalseThighTracker(DualsenseTracker dualsenseTracker) {
            Initialize(dualsenseTracker);
        }

        public async void Initialize(DualsenseTracker dualsenseTracker) {
            Task.Run(async () => {
                _dualSenseTracker = dualsenseTracker;
                _macSpoof = CalculateMD5Hash(dualsenseTracker.MacSpoof);
                udpHandler = new UDPHandler("FalseThighSense5", dualsenseTracker.Id + 500,
                 new byte[] { (byte)_macSpoof[0], (byte)_macSpoof[1], (byte)_macSpoof[2],
                     (byte) _macSpoof[3], (byte) _macSpoof[4], (byte) _macSpoof[5] });
                _ready = true;
            });
        }

        public async Task<bool> Update() {
            if (_ready) {
                Vector3 euler = _dualSenseTracker.Euler;
               Vector3 gyro = _dualSenseTracker.Gyro;
                Vector3 acceleration = _dualSenseTracker.Acceleration;
                _debug =
                $"Device Id: {_macSpoof}\r\n" +
                $"Euler Rotation:\r\n" +
                $"X:{-euler.X}, Y:{euler.Y}, Z:{euler.Z}" +
                $"\r\nGyro:\r\n" +
                $"X:{gyro.X}, Y:{gyro.Y}, Z:{gyro.Z}" +
                $"\r\nAcceleration:\r\n" +
                $"X:{acceleration.X}, Y:{acceleration.Y}, Z:{acceleration.Z}";
                await udpHandler.SetSensorRotation(new Vector3(-euler.X > 132 ? -euler.X : float.Clamp(-euler.X, -float.MaxValue, -5), euler.Y, euler.Z).ToQuaternion());
            }
            return _ready;
        }

        public static string CalculateMD5Hash(string input) {
            using (MD5 md5 = MD5.Create()) {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++) {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

    }
}
