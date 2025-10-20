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
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            refreshTimer = new System.Windows.Forms.Timer(components);
            controllerDeviceList = new ListBox();
            falseThighSimulationCheckBox = new CheckBox();
            tabOptions = new TabControl();
            trackerSettingsPage = new TabPage();
            hapticValueTestButton = new Button();
            intensityTestButton = new Button();
            yawSourceDisclaimer2 = new Label();
            yawSourceDisclaimer1 = new Label();
            hapticJointAssignmentLabel = new Label();
            extensionSourceLabel = new Label();
            yawSourceLabel = new Label();
            extensionYawForSimulatedTracker = new ComboBox();
            hapticJointAssignment = new ComboBox();
            identifyButton = new Button();
            yawForSimulatedTracker = new ComboBox();
            rediscoverTrackerButton = new Button();
            trackerConfigLabel = new Label();
            debugPage = new TabPage();
            debugText = new TextBox();
            errorLog = new TabPage();
            errorLogText = new TextBox();
            oscSettings = new TabPage();
            removePortOutputButton = new Button();
            addOutputPortButton = new Button();
            label6 = new Label();
            oscPortOutputTextBox = new TextBox();
            label5 = new Label();
            oscOutputPorts = new ListBox();
            oscPortIn = new Label();
            oscIpLabel = new Label();
            oscInputPortTextBox = new TextBox();
            oscIpAddressTextBox = new TextBox();
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
            label10 = new Label();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            audioHapticsActive = new CheckBox();
            midiButtonTest = new Button();
            tabOptions.SuspendLayout();
            trackerSettingsPage.SuspendLayout();
            debugPage.SuspendLayout();
            errorLog.SuspendLayout();
            oscSettings.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
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
            controllerDeviceList.FormattingEnabled = true;
            controllerDeviceList.ItemHeight = 15;
            controllerDeviceList.Location = new Point(0, 26);
            controllerDeviceList.Name = "controllerDeviceList";
            controllerDeviceList.Size = new Size(104, 244);
            controllerDeviceList.TabIndex = 0;
            controllerDeviceList.SelectedIndexChanged += selectedDevice_SelectedIndexChanged;
            // 
            // falseThighSimulationCheckBox
            // 
            falseThighSimulationCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            falseThighSimulationCheckBox.AutoSize = true;
            falseThighSimulationCheckBox.Location = new Point(545, 437);
            falseThighSimulationCheckBox.Name = "falseThighSimulationCheckBox";
            falseThighSimulationCheckBox.Size = new Size(236, 19);
            falseThighSimulationCheckBox.TabIndex = 1;
            falseThighSimulationCheckBox.Text = "Thigh Tracker Simulation (Experimental)";
            falseThighSimulationCheckBox.UseVisualStyleBackColor = true;
            falseThighSimulationCheckBox.CheckedChanged += falseThighSimulationCheckBox_CheckedChanged;
            // 
            // tabOptions
            // 
            tabOptions.Controls.Add(trackerSettingsPage);
            tabOptions.Controls.Add(debugPage);
            tabOptions.Controls.Add(errorLog);
            tabOptions.Controls.Add(oscSettings);
            tabOptions.Location = new Point(231, 2);
            tabOptions.Name = "tabOptions";
            tabOptions.SelectedIndex = 0;
            tabOptions.Size = new Size(341, 298);
            tabOptions.TabIndex = 2;
            // 
            // trackerSettingsPage
            // 
            trackerSettingsPage.Controls.Add(hapticValueTestButton);
            trackerSettingsPage.Controls.Add(intensityTestButton);
            trackerSettingsPage.Controls.Add(yawSourceDisclaimer2);
            trackerSettingsPage.Controls.Add(yawSourceDisclaimer1);
            trackerSettingsPage.Controls.Add(hapticJointAssignmentLabel);
            trackerSettingsPage.Controls.Add(extensionSourceLabel);
            trackerSettingsPage.Controls.Add(yawSourceLabel);
            trackerSettingsPage.Controls.Add(extensionYawForSimulatedTracker);
            trackerSettingsPage.Controls.Add(hapticJointAssignment);
            trackerSettingsPage.Controls.Add(identifyButton);
            trackerSettingsPage.Controls.Add(yawForSimulatedTracker);
            trackerSettingsPage.Controls.Add(rediscoverTrackerButton);
            trackerSettingsPage.Controls.Add(trackerConfigLabel);
            trackerSettingsPage.Location = new Point(4, 24);
            trackerSettingsPage.Name = "trackerSettingsPage";
            trackerSettingsPage.Padding = new Padding(3);
            trackerSettingsPage.Size = new Size(333, 270);
            trackerSettingsPage.TabIndex = 0;
            trackerSettingsPage.Text = "Tracker Settings";
            trackerSettingsPage.UseVisualStyleBackColor = true;
            trackerSettingsPage.Click += tabPage1_Click;
            // 
            // hapticValueTestButton
            // 
            hapticValueTestButton.Location = new Point(5, 244);
            hapticValueTestButton.Name = "hapticValueTestButton";
            hapticValueTestButton.RightToLeft = RightToLeft.Yes;
            hapticValueTestButton.Size = new Size(119, 23);
            hapticValueTestButton.TabIndex = 15;
            hapticValueTestButton.Text = "Haptic Value Tester";
            hapticValueTestButton.UseVisualStyleBackColor = true;
            hapticValueTestButton.Click += hapticValueButton_Click;
            // 
            // intensityTestButton
            // 
            intensityTestButton.Location = new Point(173, 215);
            intensityTestButton.Name = "intensityTestButton";
            intensityTestButton.Size = new Size(154, 23);
            intensityTestButton.TabIndex = 14;
            intensityTestButton.Text = "Intensity Test";
            intensityTestButton.UseVisualStyleBackColor = true;
            intensityTestButton.Visible = false;
            intensityTestButton.Click += intensityTestButton_Click;
            // 
            // yawSourceDisclaimer2
            // 
            yawSourceDisclaimer2.AutoSize = true;
            yawSourceDisclaimer2.Location = new Point(5, 41);
            yawSourceDisclaimer2.Name = "yawSourceDisclaimer2";
            yawSourceDisclaimer2.Size = new Size(322, 15);
            yawSourceDisclaimer2.TabIndex = 13;
            yawSourceDisclaimer2.Text = "Device Tracker Yaw has more mounting flexibility, but drifts.";
            yawSourceDisclaimer2.Visible = false;
            // 
            // yawSourceDisclaimer1
            // 
            yawSourceDisclaimer1.AutoSize = true;
            yawSourceDisclaimer1.Location = new Point(5, 22);
            yawSourceDisclaimer1.Name = "yawSourceDisclaimer1";
            yawSourceDisclaimer1.Size = new Size(321, 15);
            yawSourceDisclaimer1.TabIndex = 12;
            yawSourceDisclaimer1.Text = "Yaw sources will require front or back mounting in SlimeVR.";
            yawSourceDisclaimer1.Visible = false;
            // 
            // hapticJointAssignmentLabel
            // 
            hapticJointAssignmentLabel.AutoSize = true;
            hapticJointAssignmentLabel.Location = new Point(6, 169);
            hapticJointAssignmentLabel.Name = "hapticJointAssignmentLabel";
            hapticJointAssignmentLabel.Size = new Size(267, 15);
            hapticJointAssignmentLabel.TabIndex = 11;
            hapticJointAssignmentLabel.Text = "Haptic Joint Assignment (bHaptics compatibility)";
            hapticJointAssignmentLabel.Visible = false;
            hapticJointAssignmentLabel.Click += label7_Click;
            // 
            // extensionSourceLabel
            // 
            extensionSourceLabel.AutoSize = true;
            extensionSourceLabel.Location = new Point(6, 114);
            extensionSourceLabel.Name = "extensionSourceLabel";
            extensionSourceLabel.Size = new Size(222, 15);
            extensionSourceLabel.TabIndex = 10;
            extensionSourceLabel.Text = "Extension Yaw Source (IE: Wii Nunchuck)";
            extensionSourceLabel.Visible = false;
            // 
            // yawSourceLabel
            // 
            yawSourceLabel.AutoSize = true;
            yawSourceLabel.Location = new Point(6, 61);
            yawSourceLabel.Name = "yawSourceLabel";
            yawSourceLabel.Size = new Size(111, 15);
            yawSourceLabel.TabIndex = 9;
            yawSourceLabel.Text = "Primary Yaw Source";
            yawSourceLabel.Visible = false;
            yawSourceLabel.Click += yawSourceLabel_Click;
            // 
            // extensionYawForSimulatedTracker
            // 
            extensionYawForSimulatedTracker.FormattingEnabled = true;
            extensionYawForSimulatedTracker.Items.AddRange(new object[] { "HMD Yaw (No Drift)", "Waist Yaw (Waist Tracker Drift)", "Chest Tracker Yaw (Chest Tracker Drift)", "Device Tracker Yaw (Device Drift)" });
            extensionYawForSimulatedTracker.Location = new Point(6, 132);
            extensionYawForSimulatedTracker.Name = "extensionYawForSimulatedTracker";
            extensionYawForSimulatedTracker.Size = new Size(323, 23);
            extensionYawForSimulatedTracker.TabIndex = 8;
            extensionYawForSimulatedTracker.Visible = false;
            extensionYawForSimulatedTracker.SelectedIndexChanged += extensionYawForSimulatedTracker_SelectedIndexChanged;
            // 
            // hapticJointAssignment
            // 
            hapticJointAssignment.FormattingEnabled = true;
            hapticJointAssignment.Items.AddRange(new object[] { "Right Thigh Haptics", "Right Calf Haptics", "Left Thigh Haptics", "Left Calf Haptics", "Right Upper Arm Haptics", "Right Fore Arm Haptics", "Left Upper Arm Haptics", "Left Fore Arm Haptics", "Chest Haptics", "Right Foot Haptics", "Left Foot Haptics", "Right Hand Haptics", "Left Hand Haptics", "Right Shoulder Haptics", "Left Shoulder Haptics", "Head Haptics", "Hips Haptics", "Chest Front Haptics", "Hips Front Haptics", "Chest Back Haptics", "Hips Back Haptics", "Chest And Hips Haptics", "Chest And Hips Front Haptics", "Chest And Hips Back Haptics" });
            hapticJointAssignment.Location = new Point(6, 187);
            hapticJointAssignment.Name = "hapticJointAssignment";
            hapticJointAssignment.Size = new Size(323, 23);
            hapticJointAssignment.TabIndex = 7;
            hapticJointAssignment.Visible = false;
            hapticJointAssignment.SelectedIndexChanged += hapticJointAssignment_SelectedIndexChanged;
            // 
            // identifyButton
            // 
            identifyButton.Location = new Point(88, 215);
            identifyButton.Name = "identifyButton";
            identifyButton.Size = new Size(79, 23);
            identifyButton.TabIndex = 6;
            identifyButton.Text = "Identify";
            identifyButton.UseVisualStyleBackColor = true;
            identifyButton.Visible = false;
            identifyButton.Click += identifyButton_Click;
            // 
            // yawForSimulatedTracker
            // 
            yawForSimulatedTracker.FormattingEnabled = true;
            yawForSimulatedTracker.Items.AddRange(new object[] { "HMD Yaw (No Drift)", "Waist Yaw (Waist Tracker Drift)", "Chest Tracker Yaw (Chest Tracker Drift)", "Left Ankle Tracker Yaw (Ankle Tracker Drift)", "Right Ankle Tracker Yaw (Ankle Tracker Drift)", "Device Tracker Yaw (Device Drift)" });
            yawForSimulatedTracker.Location = new Point(6, 79);
            yawForSimulatedTracker.Name = "yawForSimulatedTracker";
            yawForSimulatedTracker.Size = new Size(323, 23);
            yawForSimulatedTracker.TabIndex = 5;
            yawForSimulatedTracker.Visible = false;
            yawForSimulatedTracker.SelectedIndexChanged += yawForSimulatedTracker_SelectedIndexChanged;
            // 
            // rediscoverTrackerButton
            // 
            rediscoverTrackerButton.Location = new Point(6, 215);
            rediscoverTrackerButton.Name = "rediscoverTrackerButton";
            rediscoverTrackerButton.RightToLeft = RightToLeft.Yes;
            rediscoverTrackerButton.Size = new Size(76, 23);
            rediscoverTrackerButton.TabIndex = 3;
            rediscoverTrackerButton.Text = "Rediscover";
            rediscoverTrackerButton.UseVisualStyleBackColor = true;
            rediscoverTrackerButton.Visible = false;
            rediscoverTrackerButton.Click += rediscoverTrackerButton_Clicked;
            // 
            // trackerConfigLabel
            // 
            trackerConfigLabel.AutoSize = true;
            trackerConfigLabel.Location = new Point(3, 3);
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
            debugPage.Size = new Size(333, 270);
            debugPage.TabIndex = 1;
            debugPage.Text = "Debug";
            debugPage.UseVisualStyleBackColor = true;
            // 
            // debugText
            // 
            debugText.Location = new Point(6, 6);
            debugText.Multiline = true;
            debugText.Name = "debugText";
            debugText.Size = new Size(321, 258);
            debugText.TabIndex = 0;
            // 
            // errorLog
            // 
            errorLog.Controls.Add(errorLogText);
            errorLog.Location = new Point(4, 24);
            errorLog.Name = "errorLog";
            errorLog.Size = new Size(333, 270);
            errorLog.TabIndex = 2;
            errorLog.Text = "Error Log";
            errorLog.UseVisualStyleBackColor = true;
            // 
            // errorLogText
            // 
            errorLogText.Location = new Point(5, 3);
            errorLogText.Multiline = true;
            errorLogText.Name = "errorLogText";
            errorLogText.Size = new Size(323, 264);
            errorLogText.TabIndex = 0;
            // 
            // oscSettings
            // 
            oscSettings.Controls.Add(removePortOutputButton);
            oscSettings.Controls.Add(addOutputPortButton);
            oscSettings.Controls.Add(label6);
            oscSettings.Controls.Add(oscPortOutputTextBox);
            oscSettings.Controls.Add(label5);
            oscSettings.Controls.Add(oscOutputPorts);
            oscSettings.Controls.Add(oscPortIn);
            oscSettings.Controls.Add(oscIpLabel);
            oscSettings.Controls.Add(oscInputPortTextBox);
            oscSettings.Controls.Add(oscIpAddressTextBox);
            oscSettings.Location = new Point(4, 24);
            oscSettings.Name = "oscSettings";
            oscSettings.Size = new Size(333, 270);
            oscSettings.TabIndex = 3;
            oscSettings.Text = "OSC Settings";
            oscSettings.UseVisualStyleBackColor = true;
            oscSettings.Click += oscSettings_Click;
            // 
            // removePortOutputButton
            // 
            removePortOutputButton.Anchor = AnchorStyles.Bottom;
            removePortOutputButton.Location = new Point(171, 243);
            removePortOutputButton.Name = "removePortOutputButton";
            removePortOutputButton.Size = new Size(157, 24);
            removePortOutputButton.TabIndex = 25;
            removePortOutputButton.Text = "Remove Port Output";
            removePortOutputButton.UseVisualStyleBackColor = true;
            removePortOutputButton.Click += removePortOutputButton_Click;
            // 
            // addOutputPortButton
            // 
            addOutputPortButton.Anchor = AnchorStyles.Bottom;
            addOutputPortButton.Location = new Point(7, 243);
            addOutputPortButton.Name = "addOutputPortButton";
            addOutputPortButton.Size = new Size(157, 24);
            addOutputPortButton.TabIndex = 24;
            addOutputPortButton.Text = "Add Port Output";
            addOutputPortButton.UseVisualStyleBackColor = true;
            addOutputPortButton.Click += addOutputPortButton_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 219);
            label6.Name = "label6";
            label6.Size = new Size(96, 15);
            label6.TabIndex = 7;
            label6.Text = "OSC Port Output";
            // 
            // oscPortOutputTextBox
            // 
            oscPortOutputTextBox.Location = new Point(108, 216);
            oscPortOutputTextBox.Name = "oscPortOutputTextBox";
            oscPortOutputTextBox.Size = new Size(220, 23);
            oscPortOutputTextBox.TabIndex = 6;
            oscPortOutputTextBox.TextChanged += textBox1_TextChanged_2;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(5, 80);
            label5.Name = "label5";
            label5.Size = new Size(101, 15);
            label5.TabIndex = 5;
            label5.Text = "OSC Port Outputs";
            label5.Click += label5_Click;
            // 
            // oscOutputPorts
            // 
            oscOutputPorts.FormattingEnabled = true;
            oscOutputPorts.ItemHeight = 15;
            oscOutputPorts.Location = new Point(5, 101);
            oscOutputPorts.Name = "oscOutputPorts";
            oscOutputPorts.Size = new Size(323, 109);
            oscOutputPorts.TabIndex = 4;
            // 
            // oscPortIn
            // 
            oscPortIn.AutoSize = true;
            oscPortIn.Location = new Point(5, 41);
            oscPortIn.Name = "oscPortIn";
            oscPortIn.Size = new Size(86, 15);
            oscPortIn.TabIndex = 3;
            oscPortIn.Text = "OSC Port Input";
            // 
            // oscIpLabel
            // 
            oscIpLabel.AutoSize = true;
            oscIpLabel.Location = new Point(6, 12);
            oscIpLabel.Name = "oscIpLabel";
            oscIpLabel.Size = new Size(88, 15);
            oscIpLabel.TabIndex = 2;
            oscIpLabel.Text = "OSC IP Address";
            // 
            // oscInputPortTextBox
            // 
            oscInputPortTextBox.Location = new Point(108, 38);
            oscInputPortTextBox.Name = "oscInputPortTextBox";
            oscInputPortTextBox.Size = new Size(220, 23);
            oscInputPortTextBox.TabIndex = 1;
            oscInputPortTextBox.Text = "9001";
            oscInputPortTextBox.Leave += oscInputPortTextBox_Leave;
            // 
            // oscIpAddressTextBox
            // 
            oscIpAddressTextBox.Location = new Point(108, 9);
            oscIpAddressTextBox.Name = "oscIpAddressTextBox";
            oscIpAddressTextBox.Size = new Size(220, 23);
            oscIpAddressTextBox.TabIndex = 0;
            oscIpAddressTextBox.Text = "127.0.0.1";
            oscIpAddressTextBox.Leave += oscIpAddressTextBox_Leave;
            // 
            // trackerCalibrationButton
            // 
            trackerCalibrationButton.Anchor = AnchorStyles.Bottom;
            trackerCalibrationButton.Location = new Point(5, 301);
            trackerCalibrationButton.Name = "trackerCalibrationButton";
            trackerCalibrationButton.Size = new Size(223, 23);
            trackerCalibrationButton.TabIndex = 4;
            trackerCalibrationButton.Text = "Recalibrate";
            trackerCalibrationButton.UseVisualStyleBackColor = true;
            trackerCalibrationButton.Click += trackerCalibrationButton_Click;
            // 
            // donateButton
            // 
            donateButton.Anchor = AnchorStyles.Bottom;
            donateButton.BackColor = Color.RosyBrown;
            donateButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            donateButton.ForeColor = Color.Snow;
            donateButton.Location = new Point(5, 330);
            donateButton.Name = "donateButton";
            donateButton.Size = new Size(223, 25);
            donateButton.TabIndex = 5;
            donateButton.Text = "Donate";
            donateButton.UseVisualStyleBackColor = false;
            donateButton.Click += donateButton_Click;
            // 
            // threeDsDeviceList
            // 
            threeDsDeviceList.FormattingEnabled = true;
            threeDsDeviceList.ItemHeight = 15;
            threeDsDeviceList.Location = new Point(4, 25);
            threeDsDeviceList.Name = "threeDsDeviceList";
            threeDsDeviceList.Size = new Size(104, 244);
            threeDsDeviceList.TabIndex = 8;
            threeDsDeviceList.SelectedIndexChanged += threeDsDeviceList_SelectedIndexChanged;
            // 
            // wiimoteDeviceList
            // 
            wiimoteDeviceList.FormattingEnabled = true;
            wiimoteDeviceList.ItemHeight = 15;
            wiimoteDeviceList.Location = new Point(114, 25);
            wiimoteDeviceList.Name = "wiimoteDeviceList";
            wiimoteDeviceList.Size = new Size(104, 244);
            wiimoteDeviceList.TabIndex = 9;
            wiimoteDeviceList.SelectedIndexChanged += wiimoteDeviceList_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 6);
            label1.Name = "label1";
            label1.Size = new Size(90, 15);
            label1.TabIndex = 11;
            label1.Text = "Local Bluetooth";
            label1.Click += label1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 5);
            label2.Name = "label2";
            label2.Size = new Size(96, 15);
            label2.TabIndex = 12;
            label2.Text = "Remote 3DS/2DS";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(114, 5);
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
            label4.Location = new Point(107, 6);
            label4.Name = "label4";
            label4.Size = new Size(104, 15);
            label4.TabIndex = 15;
            label4.Text = "Haptic Wifi Device";
            label4.Click += label4_Click;
            // 
            // hapticDeviceList
            // 
            hapticDeviceList.FormattingEnabled = true;
            hapticDeviceList.ItemHeight = 15;
            hapticDeviceList.Location = new Point(109, 86);
            hapticDeviceList.Name = "hapticDeviceList";
            hapticDeviceList.Size = new Size(104, 184);
            hapticDeviceList.TabIndex = 14;
            hapticDeviceList.SelectedIndexChanged += hapticDeviceList_SelectedIndexChanged;
            // 
            // newIpFeild
            // 
            newIpFeild.Location = new Point(108, 26);
            newIpFeild.Name = "newIpFeild";
            newIpFeild.Size = new Size(105, 23);
            newIpFeild.TabIndex = 16;
            newIpFeild.TextChanged += newIpFeild_TextChanged;
            // 
            // newHapticCellphoneButton
            // 
            newHapticCellphoneButton.Anchor = AnchorStyles.Top;
            newHapticCellphoneButton.Location = new Point(106, 55);
            newHapticCellphoneButton.Name = "newHapticCellphoneButton";
            newHapticCellphoneButton.Size = new Size(105, 23);
            newHapticCellphoneButton.TabIndex = 17;
            newHapticCellphoneButton.Text = "Add Haptic IP";
            newHapticCellphoneButton.UseVisualStyleBackColor = true;
            newHapticCellphoneButton.Click += newHapticCellphoneButton_Click;
            // 
            // testHaptics
            // 
            testHaptics.Anchor = AnchorStyles.Bottom;
            testHaptics.Location = new Point(464, 301);
            testHaptics.Name = "testHaptics";
            testHaptics.Size = new Size(99, 24);
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
            listRefreshTimer.Interval = 3000;
            listRefreshTimer.Tick += listRefreshTimer_Tick;
            // 
            // lockInDetectedDevicesButton
            // 
            lockInDetectedDevicesButton.Anchor = AnchorStyles.Bottom;
            lockInDetectedDevicesButton.Location = new Point(235, 331);
            lockInDetectedDevicesButton.Name = "lockInDetectedDevicesButton";
            lockInDetectedDevicesButton.Size = new Size(328, 24);
            lockInDetectedDevicesButton.TabIndex = 21;
            lockInDetectedDevicesButton.Text = "Disable New Device Detection (Reduces Drift)";
            lockInDetectedDevicesButton.UseVisualStyleBackColor = true;
            lockInDetectedDevicesButton.Click += lockInDetectedDevicesButton_Click;
            // 
            // label10
            // 
            label10.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            label10.AutoSize = true;
            label10.Location = new Point(12, 403);
            label10.Name = "label10";
            label10.Size = new Size(387, 15);
            label10.TabIndex = 14;
            label10.Text = "Disable new device detection once you've connected all desired devices.";
            label10.Click += label10_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(5, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(227, 298);
            tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(controllerDeviceList);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(hapticDeviceList);
            tabPage1.Controls.Add(newHapticCellphoneButton);
            tabPage1.Controls.Add(newIpFeild);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(219, 270);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Common Devices";
            tabPage1.UseVisualStyleBackColor = true;
            tabPage1.Click += tabPage1_Click_1;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(threeDsDeviceList);
            tabPage2.Controls.Add(wiimoteDeviceList);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(label3);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(219, 270);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Experimental";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // audioHapticsActive
            // 
            audioHapticsActive.AutoSize = true;
            audioHapticsActive.Checked = true;
            audioHapticsActive.CheckState = CheckState.Checked;
            audioHapticsActive.Location = new Point(238, 304);
            audioHapticsActive.Name = "audioHapticsActive";
            audioHapticsActive.Size = new Size(101, 19);
            audioHapticsActive.TabIndex = 22;
            audioHapticsActive.Text = "Audio Haptics";
            audioHapticsActive.UseVisualStyleBackColor = true;
            audioHapticsActive.CheckedChanged += audioHapticsActive_CheckedChanged;
            // 
            // midiButtonTest
            // 
            midiButtonTest.Anchor = AnchorStyles.Bottom;
            midiButtonTest.Location = new Point(359, 301);
            midiButtonTest.Name = "midiButtonTest";
            midiButtonTest.Size = new Size(99, 24);
            midiButtonTest.TabIndex = 23;
            midiButtonTest.Text = "Test Midi";
            midiButtonTest.UseVisualStyleBackColor = true;
            midiButtonTest.Click += midiButton_Click;
            // 
            // ConfigurationDisplay
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(566, 355);
            Controls.Add(midiButtonTest);
            Controls.Add(audioHapticsActive);
            Controls.Add(tabControl1);
            Controls.Add(label10);
            Controls.Add(lockInDetectedDevicesButton);
            Controls.Add(testHaptics);
            Controls.Add(falseThighSimulationCheckBox);
            Controls.Add(donateButton);
            Controls.Add(trackerCalibrationButton);
            Controls.Add(tabOptions);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "ConfigurationDisplay";
            Text = "Everything To IMU And Haptics";
            Load += ConfigurationDisplay_Load;
            tabOptions.ResumeLayout(false);
            trackerSettingsPage.ResumeLayout(false);
            trackerSettingsPage.PerformLayout();
            debugPage.ResumeLayout(false);
            debugPage.PerformLayout();
            errorLog.ResumeLayout(false);
            errorLog.PerformLayout();
            oscSettings.ResumeLayout(false);
            oscSettings.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Timer refreshTimer;
        private ListBox controllerDeviceList;
        private CheckBox falseThighSimulationCheckBox;
        private TabControl tabOptions;
        private TabPage trackerSettingsPage;
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
        private Button identifyButton;
        private ComboBox hapticJointAssignment;
        private ListBox hapticDeviceList;
        private TextBox newIpFeild;
        private Button newHapticCellphoneButton;
        private Button testHaptics;
        private Label wiimoteRateLabel;
        private TrackBar wiimoteRate;
        private ComboBox extensionYawForSimulatedTracker;
        private Label hapticJointAssignmentLabel;
        private Label extensionSourceLabel;
        private Label yawSourceLabel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.Timer listRefreshTimer;
        private Button lockInDetectedDevicesButton;
        private Label yawSourceDisclaimer1;
        private Label yawSourceDisclaimer2;
        private Label label10;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Button intensityTestButton;
        private CheckBox audioHapticsActive;
        private Button midiButtonTest;
        private Button hapticValueTestButton;
        private TabPage oscSettings;
        private Label oscIpLabel;
        private TextBox oscInputPortTextBox;
        private TextBox oscIpAddressTextBox;
        private Label oscPortIn;
        private Label label5;
        private ListBox oscOutputPorts;
        private TextBox oscPortOutputTextBox;
        private Label label6;
        private Button addOutputPortButton;
        private Button removePortOutputButton;
    }
}
