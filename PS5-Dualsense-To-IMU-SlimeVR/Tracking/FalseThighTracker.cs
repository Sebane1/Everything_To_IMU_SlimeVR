using SlimeImuProtocol.SlimeVR;
using SlimeImuProtocol.Utility;
using System.Configuration;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using static Everything_To_IMU_SlimeVR.TrackerConfig;


namespace Everything_To_IMU_SlimeVR.Tracking {
    internal class FalseThighTracker : IDisposable {
        public RotationReferenceType YawReferenceTypeValue { get; private set; }

        private string _macSpoof;
        private UDPHandler _udpHandler;
        private bool _ready;
        private float _calibratedHeight;
        private string _debug;
        private bool _isClamped;
        private float _smoothedLegBend;
        private float _trackerEuler;
        private float _lastEulerPositon;

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

        public FalseThighTracker(RotationReferenceType rotationReferenceType) {
            Initialize(rotationReferenceType);
        }

        public async void Initialize(RotationReferenceType rotationReferenceType) {
            Task.Run(async () => {
                YawReferenceTypeValue = rotationReferenceType;
                _macSpoof = HashUtility.CalculateMD5Hash(rotationReferenceType.ToString());
                _udpHandler = new UDPHandler("FalseTracker_" + rotationReferenceType.ToString(),
                 new byte[] { (byte)_macSpoof[0], (byte)_macSpoof[1], (byte)_macSpoof[2],
                     (byte) _macSpoof[3], (byte) _macSpoof[4], (byte) _macSpoof[5] },
                 FirmwareConstants.BoardType.UNKNOWN, FirmwareConstants.ImuType.UNKNOWN, FirmwareConstants.McuType.UNKNOWN, FirmwareConstants.MagnetometerStatus.NOT_SUPPORTED,1);
                IsActive = Configuration.Instance.SimulatesThighs;

                _ready = true;
                Recalibrate();
                while (true) {
                    if (Configuration.Instance.SimulatesThighs) {
                        _udpHandler.Active = true;
                        await Update();
                        Thread.Sleep(16);
                    } else {
                        _udpHandler.Active = false;
                        Thread.Sleep(10000);
                    }
                }
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
                var trackerRotation = OpenVRReader.GetTrackerRotation(YawReferenceTypeValue);
                _trackerEuler = trackerRotation.GetYawFromQuaternion();
                _lastEulerPositon = -_trackerEuler;
                var hmdHeight = OpenVRReader.GetHMDHeight();
                var hmdHeightToQuadrants = (_calibratedHeight / 4f);
                var legDifferenceToSubtract = hmdHeightToQuadrants * 3;
                var legCalibratedHmdHeight = _calibratedHeight - legDifferenceToSubtract;
                var legHmdHeight = hmdHeight - legDifferenceToSubtract;
                bool sitting = hmdHeight < _calibratedHeight / 2 && hmdHeight > OpenVRReader.GetWaistTrackerHeight();
                Vector3 euler = trackerRotation.QuaternionToEuler();
                if (GenericTrackerManager.DebugOpen) {
                    _debug =
                $"Device Id: {_macSpoof}\r\n" +
                $"HMD Height: {hmdHeight}\r\n" +
                $"Euler Rotation:\r\n" +
                $"X:{-euler.X}, Y:{euler.Y}, Z:{euler.Z}\r\n";
                }
                var directionalData = OpenVRReader.WaistIsInFrontOfHMD();
                if (GenericTrackerManager.DebugOpen) {
                    _debug += $"Is Leaning Forward: {directionalData.Item1.ToString()} \r\n";
                    _debug += directionalData.Item2;
                }
                float bendPercentage = Math.Clamp((legHmdHeight / legCalibratedHmdHeight), 0, 1);
                float upperLegBend = sitting || directionalData.Item3 > 30 || hmdHeight < legDifferenceToSubtract - (_calibratedHeight * 0.025f) ? 0 : -float.Lerp(0, 90, 1 - bendPercentage);
                _smoothedLegBend = float.Lerp(_smoothedLegBend, upperLegBend, 0.1f);
                float newX = sitting && directionalData.Item1 ? SpecialClamp(euler.X, 270, -270) : SpecialClamp(euler.X, -180, _smoothedLegBend, _smoothedLegBend);
                _isClamped = Math.Round(newX) == 0;
                float finalX = sitting && euler.X > -94 && !directionalData.Item1 ? -newX + 180 : newX;
                float finalY = euler.Y;
                float finalZ = -euler.Z;
                await _udpHandler.SetSensorRotation(new Vector3(finalX, finalZ, finalY).ToQuaternion(), 0);
            }
            return _ready;
        }


        public void Dispose() {
            _ready = false;
        }

        internal void Recalibrate() {
            _calibratedHeight = OpenVRReader.GetHMDHeight();
        }
    }
}
