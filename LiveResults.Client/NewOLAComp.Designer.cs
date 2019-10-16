﻿namespace LiveResults.Client
{
    partial class NewOLAComp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewOLAComp));
            this.wizard1 = new Gui.Wizard.Wizard();
            this.wizardPage1 = new Gui.Wizard.WizardPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.txtOlaDb = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtPw = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.wizardPage5 = new Gui.Wizard.WizardPage();
            this.chkCreateRadioControls = new System.Windows.Forms.CheckBox();
            this.txtCompName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtCompID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.wizardPage4 = new Gui.Wizard.WizardPage();
            this.cmbOLAEtapp = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.wizardPage3 = new Gui.Wizard.WizardPage();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbOLAComp = new System.Windows.Forms.ComboBox();
            this.wizardPage2 = new Gui.Wizard.WizardPage();
            this.lstDB = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.wizard1.SuspendLayout();
            this.wizardPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.wizardPage5.SuspendLayout();
            this.wizardPage4.SuspendLayout();
            this.wizardPage3.SuspendLayout();
            this.wizardPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard1
            // 
            this.wizard1.Controls.Add(this.wizardPage5);
            this.wizard1.Controls.Add(this.wizardPage4);
            this.wizard1.Controls.Add(this.wizardPage3);
            this.wizard1.Controls.Add(this.wizardPage2);
            this.wizard1.Controls.Add(this.wizardPage1);
            this.wizard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizard1.Location = new System.Drawing.Point(0, 0);
            this.wizard1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.wizard1.Name = "wizard1";
            this.wizard1.Pages.AddRange(new Gui.Wizard.WizardPage[] {
            this.wizardPage1,
            this.wizardPage2,
            this.wizardPage3,
            this.wizardPage4,
            this.wizardPage5});
            this.wizard1.Size = new System.Drawing.Size(675, 341);
            this.wizard1.TabIndex = 0;
            this.wizard1.Load += new System.EventHandler(this.wizard1_Load);
            // 
            // wizardPage1
            // 
            this.wizardPage1.Controls.Add(this.panel1);
            this.wizardPage1.Controls.Add(this.txtPw);
            this.wizardPage1.Controls.Add(this.label5);
            this.wizardPage1.Controls.Add(this.txtUser);
            this.wizardPage1.Controls.Add(this.txtPort);
            this.wizardPage1.Controls.Add(this.txtHost);
            this.wizardPage1.Controls.Add(this.label4);
            this.wizardPage1.Controls.Add(this.label3);
            this.wizardPage1.Controls.Add(this.label2);
            this.wizardPage1.Controls.Add(this.comboBox1);
            this.wizardPage1.Controls.Add(this.label1);
            this.wizardPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage1.IsFinishPage = false;
            this.wizardPage1.Location = new System.Drawing.Point(0, 0);
            this.wizardPage1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.Size = new System.Drawing.Size(675, 282);
            this.wizardPage1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.txtOlaDb);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Location = new System.Drawing.Point(17, 144);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(641, 103);
            this.panel1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(588, 37);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(49, 28);
            this.button1.TabIndex = 2;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtOlaDb
            // 
            this.txtOlaDb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOlaDb.Location = new System.Drawing.Point(8, 37);
            this.txtOlaDb.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtOlaDb.Name = "txtOlaDb";
            this.txtOlaDb.Size = new System.Drawing.Size(571, 24);
            this.txtOlaDb.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(4, 1);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(389, 34);
            this.label11.TabIndex = 0;
            this.label11.Text = "Select the database file from the server-share\r\nOn the server its located in the " +
    "AppData folder of the OLA user";
            // 
            // txtPw
            // 
            this.txtPw.Location = new System.Drawing.Point(375, 111);
            this.txtPw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPw.Name = "txtPw";
            this.txtPw.Size = new System.Drawing.Size(227, 24);
            this.txtPw.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(293, 114);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "Password";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(375, 78);
            this.txtUser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(227, 24);
            this.txtUser.TabIndex = 7;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(375, 44);
            this.txtPort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(227, 24);
            this.txtPort.TabIndex = 6;
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(376, 11);
            this.txtHost.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(225, 24);
            this.txtHost.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(293, 81);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 17);
            this.label4.TabIndex = 4;
            this.label4.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(331, 48);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(328, 15);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Host";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(20, 31);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(232, 25);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Type of OLA server";
            // 
            // wizardPage5
            // 
            this.wizardPage5.Controls.Add(this.chkCreateRadioControls);
            this.wizardPage5.Controls.Add(this.txtCompName);
            this.wizardPage5.Controls.Add(this.label10);
            this.wizardPage5.Controls.Add(this.txtCompID);
            this.wizardPage5.Controls.Add(this.label9);
            this.wizardPage5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage5.IsFinishPage = false;
            this.wizardPage5.Location = new System.Drawing.Point(0, 0);
            this.wizardPage5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.wizardPage5.Name = "wizardPage5";
            this.wizardPage5.Size = new System.Drawing.Size(675, 282);
            this.wizardPage5.TabIndex = 5;
            this.wizardPage5.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wizardPage5_CloseFromNext);
            // 
            // chkCreateRadioControls
            // 
            this.chkCreateRadioControls.AutoSize = true;
            this.chkCreateRadioControls.Checked = true;
            this.chkCreateRadioControls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateRadioControls.Location = new System.Drawing.Point(295, 33);
            this.chkCreateRadioControls.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkCreateRadioControls.Name = "chkCreateRadioControls";
            this.chkCreateRadioControls.Size = new System.Drawing.Size(236, 21);
            this.chkCreateRadioControls.TabIndex = 4;
            this.chkCreateRadioControls.Text = "Automatically create radiocontrols";
            this.chkCreateRadioControls.UseVisualStyleBackColor = true;
            // 
            // txtCompName
            // 
            this.txtCompName.Location = new System.Drawing.Point(20, 80);
            this.txtCompName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtCompName.Name = "txtCompName";
            this.txtCompName.Size = new System.Drawing.Size(232, 24);
            this.txtCompName.TabIndex = 3;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(16, 60);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(43, 17);
            this.label10.TabIndex = 2;
            this.label10.Text = "Name";
            // 
            // txtCompID
            // 
            this.txtCompID.Location = new System.Drawing.Point(20, 31);
            this.txtCompID.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtCompID.Name = "txtCompID";
            this.txtCompID.Size = new System.Drawing.Size(232, 24);
            this.txtCompID.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 11);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(94, 17);
            this.label9.TabIndex = 0;
            this.label9.Text = "CompetitonID";
            // 
            // wizardPage4
            // 
            this.wizardPage4.Controls.Add(this.cmbOLAEtapp);
            this.wizardPage4.Controls.Add(this.label8);
            this.wizardPage4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage4.IsFinishPage = false;
            this.wizardPage4.Location = new System.Drawing.Point(0, 0);
            this.wizardPage4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.wizardPage4.Name = "wizardPage4";
            this.wizardPage4.Size = new System.Drawing.Size(675, 282);
            this.wizardPage4.TabIndex = 4;
            this.wizardPage4.ShowFromNext += new System.EventHandler(this.wizardPage4_ShowFromNext);
            // 
            // cmbOLAEtapp
            // 
            this.cmbOLAEtapp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOLAEtapp.FormattingEnabled = true;
            this.cmbOLAEtapp.Location = new System.Drawing.Point(16, 31);
            this.cmbOLAEtapp.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbOLAEtapp.Name = "cmbOLAEtapp";
            this.cmbOLAEtapp.Size = new System.Drawing.Size(211, 25);
            this.cmbOLAEtapp.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 11);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(43, 17);
            this.label8.TabIndex = 0;
            this.label8.Text = "Stage";
            // 
            // wizardPage3
            // 
            this.wizardPage3.Controls.Add(this.label7);
            this.wizardPage3.Controls.Add(this.cmbOLAComp);
            this.wizardPage3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage3.IsFinishPage = false;
            this.wizardPage3.Location = new System.Drawing.Point(0, 0);
            this.wizardPage3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.wizardPage3.Name = "wizardPage3";
            this.wizardPage3.Size = new System.Drawing.Size(675, 282);
            this.wizardPage3.TabIndex = 3;
            this.wizardPage3.ShowFromNext += new System.EventHandler(this.wizardPage3_ShowFromNext);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 5);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 17);
            this.label7.TabIndex = 1;
            this.label7.Text = "Event";
            // 
            // cmbOLAComp
            // 
            this.cmbOLAComp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOLAComp.FormattingEnabled = true;
            this.cmbOLAComp.Location = new System.Drawing.Point(16, 25);
            this.cmbOLAComp.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cmbOLAComp.Name = "cmbOLAComp";
            this.cmbOLAComp.Size = new System.Drawing.Size(232, 25);
            this.cmbOLAComp.TabIndex = 0;
            // 
            // wizardPage2
            // 
            this.wizardPage2.Controls.Add(this.lstDB);
            this.wizardPage2.Controls.Add(this.label6);
            this.wizardPage2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage2.IsFinishPage = false;
            this.wizardPage2.Location = new System.Drawing.Point(0, 0);
            this.wizardPage2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.wizardPage2.Name = "wizardPage2";
            this.wizardPage2.Size = new System.Drawing.Size(675, 282);
            this.wizardPage2.TabIndex = 2;
            this.wizardPage2.ShowFromBack += new System.EventHandler(this.wizardPage2_ShowFromBack);
            this.wizardPage2.ShowFromNext += new System.EventHandler(this.wizardPage2_ShowFromNext);
            // 
            // lstDB
            // 
            this.lstDB.FormattingEnabled = true;
            this.lstDB.ItemHeight = 17;
            this.lstDB.Location = new System.Drawing.Point(17, 32);
            this.lstDB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstDB.Name = "lstDB";
            this.lstDB.Size = new System.Drawing.Size(289, 89);
            this.lstDB.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 11);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 17);
            this.label6.TabIndex = 0;
            this.label6.Text = "Database";
            // 
            // NewOLAComp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 341);
            this.Controls.Add(this.wizard1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "NewOLAComp";
            this.Text = "New OLA-connection";
            this.wizard1.ResumeLayout(false);
            this.wizardPage1.ResumeLayout(false);
            this.wizardPage1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.wizardPage5.ResumeLayout(false);
            this.wizardPage5.PerformLayout();
            this.wizardPage4.ResumeLayout(false);
            this.wizardPage4.PerformLayout();
            this.wizardPage3.ResumeLayout(false);
            this.wizardPage3.PerformLayout();
            this.wizardPage2.ResumeLayout(false);
            this.wizardPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Gui.Wizard.Wizard wizard1;
        private Gui.Wizard.WizardPage wizardPage1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPw;
        private System.Windows.Forms.Label label5;
        private Gui.Wizard.WizardPage wizardPage3;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbOLAComp;
        private Gui.Wizard.WizardPage wizardPage4;
        private System.Windows.Forms.ComboBox cmbOLAEtapp;
        private System.Windows.Forms.Label label8;
        private Gui.Wizard.WizardPage wizardPage5;
        private System.Windows.Forms.TextBox txtCompID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCompName;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.CheckBox chkCreateRadioControls;
        private Gui.Wizard.WizardPage wizardPage2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtOlaDb;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ListBox lstDB;
        private System.Windows.Forms.Label label6;
    }
}