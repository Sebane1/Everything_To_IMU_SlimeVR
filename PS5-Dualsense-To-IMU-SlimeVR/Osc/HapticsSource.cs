// Implementation borrowed from https://github.com/ButterscotchV/AXSlime
using LucHeart.CoreOSC;

namespace AxSlime.Osc
{
    public interface HapticsSource
    {
        public HapticEvent[] ComputeHaptics(string parameter, OscMessage message);
        public bool IsSource(string parameter, OscMessage message);
    }
}
