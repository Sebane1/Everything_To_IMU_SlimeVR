namespace PS5_Dualsense_To_IMU_SlimeVR {
    partial class DebugDisplay {
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
            deviceList = new ListBox();
            falseThighSimulationCheckBox = new CheckBox();
            tabControl1 = new TabControl();
            settingsPage = new TabPage();
            debugPage = new TabPage();
            debugText = new TextBox();
            tabControl1.SuspendLayout();
            settingsPage.SuspendLayout();
            debugPage.SuspendLayout();
            SuspendLayout();
            // 
            // refreshTimer
            // 
            refreshTimer.Enabled = true;
            refreshTimer.Interval = 500;
            refreshTimer.Tick += refreshTimer_Tick;
            // 
            // deviceList
            // 
            deviceList.FormattingEnabled = true;
            deviceList.ItemHeight = 15;
            deviceList.Location = new Point(-2, -2);
            deviceList.Name = "deviceList";
            deviceList.Size = new Size(104, 394);
            deviceList.TabIndex = 0;
            deviceList.SelectedIndexChanged += selectedDevice_SelectedIndexChanged;
            // 
            // falseThighSimulationCheckBox
            // 
            falseThighSimulationCheckBox.AutoSize = true;
            falseThighSimulationCheckBox.Location = new Point(6, 15);
            falseThighSimulationCheckBox.Name = "falseThighSimulationCheckBox";
            falseThighSimulationCheckBox.Size = new Size(117, 19);
            falseThighSimulationCheckBox.TabIndex = 1;
            falseThighSimulationCheckBox.Text = "Uses False Thighs";
            falseThighSimulationCheckBox.UseVisualStyleBackColor = true;
            falseThighSimulationCheckBox.CheckedChanged += falseThighSimulationCheckBox_CheckedChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(settingsPage);
            tabControl1.Controls.Add(debugPage);
            tabControl1.Location = new Point(99, -2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(322, 394);
            tabControl1.TabIndex = 2;
            // 
            // settingsPage
            // 
            settingsPage.Controls.Add(falseThighSimulationCheckBox);
            settingsPage.Location = new Point(4, 24);
            settingsPage.Name = "settingsPage";
            settingsPage.Padding = new Padding(3);
            settingsPage.Size = new Size(314, 366);
            settingsPage.TabIndex = 0;
            settingsPage.Text = "Settings";
            settingsPage.UseVisualStyleBackColor = true;
            settingsPage.Click += tabPage1_Click;
            // 
            // debugPage
            // 
            debugPage.Controls.Add(debugText);
            debugPage.Location = new Point(4, 24);
            debugPage.Name = "debugPage";
            debugPage.Padding = new Padding(3);
            debugPage.Size = new Size(314, 366);
            debugPage.TabIndex = 1;
            debugPage.Text = "Debug";
            debugPage.UseVisualStyleBackColor = true;
            // 
            // debugText
            // 
            debugText.Location = new Point(6, 6);
            debugText.Multiline = true;
            debugText.Name = "debugText";
            debugText.Size = new Size(296, 339);
            debugText.TabIndex = 0;
            // 
            // DebugDisplay
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(421, 389);
            Controls.Add(tabControl1);
            Controls.Add(deviceList);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "DebugDisplay";
            Text = "Debug Display";
            tabControl1.ResumeLayout(false);
            settingsPage.ResumeLayout(false);
            settingsPage.PerformLayout();
            debugPage.ResumeLayout(false);
            debugPage.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Timer refreshTimer;
        private ListBox deviceList;
        private CheckBox falseThighSimulationCheckBox;
        private TabControl tabControl1;
        private TabPage settingsPage;
        private TabPage debugPage;
        private TextBox debugText;
    }
}
