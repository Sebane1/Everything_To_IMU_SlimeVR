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
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            extensionYawForSimulatedTracker = new ComboBox();
            hapticJointAssignment = new ComboBox();
            indentifyButton = new Button();
            yawForSimulatedTracker = new ComboBox();
            rediscoverTrackerButton = new Button();
            trackerConfigLabel = new Label();
            debugPage = new TabPage();
            debugText = new TextBox();
            errorLog = new TabPage();
            errorLogText = new TextBox();
            trackerCalibrationButton = new Button();
            donateButton = new Button();
            threeDsDeviceList = new ListBox();
            wiimoteDeviceList = new ListBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            memoryResetTimer = new System.Windows.Forms.Timer(components);
            label4 = new Label();
            hapticDeviceList = new ListBox();
            newIpFeild = new TextBox();
            newHapticCellphoneButton = new Button();
            testHaptics = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            timer2 = new System.Windows.Forms.Timer(components);
            timer3 = new System.Windows.Forms.Timer(components);
            listRefreshTimer = new System.Windows.Forms.Timer(components);
            lockInDetectedDevicesButton = new Button();
            tabOptions.SuspendLayout();
            settingsPage.SuspendLayout();
            debugPage.SuspendLayout();
            errorLog.SuspendLayout();
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
            controllerDeviceList.Size = new Size(104, 379);
            controllerDeviceList.TabIndex = 0;
            controllerDeviceList.SelectedIndexChanged += selectedDevice_SelectedIndexChanged;
            // 
            // falseThighSimulationCheckBox
            // 
            falseThighSimulationCheckBox.AutoSize = true;
            falseThighSimulationCheckBox.Location = new Point(545, 439);
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
            tabOptions.Location = new Point(438, -2);
            tabOptions.Name = "tabOptions";
            tabOptions.SelectedIndex = 0;
            tabOptions.Size = new Size(348, 405);
            tabOptions.TabIndex = 2;
            // 
            // settingsPage
            // 
            settingsPage.Controls.Add(label7);
            settingsPage.Controls.Add(label6);
            settingsPage.Controls.Add(label5);
            settingsPage.Controls.Add(extensionYawForSimulatedTracker);
            settingsPage.Controls.Add(hapticJointAssignment);
            settingsPage.Controls.Add(indentifyButton);
            settingsPage.Controls.Add(yawForSimulatedTracker);
            settingsPage.Controls.Add(rediscoverTrackerButton);
            settingsPage.Controls.Add(trackerConfigLabel);
            settingsPage.Location = new Point(4, 24);
            settingsPage.Name = "settingsPage";
            settingsPage.Padding = new Padding(3);
            settingsPage.Size = new Size(340, 377);
            settingsPage.TabIndex = 0;
            settingsPage.Text = "Settings";
            settingsPage.UseVisualStyleBackColor = true;
            settingsPage.Click += tabPage1_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(10, 168);
            label7.Name = "label7";
            label7.Size = new Size(248, 15);
            label7.TabIndex = 11;
            label7.Text = "Haptic Joint Assignment (bHaptics Over OSC)";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(10, 113);
            label6.Name = "label6";
            label6.Size = new Size(222, 15);
            label6.TabIndex = 10;
            label6.Text = "Extension Yaw Source (IE: Wii Nunchuck)";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 60);
            label5.Name = "label5";
            label5.Size = new Size(111, 15);
            label5.TabIndex = 9;
            label5.Text = "Primary Yaw Source";
            // 
            // extensionYawForSimulatedTracker
            // 
            extensionYawForSimulatedTracker.FormattingEnabled = true;
            extensionYawForSimulatedTracker.Items.AddRange(new object[] { "HMD Yaw (No Drift)", "Waist Yaw (Waist Tracker Drift)", "Chest Tracker Yaw (Chest Tracker Drift)", "Device Tracker Yaw (Device Drift)" });
            extensionYawForSimulatedTracker.Location = new Point(10, 131);
            extensionYawForSimulatedTracker.Name = "extensionYawForSimulatedTracker";
            extensionYawForSimulatedTracker.Size = new Size(319, 23);
            extensionYawForSimulatedTracker.TabIndex = 8;
            extensionYawForSimulatedTracker.SelectedIndexChanged += extensionYawForSimulatedTracker_SelectedIndexChanged;
            // 
            // hapticJointAssignment
            // 
            hapticJointAssignment.FormattingEnabled = true;
            hapticJointAssignment.Items.AddRange(new object[] { "Right Thigh Haptics", "Right Calf Haptics", "Left Thigh Haptics", "Left Calf Haptics", "Right Upper Arm Haptics", "Right Fore Arm Haptics", "Left Upper Arm Haptics", "Left Fore Arm Haptics", "Chest Haptics", "Right Foot Haptics", "Left Foot Haptics", "Right Hand Haptics", "Left Hand Haptics", "Right Shoulder Haptics", "Left Shoulder Haptics", "Head Haptics", "Hips Haptics", "Chest Front Haptics", "Hips Front Haptics", "Chest Back Haptics", "Hips Back Haptics", "Chest And Hips Haptics", "Chest And Hips Front Haptics", "Chest And Hips Back Haptics" });
            hapticJointAssignment.Location = new Point(10, 186);
            hapticJointAssignment.Name = "hapticJointAssignment";
            hapticJointAssignment.Size = new Size(319, 23);
            hapticJointAssignment.TabIndex = 7;
            hapticJointAssignment.SelectedIndexChanged += hapticJointAssignment_SelectedIndexChanged;
            // 
            // indentifyButton
            // 
            indentifyButton.Location = new Point(165, 213);
            indentifyButton.Name = "indentifyButton";
            indentifyButton.Size = new Size(164, 23);
            indentifyButton.TabIndex = 6;
            indentifyButton.Text = "Rumble Identification";
            indentifyButton.UseVisualStyleBackColor = true;
            indentifyButton.Click += identifyButton_Click;
            // 
            // yawForSimulatedTracker
            // 
            yawForSimulatedTracker.FormattingEnabled = true;
            yawForSimulatedTracker.Items.AddRange(new object[] { "HMD Yaw (No Drift)", "Waist Yaw (Waist Tracker Drift)", "Chest Tracker Yaw (Chest Tracker Drift)", "Left Ankle Tracker Yaw (Ankle Tracker Drift)", "Right Ankle Tracker Yaw (Ankle Tracker Drift)", "Device Tracker Yaw (Device Drift)" });
            yawForSimulatedTracker.Location = new Point(10, 78);
            yawForSimulatedTracker.Name = "yawForSimulatedTracker";
            yawForSimulatedTracker.Size = new Size(319, 23);
            yawForSimulatedTracker.TabIndex = 5;
            yawForSimulatedTracker.SelectedIndexChanged += yawForSimulatedTracker_SelectedIndexChanged;
            // 
            // rediscoverTrackerButton
            // 
            rediscoverTrackerButton.Location = new Point(10, 213);
            rediscoverTrackerButton.Name = "rediscoverTrackerButton";
            rediscoverTrackerButton.Size = new Size(149, 23);
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
            debugPage.Size = new Size(340, 377);
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
            errorLog.Size = new Size(340, 377);
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
            trackerCalibrationButton.Location = new Point(442, 408);
            trackerCalibrationButton.Name = "trackerCalibrationButton";
            trackerCalibrationButton.Size = new Size(212, 23);
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
            donateButton.Location = new Point(652, 408);
            donateButton.Name = "donateButton";
            donateButton.Size = new Size(127, 23);
            donateButton.TabIndex = 5;
            donateButton.Text = "Donate";
            donateButton.UseVisualStyleBackColor = false;
            donateButton.Click += donateButton_Click;
            // 
            // threeDsDeviceList
            // 
            threeDsDeviceList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            threeDsDeviceList.FormattingEnabled = true;
            threeDsDeviceList.ItemHeight = 15;
            threeDsDeviceList.Location = new Point(113, 22);
            threeDsDeviceList.Name = "threeDsDeviceList";
            threeDsDeviceList.Size = new Size(104, 379);
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
            wiimoteDeviceList.Size = new Size(104, 379);
            wiimoteDeviceList.TabIndex = 9;
            wiimoteDeviceList.SelectedIndexChanged += wiimoteDeviceList_SelectedIndexChanged;
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
            // memoryResetTimer
            // 
            memoryResetTimer.Enabled = true;
            memoryResetTimer.Interval = 3600000;
            memoryResetTimer.Tick += memoryResetTimer_Tick;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(332, 2);
            label4.Name = "label4";
            label4.Size = new Size(99, 15);
            label4.TabIndex = 15;
            label4.Text = "Haptic Cellphone";
            label4.Click += label4_Click;
            // 
            // hapticDeviceList
            // 
            hapticDeviceList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            hapticDeviceList.FormattingEnabled = true;
            hapticDeviceList.ItemHeight = 15;
            hapticDeviceList.Location = new Point(332, 52);
            hapticDeviceList.Name = "hapticDeviceList";
            hapticDeviceList.Size = new Size(104, 349);
            hapticDeviceList.TabIndex = 14;
            hapticDeviceList.SelectedIndexChanged += hapticDeviceList_SelectedIndexChanged;
            // 
            // newIpFeild
            // 
            newIpFeild.Location = new Point(331, 22);
            newIpFeild.Name = "newIpFeild";
            newIpFeild.Size = new Size(105, 23);
            newIpFeild.TabIndex = 16;
            // 
            // newHapticCellphoneButton
            // 
            newHapticCellphoneButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            newHapticCellphoneButton.Location = new Point(331, -137);
            newHapticCellphoneButton.Name = "newHapticCellphoneButton";
            newHapticCellphoneButton.Size = new Size(105, 23);
            newHapticCellphoneButton.TabIndex = 17;
            newHapticCellphoneButton.Text = "Add Haptic IP";
            newHapticCellphoneButton.UseVisualStyleBackColor = true;
            newHapticCellphoneButton.Click += newHapticCellphoneButton_Click;
            // 
            // testHaptics
            // 
            testHaptics.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            testHaptics.Location = new Point(442, 437);
            testHaptics.Name = "testHaptics";
            testHaptics.Size = new Size(94, 23);
            testHaptics.TabIndex = 18;
            testHaptics.Text = "Test Haptics";
            testHaptics.UseVisualStyleBackColor = true;
            testHaptics.Click += testHaptics_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 500;
            // 
            // timer2
            // 
            timer2.Enabled = true;
            timer2.Interval = 500;
            // 
            // timer3
            // 
            timer3.Enabled = true;
            timer3.Interval = 500;
            // 
            // listRefreshTimer
            // 
            listRefreshTimer.Enabled = true;
            listRefreshTimer.Interval = 15000;
            listRefreshTimer.Tick += listRefreshTimer_Tick;
            // 
            // lockInDetectedDevicesButton
            // 
            lockInDetectedDevicesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lockInDetectedDevicesButton.Location = new Point(7, 408);
            lockInDetectedDevicesButton.Name = "lockInDetectedDevicesButton";
            lockInDetectedDevicesButton.Size = new Size(429, 52);
            lockInDetectedDevicesButton.TabIndex = 21;
            lockInDetectedDevicesButton.Text = "Disable New Device Detection (Reduces Drift)";
            lockInDetectedDevicesButton.UseVisualStyleBackColor = true;
            lockInDetectedDevicesButton.Click += lockInDetectedDevicesButton_Click;
            // 
            // ConfigurationDisplay
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(790, 461);
            Controls.Add(lockInDetectedDevicesButton);
            Controls.Add(testHaptics);
            Controls.Add(newHapticCellphoneButton);
            Controls.Add(newIpFeild);
            Controls.Add(label4);
            Controls.Add(hapticDeviceList);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(falseThighSimulationCheckBox);
            Controls.Add(wiimoteDeviceList);
            Controls.Add(threeDsDeviceList);
            Controls.Add(donateButton);
            Controls.Add(trackerCalibrationButton);
            Controls.Add(tabOptions);
            Controls.Add(controllerDeviceList);
            DoubleBuffered = true;
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
        private Button indentifyButton;
        private ComboBox hapticJointAssignment;
        private ListBox hapticDeviceList;
        private TextBox newIpFeild;
        private Button newHapticCellphoneButton;
        private Button testHaptics;
        private Label wiimoteRateLabel;
        private TrackBar wiimoteRate;
        private ComboBox extensionYawForSimulatedTracker;
        private Label label7;
        private Label label6;
        private Label label5;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.Timer listRefreshTimer;
        private Button lockInDetectedDevicesButton;
    }
}
