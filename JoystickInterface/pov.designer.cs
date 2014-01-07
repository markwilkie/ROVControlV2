namespace JoystickSample
{
    partial class POV
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPOV = new System.Windows.Forms.Label();
            this.tbTiltPos = new System.Windows.Forms.TrackBar();
            this.tbPanPos = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.tbTiltPos)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPanPos)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPOV
            // 
            this.lblPOV.AutoSize = true;
            this.lblPOV.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPOV.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPOV.Location = new System.Drawing.Point(0, 0);
            this.lblPOV.Name = "lblPOV";
            this.lblPOV.Size = new System.Drawing.Size(118, 13);
            this.lblPOV.TabIndex = 0;
            this.lblPOV.Text = "Axis: #  Pos: 32767";
            // 
            // tbTiltPos
            // 
            this.tbTiltPos.LargeChange = 10000;
            this.tbTiltPos.Location = new System.Drawing.Point(95, 76);
            this.tbTiltPos.Maximum = 65535;
            this.tbTiltPos.Name = "tbTiltPos";
            this.tbTiltPos.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.tbTiltPos.Size = new System.Drawing.Size(45, 205);
            this.tbTiltPos.SmallChange = 5000;
            this.tbTiltPos.TabIndex = 1;
            this.tbTiltPos.TickFrequency = 5000;
            this.tbTiltPos.Value = 32767;
            // 
            // tbPanPos
            // 
            this.tbPanPos.LargeChange = 10000;
            this.tbPanPos.Location = new System.Drawing.Point(17, 25);
            this.tbPanPos.Maximum = 65535;
            this.tbPanPos.Name = "tbPanPos";
            this.tbPanPos.Size = new System.Drawing.Size(205, 45);
            this.tbPanPos.SmallChange = 5000;
            this.tbPanPos.TabIndex = 2;
            this.tbPanPos.TickFrequency = 5000;
            this.tbPanPos.Value = 32767;
            // 
            // POV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tbPanPos);
            this.Controls.Add(this.tbTiltPos);
            this.Controls.Add(this.lblPOV);
            this.Name = "POV";
            this.Size = new System.Drawing.Size(250, 300);
            this.Load += new System.EventHandler(this.POV_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tbTiltPos)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbPanPos)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblPOV;
        private System.Windows.Forms.TrackBar tbTiltPos;
        private System.Windows.Forms.TrackBar tbPanPos;
    }
}
