using Newtonsoft.Json;
namespace PS5_Dualsense_To_IMU_SlimeVR {
    public class Configuration {
        List<TrackerConfig> trackerConfigs = new List<TrackerConfig>();

        public List<TrackerConfig> TrackerConfigs { get => trackerConfigs; set => trackerConfigs = value; }

        public void SaveConfig() {
            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            File.WriteAllText(savePath, JsonConvert.SerializeObject(this));
        }

        public static Configuration LoadConfig() {
            string openPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(openPath)) {
                return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(openPath));
            } else {
                return new Configuration();
            }
        }
    }
}
