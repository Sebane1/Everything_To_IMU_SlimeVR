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

namespace Everything_To_IMU_SlimeVR.Tracking {
    public class UDPHapticDevice : IBodyTracker {
        private HapticNodeBinding _hapticNodeBinding;
        private string _ipAddress;
        private UdpClient _udpServer;
        private IPEndPoint _clientEndPoint;
        private bool isAlreadyVibrating;

        public UDPHapticDevice(string ipAddress) {
            // Set up UDP server
            _ipAddress = ipAddress;
            Task.Run(() => {
                _udpServer = new UdpClient();
                _udpServer.Connect(ipAddress, 9005);
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

        public void DisableHaptics() {
        }

        public void EngageHaptics(int duration, bool timed = true) {
            if (!isAlreadyVibrating) {
                isAlreadyVibrating = true;
                Task.Run(() => {
                    string message = "1"; // This message will trigger vibration on the Android client
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    _udpServer.Send(data, data.Length, _clientEndPoint);
                    if (timed) {
                        Thread.Sleep(duration);
                        isAlreadyVibrating = false;
                    }
                });
            }
        }

        public Vector3 GetCalibration() {
            return new Vector3();
        }

        public void Identify() {
            EngageHaptics(300);
        }

        public void Rediscover() {
            MessageBox.Show(Debug);
        }

        public override string ToString() {
            return _ipAddress;
        }
    }
}
