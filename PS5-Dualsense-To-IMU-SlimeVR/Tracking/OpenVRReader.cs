using OVRSharp.Math;
using PS5_Dualsense_To_IMU_SlimeVR.Utility;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Valve.VR;
using static OVRSharp.Overlay;

namespace PS5_Dualsense_To_IMU_SlimeVR.Tracking {
    internal class OpenVRReader {
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
                return trackedDevices[0].mDeviceToAbsoluteTracking.m7;
            } else {
                return 1.5f;
            }
        }


        /// <summary>
        /// Math isn't mathing with this implementation. Supposed to detect if the waist is in front of the HMD and report true if so.
        /// </summary>
        /// <returns></returns>
        public static Tuple<bool, string> WaistIsInFrontOfHMDOld() {
            string debug = "";
            var waistRotation = GetWaistTrackerRotation().GetYawFromQuaternion() + 180;
            var direction = waistRotation.AngleToForward();
            var hmdPosition = GetHMDPosition();
            var waistPosition = GetWaistTrackerPosition();

            var yStrippedHMDPosition = new Vector3(hmdPosition.X, 0, hmdPosition.Z);
            var yStrippedWaistPosition = new Vector3(waistPosition.X, 0, waistPosition.Z);

            var targetForward = yStrippedHMDPosition + direction;


            Vector3 delta = Vector3.Normalize((yStrippedHMDPosition - yStrippedWaistPosition));
            Vector3 cross = Vector3.Cross(delta, targetForward);

            debug += $"Waist Rotation: {waistRotation}\r\n";
            debug += $"Waist Direction: {direction}\r\n";
            debug += $"HMD Position: {hmdPosition}\r\n";
            debug += $"Waist Position: {waistPosition}\r\n";
            debug += $"Y Stripped HMD Position: {yStrippedHMDPosition}\r\n";
            debug += $"Y Stripped Waist Position: {yStrippedWaistPosition}\r\n";

            debug += $"Forward Target: {targetForward}\r\n";

            return new Tuple<bool, string>(cross == Vector3.Zero, debug);
        }


        /// <summary>
        /// Detect if waist is in front of hmd.
        /// </summary>
        /// <returns></returns>
        public static Tuple<bool, string, float> WaistIsInFrontOfHMD() {
            string debug = "";
            var waistRotation = GetWaistTrackerRotation().GetXAxisFromQuaternion();
            if (GenericControllerTrackerManager.DebugOpen) {
                debug += $"Waist X Rotation: {waistRotation}\r\n";
            }

            return new Tuple<bool, string, float>(waistRotation > 0, debug, waistRotation);
        }

        public static Vector3 GetHMDPosition() {
            TrackedDevicePose_t[] trackedDevices = new TrackedDevicePose_t[1] { new TrackedDevicePose_t() };
            if (_vrSystem == null && IsSteamVRRunning()) {
                try {
                    var err = EVRInitError.None;
                    _vrSystem = OpenVR.Init(ref err, EVRApplicationType.VRApplication_Utility);
                } catch {

                }
            }
            if (_vrSystem != null) {
                _vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0, trackedDevices);
                return trackedDevices[0].mDeviceToAbsoluteTracking.ToMatrix4x4().Translation;

            } else {
                return new Vector3();
            }
        }
        public static float GetWaistTrackerHeight() {
            EVRInitError eError = EVRInitError.None;
            if (_vrSystem == null && IsSteamVRRunning()) {
                OpenVR.Init(ref eError, EVRApplicationType.VRApplication_Utility);
            } else {
                if (eError != EVRInitError.None) {
                    Console.WriteLine("Error initializing OpenVR: " + eError.ToString());
                }

                // Initialize an array to hold the device indices
                uint[] deviceIndices = new uint[20];

                // Get sorted tracked device indices of class GenericTracker (trackers like waist trackers)
                uint numDevices = OpenVR.System.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.GenericTracker, deviceIndices, 0);

                if (numDevices > 0) {
                    for (uint i = 0; i < numDevices; i++) {
                        uint deviceIndex = deviceIndices[i];

                        // Check if the device is a waist tracker
                        if (IsWaistTracker(deviceIndex)) {
                            // Get the device pose (position and rotation)
                            TrackedDevicePose_t[] poseArray = new TrackedDevicePose_t[20];
                            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poseArray);

                            // Get height
                            return poseArray[deviceIndex].mDeviceToAbsoluteTracking.m7;
                        }
                    }
                } else {
                    Console.WriteLine("No trackers found.");
                }
            }
            return 0.0f;
        }
        public static Vector3 GetWaistTrackerPosition() {
            EVRInitError eError = EVRInitError.None;
            if (_vrSystem == null && IsSteamVRRunning()) {
                OpenVR.Init(ref eError, EVRApplicationType.VRApplication_Utility);
            } else {
                if (eError != EVRInitError.None) {
                    Console.WriteLine("Error initializing OpenVR: " + eError.ToString());
                }

                // Initialize an array to hold the device indices
                uint[] deviceIndices = new uint[20];

                // Get sorted tracked device indices of class GenericTracker (trackers like waist trackers)
                uint numDevices = OpenVR.System.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.GenericTracker, deviceIndices, 0);

                if (numDevices > 0) {
                    for (uint i = 0; i < numDevices; i++) {
                        uint deviceIndex = deviceIndices[i];

                        // Check if the device is a waist tracker
                        if (IsWaistTracker(deviceIndex)) {
                            // Get the device pose (position and rotation)
                            TrackedDevicePose_t[] poseArray = new TrackedDevicePose_t[20];
                            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated, 0, poseArray);

                            // Get position
                            return poseArray[deviceIndex].mDeviceToAbsoluteTracking.ToMatrix4x4().Translation;
                        }
                    }
                } else {
                    Console.WriteLine("No trackers found.");
                }
            }
            return new Vector3();
        }
        public static Quaternion GetWaistTrackerRotation() {
            try {
                EVRInitError eError = EVRInitError.None;
                if (_vrSystem == null && IsSteamVRRunning()) {
                    OpenVR.Init(ref eError, EVRApplicationType.VRApplication_Utility);
                } else {
                    if (eError != EVRInitError.None) {
                        Console.WriteLine("Error initializing OpenVR: " + eError.ToString());
                    }

                    // Initialize an array to hold the device indices
                    uint[] deviceIndices = new uint[20];

                    // Get sorted tracked device indices of class GenericTracker (trackers like waist trackers)
                    uint numDevices = OpenVR.System.GetSortedTrackedDeviceIndicesOfClass(ETrackedDeviceClass.GenericTracker, deviceIndices, 0);

                    if (numDevices > 0) {
                        for (uint i = 0; i < numDevices; i++) {
                            uint deviceIndex = deviceIndices[i];

                            // Check if the device is a waist tracker
                            if (IsWaistTracker(deviceIndex)) {
                                // Get the device pose (position and rotation)
                                TrackedDevicePose_t[] poseArray = new TrackedDevicePose_t[20];
                                OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poseArray);

                                // Process the pose data (position/rotation) for the waist tracker
                                // Console.WriteLine($"Waist Tracker Position: {pose.mDeviceToAbsoluteTracking.m0}, {pose.mDeviceToAbsoluteTracking.m1}, {pose.mDeviceToAbsoluteTracking.m2}");
                                return Quaternion.CreateFromRotationMatrix(poseArray[deviceIndex].mDeviceToAbsoluteTracking.ToMatrix4x4());
                            }
                        }
                    } else {
                        Console.WriteLine("No trackers found.");
                    }
                }
            } catch {

            }
            return Quaternion.Identity;
        }

        private static bool IsWaistTracker(uint deviceIndex) {
            // You can check the device properties or model number to identify the waist tracker
            // Prepare a buffer to hold the model name
            StringBuilder deviceName = new StringBuilder(64);
            ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;

            // Get the model number of the device
            uint result = OpenVR.System.GetStringTrackedDeviceProperty(
                deviceIndex,
                ETrackedDeviceProperty.Prop_ControllerType_String,
                deviceName,
                (uint)deviceName.Capacity,
                ref error
            );

            // For example, check if the device name contains "Waist" or a specific ID
            Console.WriteLine($"Device {deviceIndex} Model Number: {deviceName.ToString()}");

            // Check if the device name matches the waist tracker (this is an example check)
            return deviceName.ToString().ToLower().Contains("waist");

        }
        public static bool IsSteamVRRunning() {
            Process[] processes = Process.GetProcessesByName("vrserver");
            return processes.Length > 0;
        }
    }
}
