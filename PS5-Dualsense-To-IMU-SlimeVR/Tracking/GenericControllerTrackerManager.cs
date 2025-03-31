namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    public class GenericControllerTrackerManager {
        private List<GenericControllerTracker> _trackers = new List<GenericControllerTracker>();
        private Dictionary<int, KeyValuePair<int, bool>> _trackerInfo = new Dictionary<int, KeyValuePair<int, bool>>();
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
                                _trackers.Add(newTracker);
                                _trackerInfo[i] = new KeyValuePair<int, bool>(info.Key, true);
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
    }
}
