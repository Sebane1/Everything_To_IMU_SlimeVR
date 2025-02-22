using OVRSharp.Math;
using PS5_Dualsense_To_IMU_SlimeVR.Utility;
using System.Numerics;
using Valve.VR;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class HmdReader {
        private static CVRSystem _vrSystem;
        private static Vector3 hmdOffset = new Vector3(0, -90, 90);

        public static Vector3 HmdOffset { get => hmdOffset; set => hmdOffset = value; }

        public static Quaternion GetHMDRotation() {
            TrackedDevicePose_t[] trackedDevices = new TrackedDevicePose_t[1] { new TrackedDevicePose_t() };
            try {
                var err = EVRInitError.None;
                _vrSystem = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Utility);
            } catch {

            }
            if (_vrSystem != null) {
                _vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, trackedDevices);
            }
            return Quaternion.CreateFromRotationMatrix(trackedDevices[0].mDeviceToAbsoluteTracking.ToMatrix4x4());
        }

    }
}
