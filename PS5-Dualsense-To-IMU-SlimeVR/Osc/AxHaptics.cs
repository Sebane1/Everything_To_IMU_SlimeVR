// Implementation borrowed from https://github.com/ButterscotchV/AXSlime
using LucHeart.CoreOSC;

namespace Everything_To_IMU_SlimeVR.Osc
{
    public class AxHaptics : HapticsSource
    {
        public static readonly string AxHapticsPrefix = "VRCOSC/AXHaptics/";
        public static readonly string BinaryPrefix = "Touched";
        public static readonly string AnalogPrefix = "Proximity";

        private static readonly Dictionary<string, HapticNodeBinding> _nameToNode =
            Enum.GetValues<HapticNodeBinding>().ToDictionary(v => Enum.GetName(v)!);

        public AxHaptics()
        {
        }

        public HapticEvent[] ComputeHaptics(string parameter, OscMessage message)
        {
            var axHaptics = parameter[AxHapticsPrefix.Length..];
            if (axHaptics.StartsWith(BinaryPrefix))
            {
                var trigger = message.Arguments[0] as bool?;
                if (trigger != true)
                    return [];

                if (_nameToNode.TryGetValue(axHaptics[BinaryPrefix.Length..], out var nodeVal))
                    return [new HapticEvent(nodeVal)];
            } else if (axHaptics.StartsWith(AnalogPrefix)) {
                var proximity = message.Arguments[0] as float? ?? -1f;
                if (proximity <= 0.5f)
                    return [];

                var intensity = proximity;
                if (
                    intensity > 0f
                    && _nameToNode.TryGetValue(axHaptics[AnalogPrefix.Length..], out var nodeVal)
                )
                    return [new HapticEvent(nodeVal, intensity, 300)];
            }

            return [];
        }

        public bool IsSource(string parameter, OscMessage message)
        {
            return parameter.StartsWith(AxHapticsPrefix);
        }
    }
}
