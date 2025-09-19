using Everything_To_IMU_SlimeVR.Osc;
using SlimeImuProtocol.SlimeVR;
using System.Net.Sockets;

namespace Everything_To_IMU_SlimeVR.Tracking
{
    public class GenericTrackerManager
    {
        private static List<IBodyTracker> _allTrackers = new List<IBodyTracker>();
        private static List<GenericControllerTracker> _trackersBluetooth = new List<GenericControllerTracker>();
        private static List<ThreeDsControllerTracker> _trackers3ds = new List<ThreeDsControllerTracker>();
        private static List<WiiTracker> _trackersWiimote = new List<WiiTracker>();
        private static List<WiiTracker> _trackersNunchuck = new List<WiiTracker>();
        private static Dictionary<string, UDPHapticDevice> _trackersUdpHapticDevice = new Dictionary<string, UDPHapticDevice>();
        private static Dictionary<int, KeyValuePair<int, bool>> _trackerInfo = new Dictionary<int, KeyValuePair<int, bool>>();
        private static Dictionary<int, KeyValuePair<string, bool>> _trackerInfo3ds = new Dictionary<int, KeyValuePair<string, bool>>();
        private static Dictionary<string, KeyValuePair<string, bool>> _trackerInfoWiimote = new Dictionary<string, KeyValuePair<string, bool>>();
        private static Dictionary<string, KeyValuePair<int, bool>> _trackerInfoUdpHapticDevice = new Dictionary<string, KeyValuePair<int, bool>>();
        public static bool lockInDetectedDevices = false;
        private bool disposed = false;
        public event EventHandler<string> OnTrackerError;
        private int pollingRate = 8;
        Color[] colours = new Color[] {
                Color.Aqua,
                Color.Red,
                Color.Green,
                Color.Orange,
                Color.Blue,
                Color.Magenta,
                Color.DarkSeaGreen,
                Color.Yellow
            };
        private static int _controllerCount;
        private Configuration _configuration;
        private OscHandler _oscHandler;
        private FalseThighTracker _falseThighTrackerLeft;
        private FalseThighTracker _falseThighTrackerRight;
        private SlimeVRClient _solarXR;
        private int _pollingRatePerTracker;

        public GenericTrackerManager(Configuration configuration)
        {
            _configuration = configuration;
            _oscHandler = new OscHandler();
            _solarXR = new SlimeVRClient();
            _solarXR.NewDataReceived += delegate
            {
                foreach (var item in _solarXR.Trackers)
                {
                    if (!string.IsNullOrEmpty(item.Value.Ip))
                    {
                        if (item.Value.Firmware.Contains("Eros"))
                        {
                            AddRemoteHapticDevice(item.Value.Ip, item.Key);
                        }
                    }
                }
            };
            UDPHandler.Endpoint = "127.0.0.1";
            int handshakeDelay = _configuration.SwitchingSessions ? 10 : 100;
            Task.Run(async () =>
            {
                foreach (var item in _configuration.TrackerConfigUdpHaptics)
                {
                    AddRemoteHapticDevice(item.Key, "");
                }
                _solarXR.Start();
                while (!disposed)
                {
                    try
                    {
                        if (!lockInDetectedDevices)
                        {
                            _controllerCount = JSL.JslConnectDevices();
                            // Loop through currently connected controllers.
                            for (int i = 0; i < _controllerCount; i++)
                            {
                                // Track whether or not we've seen this controller before this session.
                                if (!_trackerInfo.ContainsKey(i))
                                {
                                    _trackerInfo[i] = new KeyValuePair<int, bool>(_trackersBluetooth.Count, false);
                                }

                                // Get this controllers information.
                                var info = _trackerInfo[i];

                                // Have we dealt with setting up this controller tracker yet?
                                if (!info.Value)
                                {
                                    // Set up the controller tracker.
                                    var newTracker = new GenericControllerTracker(info.Key, colours[info.Key]);
                                    while (!newTracker.Ready)
                                    {
                                        Thread.Sleep(100);
                                    }
                                    newTracker.OnTrackerError += NewTracker_OnTrackerError;
                                    if (i > _configuration.TrackerConfigs.Count - 1)
                                    {
                                        _configuration.TrackerConfigs.Add(new TrackerConfig());
                                    }
                                    newTracker.YawReferenceTypeValue = _configuration.TrackerConfigs[i].YawReferenceTypeValue;
                                    newTracker.ExtensionYawReferenceTypeValue = _configuration.TrackerConfigs[i].YawReferenceTypeValue;
                                    newTracker.HapticNodeBinding = _configuration.TrackerConfigs[i].HapticNodeBinding;
                                    _trackersBluetooth.Add(newTracker);
                                    _allTrackers.Add(newTracker);
                                    _trackerInfo[i] = new KeyValuePair<int, bool>(info.Key, true);
                                }
                            }
                            for (int i = 0; i < Forwarded3DSDataManager.DeviceMap.Count; i++)
                            {
                                string key = Forwarded3DSDataManager.DeviceMap.ElementAt(i).Key;
                                // Track whether or not we've seen this controller before this session.
                                if (!_trackerInfo3ds.ContainsKey(i))
                                {
                                    _trackerInfo3ds[i] = new KeyValuePair<string, bool>(key, false);
                                }

                                // Get this controllers information.
                                var info = _trackerInfo3ds[i];

                                // Have we dealt with setting up this controller tracker yet?
                                if (!info.Value)
                                {
                                    // Set up the controller tracker.
                                    var newTracker = new ThreeDsControllerTracker(key);
                                    while (!newTracker.Ready)
                                    {
                                        Thread.Sleep(100);
                                    }
                                    newTracker.OnTrackerError += NewTracker_OnTrackerError;
                                    if (i > _configuration.TrackerConfigs3ds.Count - 1)
                                    {
                                        _configuration.TrackerConfigs3ds.Add(new TrackerConfig());
                                    }
                                    newTracker.YawReferenceTypeValue = _configuration.TrackerConfigs3ds[i].YawReferenceTypeValue;
                                    newTracker.ExtensionYawReferenceTypeValue = _configuration.TrackerConfigs3ds[i].YawReferenceTypeValue;
                                    _trackers3ds.Add(newTracker);
                                    _allTrackers.Add(newTracker);
                                    _trackerInfo3ds[i] = new KeyValuePair<string, bool>(key, true);
                                }
                            }
                            for (int i = 0; i < ForwardedWiimoteManager.Wiimotes.Count; i++)
                            {
                                // Track whether or not we've seen this controller before this session.
                                string key = ForwardedWiimoteManager.Wiimotes.ElementAt(i).Key;
                                if (!_trackerInfoWiimote.ContainsKey(key))
                                {
                                    _trackerInfoWiimote[key] = new KeyValuePair<string, bool>(key, false);
                                }

                                // Get this controllers information.
                                var info = _trackerInfoWiimote[key];

                                // Have we dealt with setting up this controller tracker yet?
                                if (!info.Value)
                                {
                                    // Set up the controller tracker.
                                    var newTracker = new WiiTracker(info.Key);
                                    while (!newTracker.Ready)
                                    {
                                        Thread.Sleep(100);
                                    }
                                    newTracker.OnTrackerError += NewTracker_OnTrackerError;
                                    if (!_configuration.TrackerConfigWiimote.ContainsKey(key))
                                    {
                                        _configuration.TrackerConfigWiimote.Add(key, new TrackerConfig());
                                    }
                                    newTracker.YawReferenceTypeValue = _configuration.TrackerConfigWiimote[key].YawReferenceTypeValue;
                                    newTracker.ExtensionYawReferenceTypeValue = _configuration.TrackerConfigWiimote[key].ExtensionYawReferenceTypeValue;
                                    newTracker.HapticNodeBinding = _configuration.TrackerConfigWiimote[key].HapticNodeBinding;
                                    _trackersWiimote.Add(newTracker);
                                    _allTrackers.Add(newTracker);
                                    _trackerInfoWiimote[key] = new KeyValuePair<string, bool>(key, true);
                                }
                            }
                        }
                        Thread.Sleep(10000);
                    } catch (Exception e)
                    {
                        OnTrackerError?.Invoke(this, e.Message);
                    }
                }
            });
            Task.Run(async () =>
            {
                while (true)
                {
                    // Loop through all the controller based trackers.
                    for (int i = 0; i < _trackersBluetooth.Count; i++)
                    {
                        var tracker = _trackersBluetooth[i];
                        // Remove tracker if its been disconnected.
                        if (tracker.Disconnected)
                        {
                            var info = _trackerInfo[i];
                            _trackerInfo[i] = new KeyValuePair<int, bool>(info.Key, false);
                            _trackersBluetooth.RemoveAt(i);
                            i = 0;
                            tracker.Dispose();
                        }
                    }
                    for (int i = 0; i < _trackers3ds.Count; i++)
                    {
                        var tracker = _trackers3ds[i];
                        // Remove tracker if its been disconnected.
                        if (tracker.Disconnected)
                        {
                            var info = _trackerInfo3ds[i];
                            _trackerInfo3ds[i] = new KeyValuePair<string, bool>(info.Key, false);
                            _trackers3ds.RemoveAt(i);
                            i = 0;
                            tracker.Dispose();
                        }
                    }
                    Thread.Sleep(10000);
                }
            });
        }

        private void NewTracker_OnTrackerError(object? sender, string e)
        {
            OnTrackerError.Invoke(sender, e);
        }

        public void AddRemoteHapticDevice(string ip, string alias)
        {
            // Track whether or not we've seen this controller before this session.
            if (!_trackerInfoUdpHapticDevice.ContainsKey(ip))
            {
                _trackerInfoUdpHapticDevice[ip] = new KeyValuePair<int, bool>(_trackerInfoUdpHapticDevice.Count, false);
            }

            // Get this controllers information.
            var info = _trackerInfoUdpHapticDevice[ip];

            // Have we dealt with setting up this controller tracker yet?
            if (!info.Value)
            {
                // Set up the controller tracker.
                var newTracker = new UDPHapticDevice(ip, alias);
                if (!_configuration.TrackerConfigUdpHaptics.ContainsKey(ip))
                {
                    _configuration.TrackerConfigUdpHaptics.Add(ip, new TrackerConfig() { HapticNodeBinding = GetDefaultBinding(alias) });
                }
                newTracker.YawReferenceTypeValue = _configuration.TrackerConfigUdpHaptics[ip].YawReferenceTypeValue;
                newTracker.HapticNodeBinding = _configuration.TrackerConfigUdpHaptics[ip].HapticNodeBinding;
                _trackersUdpHapticDevice[ip] = newTracker;
                _allTrackers.Add(newTracker);
                _trackerInfoUdpHapticDevice[ip] = new KeyValuePair<int, bool>(info.Key, true);
            } else
            {
                try
                {
                    _trackersUdpHapticDevice[ip].Alias = alias;
                } catch
                {

                }
            }
        }

        public void HapticTest()
        {
            Task.Run(() =>
            {
                foreach (var tracker in _allTrackers)
                {
                    tracker.Identify();
                    Thread.Sleep(1000);
                }
            });
        }

        public HapticNodeBinding GetDefaultBinding(string alias)
        {
            switch (alias)
            {
                case "HEAD":
                    return HapticNodeBinding.Head;
                case "UPPER_CHEST":
                case "CHEST":
                    return HapticNodeBinding.Chest;
                case "WAIST":
                case "HIP":
                case "LEFT_HIP":
                case "RIGHT_HIP":
                    return HapticNodeBinding.Hips;
                case "LEFT_UPPER_LEG":
                    return HapticNodeBinding.LeftThigh;
                case "RIGHT_UPPER_LEG":
                    return HapticNodeBinding.RightThigh;
                case "LEFT_LOWER_LEG":
                    return HapticNodeBinding.LeftCalf;
                case "RIGHT_LOWER_LEG":
                    return HapticNodeBinding.RightCalf;
                case "RIGHT_FOOT":
                    return HapticNodeBinding.RightFoot;
                case "LEFT_FOOT":
                    return HapticNodeBinding.LeftFoot;
                case "LEFT_SHOULDER":
                    return HapticNodeBinding.LeftShoulder;
                case "RIGHT_SHOULDER":
                    return HapticNodeBinding.RightShoulder;
                case "LEFT_UPPER_ARM":
                    return HapticNodeBinding.LeftUpperArm;
                case "RIGHT_LOWER_ARM":
                    return HapticNodeBinding.RightUpperArm;
                case "RIGHT_HAND":
                    return HapticNodeBinding.RightHand;
                case "LEFT_HAND":
                    return HapticNodeBinding.LeftHand;
                default:
                    return HapticNodeBinding.Chest;
            }
        }

        internal static List<GenericControllerTracker> TrackersBluetooth { get => _trackersBluetooth; set => _trackersBluetooth = value; }
        public static int ControllerCount { get => _controllerCount; set => _controllerCount = value; }
        public int PollingRate { get => pollingRate; set => pollingRate = value; }
        public static bool DebugOpen { get; set; }
        public static List<ThreeDsControllerTracker> Trackers3ds { get => _trackers3ds; set => _trackers3ds = value; }
        public Dictionary<int, KeyValuePair<string, bool>> TrackerInfo3ds { get => _trackerInfo3ds; set => _trackerInfo3ds = value; }
        public static List<WiiTracker> TrackersWiimote { get => _trackersWiimote; set => _trackersWiimote = value; }
        public static List<WiiTracker> TrackersNunchuck { get => _trackersNunchuck; set => _trackersNunchuck = value; }
        public Dictionary<string, KeyValuePair<string, bool>> TrackerInfoWiimote { get => _trackerInfoWiimote; set => _trackerInfoWiimote = value; }
        public static List<IBodyTracker> AllTrackers { get => _allTrackers; set => _allTrackers = value; }
        public static Dictionary<string, UDPHapticDevice> TrackersUdpHapticDevice { get => _trackersUdpHapticDevice; set => _trackersUdpHapticDevice = value; }
        internal FalseThighTracker FalseThighTrackerLeft { get => _falseThighTrackerLeft; set => _falseThighTrackerLeft = value; }
        internal FalseThighTracker FalseThighTrackerRight { get => _falseThighTrackerRight; set => _falseThighTrackerRight = value; }
    }
}
