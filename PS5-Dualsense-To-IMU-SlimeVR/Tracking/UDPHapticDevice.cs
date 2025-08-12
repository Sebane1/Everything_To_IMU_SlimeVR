using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using static Everything_To_IMU_SlimeVR.TrackerConfig;
using Everything_To_IMU_SlimeVR.SlimeVR;

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class UDPHapticDevice : IBodyTracker {
        private HapticNodeBinding _hapticNodeBinding;
        private PacketBuilder _packetBuilder;
        private string _ipAddress;
        private UdpClient _udpServer;
        private IPEndPoint _clientEndPoint;
        private bool isAlreadyVibrating;
        private RotationReferenceType _extensionYawReferenceTypeValue;
        DateTime _hapticEndTime;
        public UDPHapticDevice(string ipAddress) {
            _packetBuilder = new PacketBuilder("");
            // Set up UDP server
            _ipAddress = ipAddress;
            _clientEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), 6969);
            Task.Run(() => {
                _udpServer = new UdpClient();
                _udpServer.Connect(ipAddress, 6969);
                Ready = true;
            });
        }

        public int Id { get; set; }
        public string MacSpoof { get; set; }
        public Vector3 Euler { get; set; }
        public float LastHmdPositon { get; set; }
        public bool SimulateThighs { get; set; }
        public HapticNodeBinding HapticNodeBinding { get => _hapticNodeBinding; set => _hapticNodeBinding = value; }
        public TrackerConfig.RotationReferenceType YawReferenceTypeValue { get; set; }

        public string Debug => "Use owotrack to get SlimeVR Tracking on cellphones. Only haptics are provided by this application.";

        public bool Ready { get; set; }
        public RotationReferenceType ExtensionYawReferenceTypeValue { get => _extensionYawReferenceTypeValue; set => _extensionYawReferenceTypeValue = value; }
        public Vector3 RotationCalibration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool SupportsHaptics => true;

        public bool SupportsIMU => false;

        public void DisableHaptics() {
            var data = _packetBuilder.BuildHapticPacket(0, 0);
            _udpServer.Send(data, data.Length);
            _hapticEndTime = new DateTime();
        }

        public void EngageHaptics(int duration, float intensity) {
            _hapticEndTime = DateTime.Now.AddMilliseconds(duration);
            if (!isAlreadyVibrating) {
                isAlreadyVibrating = true;
                Task.Run(() => {
                    var data = _packetBuilder.BuildHapticPacket(intensity, duration);
                    _udpServer.Send(data, data.Length);
                    while (DateTime.Now < _hapticEndTime) {
                        Thread.Sleep(10);
                    }
                    isAlreadyVibrating = false;
                    data = _packetBuilder.BuildHapticPacket(0, 0);
                    _udpServer.Send(data, data.Length);
                });
            }
        }

        public Vector3 GetCalibration() {
            return new Vector3();
        }

        public void Identify() {
            EngageHaptics(300, 1);
        }

        public void Rediscover() {
            MessageBox.Show(Debug);
        }

        public override string ToString() {
            return _ipAddress;
        }
    }
}
