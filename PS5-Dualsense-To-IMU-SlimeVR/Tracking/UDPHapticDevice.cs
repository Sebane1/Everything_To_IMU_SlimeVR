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
using SlimeImuProtocol.SlimeVR;

namespace Everything_To_IMU_SlimeVR.Tracking
{
    public class UDPHapticDevice : IBodyTracker
    {
        private HapticNodeBinding _hapticNodeBinding;
        private PacketBuilder _packetBuilder;
        private string _ipAddress;
        private string _alias;
        private UdpClient _udpServer;
        private IPEndPoint _clientEndPoint;
        private bool isAlreadyVibrating;
        private RotationReferenceType _extensionYawReferenceTypeValue;
        DateTime _hapticEndTime;
        private float _lastIntensity;

        public UDPHapticDevice(string ipAddress, string alias)
        {
            _packetBuilder = new PacketBuilder("");
            // Set up UDP server
            _ipAddress = ipAddress;
            _alias = alias;
            _clientEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), 6969);
            _udpServer = new UdpClient();
            _udpServer.Connect(ipAddress, 6969);
            Ready = true;
        }

        public int Id { get; set; }
        public string MacSpoof { get; set; }
        public Vector3 Euler { get; set; }
        public float LastHmdPositon { get; set; }
        public bool SimulateThighs { get; set; }
        public HapticNodeBinding HapticNodeBinding { get => _hapticNodeBinding; set => _hapticNodeBinding = value; }
        public TrackerConfig.RotationReferenceType YawReferenceTypeValue { get; set; }

        public string Debug => "Wifi haptic devices do not provide tracking data to this software.";

        public bool Ready { get; set; }
        public RotationReferenceType ExtensionYawReferenceTypeValue { get => _extensionYawReferenceTypeValue; set => _extensionYawReferenceTypeValue = value; }
        public Vector3 RotationCalibration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool SupportsHaptics => true;

        public bool SupportsIMU => false;

        public string Alias { get => _alias; set => _alias = value; }
        public string IpAddress { get => _ipAddress; set => _ipAddress = value; }

        public void DisableHaptics()
        {
            var data = _packetBuilder.BuildHapticPacket(0, 0);
            _udpServer.SendAsync(data);
            _hapticEndTime = new DateTime();
        }

        public void EngageHaptics(int duration, float intensity)
        {
            _hapticEndTime = DateTime.Now.AddMilliseconds(duration);
            if (!isAlreadyVibrating || intensity != _lastIntensity)
            {
                Task.Run(() =>
                {
                    var data = _packetBuilder.BuildHapticPacket(intensity, duration);
                    _udpServer.SendAsync(data);
                });
                _lastIntensity = intensity;
            }
            if (!isAlreadyVibrating)
            {
                isAlreadyVibrating = true;
                Task.Run(() =>
                {
                    while (DateTime.Now < _hapticEndTime)
                    {
                        Thread.Sleep(10);
                    }
                    isAlreadyVibrating = false;
                    var data = _packetBuilder.BuildHapticPacket(0, 0);
                    _udpServer.SendAsync(data);
                });
            }
        }

        public Vector3 GetCalibration()
        {
            return new Vector3();
        }

        public void HapticIntensityTest()
        {
            for (byte i = 0; i < 255; i++)
            {
                EngageHaptics(50, i / 255f);
                Thread.Sleep(45);
            }
        }

        public void Identify()
        {
            EngageHaptics(300, 1);
        }

        public void Rediscover()
        {
            MessageBox.Show(Debug);
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(_alias) ? _alias : _ipAddress;
        }
    }
}
