namespace Everything_To_IMU_SlimeVR {
    public class TrackerConfig {
        bool _simulatesThighs;
        RotationReferenceType _yawReferenceTypeValue;
        HapticNodeBinding _hapticNodeBinding;
        private RotationReferenceType _extensionYawReferenceTypeValue;

        public RotationReferenceType YawReferenceTypeValue { get => _yawReferenceTypeValue; set => _yawReferenceTypeValue = value; }
        public RotationReferenceType ExtensionYawReferenceTypeValue { get => _extensionYawReferenceTypeValue; set => _extensionYawReferenceTypeValue = value; }
        public HapticNodeBinding HapticNodeBinding { get => _hapticNodeBinding; set => _hapticNodeBinding = value; }

        public enum RotationReferenceType {
            HmdRotation = 0,
            WaistRotation = 1,
            ChestRotation = 2,
            LeftAnkleRotation = 3,
            RightAnkleRotation = 4,
            TrustDeviceYaw = 5,
        }
    }
}

