using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    public class GenericControllerTrackerManager {
        private List<GenericControllerTracker> _trackers = new List<GenericControllerTracker>();
        private Dictionary<int, KeyValuePair<int, bool>> _trackerInfo = new Dictionary<int, KeyValuePair<int, bool>>();
        private bool disposed = false;
        public event EventHandler<string> OnTrackerError;

        public GenericControllerTrackerManager() {
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
            Task.Run(async () => {
                while (!disposed) {
                    try {
                        var controllerCount = JSL.JslConnectDevices();
                        for (int i = 0; i < controllerCount; i++) {
                            if (!_trackerInfo.ContainsKey(i)) {
                                _trackerInfo[i] = new KeyValuePair<int, bool>(_trackers.Count, false);
                            }
                            var info = _trackerInfo[i];
                            if (!info.Value) {
                                var newTracker = new GenericControllerTracker(info.Key, colours[info.Key]);
                                while (!newTracker.Ready) {
                                    Thread.Sleep(100);
                                }
                                newTracker.OnTrackerError += NewTracker_OnTrackerError;
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
                // Turn each connected Dualsense into a tracker
                while (true) {
                    for (int i = 0; i < _trackers.Count; i++) {
                        var tracker = _trackers[i];
                        await tracker.Update();
                        if (tracker.Disconnected) {
                            var info = _trackerInfo[i];
                            _trackerInfo[i] = new KeyValuePair<int, bool>(info.Key, false);
                            _trackers.RemoveAt(i);
                            i = 0;
                            tracker.Dispose();
                        }
                    }
                    Thread.Sleep(8);
                }
            });
        }

        private void NewTracker_OnTrackerError(object? sender, string e) {
            OnTrackerError.Invoke(sender, e);
        }

        internal List<GenericControllerTracker> Trackers { get => _trackers; set => _trackers = value; }
    }
}
