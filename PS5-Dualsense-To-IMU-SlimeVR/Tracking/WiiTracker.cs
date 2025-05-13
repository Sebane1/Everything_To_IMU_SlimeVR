using Newtonsoft.Json.Linq;
using Everything_To_IMU_SlimeVR.SlimeVR;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using Everything_To_IMU_SlimeVR.Utility;
using static Everything_To_IMU_SlimeVR.TrackerConfig;
using static Everything_To_IMU_SlimeVR.Tracking.ForwardedWiimoteManager;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class WiiTracker : IDisposable, IBodyTracker {
        private string _debug;
        private int _index;
        private int _id;
        private string _firmwareId;
        private bool _nunchuck;
        private ConcurrentDictionary<string, WiimotePacket> _motionStateList;
        private string macSpoof;
        private SensorOrientation _sensorOrientation;
        private UDPHandler udpHandler;
        private Vector3 _wiimoteRotationCalibration;
        private Vector3 _nunchuckRotationCalibration;
        private float _calibratedHeight;
        private bool _ready;
        private bool _disconnected;
        private string _lastDualSenseId;
        private bool _simulateThighs = true;
        private bool _useWaistTrackerForYaw;
        private FalseThighTracker _falseThighTracker;
        private float _lastEulerPositon;
        private Quaternion _rotation;
        private Vector3 _eulerUncalibrated;
        private Vector3 _euler;
        private Vector3 _gyro;
        private Vector3 _acceleration;
        private bool _waitForRelease;
        private string _rememberedStringId;
        private RotationReferenceType _yawReferenceTypeValue = RotationReferenceType.WaistRotation;
        Stopwatch buttonPressTimer = new Stopwatch();
        WiiTracker _connectedWiimote;
        private HapticNodeBinding _hapticNodeBinding;
        private bool isAlreadyVibrating;

        public event EventHandler<string> OnTrackerError;

        public WiiTracker(int index) {
            Initialize(index);
        }
        public async void Initialize(int index) {
            Task.Run(async () => {
                try {
                    _index = index;
                    _id = index + 1;
                    _firmwareId = "";
                    _rememberedStringId = index.ToString();
                    macSpoof = HashUtility.CalculateMD5Hash(_rememberedStringId + "Wiimote_Tracker");
                    _sensorOrientation = new SensorOrientation(index, SensorOrientation.SensorType.Wiimote);
                    _firmwareId = "Wiimote_Tracker" + _rememberedStringId;
                    _motionStateList = ForwardedWiimoteManager.Wiimotes;
                    if (_simulateThighs) {
                        _falseThighTracker = new FalseThighTracker(this);
                    }
                    udpHandler = new UDPHandler(_firmwareId,
                     new byte[] { (byte)macSpoof[0], (byte)macSpoof[1], (byte)macSpoof[2], (byte)macSpoof[3], (byte)macSpoof[4], (byte)macSpoof[5] }, 2);
                    udpHandler.Active = true;
                    Recalibrate();
                    _ready = true;
                    ForwardedWiimoteManager.NewPacketReceived += delegate {
                        Update();
                    };
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

                    var value = _motionStateList.ElementAt(_index);
                    _rotation = new Quaternion(value.Value.WiimoteQuatX, value.Value.WiimoteQuatY, value.Value.WiimoteQuatZ, value.Value.WiimoteQuatW);
                    _eulerUncalibrated = _rotation.QuaternionToEuler();
                    _euler = _eulerUncalibrated + _wiimoteRotationCalibration;
                    _gyro = _sensorOrientation.GyroData;
                    _acceleration = _sensorOrientation.AccelerometerData;

                    if (GenericControllerTrackerManager.DebugOpen) {
                        _debug =
                        $"Device Id: {macSpoof}\r\n" +
                        $"Euler Rotation Uncalibrated:\r\n" +
                        $"X:{_eulerUncalibrated.X}, Y:{_eulerUncalibrated.Y}, Z:{_eulerUncalibrated.Z}" +
                        $"\r\nEuler Rotation:\r\n" +
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
                    float finalX = !_nunchuck ? -_euler.X : _euler.X;
                    float finalY = !_nunchuck ? _euler.Y : _connectedWiimote.Euler.Y;
                    float finalZ = 0;

                    await udpHandler.SetSensorBattery(value.Value.BatteryLevel / 255f);
                    await udpHandler.SetSensorRotation(new Vector3(finalX, finalY, _lastEulerPositon).ToQuaternion(), 0);

                    if (value.Value.NunchukConnected != 0) {
                        _rotation = new Quaternion(value.Value.NunchukQuatX, value.Value.NunchukQuatY, value.Value.NunchukQuatZ, value.Value.NunchukQuatW);
                        _eulerUncalibrated = _rotation.QuaternionToEuler();
                        _euler = _eulerUncalibrated + _nunchuckRotationCalibration;
                        _gyro = _sensorOrientation.GyroData;
                        _acceleration = _sensorOrientation.AccelerometerData;

                        if (GenericControllerTrackerManager.DebugOpen) {
                            _debug +=
                            $"\r\n\r\nNunchuck" +
                            $"Euler Rotation Uncalibrated:\r\n" +
                            $"X:{_eulerUncalibrated.X}, Y:{_eulerUncalibrated.Y}, Z:{_eulerUncalibrated.Z}" +
                            $"\r\nEuler Rotation:\r\n" +
                            $"X:{_euler.X}, Y:{_euler.Y}, Z:{_rotation.Z}" +
                            $"\r\nGyro:\r\n" +
                            $"X:{_gyro.X}, Y:{_gyro.Y}, Z:{_gyro.Z}" +
                            $"\r\nAcceleration:\r\n" +
                            $"X:{_acceleration.X}, Y:{_acceleration.Y}, Z:{_acceleration.Z}\r\n" +
                            $"Yaw Reference Rotation:\r\n" +
                            $"Y:{trackerEuler}\r\n";
                        }
                        finalX = _euler.X;
                        await udpHandler.SetSensorRotation(new Vector3(finalX, finalY, _lastEulerPositon).ToQuaternion(), 1);
                    }


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
            await Task.Delay(5000);
            _calibratedHeight = OpenVRReader.GetHMDHeight();
            var value = _motionStateList.ElementAt(_index);
            _rotation = new Quaternion(value.Value.WiimoteQuatX, value.Value.WiimoteQuatY, value.Value.WiimoteQuatZ, value.Value.WiimoteQuatW);
            _wiimoteRotationCalibration = -_rotation.QuaternionToEuler();

            _rotation = new Quaternion(value.Value.NunchukQuatX, value.Value.NunchukQuatY, value.Value.NunchukQuatZ, value.Value.NunchukQuatW);
            _nunchuckRotationCalibration = -_rotation.QuaternionToEuler();

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
            throw new NotImplementedException();
        }

        public void Identify() {
            EngageHaptics(300);
        }

        public void EngageHaptics(int duration, bool timed = true) {
            Task.Run(() => {
                if (!isAlreadyVibrating) {
                    isAlreadyVibrating = true;
                    ForwardedWiimoteManager.RumbleState[_index] = 1;
                    if (timed) {
                        Thread.Sleep(duration);
                        ForwardedWiimoteManager.RumbleState[_index] = 0;
                        isAlreadyVibrating = false;
                    }
                }
            });
        }
        public void DisableHaptics() {
            isAlreadyVibrating = false;
            ForwardedWiimoteManager.RumbleState[_index] = 0;
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
        public HapticNodeBinding HapticNodeBinding { get => _hapticNodeBinding; set => _hapticNodeBinding = value; }
    }
}