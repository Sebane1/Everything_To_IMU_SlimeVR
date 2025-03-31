using PS5_Dualsense_To_IMU_SlimeVR.SlimeVR;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;


namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class FalseThighTracker : IDisposable {
        private IBodyTracker _tracker;
        private string _macSpoof;
        private UDPHandler _udpHandler;
        private bool _ready;
        private float _calibratedHeight;
        private string _debug;
        private bool _isClamped;

        public bool IsActive {
            get {
                return _udpHandler.Active;
            }
            set {
                _udpHandler.Active = value;
            }
        }
        public string Debug { get => _debug; set => _debug = value; }
        public UDPHandler UdpHandler { get => _udpHandler; set => _udpHandler = value; }
        public bool IsClamped { get => _isClamped; set => _isClamped = value; }

        public FalseThighTracker(IBodyTracker dualsenseTracker) {
            Initialize(dualsenseTracker);
        }

        public async void Initialize(IBodyTracker tracker) {
            Task.Run(async () => {
                _tracker = tracker;
                _macSpoof = CalculateMD5Hash(tracker.MacSpoof);
                _udpHandler = new UDPHandler("FalseTracker", tracker.Id + 500,
                 new byte[] { (byte)_macSpoof[0], (byte)_macSpoof[1], (byte)_macSpoof[2],
                     (byte) _macSpoof[3], (byte) _macSpoof[4], (byte) _macSpoof[5] });
                _ready = true;
                Recalibrate();
            });
        }
        public float SpecialClamp(float value, float lessThan, float greaterThan, float clamp) {
            if (value < lessThan || value > greaterThan) {
                return clamp;
            }
            return value;
        }
        public float SpecialClamp(float value, float lessThan, float clamp) {
            if (value < lessThan) {
                return clamp;
            }
            return value;
        }
        public async Task<bool> Update() {
            if (_ready) {
                var hmdHeight = OpenVRReader.GetHMDHeight();
                bool sitting = hmdHeight < _calibratedHeight / 2 && hmdHeight > OpenVRReader.GetWaistTrackerHeight();
                Vector3 euler = _tracker.Euler;
                _debug =
                $"Device Id: {_macSpoof}\r\n" +
                $"HMD Height: {hmdHeight}\r\n" +
                $"Euler Rotation:\r\n" +
                $"X:{-euler.X}, Y:{euler.Y}, Z:{euler.Z}";
                float newX = sitting && OpenVRReader.IsTiltedMostlyForward() ? SpecialClamp(-euler.X, -120, -270) : SpecialClamp(-euler.X, -180, 0, 0);
                _isClamped = Math.Round(newX) == 0;
                float finalX = sitting && -euler.X > -94 ? -newX + 180 : newX;
                float finalY = euler.Y;
                float finalZ = !_isClamped ? -euler.Z : euler.Z;
                await _udpHandler.SetSensorRotation(new Vector3(finalX, finalY, finalZ + _tracker.LastHmdPositon).ToQuaternion());
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

        internal void Recalibrate() {
            _calibratedHeight = OpenVRReader.GetHMDHeight();
        }
    }
}
