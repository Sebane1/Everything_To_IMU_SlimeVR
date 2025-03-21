using PS5_Dualsense_To_IMU_SlimeVR.Tracking;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    public partial class ConfigurationDisplay : Form {
        private GenericControllerTrackerManager _genericControllerTranslator;
        private Configuration _configuration = new Configuration();
        Queue<string> errorQueue = new Queue<string>();
        int _currentIndex = 0;
        private bool _suppressCheckBoxEvent;

        public int CurrentIndex { get => _currentIndex; set => _currentIndex = value; }

        public ConfigurationDisplay() {
            InitializeComponent();
            AutoScaleDimensions = new SizeF(96, 96);
            _configuration = Configuration.LoadConfig();
            _genericControllerTranslator = new GenericControllerTrackerManager(_configuration);
            _genericControllerTranslator.OnTrackerError += _genericControllerTranslator_OnTrackerError;
        }

        private void _genericControllerTranslator_OnTrackerError(object? sender, string e) {
            errorQueue.Enqueue(e);
        }

        private void refreshTimer_Tick(object sender, EventArgs e) {
            debugText.Text = "";
            try {
                debugText.Text += _genericControllerTranslator.Trackers[_currentIndex].Debug;
            } catch {

            }
            deviceList.Items.Clear();
            foreach (var item in _genericControllerTranslator.Trackers) {
                deviceList.Items.Add("Tracker " + item.Id);
            }
            if (deviceList.Items.Count > 0) {
                deviceList.SelectedIndex = _currentIndex;
                rediscoverTrackerButton.Visible = true;
                falseThighSimulationCheckBox.Visible = true;
            } else {
                falseThighSimulationCheckBox.Visible = false;
                rediscoverTrackerButton.Visible = false;
            }
            if (errorQueue.Count > 0) {
                var value = errorQueue.Dequeue();
                MessageBox.Show(value, "Tracking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void selectedDevice_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            _currentIndex = deviceList.SelectedIndex;
            _suppressCheckBoxEvent = true;
            falseThighSimulationCheckBox.Checked = _configuration.TrackerConfigs[_currentIndex].SimulatesThighs;
            falseThighSimulationCheckBox.Checked = _genericControllerTranslator.Trackers[_currentIndex].SimulateThighs;
            trackerConfigLabel.Text = $"Tracker {_currentIndex + 1} Config";
            _suppressCheckBoxEvent = false;
            refreshTimer.Start();
        }

        private void tabPage1_Click(object sender, EventArgs e) {

        }

        private void falseThighSimulationCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (!_suppressCheckBoxEvent) {
                _genericControllerTranslator.Trackers[_currentIndex].SimulateThighs = falseThighSimulationCheckBox.Checked;
                _configuration.TrackerConfigs[_currentIndex].SimulatesThighs = falseThighSimulationCheckBox.Checked;
                _configuration.SaveConfig();
            }
        }

        private void rediscoverTrackerButton_Clicked(object sender, EventArgs e) {
            _genericControllerTranslator.Trackers[_currentIndex].Rediscover();
        }

        private void trackerCalibrationButton_Click(object sender, EventArgs e) {
            foreach (var item in _genericControllerTranslator.Trackers) {
                item.Recalibrate();
            }
        }

        private void donateButton_Click(object sender, EventArgs e) {
            OpenURL("https://ko-fi.com/sebastina");
        }
        public void OpenURL(string url) {
            try {
                Process.Start(url);
            } catch {
                // Handle exceptions, such as the browser not being installed
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    // Handle for Linux and macOS
                    Process process = new Process();
                    process.StartInfo = new ProcessStartInfo("xdg-open", url) { UseShellExecute = false };
                    process.Start();
                }
            }
        }
    }
}
