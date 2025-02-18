using AQuestReborn;
using System.Numerics;
using Wujek_Dualsense_API;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    public class DualSenseTranslator {
        private List<DualsenseTracker> _trackers;
        public DualSenseTranslator() {
            int count = 0;
            Color[] colours = new Color[4] {
                Color.Aqua,
                Color.Red,
                Color.Green,
                Color.Orange
            };

            _trackers = new List<DualsenseTracker>();

            Task.Run(async () => {
                // Turn each connected Dualsense into a tracker
                foreach (var dualsenseId in DualsenseUtils.GetControllerIDs()) {
                    var newTracker = new DualsenseTracker(count, dualsenseId, colours[count++]);
                    while (!newTracker.Ready) {
                        Thread.Sleep(100);
                    }
                    _trackers.Add(newTracker);
                }

                while (true) {
                    foreach (var tracker in _trackers) {
                        await tracker.Update();
                    }
                }
            });
        }
        internal List<DualsenseTracker> Trackers { get => _trackers; set => _trackers = value; }
    }
}
