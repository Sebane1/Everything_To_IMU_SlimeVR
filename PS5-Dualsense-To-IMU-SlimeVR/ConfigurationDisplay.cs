using Everything_To_IMU_SlimeVR.Tracking;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using static Everything_To_IMU_SlimeVR.TrackerConfig;

namespace Everything_To_IMU_SlimeVR {
    public partial class ConfigurationDisplay : Form {
        private GenericTrackerManager _genericControllerTranslator;
        private Configuration _configuration = new Configuration();
        Queue<string> errorQueue = new Queue<string>();
        private TrackerConfig _currentTrackerConfig;
        private IBodyTracker _currentTracker;
        private bool _suppressCheckBoxEvent;
        string _lastErrorLog = "";
        private bool _legacyWiiClientDetected;
        private readonly ForwardedWiimoteManager _forwardedWiimoteManager;
        private readonly Forwarded3DSDataManager _forwarded3DSDataManager;

        public ConfigurationDisplay() {
            InitializeComponent();
            AutoScaleDimensions = new SizeF(96, 96);
            _configuration = Configuration.LoadConfig();
            _configuration.WiiPollingRate = 64;
            if (_configuration.SwitchingSessions) {
                _configuration.LastCalibration = DateTime.UtcNow;
            }
            _genericControllerTranslator = new GenericTrackerManager(_configuration);
            _genericControllerTranslator.OnTrackerError += _genericControllerTranslator_OnTrackerError;
            _genericControllerTranslator.PollingRate = _configuration.PollingRate;

            _forwardedWiimoteManager = new ForwardedWiimoteManager();
            _forwarded3DSDataManager = new Forwarded3DSDataManager();
            _configuration.SwitchingSessions = false;
            falseThighSimulationCheckBox.Checked = _configuration.SimulatesThighs;
            audioHapticsActive.Checked = _configuration.AudioHapticsActive;
            _configuration.SaveConfig();
        }

        private void _genericControllerTranslator_OnTrackerError(object? sender, string e) {
            errorQueue.Enqueue(e);
        }

        private void refreshTimer_Tick(object sender, EventArgs e) {
            if (tabOptions.SelectedIndex == 1) {
                debugText.Text = "";
                try {
                    debugText.Text += _currentTracker.Debug;
                } catch {

                }
                GenericTrackerManager.DebugOpen = true;
                refreshTimer.Interval = 1;
            } else {
                GenericTrackerManager.DebugOpen = false;
                refreshTimer.Interval = 1000;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void selectedDevice_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = controllerDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigs[currentIndex];
                _currentTracker = GenericTrackerManager.TrackersBluetooth[currentIndex];
            }
            RefreshTracker();
        }
        private void threeDsDeviceList_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = threeDsDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigs3ds[currentIndex];
                _currentTracker = GenericTrackerManager.Trackers3ds[currentIndex];
            }
            RefreshTracker();
        }
        private void wiimoteDeviceList_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = wiimoteDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                string value = wiimoteDeviceList.SelectedItem.ToString();
                if (!_configuration.TrackerConfigWiimote.ContainsKey(value)) {
                    _configuration.TrackerConfigWiimote[value] = new TrackerConfig();
                }
                _currentTrackerConfig = _configuration.TrackerConfigWiimote[value];
                _currentTracker = GenericTrackerManager.TrackersWiimote[currentIndex];
            }
            RefreshTracker();
        }

        private void nunchuckDeviceList_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = nunchuckDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigNunchuck[currentIndex];
                _currentTracker = GenericTrackerManager.TrackersNunchuck[currentIndex];
            }
            RefreshTracker();
        }
        void RefreshTracker() {
            if (_currentTracker != null) {
                _suppressCheckBoxEvent = true;
                falseThighSimulationCheckBox.Checked = Configuration.Instance.SimulatesThighs;

                yawForSimulatedTracker.SelectedIndex = (int)_currentTrackerConfig.YawReferenceTypeValue;
                _currentTracker.YawReferenceTypeValue = _currentTrackerConfig.YawReferenceTypeValue;

                extensionYawForSimulatedTracker.SelectedIndex = (int)_currentTrackerConfig.ExtensionYawReferenceTypeValue;
                _currentTracker.ExtensionYawReferenceTypeValue = _currentTrackerConfig.ExtensionYawReferenceTypeValue;

                hapticJointAssignment.SelectedIndex = (int)_currentTracker.HapticNodeBinding;
                _currentTracker.HapticNodeBinding = _currentTrackerConfig.HapticNodeBinding;

                identifyButton.Visible = _currentTracker.SupportsHaptics;
                hapticJointAssignment.Visible = _currentTracker.SupportsHaptics;
                yawForSimulatedTracker.Visible = _currentTracker.SupportsIMU;
                extensionYawForSimulatedTracker.Visible = _currentTracker.SupportsIMU;
                rediscoverTrackerButton.Visible = _currentTracker.SupportsIMU;

                yawSourceDisclaimer1.Visible = _currentTracker.SupportsIMU;
                yawSourceDisclaimer2.Visible = _currentTracker.SupportsIMU;
                yawSourceLabel.Visible = _currentTracker.SupportsIMU;
                extensionSourceLabel.Visible = _currentTracker.SupportsIMU;
                extensionYawForSimulatedTracker.Visible = _currentTracker.SupportsIMU;
                hapticJointAssignmentLabel.Visible = _currentTracker.SupportsHaptics;
                intensityTestButton.Visible = _currentTracker.SupportsHaptics;

                trackerConfigLabel.Text = $"{_currentTracker.ToString()} Config";
                _suppressCheckBoxEvent = false;
                refreshTimer.Start();
            }
        }
        private void tabPage1_Click(object sender, EventArgs e) {

        }

        private void falseThighSimulationCheckBox_CheckedChanged(object sender, EventArgs e) {
            //yawForSimulatedTracker.Enabled = falseThighSimulationCheckBox.Checked;
            if (!_suppressCheckBoxEvent) {
                _configuration.SimulatesThighs = falseThighSimulationCheckBox.Checked;
                _configuration.SaveConfig();
            }
        }

        private void rediscoverTrackerButton_Clicked(object sender, EventArgs e) {
            _currentTracker.Rediscover();
        }

        private void trackerCalibrationButton_Click(object sender, EventArgs e) {
            foreach (var item in GenericTrackerManager.TrackersBluetooth) {
                item.Recalibrate();
            }
            foreach (var item in GenericTrackerManager.Trackers3ds) {
                item.Recalibrate();
            }
            foreach (var item in GenericTrackerManager.TrackersWiimote) {
                item.Recalibrate();
            }
            foreach (var item in GenericTrackerManager.TrackersNunchuck) {
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

        private void ConfigurationDisplay_Load(object sender, EventArgs e) {
            ForwardedWiimoteManager.LegacyClientDetected += delegate {
                if (!_legacyWiiClientDetected) {
                    _legacyWiiClientDetected = true;
                    if (MessageBox.Show("Your Wii client is outdated! Please consider updating to the latest version.", "Outdated Wii Client", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK) {
                        string url = "https://github.com/Sebane1/Everything_To_IMU_SlimeVR/releases";

                        try {
                            Process.Start(new ProcessStartInfo {
                                FileName = url,
                                UseShellExecute = true // Required for opening URLs in default browser
                            });
                        } catch (Exception ex) {
                            Console.WriteLine("Failed to open URL: " + ex.Message);
                        }
                    }
                }
            };
        }

        private void pollingRate_Scroll(object sender, EventArgs e) {
            _configuration.PollingRate = pollingRate.Value;
            _genericControllerTranslator.PollingRate = _configuration.PollingRate;
            polllingRateLabel.Text = "Polling Rate: " + pollingRate.Value + "ms";
            _configuration.SaveConfig();
        }

        private void yawForSimulatedTracker_SelectedIndexChanged(object sender, EventArgs e) {
            if (!_suppressCheckBoxEvent) {
                _currentTracker.YawReferenceTypeValue = (RotationReferenceType)yawForSimulatedTracker.SelectedIndex;
                _currentTrackerConfig.YawReferenceTypeValue = (RotationReferenceType)yawForSimulatedTracker.SelectedIndex;
                _configuration.SaveConfig();
            }
        }

        private void label1_Click(object sender, EventArgs e) {

        }

        private void memoryResetTimer_Tick(object sender, EventArgs e) {
            //string resetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            //_configuration.SwitchingSessions = true;
            //_configuration.SaveConfig();
            //Process.Start(resetPath.Replace(".dll", ".exe"));
            //Application.Exit();
        }

        private void identifyButton_Click(object sender, EventArgs e) {
            if (_currentTracker != null) {
                _currentTracker.Identify();
            }
        }

        private void hapticJointAssignment_SelectedIndexChanged(object sender, EventArgs e) {
            if (!_suppressCheckBoxEvent) {
                _currentTracker.HapticNodeBinding = (HapticNodeBinding)hapticJointAssignment.SelectedIndex;
                _currentTrackerConfig.HapticNodeBinding = (HapticNodeBinding)hapticJointAssignment.SelectedIndex;
                _configuration.SaveConfig();
            }
        }

        private void label4_Click(object sender, EventArgs e) {

        }

        private void newHapticCellphoneButton_Click(object sender, EventArgs e) {
            if (IPAddress.TryParse(newIpFeild.Text, out var validIp)) {
                _genericControllerTranslator.AddRemoteHapticDevice(newIpFeild.Text);
            } else {
                MessageBox.Show("Invalid ip address!");
            }
            newIpFeild.Text = string.Empty;
            _configuration.SaveConfig();
        }

        private void hapticDeviceList_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = hapticDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigUdpHaptics[hapticDeviceList.SelectedItem.ToString()];
                _currentTracker = GenericTrackerManager.TrackersUdpHapticDevice[currentIndex];
            }
            RefreshTracker();
        }

        private void testHaptics_Click(object sender, EventArgs e) {
            _genericControllerTranslator.HapticTest();
        }

        private void wiimoteRate_Scroll(object sender, EventArgs e) {
            _configuration.WiiPollingRate = (byte)wiimoteRate.Value;
            wiimoteRateLabel.Text = "Wiimote Rate: " + wiimoteRate.Value + "ms";
            _configuration.SaveConfig();
        }

        private void extensionYawForSimulatedTracker_SelectedIndexChanged(object sender, EventArgs e) {
            if (!_suppressCheckBoxEvent) {
                _currentTracker.ExtensionYawReferenceTypeValue = (RotationReferenceType)extensionYawForSimulatedTracker.SelectedIndex;
                _currentTrackerConfig.ExtensionYawReferenceTypeValue = (RotationReferenceType)extensionYawForSimulatedTracker.SelectedIndex;
                _configuration.SaveConfig();
            }
        }

        private void listRefreshTimer_Tick(object sender, EventArgs e) {
            controllerDeviceList.Items.Clear();
            threeDsDeviceList.Items.Clear();
            wiimoteDeviceList.Items.Clear();
            hapticDeviceList.Items.Clear();
            try {
                foreach (var item in GenericTrackerManager.TrackersBluetooth) {
                    controllerDeviceList.Items.Add(item.ToString());
                }
                foreach (var item in GenericTrackerManager.Trackers3ds) {
                    threeDsDeviceList.Items.Add(item.ToString());
                }
                foreach (var item in GenericTrackerManager.TrackersWiimote) {
                    wiimoteDeviceList.Items.Add(item.ToString());
                }
                foreach (var item in GenericTrackerManager.TrackersUdpHapticDevice) {
                    hapticDeviceList.Items.Add(item.ToString());
                }
            } catch {

            }
            if (_currentTracker != null) {
                rediscoverTrackerButton.Visible = _currentTracker.SupportsIMU;
            } else {
                rediscoverTrackerButton.Visible = false;
            }
            if (errorQueue.Count > 0) {
                var value = errorQueue.Dequeue();
                if (_lastErrorLog != value) {
                    errorLogText.Text += value + "\r\n";
                }
                _lastErrorLog = value;
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errors.log"), _lastErrorLog + "\r\n");
            }
        }

        private void lockInDetectedDevicesButton_Click(object sender, EventArgs e) {
            GenericTrackerManager.lockInDetectedDevices = !GenericTrackerManager.lockInDetectedDevices;
            if (!GenericTrackerManager.lockInDetectedDevices) {
                lockInDetectedDevicesButton.Text = "Disable New Device Detection (Reduces Drift)";
            } else {
                lockInDetectedDevicesButton.Text = "Enable New Device Detection (Increases Drift)";
            }
        }

        private void label10_Click(object sender, EventArgs e) {

        }

        private void label7_Click(object sender, EventArgs e) {

        }

        private void label2_Click(object sender, EventArgs e) {

        }

        private void newIpFeild_TextChanged(object sender, EventArgs e) {

        }

        private void tabPage1_Click_1(object sender, EventArgs e) {

        }

        private void intensityTestButton_Click(object sender, EventArgs e) {
            _currentTracker.HapticIntensityTest();
        }

        private void audioHapticsActive_CheckedChanged(object sender, EventArgs e) {
            Configuration.Instance.AudioHapticsActive = audioHapticsActive.Checked;
            Configuration.Instance.SaveConfig();
        }

        private void yawSourceLabel_Click(object sender, EventArgs e) {

        }
    }
}
