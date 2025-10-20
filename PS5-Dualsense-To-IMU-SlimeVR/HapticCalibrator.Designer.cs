namespace Everything_To_IMU_SlimeVR
{
    partial class HapticCalibrator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            intensityBar = new TrackBar();
            Intensity = new Label();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)intensityBar).BeginInit();
            SuspendLayout();
            // 
            // intensityBar
            // 
            intensityBar.Dock = DockStyle.Top;
            intensityBar.Location = new Point(0, 0);
            intensityBar.Maximum = 10000;
            intensityBar.Name = "intensityBar";
            intensityBar.Size = new Size(729, 45);
            intensityBar.TabIndex = 0;
            intensityBar.ValueChanged += intensityBar_ValueChanged;
            // 
            // Intensity
            // 
            Intensity.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            Intensity.AutoSize = true;
            Intensity.Location = new Point(339, 65);
            Intensity.Name = "Intensity";
            Intensity.Size = new Size(22, 15);
            Intensity.TabIndex = 3;
            Intensity.Text = "0.0";
            Intensity.Click += Intensity_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 90;
            timer1.Tick += timer1_Tick;
            // 
            // HapticCalibrator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(729, 103);
            Controls.Add(Intensity);
            Controls.Add(intensityBar);
            Name = "HapticCalibrator";
            Text = "HapticCalibrator";
            Load += HapticCalibrator_Load;
            ((System.ComponentModel.ISupportInitialize)intensityBar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TrackBar intensityBar;
        private Label Intensity;
        private System.Windows.Forms.Timer timer1;
    }
}