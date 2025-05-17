using Newtonsoft.Json;
using System;
using System.Numerics;
namespace Everything_To_IMU_SlimeVR {
    public class Configuration {
        private List<TrackerConfig> _trackerConfigs = new List<TrackerConfig>();
        private List<TrackerConfig> _trackerConfig3ds = new List<TrackerConfig>();
        private List<TrackerConfig> _trackerConfigWiimote = new List<TrackerConfig>();
        private List<TrackerConfig> _trackerConfigNunchuck = new List<TrackerConfig>();
        private Dictionary<string, Vector3> _calibrationConfigurations = new Dictionary<string, Vector3>();
        private DateTime _lastConfigSave = new DateTime();
        private bool _switchingSessions = false;
        private bool _enableHaptics = true;

        private int _pollingRate = 8;
        private byte _wiiPollingRate = 32;
        private Dictionary<string, TrackerConfig> _trackerConfigUdpHaptics = new Dictionary<string, TrackerConfig>();

        public List<TrackerConfig> TrackerConfigs { get => _trackerConfigs; set => _trackerConfigs = value; }
        public List<TrackerConfig> TrackerConfigs3ds { get => _trackerConfig3ds; set => _trackerConfig3ds = value; }
        public List<TrackerConfig> TrackerConfigWiimote { get => _trackerConfigWiimote; set => _trackerConfigWiimote = value; }
        public List<TrackerConfig> TrackerConfigNunchuck { get => _trackerConfigNunchuck; set => _trackerConfigNunchuck = value; }
        public Dictionary<string, TrackerConfig> TrackerConfigUdpHaptics { get => _trackerConfigUdpHaptics; set => _trackerConfigUdpHaptics = value; }
        public Dictionary<string, Vector3> CalibrationConfigurations { get => _calibrationConfigurations; set => _calibrationConfigurations = value; }
        public DateTime LastCalibration { get => _lastConfigSave; set => _lastConfigSave = value; }
        public int PollingRate { get => _pollingRate; set => _pollingRate = value; }
        public bool SwitchingSessions { get => _switchingSessions; set => _switchingSessions = value; }
        public static Configuration? Instance { get; private set; }
        public byte WiiPollingRate { get => _wiiPollingRate; set => _wiiPollingRate = value; }

        public void SaveConfig() {
            _lastConfigSave = DateTime.UtcNow;
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            File.WriteAllText(savePath, JsonConvert.SerializeObject(this));
        }
        public TimeSpan TimeSinceLastConfig() {
            return DateTime.UtcNow - _lastConfigSave;
        }
        public static Configuration LoadConfig() {
            string openPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(openPath)) {
                var values = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(openPath));
                Instance = values;
                return Instance = (values == null ? new Configuration() : values);
            } else {
                return Instance = new Configuration();
            }
        }
    }
}
