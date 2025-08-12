using Everything_To_IMU_SlimeVR.Tracking;


namespace Everything_To_IMU_SlimeVR.Osc {
    public static class HapticsManager {
        public static bool HapticsEngaged = false;
        public static void SetNodeVibration(HapticNodeBinding hapticNodeBinding, int duration, float intensity) {
            foreach (var tracker in GenericTrackerManager.AllTrackers) {
                if (tracker.HapticNodeBinding == hapticNodeBinding) {
                    tracker.EngageHaptics(duration, intensity);
                    HapticsEngaged = true;
                }
            }
        }
        public static void StopNodeVibration(HapticNodeBinding hapticNodeBinding) {
            foreach (var tracker in GenericTrackerManager.AllTrackers) {
                if (tracker.HapticNodeBinding == hapticNodeBinding) {
                    tracker.DisableHaptics();
                }
            }
        }
        public static void StopNodeVibrations() {
            foreach (var tracker in GenericTrackerManager.AllTrackers) {
                tracker.DisableHaptics();
            }
            HapticsEngaged = false;
        }
    }
}