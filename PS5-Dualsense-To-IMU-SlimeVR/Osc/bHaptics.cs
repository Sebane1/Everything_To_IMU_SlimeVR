// Implementation borrowed from https://github.com/ButterscotchV/AXSlime
using LucHeart.CoreOSC;

namespace Everything_To_IMU_SlimeVR.Osc {
    public class bHaptics : HapticsSource {
        public static readonly string bHapticsPrefix = "bOSC_v1_";
        public static readonly string bHapticsPrefix2 = "bHapticsOSC_";

        private static readonly Dictionary<string, HapticNodeBinding[]> _mappings =
            new()
            {
                { "VestFront_0", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront]},
                { "VestFront_1", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront]},
                { "VestFront_2", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_3", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_4", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_5", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_6", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_7", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_8", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_9", [HapticNodeBinding.Chest, HapticNodeBinding.ChestFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_10", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_11", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_12", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_13", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_14", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_15", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_16", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_17", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_18", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestFront_19", [HapticNodeBinding.Hips, HapticNodeBinding.HipsFront, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsFront] },
                { "VestBack_0", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_1", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_2", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_3", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_4", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_5", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_6", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_7", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_8", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_9", [HapticNodeBinding.Chest, HapticNodeBinding.ChestBack, HapticNodeBinding.ChestAndHips, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_10", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_11", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_12", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_13", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_14", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_15", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_16", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_17", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_18", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "VestBack_19", [HapticNodeBinding.Hips, HapticNodeBinding.ChestAndHips, HapticNodeBinding.HipsBack, HapticNodeBinding.ChestAndHipsBack] },
                { "ArmL", [HapticNodeBinding.LeftUpperArm, HapticNodeBinding.LeftForeArm] },
                { "ArmR", [HapticNodeBinding.RightUpperArm, HapticNodeBinding.RightForeArm] },
                { "FootL", [HapticNodeBinding.LeftFoot] },
                { "CalfL", [HapticNodeBinding.LeftCalf]},
                { "ThighL", [HapticNodeBinding.LeftThigh]},
                { "FootR",  [HapticNodeBinding.RightFoot]},
                { "CalfR",[HapticNodeBinding.RightCalf]},
                { "ThighR", [HapticNodeBinding.RightThigh] },
                { "HandLeft", [HapticNodeBinding.LeftHand] },
                { "HandRight", [HapticNodeBinding.RightHand] },
                { "Head", [HapticNodeBinding.Head] },
                { "Pat", [HapticNodeBinding.Head] },
            };

        private static readonly Dictionary<string, HapticEvent[]> _eventMap = _mappings
            .Select(m => (m.Key, m.Value.Select(n => new HapticEvent(n)).ToArray()))
            .ToDictionary();

        public bHaptics() {
        }

        public HapticEvent[] ComputeHaptics(string parameter, OscMessage message) {
            bool isFloatBased = parameter.StartsWith(bHapticsPrefix);
            float intensity = 0f;
            if (isFloatBased) {
                float trigger = (float)message.Arguments[0];
                intensity = trigger;
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
                if (bHaptics.ToLower().Contains(binding.Key.ToLower())) {
                    for(int i =0; i <binding.Value.Length;i++) {
                        binding.Value[i].Intensity = intensity;
                    }
                    return binding.Value;
                }
            }

            return [];
        }

        public bool IsSource(string parameter, OscMessage message) {
            return parameter.StartsWith(bHapticsPrefix) || parameter.StartsWith(bHapticsPrefix2) || parameter.StartsWith("Pat");
        }
    }
}
