// Implementation borrowed from https://github.com/ButterscotchV/AXSlime
using LucHeart.CoreOSC;

namespace Everything_To_IMU_SlimeVR.Osc
{
    public interface HapticsSource
    {
        public HapticEvent[] ComputeHaptics(string parameter, OscMessage message);
        public bool IsSource(string parameter, OscMessage message);
    }
}
