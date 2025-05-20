using Everything_To_IMU_SlimeVR.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Everything_To_IMU_SlimeVR.Osc {
    public static class HapticsManager {
        public static bool HapticsEngaged = false;
        public static void SetNodeVibration(HapticNodeBinding hapticNodeBinding, int duration, float intensity) {
            foreach (var tracker in GenericTrackerManager.AllTrackers) {
                if (tracker.HapticNodeBinding == hapticNodeBinding) {
                    tracker.EngageHaptics(duration, intensity, false);
                    HapticsEngaged = true;
                } else { 
                    tracker.DisableHaptics(); 
                }
            }
        }
        public static void StopNodeVibrations() {
            foreach(var tracker in GenericTrackerManager.AllTrackers) {
                tracker.DisableHaptics();
            }
            HapticsEngaged = false;
        }
    }
}