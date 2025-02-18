using PS5_Dualsense_To_IMU_SlimeVR.Tracking;

namespace PS5_Dualsense_To_IMU_SlimeVR
{
    public partial class DebugDisplay : Form {
        DualSenseTrackerManager dualSenseTranslator;
        public DebugDisplay() {
            InitializeComponent();
            dualSenseTranslator = new DualSenseTrackerManager();
        }

        private void refreshTimer_Tick(object sender, EventArgs e) {
            textBox1.Text = "";
            foreach (var item in dualSenseTranslator.Trackers) {
                textBox1.Text += item.Debug + "\r\n\r\n";
            }
        }
    }
}
