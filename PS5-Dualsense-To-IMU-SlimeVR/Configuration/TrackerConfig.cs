namespace Everything_To_IMU_SlimeVR {
    public class TrackerConfig {
        bool _simulatesThighs;

        RotationReferenceType _yawReferenceTypeValue;

        public bool SimulatesThighs { get => _simulatesThighs; set => _simulatesThighs = value; }
        public RotationReferenceType YawReferenceTypeValue { get => _yawReferenceTypeValue; set => _yawReferenceTypeValue = value; }

        public enum RotationReferenceType {
            HmdRotation = 0,
            WaistRotation = 1,
            TrackerRotation = 2,
            ChestRotation = 3
        }
    }
}

