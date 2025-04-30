using Newtonsoft.Json.Linq;
using Everything_To_IMU_SlimeVR.SlimeVR;
using System.Diagnostics;
using System.Numerics;
using static Everything_To_IMU_SlimeVR.TrackerConfig;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class ThreeDsControllerTracker : IDisposable, IBodyTracker {
        private string _debug;
        private int _index;
        private int _id;
        private string macSpoof;
        private SensorOrientation _sensorOrientation;
        private UDPHandler udpHandler;
        private Vector3 _rotationCalibration;
        private float _calibratedHeight;
        private bool _ready;
        private bool _disconnected;
        private string _lastDualSenseId;
        private bool _simulateThighs = true;
        private bool _useWaistTrackerForYaw;
        private FalseThighTracker _falseThighTracker;
        private float _lastEulerPositon;
        private Quaternion _rotation;
        private Vector3 _euler;
        private Vector3 _gyro;
        private Vector3 _acceleration;
        private bool _waitForRelease;
        private string _rememberedStringId;
        private RotationReferenceType _yawReferenceTypeValue = RotationReferenceType.WaistRotation;
        Stopwatch buttonPressTimer = new Stopwatch();

        public event EventHandler<string> OnTrackerError;

        public ThreeDsControllerTracker(int index) {
            Initialize(index);
        }
        public async void Initialize(int index) {
            Task.Run(async () => {
                try {
                    _index = index;
                    _id = index + 1;
                    _rememberedStringId = Forwarded3DSDataManager.DeviceMap.Keys.ElementAt(index).ToString();
                    macSpoof = _rememberedStringId + "3DS_Tracker";
                    _sensorOrientation = new SensorOrientation(index, SensorOrientation.SensorType.ThreeDs);
                    if (_simulateThighs) {
                        _falseThighTracker = new FalseThighTracker(this);
                    }
                    udpHandler = new UDPHandler("3DS_Tracker" + _rememberedStringId, _id,
                     new byte[] { (byte)macSpoof[0], (byte)macSpoof[1], (byte)macSpoof[2], (byte)macSpoof[3], (byte)macSpoof[4], (byte)macSpoof[5] });
                    udpHandler.Active = true;
                    Recalibrate();
                    _ready = true;
                } catch (Exception e) {
                    OnTrackerError?.Invoke(this, e.Message);
                }
            });
        }

        private Quaternion GetTrackerRotation(RotationReferenceType yawReferenceType) {
            try {
                switch (yawReferenceType) {
                    case RotationReferenceType.HmdRotation:
                        return OpenVRReader.GetHMDRotation();
                    case RotationReferenceType.WaistRotation:
                        return OpenVRReader.GetTrackerRotation("waist");
                    case RotationReferenceType.ChestRotation:
                        return OpenVRReader.GetTrackerRotation("chest");
                    case RotationReferenceType.TrackerRotation:
                        var value = Forwarded3DSDataManager.DeviceMap.ElementAt(_index);
                        var motionQuaternion = new Quaternion(value.Value.quatX, value.Value.quatY, value.Value.quatZ, value.Value.quatW);
                        return motionQuaternion;
                }
            } catch {
            }
            return Quaternion.Identity;
        }

        public async Task<bool> Update() {
            if (_ready) {
                try {
                    var hmdHeight = OpenVRReader.GetHMDHeight();
                    bool isClamped = !_falseThighTracker.IsClamped;
                    var trackerRotation = GetTrackerRotation(RotationReferenceType.WaistRotation);
                    float trackerEuler = trackerRotation.GetYawFromQuaternion();
                    if (!isClamped || YawReferenceTypeValue != RotationReferenceType.HmdRotation) {
                        _lastEulerPositon = YawReferenceTypeValue != RotationReferenceType.TrackerRotation ? -trackerEuler : trackerEuler;
                    }
                    var value = Forwarded3DSDataManager.DeviceMap.ElementAt(_index);
                    _rotation = new Quaternion(value.Value.quatX, value.Value.quatY, value.Value.quatZ, value.Value.quatW);
                    _euler = _rotation.QuaternionToEuler() + _rotationCalibration;
                    _gyro = _sensorOrientation.GyroData;
                    _acceleration = _sensorOrientation.AccelerometerData;

                    if (GenericControllerTrackerManager.DebugOpen) {
                        _debug =
                        $"Device Id: {macSpoof}\r\n" +
                        $"Euler Rotation:\r\n" +
                        $"X:{_euler.X}, Y:{_euler.Y}, Z:{_rotation.Z}" +
                        $"\r\nGyro:\r\n" +
                        $"X:{_gyro.X}, Y:{_gyro.Y}, Z:{_gyro.Z}" +
                        $"\r\nAcceleration:\r\n" +
                        $"X:{_acceleration.X}, Y:{_acceleration.Y}, Z:{_acceleration.Z}\r\n" +
                        $"Yaw Reference Rotation:\r\n" +
                        $"Y:{trackerEuler}\r\n"
                        + _falseThighTracker.Debug;
                    }
                    //await udpHandler.SetSensorBattery(100);
                    if (!_simulateThighs) {
                        await udpHandler.SetSensorRotation(new Vector3(-_euler.Y, _euler.Z, _euler.X + _lastEulerPositon).ToQuaternion());
                    } else {
                        float finalY = _euler.Y;
                        float finalZ = isClamped ? _euler.Z : _euler.Z;
                        await udpHandler.SetSensorRotation((new Vector3(-_euler.X, finalY, finalZ + _lastEulerPositon)).ToQuaternion());
                        await _falseThighTracker.Update();
                    }
                    _falseThighTracker.IsActive = _simulateThighs;
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
            _falseThighTracker.Recalibrate();
            await udpHandler.SendButton();
            await _falseThighTracker.UdpHandler.SendButton();
        }
        public void Rediscover() {
            udpHandler.Initialize();
            if (SimulateThighs) {
                _falseThighTracker.UdpHandler.Initialize();
            }
        }

        public void Dispose() {
            _ready = false;
            _disconnected = true;
            _falseThighTracker?.Dispose();
        }

        public Vector3 GetCalibration() {
            if (Configuration.Instance.TimeSinceLastConfig().TotalSeconds < 10) {
                if (Configuration.Instance.CalibrationConfigurations.ContainsKey(macSpoof)) {
                    return Configuration.Instance.CalibrationConfigurations[macSpoof];
                }
            }
            return -_rotation.QuaternionToEuler();
        }

        public string Debug { get => _debug; set => _debug = value; }
        public bool Ready { get => _ready; set => _ready = value; }
        public bool Disconnected { get => _disconnected; set => _disconnected = value; }
        public int Id { get => _id; set => _id = value; }
        public string MacSpoof { get => macSpoof; set => macSpoof = value; }
        public Vector3 Euler { get => _euler; set => _euler = value; }
        public Vector3 Gyro { get => _gyro; set => _gyro = value; }
        public Vector3 Acceleration { get => _acceleration; set => _acceleration = value; }
        public float LastHmdPositon { get => _lastEulerPositon; set => _lastEulerPositon = value; }
        public bool SimulateThighs { get => _simulateThighs; set => _simulateThighs = value; }
        public bool UseWaistTrackerForYaw { get => _useWaistTrackerForYaw; set => _useWaistTrackerForYaw = value; }
        public RotationReferenceType YawReferenceTypeValue { get => _yawReferenceTypeValue; set => _yawReferenceTypeValue = value; }
    }
}