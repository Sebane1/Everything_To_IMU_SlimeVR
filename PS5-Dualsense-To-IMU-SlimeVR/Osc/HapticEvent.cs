// Implementation borrowed from https://github.com/ButterscotchV/AXSlime

namespace AxSlime.Osc
{
    public readonly record struct HapticEvent
    {
        public readonly HapticNodeBinding Node;
        public readonly float? Intensity;
        public readonly float? Duration;

        public HapticEvent(HapticNodeBinding node, float? intensity = null, float? duration = null)
        {
            Node = node;
            Intensity = intensity;
            Duration = duration;
        }
    }
}
