using Newtonsoft.Json.Linq;
using SlimeImuProtocol.SlimeVR;
using SlimeImuProtocol.Utility;
using System.Diagnostics;
using System.Numerics;
using static Everything_To_IMU_SlimeVR.TrackerConfig;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class ThreeDsControllerTracker : IDisposable, IBodyTracker {
        private string _debug;
        private int _index;
        private int _id;
        private string macSpoof;
        private byte[] _macAddressBytes;
        private SensorOrientation _sensorOrientation;
        private UDPHandler udpHandler;
        private Vector3 _rotationCalibration;
        private float _calibratedHeight;
        private bool _ready;
        private bool _disconnected;
        private string _lastDualSenseId;
        private bool _simulateThighs = true;
        private bool _useWaistTrackerForYaw;
        private float _lastEulerPositon;
        private Quaternion _rotation;
        private Vector3 _euler;
        private Vector3 _gyro;
        private Vector3 _acceleration;
        private bool _waitForRelease;
        private string _rememberedStringId;
        private string _ip;
        private RotationReferenceType _yawReferenceTypeValue = RotationReferenceType.WaistRotation;
        Stopwatch buttonPressTimer = new Stopwatch();
        private HapticNodeBinding _hapticNodeBinding;
        private RotationReferenceType _extensionYawReferenceTypeValue;

        public bool SupportsHaptics => false;
        public bool SupportsIMU => true;
        public event EventHandler<string> OnTrackerError;

        public ThreeDsControllerTracker(string id) {
            Initialize(id);
        }
        public async void Initialize(string id) {
            Task.Run(async () => {
                try {;
                    _ip = id;
                    macSpoof = id + "3DS_Tracker";
                    _macAddressBytes = new byte[] { (byte)macSpoof[0], (byte)macSpoof[1], (byte)macSpoof[2], (byte)macSpoof[3], (byte)macSpoof[4], (byte)macSpoof[5] };
                    udpHandler = new UDPHandler("3DS_Tracker" + id, _macAddressBytes,
                 FirmwareConstants.BoardType.UNKNOWN, FirmwareConstants.ImuType.UNKNOWN, FirmwareConstants.McuType.UNKNOWN, FirmwareConstants.MagnetometerStatus.NOT_SUPPORTED,1);
                    udpHandler.Active = true;
                    Recalibrate();
                    Forwarded3DSDataManager.NewPacketReceived += NewPacketReceived;
                    _ready = true;
                } catch (Exception e) {
                    OnTrackerError?.Invoke(this, e.Message);
                }
            });
        }

        private async void NewPacketReceived(object reference, string ip) {
            if (_ip == ip) {
                await Update();
            }
        }

        public async Task<bool> Update() {
            if (_ready) {
                try {
                    var hmdHeight = OpenVRReader.GetHMDHeight();
                    var trackerRotation = OpenVRReader.GetTrackerRotation(YawReferenceTypeValue);
                    float trackerEuler = trackerRotation.GetYawFromQuaternion();
                    var value = Forwarded3DSDataManager.DeviceMap[_ip];
                    _rotation = new Quaternion(value.quatX, value.quatY, value.quatZ, value.quatW);
                    _euler = _rotation.QuaternionToEuler();

                    if (GenericTrackerManager.DebugOpen) {
                        _debug =
                        $"Device Id: {macSpoof}\r\n" +
                        $"Euler Rotation:\r\n" +
                        $"X:{_euler.X}, Y:{_euler.Y}, Z:{_euler.Z}\r\n" +
                        $"Accelerometer:\r\n" +
                        $"X:{value.accelX}, Y:{value.accelY}, Z:{value.accelZ}\r\n" +
                        $"Gyro:\r\n" +
                        $"X:{value.gyroX}, Y:{value.gyroY}, Z:{value.gyroZ}\r\n" +
                        $"Yaw Reference Rotation:\r\n" +
                        $"Y:{trackerEuler}\r\n";
                    }
                    //await udpHandler.SetSensorBattery(100);
                    if (RotationReferenceType.TrustDeviceYaw != _yawReferenceTypeValue) {
                        await udpHandler.SetSensorRotation(new Vector3(_euler.X, _euler.Y, trackerEuler).ToQuaternion(), 0);
                    } else {
                        await udpHandler.SetSensorRotation(_rotation, 0);
                    }
                } catch (Exception e) {
                    OnTrackerError.Invoke(this, e.StackTrace + "\r\n" + e.Message);
                }
            }
            return _ready;
        }
        public async void Recalibrate() {
            await Task.Delay(5000);
            _calibratedHeight = OpenVRReader.GetHMDHeight();
            var value = Forwarded3DSDataManager.DeviceMap.ElementAt(_index);
            _rotation = new Quaternion(value.Value.quatX, value.Value.quatY, value.Value.quatZ, value.Value.quatW);
            _rotationCalibration = GetCalibration();
        }
        public void Rediscover() {
            udpHandler.Initialize(FirmwareConstants.BoardType.UNKNOWN, FirmwareConstants.ImuType.UNKNOWN, FirmwareConstants.McuType.UNKNOWN, FirmwareConstants.MagnetometerStatus.NOT_SUPPORTED, _macAddressBytes);
        }

        public void Dispose() {
            _ready = false;
            _disconnected = true;
        }

        public Vector3 GetCalibration() {
            if (Configuration.Instance.TimeSinceLastConfig().TotalSeconds < 10) {
                if (Configuration.Instance.CalibrationConfigurations.ContainsKey(macSpoof)) {
                    return Configuration.Instance.CalibrationConfigurations[macSpoof];
                }
            }
            return -_rotation.QuaternionToEuler();
        }

        public void Identify() {
            //throw new NotImplementedException();
        }

        public void EngageHaptics(int duration, float intensity) {
            //throw new NotImplementedException();
        }

        public void DisableHaptics() {
            //throw new NotImplementedException();
        }
        public override string ToString() {
            return "DS Tracker " + _index;
        }

        public void HapticIntensityTest() {
            //throw new NotImplementedException();
        }

        public string Debug { get => _debug; set => _debug = value; }
        public bool Ready { get => _ready; set => _ready = value; }
        public bool Disconnected { get => _disconnected; set => _disconnected = value; }
        public int  Id { get => _id; set => _id = value; }
        public string MacSpoof { get => macSpoof; set => macSpoof = value; }
        public Vector3 Euler { get => _euler; set => _euler = value; }
        public Vector3 Gyro { get => _gyro; set => _gyro = value; }
        public Vector3 Acceleration { get => _acceleration; set => _acceleration = value; }
        public float LastHmdPositon { get => _lastEulerPositon; set => _lastEulerPositon = value; }
        public bool SimulateThighs { get => _simulateThighs; set => _simulateThighs = value; }
        public bool UseWaistTrackerForYaw { get => _useWaistTrackerForYaw; set => _useWaistTrackerForYaw = value; }
        public RotationReferenceType YawReferenceTypeValue { get => _yawReferenceTypeValue; set => _yawReferenceTypeValue = value; }
        public HapticNodeBinding HapticNodeBinding { get => _hapticNodeBinding; set => _hapticNodeBinding = value; }
        public RotationReferenceType ExtensionYawReferenceTypeValue { get => _extensionYawReferenceTypeValue; set => _extensionYawReferenceTypeValue = value; }
        public Vector3 RotationCalibration => _rotationCalibration;

        Vector3 IBodyTracker.RotationCalibration { get => _rotationCalibration; set => _rotationCalibration = value; }
    }
}