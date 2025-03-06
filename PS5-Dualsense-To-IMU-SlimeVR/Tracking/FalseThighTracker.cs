using PS5_Dualsense_To_IMU_SlimeVR.SlimeVR;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;


namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class FalseThighTracker : IDisposable {
        private IBodyTracker _tracker;
        private string _macSpoof;
        private UDPHandler udpHandler;
        private bool _ready;
        private float _calibratedHeight;
        private string _debug;

        public string Debug { get => _debug; set => _debug = value; }

        public FalseThighTracker(IBodyTracker dualsenseTracker) {
            Initialize(dualsenseTracker);
        }

        public async void Initialize(IBodyTracker tracker) {
            Task.Run(async () => {
                _tracker = tracker;
                _macSpoof = CalculateMD5Hash(tracker.MacSpoof);
                udpHandler = new UDPHandler("FalseTracker", tracker.Id + 500,
                 new byte[] { (byte)_macSpoof[0], (byte)_macSpoof[1], (byte)_macSpoof[2],
                     (byte) _macSpoof[3], (byte) _macSpoof[4], (byte) _macSpoof[5] });
                _ready = true;
                _calibratedHeight = HmdReader.GetHMDHeight();
            });
        }
        public float SpecialClamp(float value) { 
            if(value < -220 || value > 0) {
                return 0;
            }
            return value;
        }
        public async Task<bool> Update() {
            if (_ready) {
                var hmdHeight = HmdReader.GetHMDHeight();
                var sitting = hmdHeight < _calibratedHeight / 2;
                Vector3 euler = _tracker.Euler;
                _debug =
                $"Device Id: {_macSpoof}\r\n" +
                $"HMD Height: {hmdHeight}\r\n" +
                $"Euler Rotation:\r\n" +
                $"X:{-euler.X}, Y:{euler.Y}, Z:{euler.Z}";
                float newX = SpecialClamp(-euler.X);
                float finalX = sitting && -euler.X > -94 ? -newX + 180 : newX;
                float finalY = !sitting ? -euler.Y : euler.Y;
                float finalZ = sitting ? -euler.Z : euler.Z;
                await udpHandler.SetSensorRotation(new Vector3(finalX, finalY, finalZ + _tracker.LastHmdPositon).ToQuaternion());
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

        public void Dispose() {
            _ready = false;
        }
    }
}
