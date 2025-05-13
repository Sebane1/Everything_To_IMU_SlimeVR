// Implementation borrowed from https://github.com/ButterscotchV/AXSlime

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Everything_To_IMU_SlimeVR;
using LucHeart.CoreOSC;

namespace AxSlime.Osc {
    public class OscHandler : IDisposable {
        public static readonly string BundleAddress = "#bundle\0";
        public static readonly byte[] BundleAddressBytes = Encoding.ASCII.GetBytes(BundleAddress);
        public static readonly string AvatarParamPrefix = "/avatar/parameters/";
        public static List<string> parameterList = new List<string>();

        private readonly UdpClient _oscClient;

        private readonly CancellationTokenSource _cancelTokenSource = new();
        private Task _oscReceiveTask;

        private readonly AxHaptics _axHaptics;
        private readonly bHaptics _bHaptics;

        private readonly HapticsSource[] _hapticsSources;

        public OscHandler() {
            _oscClient = new UdpClient(9001);

            _axHaptics = new();
            _bHaptics = new();

            _hapticsSources = [_axHaptics, _bHaptics];
           

            Task.Run(() => {
                _oscReceiveTask = OscReceiveTask(_cancelTokenSource.Token);
            });
        }

        private static bool IsBundle(ReadOnlySpan<byte> buffer) {
            return true;
        }
        /// <summary>
        /// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Bundle containing elements and a timetag</returns>
        public static OscBundle ParseBundle(Span<byte> msg) {
            ReadOnlySpan<byte> msgReadOnly = msg;
            var messages = new List<OscMessage>();

            var index = 0;

            //var bundleTag = Encoding.ASCII.GetString(msgReadOnly[..8]);
            //index += 8;

            //var timeTag = OscPacketUtils.GetULong(msgReadOnly, index);
            //index += 8;

                //var size = OscPacketUtils.GetInt(msgReadOnly, index);
                //index += 4;

                var messageBytes = msg.Slice(index, msgReadOnly.Length - index);
                var message = OscMessage.ParseMessage(messageBytes);
                messages.Add(message);

                //index += size;
                //while (index % 4 != 0)
                //    index++;

            var output = new OscBundle((ulong)DateTime.Now.Ticks, messages.ToArray());
            return output;
        }
        private async Task OscReceiveTask(CancellationToken cancelToken = default) {
            while (true) {
                try {
                    var packet = await _oscClient.ReceiveAsync();
                    if (IsBundle(packet.Buffer)) {
                        var bundle = ParseBundle(packet.Buffer);
                        if (bundle.Timestamp > DateTime.Now) {
                            // Wait for the specified timestamp
                            _ = Task.Run(
                                async () => {
                                    await Task.Delay(bundle.Timestamp - DateTime.Now, cancelToken);
                                    OnOscBundle(bundle);
                                },
                                cancelToken
                            );
                        } else {
                            OnOscBundle(bundle);
                        }
                    } else {
                        OnOscMessage(OscMessage.ParseMessage(packet.Buffer));
                    }
                } catch (OperationCanceledException) { } catch (Exception e) {
                    Debug.WriteLine(e);
                }
            }
        }

        private void OnOscBundle(OscBundle bundle) {
            foreach (var message in bundle.Messages) {
                OnOscMessage(message);
            }
        }

        private void OnOscMessage(OscMessage message) {
            if (message.Arguments.Length <= 0) {
                return;
            }
            var events = ComputeEvents(message);
            foreach (var hapticEvent in events) {
                HapticsManager.SetNodeVibration(hapticEvent.Node, 300);
            }
        }

        private HapticEvent[] ComputeEvents(OscMessage message) {
            if (message.Address.Length <= AvatarParamPrefix.Length) {
                return [];
            }

            var param = message.Address[AvatarParamPrefix.Length..];
            if (!parameterList.Contains(param)) {
                parameterList.Add(param);
                Debug.WriteLine(param);
            }
            foreach (var source in _hapticsSources) {
                if (source.IsSource(param, message)) {
                    return source.ComputeHaptics(param, message);
                }
            }

            return [];
        }

        public void Dispose() {
            _cancelTokenSource.Cancel();
            _oscReceiveTask.Wait();
            _cancelTokenSource.Dispose();
            _oscClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
