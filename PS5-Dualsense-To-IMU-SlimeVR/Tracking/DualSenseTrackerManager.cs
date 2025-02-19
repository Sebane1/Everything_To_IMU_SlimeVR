using Wujek_Dualsense_API;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    public class DualSenseTrackerManager {
        private List<DualsenseTracker> _trackers;
        private Dictionary<string, KeyValuePair<int, bool>> _trackerInfo = new Dictionary<string, KeyValuePair<int, bool>>();
        public DualSenseTrackerManager() {
            Color[] colours = new Color[4] {
                Color.Aqua,
                Color.Red,
                Color.Green,
                Color.Orange
            };

            _trackers = new List<DualsenseTracker>();
            Task.Run(async () => {
                while (true) {
                    var controllers = DualsenseUtils.GetControllerIDs();
                    var controllerCount = controllers.Count();
                    for (int i = 0; i < controllerCount; i++) {
                        var dualsenseId = controllers[i];
                        if (!_trackerInfo.ContainsKey(dualsenseId)) {
                            _trackerInfo[dualsenseId] = new KeyValuePair<int, bool>(_trackers.Count, false);
                        }
                        var info = _trackerInfo[dualsenseId];
                        if (!info.Value) {
                            var newTracker = new DualsenseTracker(info.Key, dualsenseId, colours[info.Key]);
                            while (!newTracker.Ready) {
                                Thread.Sleep(100);
                            }
                            _trackers.Add(newTracker);
                            _trackerInfo[dualsenseId] = new KeyValuePair<int, bool>(info.Key, true);
                        }
                        Thread.Sleep(500);
                    }
                    Thread.Sleep(2000);
                }
            });
            Task.Run(async () => {
                // Turn each connected Dualsense into a tracker
                while (true) {
                    for (int i = 0; i < _trackers.Count; i++) {
                        var tracker = _trackers[i];
                        await tracker.Update();
                        if (tracker.Disconnected) {
                            var info = _trackerInfo[_trackers[i].RememberedDualsenseId];
                            _trackerInfo[_trackers[i].RememberedDualsenseId] = new KeyValuePair<int, bool>(info.Key, false);
                            _trackers.RemoveAt(i);
                            i = 0;
                        }
                    }
                    Thread.Sleep(8);
                }
            });
        }
        internal List<DualsenseTracker> Trackers { get => _trackers; set => _trackers = value; }
    }
}
