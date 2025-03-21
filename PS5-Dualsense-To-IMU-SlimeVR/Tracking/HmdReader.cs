using OVRSharp.Math;
using PS5_Dualsense_To_IMU_SlimeVR.Utility;
using System.Diagnostics;
using System.Numerics;
using Valve.VR;
using static OVRSharp.Overlay;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class HmdReader {
        private static CVRSystem _vrSystem;
        public static Quaternion GetHMDRotation() {
            TrackedDevicePose_t[] trackedDevices = new TrackedDevicePose_t[1] { new TrackedDevicePose_t() };
            if (_vrSystem == null && IsSteamVRRunning()) {
                try {
                    var err = EVRInitError.None;
                    _vrSystem = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Utility);
                } catch {

                }
            }
            if (_vrSystem != null) {
                _vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, trackedDevices);
            }
            return Quaternion.CreateFromRotationMatrix(trackedDevices[0].mDeviceToAbsoluteTracking.ToMatrix4x4());
        }
        public static float GetHMDHeight() {
            TrackedDevicePose_t[] trackedDevices = new TrackedDevicePose_t[1] { new TrackedDevicePose_t() };
            if (_vrSystem == null && IsSteamVRRunning()) {
                try {
                    var err = EVRInitError.None;
                    _vrSystem = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Utility);
                } catch {

                }
            }
            if (_vrSystem != null) {
                _vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, trackedDevices);
            }
            return trackedDevices[0].mDeviceToAbsoluteTracking.m7;
        }
        public static bool IsSteamVRRunning() {
            Process[] processes = Process.GetProcessesByName("vrserver");
            return processes.Length > 0;
        }
    }
}
