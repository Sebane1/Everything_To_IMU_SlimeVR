using Everything_To_IMU_SlimeVR.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everything_To_IMU_SlimeVR.Osc {
    public static class HapticsManager {
        public static void SetNodeVibration(HapticNodeBinding hapticNodeBinding, int duration) {
            foreach (var tracker in GenericTrackerManager.AllTrackers) {
                if (tracker.HapticNodeBinding == hapticNodeBinding) {
                    tracker.EngageHaptics(duration);
                }
            }
        }
    }
}