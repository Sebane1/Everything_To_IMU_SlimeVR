namespace Everything_To_IMU_SlimeVR {
    partial class ConfigurationDisplay {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            components = new System.ComponentModel.Container();
            refreshTimer = new System.Windows.Forms.Timer(components);
            controllerDeviceList = new ListBox();
            falseThighSimulationCheckBox = new CheckBox();
            tabOptions = new TabControl();
            settingsPage = new TabPage();
            yawForSimulatedTracker = new ComboBox();
            rediscoverTrackerButton = new Button();
            trackerConfigLabel = new Label();
            debugPage = new TabPage();
            debugText = new TextBox();
            errorLog = new TabPage();
            errorLogText = new TextBox();
            trackerCalibrationButton = new Button();
            donateButton = new Button();
            pollingRate = new TrackBar();
            polllingRateLabel = new Label();
            threeDsDeviceList = new ListBox();
            wiimoteDeviceList = new ListBox();
            nunchuckDeviceList = new ListBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            memoryResetTimer = new System.Windows.Forms.Timer(components);
            tabOptions.SuspendLayout();
            settingsPage.SuspendLayout();
            debugPage.SuspendLayout();
            errorLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pollingRate).BeginInit();
            SuspendLayout();
            // 
            // refreshTimer
            // 
            refreshTimer.Enabled = true;
            refreshTimer.Interval = 500;
            refreshTimer.Tick += refreshTimer_Tick;
            // 
            // controllerDeviceList
            // 
            controllerDeviceList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            controllerDeviceList.FormattingEnabled = true;
            controllerDeviceList.ItemHeight = 15;
            controllerDeviceList.Location = new Point(3, 22);
            controllerDeviceList.Name = "controllerDeviceList";
            controllerDeviceList.Size = new Size(104, 499);
            controllerDeviceList.TabIndex = 0;
            controllerDeviceList.SelectedIndexChanged += selectedDevice_SelectedIndexChanged;
            // 
            // falseThighSimulationCheckBox
            // 
            falseThighSimulationCheckBox.AutoSize = true;
            falseThighSimulationCheckBox.Location = new Point(10, 32);
            falseThighSimulationCheckBox.Name = "falseThighSimulationCheckBox";
            falseThighSimulationCheckBox.Size = new Size(236, 19);
            falseThighSimulationCheckBox.TabIndex = 1;
            falseThighSimulationCheckBox.Text = "Thigh Tracker Simulation (Experimental)";
            falseThighSimulationCheckBox.UseVisualStyleBackColor = true;
            falseThighSimulationCheckBox.CheckedChanged += falseThighSimulationCheckBox_CheckedChanged;
            // 
            // tabOptions
            // 
            tabOptions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabOptions.Controls.Add(settingsPage);
            tabOptions.Controls.Add(debugPage);
            tabOptions.Controls.Add(errorLog);
            tabOptions.Location = new Point(443, -2);
            tabOptions.Name = "tabOptions";
            tabOptions.SelectedIndex = 0;
            tabOptions.Size = new Size(343, 531);
            tabOptions.TabIndex = 2;
            // 
            // settingsPage
            // 
            settingsPage.Controls.Add(yawForSimulatedTracker);
            settingsPage.Controls.Add(rediscoverTrackerButton);
            settingsPage.Controls.Add(trackerConfigLabel);
            settingsPage.Controls.Add(falseThighSimulationCheckBox);
            settingsPage.Location = new Point(4, 24);
            settingsPage.Name = "settingsPage";
            settingsPage.Padding = new Padding(3);
            settingsPage.Size = new Size(335, 503);
            settingsPage.TabIndex = 0;
            settingsPage.Text = "Settings";
            settingsPage.UseVisualStyleBackColor = true;
            settingsPage.Click += tabPage1_Click;
            // 
            // yawForSimulatedTracker
            // 
            yawForSimulatedTracker.FormattingEnabled = true;
            yawForSimulatedTracker.Items.AddRange(new object[] { "HMD Yaw For Simulated Thighs (No Drift)", "Waist Yaw For Simulated Thighs (Waist Tracker Drift)", "Tracker Yaw For Simulated Thighs (Can Drift)" });
            yawForSimulatedTracker.Location = new Point(10, 57);
            yawForSimulatedTracker.Name = "yawForSimulatedTracker";
            yawForSimulatedTracker.Size = new Size(319, 23);
            yawForSimulatedTracker.TabIndex = 5;
            yawForSimulatedTracker.SelectedIndexChanged += yawForSimulatedTracker_SelectedIndexChanged;
            // 
            // rediscoverTrackerButton
            // 
            rediscoverTrackerButton.Location = new Point(10, 92);
            rediscoverTrackerButton.Name = "rediscoverTrackerButton";
            rediscoverTrackerButton.Size = new Size(117, 23);
            rediscoverTrackerButton.TabIndex = 3;
            rediscoverTrackerButton.Text = "Rediscover Tracker";
            rediscoverTrackerButton.UseVisualStyleBackColor = true;
            rediscoverTrackerButton.Click += rediscoverTrackerButton_Clicked;
            // 
            // trackerConfigLabel
            // 
            trackerConfigLabel.AutoSize = true;
            trackerConfigLabel.Location = new Point(6, 3);
            trackerConfigLabel.Name = "trackerConfigLabel";
            trackerConfigLabel.Size = new Size(121, 15);
            trackerConfigLabel.TabIndex = 2;
            trackerConfigLabel.Text = "Tracker Configuration";
            // 
            // debugPage
            // 
            debugPage.Controls.Add(debugText);
            debugPage.Location = new Point(4, 24);
            debugPage.Name = "debugPage";
            debugPage.Padding = new Padding(3);
            debugPage.Size = new Size(335, 503);
            debugPage.TabIndex = 1;
            debugPage.Text = "Debug";
            debugPage.UseVisualStyleBackColor = true;
            // 
            // debugText
            // 
            debugText.Location = new Point(6, 6);
            debugText.Multiline = true;
            debugText.Name = "debugText";
            debugText.Size = new Size(323, 471);
            debugText.TabIndex = 0;
            // 
            // errorLog
            // 
            errorLog.Controls.Add(errorLogText);
            errorLog.Location = new Point(4, 24);
            errorLog.Name = "errorLog";
            errorLog.Size = new Size(335, 503);
            errorLog.TabIndex = 2;
            errorLog.Text = "Error Log";
            errorLog.UseVisualStyleBackColor = true;
            // 
            // errorLogText
            // 
            errorLogText.Location = new Point(5, 3);
            errorLogText.Multiline = true;
            errorLogText.Name = "errorLogText";
            errorLogText.Size = new Size(332, 471);
            errorLogText.TabIndex = 0;
            // 
            // trackerCalibrationButton
            // 
            trackerCalibrationButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            trackerCalibrationButton.Location = new Point(445, 530);
            trackerCalibrationButton.Name = "trackerCalibrationButton";
            trackerCalibrationButton.Size = new Size(216, 23);
            trackerCalibrationButton.TabIndex = 4;
            trackerCalibrationButton.Text = "Recalibrate Controller Trackers";
            trackerCalibrationButton.UseVisualStyleBackColor = true;
            trackerCalibrationButton.Click += trackerCalibrationButton_Click;
            // 
            // donateButton
            // 
            donateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            donateButton.BackColor = Color.RosyBrown;
            donateButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            donateButton.ForeColor = Color.Snow;
            donateButton.Location = new Point(659, 530);
            donateButton.Name = "donateButton";
            donateButton.Size = new Size(127, 23);
            donateButton.TabIndex = 5;
            donateButton.Text = "Donate";
            donateButton.UseVisualStyleBackColor = false;
            donateButton.Click += donateButton_Click;
            // 
            // pollingRate
            // 
            pollingRate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pollingRate.Location = new Point(103, 557);
            pollingRate.Maximum = 120;
            pollingRate.Minimum = 8;
            pollingRate.Name = "pollingRate";
            pollingRate.Size = new Size(679, 45);
            pollingRate.TabIndex = 6;
            pollingRate.Value = 8;
            pollingRate.Scroll += pollingRate_Scroll;
            // 
            // polllingRateLabel
            // 
            polllingRateLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            polllingRateLabel.AutoSize = true;
            polllingRateLabel.Location = new Point(2, 561);
            polllingRateLabel.Name = "polllingRateLabel";
            polllingRateLabel.Size = new Size(105, 15);
            polllingRateLabel.TabIndex = 7;
            polllingRateLabel.Text = "Update Rate: 30ms";
            // 
            // threeDsDeviceList
            // 
            threeDsDeviceList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            threeDsDeviceList.FormattingEnabled = true;
            threeDsDeviceList.ItemHeight = 15;
            threeDsDeviceList.Location = new Point(113, 22);
            threeDsDeviceList.Name = "threeDsDeviceList";
            threeDsDeviceList.Size = new Size(104, 499);
            threeDsDeviceList.TabIndex = 8;
            threeDsDeviceList.SelectedIndexChanged += threeDsDeviceList_SelectedIndexChanged;
            // 
            // wiimoteDeviceList
            // 
            wiimoteDeviceList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            wiimoteDeviceList.FormattingEnabled = true;
            wiimoteDeviceList.ItemHeight = 15;
            wiimoteDeviceList.Location = new Point(223, 22);
            wiimoteDeviceList.Name = "wiimoteDeviceList";
            wiimoteDeviceList.Size = new Size(104, 499);
            wiimoteDeviceList.TabIndex = 9;
            wiimoteDeviceList.SelectedIndexChanged += wiimoteDeviceList_SelectedIndexChanged;
            // 
            // nunchuckDeviceList
            // 
            nunchuckDeviceList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            nunchuckDeviceList.FormattingEnabled = true;
            nunchuckDeviceList.ItemHeight = 15;
            nunchuckDeviceList.Location = new Point(333, 22);
            nunchuckDeviceList.Name = "nunchuckDeviceList";
            nunchuckDeviceList.Size = new Size(104, 499);
            nunchuckDeviceList.TabIndex = 10;
            nunchuckDeviceList.SelectedIndexChanged += nunchuckDeviceList_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 2);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.TabIndex = 11;
            label1.Text = "Local Bluetooth";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(113, 2);
            label2.Name = "label2";
            label2.Size = new Size(96, 15);
            label2.TabIndex = 12;
            label2.Text = "Remote 3DS/2DS";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(223, 2);
            label3.Name = "label3";
            label3.Size = new Size(96, 15);
            label3.TabIndex = 13;
            label3.Text = "Remote Wiimote";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(333, 2);
            label4.Name = "label4";
            label4.Size = new Size(106, 15);
            label4.TabIndex = 14;
            label4.Text = "Remote Nunchuck";
            // 
            // memoryResetTimer
            // 
            memoryResetTimer.Enabled = true;
            memoryResetTimer.Interval = 3600000;
            memoryResetTimer.Tick += memoryResetTimer_Tick;
            // 
            // ConfigurationDisplay
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(786, 606);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(nunchuckDeviceList);
            Controls.Add(wiimoteDeviceList);
            Controls.Add(threeDsDeviceList);
            Controls.Add(polllingRateLabel);
            Controls.Add(pollingRate);
            Controls.Add(donateButton);
            Controls.Add(trackerCalibrationButton);
            Controls.Add(tabOptions);
            Controls.Add(controllerDeviceList);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "ConfigurationDisplay";
            Text = "Configuration Display";
            Load += ConfigurationDisplay_Load;
            tabOptions.ResumeLayout(false);
            settingsPage.ResumeLayout(false);
            settingsPage.PerformLayout();
            debugPage.ResumeLayout(false);
            debugPage.PerformLayout();
            errorLog.ResumeLayout(false);
            errorLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pollingRate).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Timer refreshTimer;
        private ListBox controllerDeviceList;
        private CheckBox falseThighSimulationCheckBox;
        private TabControl tabOptions;
        private TabPage settingsPage;
        private TabPage debugPage;
        private TextBox debugText;
        private Label trackerConfigLabel;
        private Button rediscoverTrackerButton;
        private Button trackerCalibrationButton;
        private Button donateButton;
        private TrackBar pollingRate;
        private Label polllingRateLabel;
        private ComboBox yawForSimulatedTracker;
        private TabPage errorLog;
        private TextBox errorLogText;
        private ListBox threeDsDeviceList;
        private ListBox wiimoteDeviceList;
        private ListBox nunchuckDeviceList;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private System.Windows.Forms.Timer memoryResetTimer;
    }
}
