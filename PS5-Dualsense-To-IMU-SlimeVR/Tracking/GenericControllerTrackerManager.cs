namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    public class GenericControllerTrackerManager {
        private List<GenericControllerTracker> _trackers = new List<GenericControllerTracker>();
        private List<ThreeDsControllerTracker> _trackers3ds = new List<ThreeDsControllerTracker>();
        private List<WiiTracker> _trackersWiimote = new List<WiiTracker>();
        private List<WiiTracker> _trackersNunchuck = new List<WiiTracker>();
        private Dictionary<int, KeyValuePair<int, bool>> _trackerInfo = new Dictionary<int, KeyValuePair<int, bool>>();
        private Dictionary<int, KeyValuePair<int, bool>> _trackerInfo3ds = new Dictionary<int, KeyValuePair<int, bool>>();
        private Dictionary<int, KeyValuePair<int, bool>> _trackerInfoWiimote = new Dictionary<int, KeyValuePair<int, bool>>();
        private Dictionary<int, KeyValuePair<int, bool>> _trackerInfoNunchuck = new Dictionary<int, KeyValuePair<int, bool>>();

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
        private int _pollingRatePerTracker;

        public GenericControllerTrackerManager(Configuration configuration) {
            _configuration = configuration;
            Task.Run(async () => {
                while (!disposed) {
                    try {
                        _controllerCount = JSL.JslConnectDevices();
                        // Loop through currently connected controllers.
                        for (int i = 0; i < _controllerCount; i++) {
                            // Track whether or not we've seen this controller before this session.
                            if (!_trackerInfo.ContainsKey(i)) {
                                _trackerInfo[i] = new KeyValuePair<int, bool>(_trackers.Count, false);
                            }

                            // Get this controllers information.
                            var info = _trackerInfo[i];

                            // Have we dealt with setting up this controller tracker yet?
                            if (!info.Value) {
                                // Set up the controller tracker.
                                var newTracker = new GenericControllerTracker(info.Key, colours[info.Key]);
                                while (!newTracker.Ready) {
                                    Thread.Sleep(100);
                                }
                                newTracker.OnTrackerError += NewTracker_OnTrackerError;
                                if (i > _configuration.TrackerConfigs.Count - 1) {
                                    _configuration.TrackerConfigs.Add(new TrackerConfig());
                                }
                                newTracker.SimulateThighs = _configuration.TrackerConfigs[i].SimulatesThighs;
                                newTracker.YawReferenceTypeValue = _configuration.TrackerConfigs[i].YawReferenceTypeValue;
                                _trackers.Add(newTracker);
                                _trackerInfo[i] = new KeyValuePair<int, bool>(info.Key, true);
                            }
                            Thread.Sleep(500);
                        }
                        for (int i = 0; i < Forwarded3DSDataManager.DeviceMap.Count; i++) {
                            // Track whether or not we've seen this controller before this session.
                            if (!_trackerInfo3ds.ContainsKey(i)) {
                                _trackerInfo3ds[i] = new KeyValuePair<int, bool>(_trackerInfo3ds.Count, false);
                            }

                            // Get this controllers information.
                            var info = _trackerInfo3ds[i];

                            // Have we dealt with setting up this controller tracker yet?
                            if (!info.Value) {
                                // Set up the controller tracker.
                                var newTracker = new ThreeDsControllerTracker(info.Key);
                                while (!newTracker.Ready) {
                                    Thread.Sleep(100);
                                }
                                newTracker.OnTrackerError += NewTracker_OnTrackerError;
                                if (i > _configuration.TrackerConfigs3ds.Count - 1) {
                                    _configuration.TrackerConfigs3ds.Add(new TrackerConfig());
                                }
                                newTracker.SimulateThighs = _configuration.TrackerConfigs3ds[i].SimulatesThighs;
                                newTracker.YawReferenceTypeValue = _configuration.TrackerConfigs3ds[i].YawReferenceTypeValue;
                                _trackers3ds.Add(newTracker);
                                _trackerInfo3ds[i] = new KeyValuePair<int, bool>(info.Key, true);
                            }
                            Thread.Sleep(500);
                        }
                        for (int i = 0; i < ForwardedWiimoteManager.Wiimotes.Count; i++) {
                            // Track whether or not we've seen this controller before this session.
                            if (!_trackerInfoWiimote.ContainsKey(i)) {
                                _trackerInfoWiimote[i] = new KeyValuePair<int, bool>(_trackerInfoWiimote.Count, false);
                            }

                            // Get this controllers information.
                            var info = _trackerInfoWiimote[i];

                            // Have we dealt with setting up this controller tracker yet?
                            if (!info.Value) {
                                // Set up the controller tracker.
                                var newTracker = new WiiTracker(info.Key, false);
                                while (!newTracker.Ready) {
                                    Thread.Sleep(100);
                                }
                                newTracker.OnTrackerError += NewTracker_OnTrackerError;
                                if (i > _configuration.TrackerConfigWiimote.Count - 1) {
                                    _configuration.TrackerConfigWiimote.Add(new TrackerConfig());
                                }
                                newTracker.SimulateThighs = _configuration.TrackerConfigWiimote[i].SimulatesThighs;
                                newTracker.YawReferenceTypeValue = _configuration.TrackerConfigWiimote[i].YawReferenceTypeValue;
                                _trackersWiimote.Add(newTracker);
                                _trackerInfoWiimote[i] = new KeyValuePair<int, bool>(info.Key, true);
                            }
                            Thread.Sleep(500);
                        }
                        for (int i = 0; i < ForwardedWiimoteManager.Nunchucks.Count; i++) {
                            // Track whether or not we've seen this controller before this session.
                            if (!_trackerInfoNunchuck.ContainsKey(i)) {
                                _trackerInfoNunchuck[i] = new KeyValuePair<int, bool>(_trackerInfoNunchuck.Count, false);
                            }

                            // Get this controllers information.
                            var info = _trackerInfoNunchuck[i];

                            // Have we dealt with setting up this controller tracker yet?
                            if (!info.Value) {
                                // Set up the controller tracker.
                                var newTracker = new WiiTracker(info.Key, true);
                                while (!newTracker.Ready) {
                                    Thread.Sleep(100);
                                }
                                newTracker.OnTrackerError += NewTracker_OnTrackerError;
                                if (i > _configuration.TrackerConfigNunchuck.Count - 1) {
                                    _configuration.TrackerConfigNunchuck.Add(new TrackerConfig());
                                }
                                newTracker.SimulateThighs = _configuration.TrackerConfigNunchuck[i].SimulatesThighs;
                                newTracker.YawReferenceTypeValue = _configuration.TrackerConfigNunchuck[i].YawReferenceTypeValue;
                                _trackersNunchuck.Add(newTracker);
                                _trackerInfoNunchuck[i] = new KeyValuePair<int, bool>(info.Key, true);
                            }
                            Thread.Sleep(500);
                        }
                        Thread.Sleep(2000);
                    } catch (Exception e) {
                        OnTrackerError?.Invoke(this, e.Message);
                    }
                }
            });
            Task.Run(async () => {
                while (true) {
                    // Loop through all the controller based trackers.
                    for (int i = 0; i < _trackers.Count; i++) {
                        var tracker = _trackers[i];
                        // Remove tracker if its been disconnected.
                        if (tracker.Disconnected) {
                            var info = _trackerInfo[i];
                            _trackerInfo[i] = new KeyValuePair<int, bool>(info.Key, false);
                            _trackers.RemoveAt(i);
                            i = 0;
                            tracker.Dispose();
                        } else {
                            // Update tracker.
                            await tracker.Update();
                        }
                    }
                    for (int i = 0; i < _trackers3ds .Count; i++) {
                        var tracker = _trackers3ds[i];
                        // Remove tracker if its been disconnected.
                        if (tracker.Disconnected) {
                            var info = _trackerInfo3ds[i];
                            _trackerInfo3ds[i] = new KeyValuePair<int, bool>(info.Key, false);
                            _trackers3ds.RemoveAt(i);
                            i = 0;
                            tracker.Dispose();
                        } else {
                            // Update tracker.
                            await tracker.Update();
                        }
                    }
                    for (int i = 0; i < _trackersWiimote.Count; i++) {
                        var tracker = _trackersWiimote[i];
                        // Remove tracker if its been disconnected.
                        if (tracker.Disconnected) {
                            var info = _trackerInfoWiimote[i];
                            _trackerInfoWiimote[i] = new KeyValuePair<int, bool>(info.Key, false);
                            _trackersWiimote.RemoveAt(i);
                            i = 0;
                            tracker.Dispose();
                        } else {
                            // Update tracker.
                            await tracker.Update();
                        }
                    }
                    for (int i = 0; i < _trackersNunchuck.Count; i++) {
                        var tracker = _trackersNunchuck[i];
                        // Remove tracker if its been disconnected.
                        if (tracker.Disconnected) {
                            var info = _trackerInfoNunchuck[i];
                            _trackerInfoNunchuck[i] = new KeyValuePair<int, bool>(info.Key, false);
                            _trackersNunchuck.RemoveAt(i);
                            i = 0;
                            tracker.Dispose();
                        } else {
                            // Update tracker.
                            await tracker.Update();
                        }
                    }
                    Thread.Sleep(pollingRate);
                }
            });
        }

        private void NewTracker_OnTrackerError(object? sender, string e) {
            OnTrackerError.Invoke(sender, e);
        }

        internal List<GenericControllerTracker> Trackers { get => _trackers; set => _trackers = value; }
        public static int ControllerCount { get => _controllerCount; set => _controllerCount = value; }
        public int PollingRate { get => pollingRate; set => pollingRate = value; }
        public static bool DebugOpen { get; set; }
        public List<ThreeDsControllerTracker> Trackers3ds { get => _trackers3ds; set => _trackers3ds = value; }
        public Dictionary<int, KeyValuePair<int, bool>> TrackerInfo3ds { get => _trackerInfo3ds; set => _trackerInfo3ds = value; }
        public List<WiiTracker> TrackersWiimote { get => _trackersWiimote; set => _trackersWiimote = value; }
        public List<WiiTracker> TrackersNunchuck { get => _trackersNunchuck; set => _trackersNunchuck = value; }
        public Dictionary<int, KeyValuePair<int, bool>> TrackerInfoWiimote { get => _trackerInfoWiimote; set => _trackerInfoWiimote = value; }
        public Dictionary<int, KeyValuePair<int, bool>> TrackerInfoNunchuck { get => _trackerInfoNunchuck; set => _trackerInfoNunchuck = value; }
    }
}
