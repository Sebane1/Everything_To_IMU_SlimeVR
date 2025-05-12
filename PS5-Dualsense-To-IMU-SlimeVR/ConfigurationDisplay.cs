using Everything_To_IMU_SlimeVR.Tracking;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Everything_To_IMU_SlimeVR.TrackerConfig;

namespace Everything_To_IMU_SlimeVR {
    public partial class ConfigurationDisplay : Form {
        private GenericControllerTrackerManager _genericControllerTranslator;
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
            if (_configuration.SwitchingSessions) {
                _configuration.LastCalibration = DateTime.UtcNow;
            }
            _genericControllerTranslator = new GenericControllerTrackerManager(_configuration);
            _genericControllerTranslator.OnTrackerError += _genericControllerTranslator_OnTrackerError;
            _genericControllerTranslator.PollingRate = _configuration.PollingRate;
            _forwardedWiimoteManager = new ForwardedWiimoteManager();
            _forwarded3DSDataManager = new Forwarded3DSDataManager();
            _configuration.SwitchingSessions = false;
            polllingRateLabel.Text = "Polling Rate: " + _configuration.PollingRate + "ms";
            pollingRate.Value = _configuration.PollingRate;
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
                GenericControllerTrackerManager.DebugOpen = true;
                refreshTimer.Interval = 1;
            } else {
                GenericControllerTrackerManager.DebugOpen = false;
                refreshTimer.Interval = 1000;
            }
            controllerDeviceList.Items.Clear();
            threeDsDeviceList.Items.Clear();
            wiimoteDeviceList.Items.Clear();
            try {
                foreach (var item in GenericControllerTrackerManager.Trackers) {
                    controllerDeviceList.Items.Add("Tracker " + item.Id);
                }
                foreach (var item in GenericControllerTrackerManager.Trackers3ds) {
                    threeDsDeviceList.Items.Add("Tracker " + item.Id);
                }
                foreach (var item in GenericControllerTrackerManager.TrackersWiimote) {
                    wiimoteDeviceList.Items.Add("Tracker " + item.Id);
                }
            } catch {

            }
            if (_currentTracker != null) {
                rediscoverTrackerButton.Visible = true;
                falseThighSimulationCheckBox.Visible = true;
            } else {
                falseThighSimulationCheckBox.Visible = false;
                rediscoverTrackerButton.Visible = false;
            }
            if (errorQueue.Count > 0) {
                var value = errorQueue.Dequeue();
                if (_lastErrorLog != value) {
                    errorLogText.Text += value + "\r\n";
                }
                _lastErrorLog = value;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }

        private void selectedDevice_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = controllerDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigs[currentIndex];
                _currentTracker = GenericControllerTrackerManager.Trackers[currentIndex];
            }
            RefreshTracker();
        }
        private void threeDsDeviceList_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = threeDsDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigs3ds[currentIndex];
                _currentTracker = GenericControllerTrackerManager.Trackers3ds[currentIndex];
            }
            RefreshTracker();
        }
        private void wiimoteDeviceList_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = wiimoteDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigWiimote[currentIndex];
                _currentTracker = GenericControllerTrackerManager.TrackersWiimote[currentIndex];
            }
            RefreshTracker();
        }

        private void nunchuckDeviceList_SelectedIndexChanged(object sender, EventArgs e) {
            refreshTimer.Stop();
            var currentIndex = nunchuckDeviceList.SelectedIndex;
            if (currentIndex >= 0) {
                _currentTrackerConfig = _configuration.TrackerConfigNunchuck[currentIndex];
                _currentTracker = GenericControllerTrackerManager.TrackersNunchuck[currentIndex];
            }
            RefreshTracker();
        }
        void RefreshTracker() {
            if (_currentTracker != null) {
                _suppressCheckBoxEvent = true;
                falseThighSimulationCheckBox.Checked = _currentTrackerConfig.SimulatesThighs;
                _currentTracker.SimulateThighs = _currentTrackerConfig.SimulatesThighs;

                yawForSimulatedTracker.SelectedIndex = (int)_currentTrackerConfig.YawReferenceTypeValue;
                _currentTracker.YawReferenceTypeValue = _currentTrackerConfig.YawReferenceTypeValue;

                trackerConfigLabel.Text = $"Tracker {_currentTracker.Id} Config";
                _suppressCheckBoxEvent = false;
                refreshTimer.Start();
            }
        }
        private void tabPage1_Click(object sender, EventArgs e) {

        }

        private void falseThighSimulationCheckBox_CheckedChanged(object sender, EventArgs e) {
            //yawForSimulatedTracker.Enabled = falseThighSimulationCheckBox.Checked;
            if (!_suppressCheckBoxEvent) {
                _currentTracker.SimulateThighs = falseThighSimulationCheckBox.Checked;
                _currentTrackerConfig.SimulatesThighs = falseThighSimulationCheckBox.Checked;
                _configuration.SaveConfig();
            }
        }

        private void rediscoverTrackerButton_Clicked(object sender, EventArgs e) {
            _currentTracker.Rediscover();
        }

        private void trackerCalibrationButton_Click(object sender, EventArgs e) {
            foreach (var item in GenericControllerTrackerManager.Trackers) {
                item.Recalibrate();
            }
            foreach (var item in GenericControllerTrackerManager.Trackers3ds) {
                item.Recalibrate();
            }
            foreach (var item in GenericControllerTrackerManager.TrackersWiimote) {
                item.Recalibrate();
            }
            foreach (var item in GenericControllerTrackerManager.TrackersNunchuck) {
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
                    //if (this.InvokeRequired) {
                    //    this.Invoke(new Action(() => this.SendToBack()));
                    //} else {
                    //    this.SendToBack();
                    //}
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
    }
}
