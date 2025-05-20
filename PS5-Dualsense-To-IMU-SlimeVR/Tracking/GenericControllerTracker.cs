using Everything_To_IMU_SlimeVR.SlimeVR;
using System.Diagnostics;
using System.Numerics;
using static Everything_To_IMU_SlimeVR.TrackerConfig;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class GenericControllerTracker : IDisposable, IBodyTracker {
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
        private bool _usingWiimoteKnees = true;
        private FalseThighTracker _falseThighTracker;
        private float _lastEulerPositon;
        private Quaternion _rotation;
        private Vector3 _euler;
        private Vector3 _gyro;
        private Vector3 _acceleration;
        private bool _waitForRelease;
        private string _rememberedStringId;
        private RotationReferenceType _yawReferenceTypeValue;
        Stopwatch buttonPressTimer = new Stopwatch();
        private HapticNodeBinding _hapticNodeBinding;
        private bool isAlreadyVibrating;
        private bool identifying;

        public event EventHandler<string> OnTrackerError;

        public GenericControllerTracker(int index, Color colour) {
            Initialize(index, colour);
        }
        public async void Initialize(int index, Color colour) {
            Task.Run(async () => {
                try {
                    _index = index;
                    _id = index + 1;
                    var rememberedColour = colour;
                    _rememberedStringId = index + " " + JSL.JslGetControllerType(index);
                    JSL.JslSetLightColour(index, colour.ToArgb());
                    macSpoof = _rememberedStringId + "GenericController";
                    _sensorOrientation = new SensorOrientation(index, SensorOrientation.SensorType.Bluetooth);
                    if (_simulateThighs) {
                        _falseThighTracker = new FalseThighTracker(this);
                    }
                    udpHandler = new UDPHandler("GenericController" + _rememberedStringId,
                     new byte[] { (byte)macSpoof[0], (byte)macSpoof[1], (byte)macSpoof[2], (byte)macSpoof[3], (byte)macSpoof[4], (byte)macSpoof[5] }, 1);
                    udpHandler.Active = true;
                    Recalibrate();
                    _ready = true;
                } catch (Exception e) {
                    OnTrackerError?.Invoke(this, e.Message);
                }
            });
        }
        public bool GetGlobalState(int code) {
            int connections = GenericTrackerManager.ControllerCount;
            for (int i = 0; i < connections; i++) {
                var buttons = JSL.JslGetSimpleState(i).buttons;
                if ((buttons & code) != 0) {
                    return true;
                }
            }
            return false;
        }
        public bool GetLocalState(int code) {
            int connections = GenericTrackerManager.ControllerCount;
            var buttons = JSL.JslGetSimpleState(_index).buttons;
            if ((buttons & code) != 0) {
                return true;
            }
            return false;
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
                    var trackerRotation = GetTrackerRotation(YawReferenceTypeValue);
                    float trackerEuler = trackerRotation.GetYawFromQuaternion();
                    _lastEulerPositon = -trackerEuler;
                    _rotation = _sensorOrientation.CurrentOrientation;
                    _euler = _rotation.QuaternionToEuler() + _rotationCalibration;
                    _gyro = _sensorOrientation.GyroData;
                    _acceleration = _sensorOrientation.AccelerometerData;

                    if (GenericTrackerManager.DebugOpen) {
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
                    if (GetLocalState(0x20000)) {
                        if (!_waitForRelease) {
                            await udpHandler.SendButton();
                            _waitForRelease = true;
                        }
                    } else {
                        _waitForRelease = false;
                    }
                    //await udpHandler.SetSensorAcceleration(new Vector3(_sensorOrientation.AccelerometerData.X / 10000f, _sensorOrientation.AccelerometerData.Y / 10000f, _sensorOrientation.AccelerometerData.Z / 10000f), 0);
                    await udpHandler.SetSensorRotation(new Vector3(-_euler.X, _euler.Y, _lastEulerPositon).ToQuaternion(), 0);
                    if (_simulateThighs) {
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
            _sensorOrientation.Recalibrate();
            await Task.Delay(5000);
            JSL.JslResetContinuousCalibration(_index);
            _calibratedHeight = OpenVRReader.GetHMDHeight();
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
            return -(_sensorOrientation.CurrentOrientation).QuaternionToEuler();
        }

        public void Identify() {
            identifying = true;
            EngageHaptics(300, 100);
            identifying = false;
        }

        public void EngageHaptics(int duration, float intensity, bool timed = true) {
            if (!isAlreadyVibrating) {
                isAlreadyVibrating = true;
                Task.Run(() => {
                    JSL.JslSetRumble(_index, (int)(100 * intensity), (int)(intensity * 100f));
                    if (timed) {
                        Thread.Sleep(duration);
                        JSL.JslSetRumble(_index, 0, 0);
                        isAlreadyVibrating = false;
                    }
                    isAlreadyVibrating = false;
                    identifying = false;
                });
            }
        }

        public void DisableHaptics() {
            if (!identifying) {
                isAlreadyVibrating = false;
                JSL.JslSetRumble(_index, 0, 0);
            }
        }

        public override string ToString() {
            return "Controller Tracker " + _index;
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
        public bool UsingWiimoteKnees { get => _usingWiimoteKnees; set => _usingWiimoteKnees = value; }
        public HapticNodeBinding HapticNodeBinding { get => _hapticNodeBinding; set => _hapticNodeBinding = value; }
    }
}