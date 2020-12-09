namespace Visualizer
{
    partial class MainFrm
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
            this.lblSerialPort = new System.Windows.Forms.Label();
            this.cbSerialPorts = new System.Windows.Forms.ComboBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.thermalMap = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.thermalMap)).BeginInit();
            this.SuspendLayout();
            // 
            // lblSerialPort
            // 
            this.lblSerialPort.AutoSize = true;
            this.lblSerialPort.Location = new System.Drawing.Point(26, 19);
            this.lblSerialPort.Name = "lblSerialPort";
            this.lblSerialPort.Size = new System.Drawing.Size(58, 13);
            this.lblSerialPort.TabIndex = 0;
            this.lblSerialPort.Text = "Serial Port:";
            // 
            // cbSerialPorts
            // 
            this.cbSerialPorts.FormattingEnabled = true;
            this.cbSerialPorts.Location = new System.Drawing.Point(87, 16);
            this.cbSerialPorts.Name = "cbSerialPorts";
            this.cbSerialPorts.Size = new System.Drawing.Size(143, 21);
            this.cbSerialPorts.TabIndex = 1;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(239, 15);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // thermalMap
            // 
            this.thermalMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.thermalMap.Location = new System.Drawing.Point(12, 55);
            this.thermalMap.Name = "thermalMap";
            this.thermalMap.Size = new System.Drawing.Size(742, 521);
            this.thermalMap.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.thermalMap.TabIndex = 3;
            this.thermalMap.TabStop = false;
            this.thermalMap.Paint += new System.Windows.Forms.PaintEventHandler(this.thermalMap_Paint);
            // 
            // MainFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 588);
            this.Controls.Add(this.thermalMap);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.cbSerialPorts);
            this.Controls.Add(this.lblSerialPort);
            this.Name = "MainFrm";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainFrm_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.thermalMap)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSerialPort;
        private System.Windows.Forms.ComboBox cbSerialPorts;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.PictureBox thermalMap;
    }
}

