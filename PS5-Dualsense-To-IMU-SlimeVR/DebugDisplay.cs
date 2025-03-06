using PS5_Dualsense_To_IMU_SlimeVR.Tracking;

namespace PS5_Dualsense_To_IMU_SlimeVR {
    public partial class DebugDisplay : Form {
        // DualSenseTrackerManager dualSenseTranslator;
        private GenericControllerTrackerManager _genericControllerTranslator;
        Queue<string> errorQueue = new Queue<string>();

        public DebugDisplay() {
            InitializeComponent();
            _genericControllerTranslator = new GenericControllerTrackerManager();
            _genericControllerTranslator.OnTrackerError += _genericControllerTranslator_OnTrackerError;
        }

        private void _genericControllerTranslator_OnTrackerError(object? sender, string e) {
            errorQueue.Enqueue(e);
        }

        private void refreshTimer_Tick(object sender, EventArgs e) {
            debugText.Text = "";
            try {
                foreach (var item in _genericControllerTranslator.Trackers) {
                    debugText.Text += item.Debug + "\r\n\r\n";
                }
            } catch {

            }
            if (errorQueue.Count > 0) {
                var value = errorQueue.Dequeue();
                MessageBox.Show(value, "Tracking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {

        }
    }
}
