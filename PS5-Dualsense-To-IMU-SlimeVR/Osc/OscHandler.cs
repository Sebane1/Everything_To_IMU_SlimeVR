// Implementation borrowed from https://github.com/ButterscotchV/AXSlime

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Everything_To_IMU_SlimeVR.AudioHaptics;
using Everything_To_IMU_SlimeVR.Osc;
using LucHeart.CoreOSC;

namespace Everything_To_IMU_SlimeVR.Osc
{
    public class OscHandler : IDisposable
    {
        public static readonly string BundleAddress = "#bundle\0";
        public static readonly byte[] BundleAddressBytes = Encoding.ASCII.GetBytes(BundleAddress);
        public static readonly string AvatarParamPrefix = "/avatar/parameters/";
        public static List<string> parameterList = new List<string>();

        private UdpClient _oscClient;

        private CancellationTokenSource _cancelTokenSource = new();
        private Task _oscReceiveTask;

        private readonly AxHaptics _axHaptics;
        private readonly bHaptics _bHaptics;

        private readonly HapticsSource[] _hapticsSources;
        private readonly VRCHapticLogManager _vrcHapticLogManager;
        private ConcurrentDictionary<int, UdpClient> _portToUdpClientDictionary = new ConcurrentDictionary<int, UdpClient>();
        public OscHandler()
        {

            _axHaptics = new();
            _bHaptics = new();

            _hapticsSources = [_axHaptics, _bHaptics];

            _vrcHapticLogManager = new VRCHapticLogManager();
            var monitor = new DesktopAudioHapticMonitor(
                          new AudioHapticsAdapter(), fftSize: 1024, lowBandMaxHz: 200f, highBandMinHz: 5000f,
                                                   onThresholdDb: -24f, offThresholdDb: -38f, stereoDbBias: 3f);
            monitor.Start();
            RefreshOSCPort();
        }

        public void RefreshOSCPort()
        {
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource = new CancellationTokenSource();
            }
            if (_oscClient != null)
            {
                _oscClient.Dispose();
            }
            _portToUdpClientDictionary.Clear();
            var port = int.Parse(Configuration.Instance.PortInput);
            if (!Configuration.Instance.PortOutputs.Contains(port))
            {
                _oscClient = new UdpClient(port);
                Task.Run(() =>
                {
                    _oscReceiveTask = OscReceiveTask(_cancelTokenSource.Token);
                });
            }
        }

        private static bool IsBundle(ReadOnlySpan<byte> buffer)
        {
            return true;
        }
        /// <summary>
        /// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Bundle containing elements and a timetag</returns>
        public static OscBundle ParseBundle(Span<byte> msg)
        {
            ReadOnlySpan<byte> msgReadOnly = msg;
            var messages = new List<OscMessage>();

            var index = 0;

            var messageBytes = msg.Slice(index, msgReadOnly.Length - index);
            var message = OscMessage.ParseMessage(messageBytes);
            messages.Add(message);

            var output = new OscBundle((ulong)DateTime.Now.Ticks, messages.ToArray());
            return output;
        }
        private async Task OscReceiveTask(CancellationToken cancelToken = default)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var packet = await _oscClient.ReceiveAsync();
                    if (IsBundle(packet.Buffer))
                    {
                        var bundle = ParseBundle(packet.Buffer);
                        if (bundle.Timestamp > DateTime.Now)
                        {
                            // Wait for the specified timestamp
                            _ = Task.Run(
                                async () =>
                                {
                                    await Task.Delay(bundle.Timestamp - DateTime.Now, cancelToken);
                                    OnOscBundle(bundle);
                                },
                                cancelToken
                            );
                        } else
                        {
                            OnOscBundle(bundle);
                        }
                    } else
                    {
                        OnOscMessage(OscMessage.ParseMessage(packet.Buffer));
                    }
                    foreach (var outputPort in Configuration.Instance.PortOutputs)
                    {
                        UdpClient udpClient = null;
                        if (!_portToUdpClientDictionary.ContainsKey(outputPort))
                        {
                            udpClient = new UdpClient();
                            udpClient.Connect(Configuration.Instance.OscIpAddress, outputPort);
                            _portToUdpClientDictionary[outputPort] = udpClient;
                        } else
                        {
                            udpClient = _portToUdpClientDictionary[outputPort];
                        }
                        _portToUdpClientDictionary[outputPort].SendAsync(packet.Buffer, packet.Buffer.Length);
                    }
                } catch (OperationCanceledException) { } catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        private void OnOscBundle(OscBundle bundle)
        {
            foreach (var message in bundle.Messages)
            {
                OnOscMessage(message);
            }
        }

        private void OnOscMessage(OscMessage message)
        {
            if (message.Arguments.Length <= 0)
            {
                return;
            }
            var events = ComputeEvents(message);
            foreach (var hapticEvent in events)
            {
                HapticsManager.SetNodeVibration(hapticEvent.Node, 300, (float)hapticEvent.Intensity);
            }
            //if (events.Length == 0 && HapticsManager.HapticsEngaged) {
            //    HapticsManager.StopNodeVibrations();
            //}
        }

        private HapticEvent[] ComputeEvents(OscMessage message)
        {
            if (message.Address.Length <= AvatarParamPrefix.Length)
            {
                return [];
            }

            var param = message.Address[AvatarParamPrefix.Length..];
            if (!parameterList.Contains(param))
            {
                parameterList.Add(param);
                Debug.WriteLine(param);
            }
            foreach (var source in _hapticsSources)
            {
                if (source.IsSource(param, message))
                {
                    return source.ComputeHaptics(param, message);
                }
            }

            return [];
        }

        public void Dispose()
        {
            _cancelTokenSource.Cancel();
            _oscReceiveTask.Wait();
            _cancelTokenSource.Dispose();
            _oscClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
