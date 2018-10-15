﻿namespace LiveResults.Client
{
    partial class FrmNewCompetition
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
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOLA = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.button5 = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.btnOLA);
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Controls.Add(this.button3);
            this.flowLayoutPanel1.Controls.Add(this.button4);
            this.flowLayoutPanel1.Controls.Add(this.button1);
            this.flowLayoutPanel1.Controls.Add(this.button5);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(21, 36);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(617, 256);
            this.flowLayoutPanel1.TabIndex = 0;
            this.flowLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel1_Paint);
            // 
            // btnOLA
            // 
            this.btnOLA.Image = global::LiveResults.Client.Properties.Resources.OLAImg;
            this.btnOLA.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnOLA.Location = new System.Drawing.Point(4, 4);
            this.btnOLA.Margin = new System.Windows.Forms.Padding(4);
            this.btnOLA.Name = "btnOLA";
            this.btnOLA.Size = new System.Drawing.Size(85, 111);
            this.btnOLA.TabIndex = 0;
            this.btnOLA.Text = "OLA, SOFT";
            this.btnOLA.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnOLA.UseVisualStyleBackColor = true;
            this.btnOLA.Click += new System.EventHandler(this.btnOLA_Click);
            this.btnOLA.MouseEnter += new System.EventHandler(this.btnOLA_MouseHover);
            this.btnOLA.MouseLeave += new System.EventHandler(this.btn_MouseLeave);
            // 
            // button2
            // 
            this.button2.Image = global::LiveResults.Client.Properties.Resources.iof_logohead1;
            this.button2.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button2.Location = new System.Drawing.Point(97, 4);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(152, 111);
            this.button2.TabIndex = 2;
            this.button2.Text = "IOF XML (SportSoftware OE/OS, MeOs,..)";
            this.button2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            this.button2.MouseEnter += new System.EventHandler(this.button2_MouseEnter);
            this.button2.MouseLeave += new System.EventHandler(this.btn_MouseLeave);
            // 
            // button3
            // 
            this.button3.Image = global::LiveResults.Client.Properties.Resources.ssftiming;
            this.button3.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button3.Location = new System.Drawing.Point(257, 4);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(100, 111);
            this.button3.TabIndex = 4;
            this.button3.Text = "SSF Timing (beta)";
            this.button3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            this.button3.MouseEnter += new System.EventHandler(this.button3_MouseEnter);
            this.button3.MouseLeave += new System.EventHandler(this.btn_MouseLeave);
            // 
            // button4
            // 
            this.button4.Image = global::LiveResults.Client.Properties.Resources.OEImg;
            this.button4.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.button4.Location = new System.Drawing.Point(365, 4);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(123, 111);
            this.button4.TabIndex = 5;
            this.button4.Text = "OE/OS CSV (old format)";
            this.button4.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            this.button4.MouseEnter += new System.EventHandler(this.button4_MouseEnter);
            this.button4.MouseLeave += new System.EventHandler(this.btn_MouseLeave);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(496, 4);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 111);
            this.button1.TabIndex = 3;
            this.button1.Text = "RaCom Files";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_3);
            this.button1.MouseEnter += new System.EventHandler(this.buttonRacom_MouseEnter);
            this.button1.MouseLeave += new System.EventHandler(this.btn_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Create new Live Broadcast";
            // 
            // lblInfo
            // 
            this.lblInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(21, 296);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(621, 71);
            this.lblInfo.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 404);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "Recent projects";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(21, 425);
            this.flowLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(617, 64);
            this.flowLayoutPanel2.TabIndex = 4;
            this.flowLayoutPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.flowLayoutPanel2_Paint);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(4, 123);
            this.button5.Margin = new System.Windows.Forms.Padding(4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(100, 111);
            this.button5.TabIndex = 6;
            this.button5.Text = "Meos";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            this.button5.MouseEnter += new System.EventHandler(this.button5_MouseEnter);
            // 
            // FrmNewCompetition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 500);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lblInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmNewCompetition";
            this.Text = "Create New Competition";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmNewCompetition_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FrmNewCompetition_KeyPress);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOLA;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
    }
}