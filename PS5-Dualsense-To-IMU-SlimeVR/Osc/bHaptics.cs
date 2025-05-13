// Implementation borrowed from https://github.com/ButterscotchV/AXSlime
using LucHeart.CoreOSC;

namespace AxSlime.Osc {
    public class bHaptics : HapticsSource {
        public static readonly string bHapticsPrefix = "bOSC_v1_";
        public static readonly string bHapticsPrefix2 = "bHapticsOSC_";

        private static readonly Dictionary<string, HapticNodeBinding[]> _mappings =
            new()
            {
                { "VestFront", [HapticNodeBinding.Chest, HapticNodeBinding.Hips] },
                { "VestBack", [HapticNodeBinding.Chest, HapticNodeBinding.Hips] },
                { "ArmLeft", [HapticNodeBinding.LeftUpperArm, HapticNodeBinding.LeftForeArm] },
                { "ArmRight", [HapticNodeBinding.RightUpperArm, HapticNodeBinding.RightForeArm] },
                {
                    "FootL",
                    [HapticNodeBinding.LeftFoot, HapticNodeBinding.LeftCalf, HapticNodeBinding.LeftThigh]
                },
                {
                    "FootR",
                    [HapticNodeBinding.RightFoot, HapticNodeBinding.RightCalf, HapticNodeBinding.RightThigh]
                },
                { "HandLeft", [HapticNodeBinding.LeftHand] },
                { "HandRight", [HapticNodeBinding.RightHand] },
                { "Head", [HapticNodeBinding.Head] },
            };

        private static readonly Dictionary<string, HapticEvent[]> _eventMap = _mappings
            .Select(m => (m.Key, m.Value.Select(n => new HapticEvent(n)).ToArray()))
            .ToDictionary();

        public bHaptics() {
        }

        public HapticEvent[] ComputeHaptics(string parameter, OscMessage message) {
            bool isFloatBased = parameter.StartsWith(bHapticsPrefix);
            if (isFloatBased) {
                var trigger = message.Arguments[0] as float?;
                if (trigger < 0.4f) {
                    return [];
                }
            } else {
                var trigger = message.Arguments[0] as bool?;
                if (trigger == false) {
                    return [];
                }
            }

            var bHaptics = parameter[isFloatBased ? (bHapticsPrefix.Length..) : (bHapticsPrefix2.Length..)];
            foreach (var binding in _eventMap) {
                if (bHaptics.Replace("_", "").Contains(binding.Key)) {
                    return binding.Value;
                }
            }

            return [];
        }

        public bool IsSource(string parameter, OscMessage message) {
            return parameter.StartsWith(bHapticsPrefix) || parameter.StartsWith(bHapticsPrefix2);
        }
    }
}
