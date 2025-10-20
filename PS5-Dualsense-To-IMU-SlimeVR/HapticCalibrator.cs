using Everything_To_IMU_SlimeVR.Tracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Everything_To_IMU_SlimeVR
{
    public partial class HapticCalibrator : Form
    {
        public HapticCalibrator()
        {
            InitializeComponent();
        }
        IBodyTracker bodyTracker;
        private float intensity;

        public IBodyTracker BodyTracker { get => bodyTracker; set => bodyTracker = value; }

        private void HapticCalibrator_Load(object sender, EventArgs e)
        {

        }

        private void Intensity_Click(object sender, EventArgs e)
        {

        }

        private void intensityBar_ValueChanged(object sender, EventArgs e)
        {
            if (bodyTracker != null)
            {
                intensity = (float)intensityBar.Value / (float)intensityBar.Maximum;
                Intensity.Text = intensity.ToString();
                if (bodyTracker != null)
                {
                    bodyTracker.EngageHaptics(50, intensity);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            bodyTracker.EngageHaptics(90, intensity);
        }
    }
}
