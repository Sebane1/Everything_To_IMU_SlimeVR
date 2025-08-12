using OVRSharp.Math;
using SlimeImuProtocol.Utility;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Valve.VR;
using static OVRSharp.Overlay;
using System.Collections.Concurrent;
using static Everything_To_IMU_SlimeVR.TrackerConfig;

namespace Everything_To_IMU_SlimeVR.Tracking {
    internal class OpenVRReader {
        static ConcurrentDictionary<string, uint> _foundTrackers = new ConcurrentDictionary<string, uint>();
        static ConcurrentDictionary<uint, Matrix4x4> _trackerMatrixes = new ConcurrentDictionary<uint, Matrix4x4>();
        private static CVRSystem _vrSystem;
        private static TrackedDevicePose_t[] _poseArray;
        private static Stopwatch _stopwatch  = new Stopwatch();
        private static Stopwatch _steamVRCheckCooldown = new Stopwatch();
        private static bool _steamVRWasDetected;

        public static Stopwatch Stopwatch { get => _stopwatch; set => _stopwatch = value; }

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
        /// Detect if waist is in front of hmd.
        /// </summary>
        /// <returns></returns>
        public static Tuple<bool, string, float> WaistIsInFrontOfHMD() {
            string debug = "";
            var waistRotation = GetTrackerRotation("waist").GetXAxisFromQuaternion();
            if (GenericTrackerManager.DebugOpen) {
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
                        if (IsDesiredTracker(deviceIndex, "waist")) {
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
        public static Quaternion GetTrackerRotation(string trackerName) {
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

                    if (_foundTrackers.ContainsKey(trackerName)) {
                        uint index = _foundTrackers[trackerName];
                        if (index < numDevices && IsDesiredTracker(index, trackerName)) {
                            return GetTrackingPose(index);
                        }
                    }
                    if (numDevices > 0) {
                        for (uint i = 0; i < numDevices; i++) {
                            uint deviceIndex = deviceIndices[i];

                            // Check if the device is a waist tracker
                            if (IsDesiredTracker(deviceIndex, trackerName)) {
                                return GetTrackingPose(deviceIndex);
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

        private static Quaternion GetTrackingPose(uint index) {
            // Get the device pose (position and rotation)
            if (_poseArray == null || _stopwatch.ElapsedMilliseconds > 4) {
                var poseArray = new TrackedDevicePose_t[20];
                OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poseArray);
                _trackerMatrixes[index] = poseArray[index].mDeviceToAbsoluteTracking.ToMatrix4x4();
                _stopwatch.Restart();
                _poseArray = poseArray;
            }
            // Process the pose data (position/rotation) for the waist tracker
            // Console.WriteLine($"Waist Tracker Position: {pose.mDeviceToAbsoluteTracking.m0}, {pose.mDeviceToAbsoluteTracking.m1}, {pose.mDeviceToAbsoluteTracking.m2}");
            return Quaternion.CreateFromRotationMatrix(_trackerMatrixes[index]);
        }

        private static bool IsDesiredTracker(uint deviceIndex, string trackerType) {
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
            return deviceName.ToString().ToLower().Contains(trackerType.ToLower());

        }
        public static bool IsSteamVRRunning() {
            if (!_steamVRCheckCooldown.IsRunning) {
                _steamVRCheckCooldown.Start();
            }
            if (!_steamVRWasDetected && _steamVRCheckCooldown.ElapsedMilliseconds > 10000) {
                Process[] processes = Process.GetProcessesByName("vrserver");
                _steamVRWasDetected = processes.Length > 0;
                _steamVRCheckCooldown.Restart();
            }
            return _steamVRWasDetected;
        }
        public static Quaternion GetTrackerRotation(RotationReferenceType yawReferenceType) {
            try {
                switch (yawReferenceType) {
                    case RotationReferenceType.HmdRotation:
                        return GetHMDRotation();
                    case RotationReferenceType.WaistRotation:
                        return GetTrackerRotation("waist");
                    case RotationReferenceType.ChestRotation:
                        return GetTrackerRotation("chest");
                    case RotationReferenceType.LeftAnkleRotation:
                        return GetTrackerRotation("left_foot");
                    case RotationReferenceType.RightAnkleRotation:
                        return GetTrackerRotation("right_foot");
                }
            } catch {

            }
            return Quaternion.Identity;
        }
    }
}
