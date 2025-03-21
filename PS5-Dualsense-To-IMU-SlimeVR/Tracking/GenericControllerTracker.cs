using PS5_Dualsense_To_IMU_SlimeVR.SlimeVR;
using System;
using System.Numerics;

using Valve.VR;
using OVRSharp.Math;
using System.Diagnostics;
namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class GenericControllerTracker : IDisposable, IBodyTracker {
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
        private FalseThighTracker _falseThighTracker;
        private float _lastHmdPositon;
        private Quaternion _rotation;
        private Vector3 _euler;
        private Vector3 _gyro;
        private Vector3 _acceleration;
        private bool _waitForRelease;
        private string _rememberedStringId;

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
                    _sensorOrientation = new SensorOrientation(index);
                    if (_simulateThighs) {
                        _falseThighTracker = new FalseThighTracker(this);
                    }
                    udpHandler = new UDPHandler("GenericController" + _rememberedStringId, _id,
                     new byte[] { (byte)macSpoof[0], (byte)macSpoof[1], (byte)macSpoof[2], (byte)macSpoof[3], (byte)macSpoof[4], (byte)macSpoof[5] });
                    Recalibrate();
                    _ready = true;
                } catch (Exception e) {
                    OnTrackerError?.Invoke(this, e.Message);
                }
            });
        }
        public bool GetGlobalState(int code) {
            int connections = GenericControllerTrackerManager.ControllerCount;
            for (int i = 0; i < connections; i++) {
                var buttons = JSL.JslGetSimpleState(i).buttons;
                if ((buttons & code) != 0) {
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> Update() {
            if (_ready) {
                try {
                    var hmdHeight = HmdReader.GetHMDHeight();
                    bool sitting = hmdHeight < _calibratedHeight / 2;
                    var hmdRotation = HmdReader.GetHMDRotation();
                    float hmdEuler = hmdRotation.GetYawFromQuaternion();
                    if (!sitting || GetGlobalState(0x08000)) {
                        _lastHmdPositon = -hmdEuler;
                    }
                    var motionState = JSL.JslGetMotionState(_index);
                    var motionQuaternion = new Quaternion(motionState.quatX, motionState.quatY, motionState.quatZ, motionState.quatW);

                    _rotation = !_simulateThighs ? motionQuaternion : (_sensorOrientation.CurrentOrientation);
                    _euler = _rotation.QuaternionToEuler() + (!_simulateThighs ? new Vector3() : _rotationCalibration);
                    _gyro = _sensorOrientation.GyroData;
                    _acceleration = _sensorOrientation.AccelerometerData;
                    _debug =
                    $"Device Id: {macSpoof}\r\n" +
                    $"Euler Rotation:\r\n" +
                    $"X:{_euler.X}, Y:{_euler.Y}, Z:{_rotation.Z}" +
                    $"\r\nGyro:\r\n" +
                    $"X:{_gyro.X}, Y:{_gyro.Y}, Z:{_gyro.Z}" +
                    $"\r\nAcceleration:\r\n" +
                    $"X:{_acceleration.X}, Y:{_acceleration.Y}, Z:{_acceleration.Z}\r\n" +
                    $"HMD Rotation:\r\n" +
                    $"Y:{hmdEuler}\r\n"
                    + _falseThighTracker.Debug;
                    var buttons = JSL.JslGetSimpleState(_index).buttons;
                    if ((buttons & 0x20000) != 0) {
                        if (!_waitForRelease) {
                            _waitForRelease = true;
                            Recalibrate();
                        }
                    } else {
                        _waitForRelease = false;
                    }
                    await udpHandler.SetSensorBattery(100);
                    if (!_simulateThighs) {
                        await udpHandler.SetSensorRotation(new Vector3(_euler.X, _euler.Z, _euler.Y).ToQuaternion());
                    } else {
                        float finalY = _euler.Y;
                        float finalZ = sitting ? _euler.Z : _euler.Z;
                        await udpHandler.SetSensorRotation((new Vector3(-_euler.X, finalY, finalZ + _lastHmdPositon)).ToQuaternion());
                        await _falseThighTracker.Update();
                    }
                } catch (Exception e) {
                    OnTrackerError.Invoke(this, e.Message);
                }
            }
            return _ready;
        }
        public async void Recalibrate() {
            await Task.Delay(5000);
            JSL.JslResetContinuousCalibration(_index);
            _calibratedHeight = HmdReader.GetHMDHeight();
            _rotationCalibration = -(_sensorOrientation.CurrentOrientation).QuaternionToEuler();
            await udpHandler.SendButton();
        }
        public void Dispose() {
            _ready = false;
            _disconnected = true;
            _falseThighTracker?.Dispose();
        }

        public string Debug { get => _debug; set => _debug = value; }
        public bool Ready { get => _ready; set => _ready = value; }
        public bool Disconnected { get => _disconnected; set => _disconnected = value; }
        public int Id { get => _id; set => _id = value; }
        public string MacSpoof { get => macSpoof; set => macSpoof = value; }
        public Vector3 Euler { get => _euler; set => _euler = value; }
        public Vector3 Gyro { get => _gyro; set => _gyro = value; }
        public Vector3 Acceleration { get => _acceleration; set => _acceleration = value; }
        public float LastHmdPositon { get => _lastHmdPositon; set => _lastHmdPositon = value; }
        public bool SimulateThighs { get => _simulateThighs; set => _simulateThighs = value; }
    }
}