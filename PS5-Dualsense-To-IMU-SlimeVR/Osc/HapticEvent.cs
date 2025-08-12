// Implementation borrowed from https://github.com/ButterscotchV/AXSlime

namespace Everything_To_IMU_SlimeVR.Osc
{
    public struct HapticEvent
    {
        public HapticNodeBinding Node;
        public float Intensity;
        public float Duration;

        public HapticEvent(HapticNodeBinding node, float intensity = 0, float duration = 300)
        {
            Node = node;
            Intensity = intensity;
            Duration = duration;
        }
    }
}
