using PS5_Dualsense_To_IMU_SlimeVR.Tracking;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    public partial class DebugDisplay : Form {
       // DualSenseTrackerManager dualSenseTranslator;
        private GenericControllerTrackerManager genericControllerTranslator;

        public DebugDisplay() {
            InitializeComponent();
            ///dualSenseTranslator = new DualSenseTrackerManager();
            genericControllerTranslator = new GenericControllerTrackerManager();
        }

        private void refreshTimer_Tick(object sender, EventArgs e) {
            textBox1.Text = "";
            try {
                foreach (var item in genericControllerTranslator.Trackers) {
                    textBox1.Text += item.Debug + "\r\n\r\n";
                }
            } catch {

            }
        }
    }
}
