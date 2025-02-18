using AQuestReborn;
using System;
using System.Numerics;
using Wujek_Dualsense_API;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    internal class DualsenseTracker {
        private string _debug;
        private int _index;
        private int _id;
        private SensorOrientation sensorOrientation;
        private string macSpoof;
        private UDPHandler udpHandler;
        private Vector3 rotationCalibration;
        private bool _ready;
        private Dualsense dualsense;

        public DualsenseTracker(int index, string dualsenseId, Color colour) {
            Initialize(index, dualsenseId, colour);
        }
        public async void Initialize(int index, string dualsenseId, Color colour) {
            Task.Run(async () => {
                _index = index;
                _id = index + 1;
                var rememberedColour = colour;
                var rememberedDualsenseId = dualsenseId;
                dualsense = new Dualsense(rememberedDualsenseId);
                dualsense.Start();
                dualsense.SetLightbar(rememberedColour.R, rememberedColour.G, rememberedColour.B);
                sensorOrientation = new SensorOrientation(dualsense);
                await Task.Delay(10000);
                macSpoof = dualsense.DeviceID.Split("&")[3];
                udpHandler = new UDPHandler("Dualsense5" + _id, _id,
                 new byte[] { (byte)macSpoof[0], (byte)macSpoof[1], (byte)macSpoof[2], (byte)macSpoof[3], (byte)macSpoof[4], (byte)macSpoof[5] });
                rotationCalibration = -sensorOrientation.CurrentOrientation.QuaternionToEuler();
                _ready = true;
            });
        }
        public async Task<bool> Update() {
            if (_ready) {
                Vector3 euler = sensorOrientation.CurrentOrientation.QuaternionToEuler() + rotationCalibration;
                Vector3 gyro = sensorOrientation.GyroData;
                Vector3 acceleration = sensorOrientation.AccelerometerData;
                _debug =
                $"Device Id: {macSpoof}\r\n" +
                $"Quaternion Rotation:\r\n" +
                $"X:{sensorOrientation.CurrentOrientation.X}, " +
                $"Y:{sensorOrientation.CurrentOrientation.Y}, " +
                $"Z:{sensorOrientation.CurrentOrientation.Z}, " +
                $"W:{sensorOrientation.CurrentOrientation.W}\r\n" +
                $"Euler Rotation:\r\n" +
                $"X:{euler.X}, Y:{euler.Y}, Z:{euler.Z}" +
                $"\r\nGyro:\r\n" +
                $"X:{gyro.X}, Y:{gyro.Y}, Z:{gyro.Z}" +
                $"\r\nAcceleration:\r\n" +
                $"X:{acceleration.X}, Y:{acceleration.Y}, Z:{acceleration.Z}"; ;

                await udpHandler.SetSensorRotation(sensorOrientation.CurrentOrientation + rotationCalibration.ToQuaternion());
                await udpHandler.SetSensorGyro(gyro);
                await udpHandler.SetSensorAccelleration(acceleration);
                await udpHandler.SetSensorBattery((float)dualsense.Battery.Level / 100f);
                Thread.Sleep(16);
            }
            return _ready;
        }

        public string Debug { get => _debug; set => _debug = value; }
        public bool Ready { get => _ready; set => _ready = value; }
    }
}