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
            debugText = new TextBox();
            refreshTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // debugText
            // 
            debugText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            debugText.Location = new Point(12, 12);
            debugText.Multiline = true;
            debugText.Name = "debugText";
            debugText.Size = new Size(398, 785);
            debugText.TabIndex = 0;
            debugText.TextChanged += textBox1_TextChanged;
            // 
            // refreshTimer
            // 
            refreshTimer.Enabled = true;
            refreshTimer.Tick += refreshTimer_Tick;
            // 
            // DebugDisplay
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(422, 809);
            Controls.Add(debugText);
            Name = "DebugDisplay";
            Text = "Debug Display";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox debugText;
        private System.Windows.Forms.Timer refreshTimer;
    }
}
