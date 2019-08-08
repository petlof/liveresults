namespace LiveResults.Client
{
    partial class NewETimingComp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewETimingComp));
            this.wizard1 = new Gui.Wizard.Wizard();
            this.wizardPage1 = new Gui.Wizard.WizardPage();
            this.label15 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.txtETimingDb = new System.Windows.Forms.TextBox();
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
            this.txtSleepTime = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.chkTwoEcards = new System.Windows.Forms.CheckBox();
            this.chkOneLineRelayRes = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtCompDate = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtOrgName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkDeleteEmmaIDs = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.chkCreateRadioControls = new System.Windows.Forms.CheckBox();
            this.txtCompName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtCompID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.wizardPage2 = new Gui.Wizard.WizardPage();
            this.lstDB = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.wizard1.SuspendLayout();
            this.wizardPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.wizardPage5.SuspendLayout();
            this.wizardPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard1
            // 
            this.wizard1.Controls.Add(this.wizardPage1);
            this.wizard1.Controls.Add(this.wizardPage5);
            this.wizard1.Controls.Add(this.wizardPage2);
            this.wizard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizard1.Location = new System.Drawing.Point(0, 0);
            this.wizard1.Name = "wizard1";
            this.wizard1.Pages.AddRange(new Gui.Wizard.WizardPage[] {
            this.wizardPage1,
            this.wizardPage2,
            this.wizardPage5});
            this.wizard1.Size = new System.Drawing.Size(506, 277);
            this.wizard1.TabIndex = 0;
            // 
            // wizardPage1
            // 
            this.wizardPage1.Controls.Add(this.label15);
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
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.Size = new System.Drawing.Size(506, 229);
            this.wizardPage1.TabIndex = 1;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(12, 216);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(82, 13);
            this.label15.TabIndex = 10;
            this.label15.Text = "ver 08.08.2019";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.txtETimingDb);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Location = new System.Drawing.Point(13, 117);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(481, 84);
            this.panel1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(441, 30);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(37, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtETimingDb
            // 
            this.txtETimingDb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtETimingDb.Location = new System.Drawing.Point(6, 30);
            this.txtETimingDb.Name = "txtETimingDb";
            this.txtETimingDb.Size = new System.Drawing.Size(429, 21);
            this.txtETimingDb.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 1);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(233, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Select the database file from the server-share.";
            // 
            // txtPw
            // 
            this.txtPw.Location = new System.Drawing.Point(281, 90);
            this.txtPw.Name = "txtPw";
            this.txtPw.Size = new System.Drawing.Size(171, 21);
            this.txtPw.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(220, 93);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Password";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(281, 63);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(171, 21);
            this.txtUser.TabIndex = 7;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(281, 36);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(171, 21);
            this.txtPort.TabIndex = 6;
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(282, 9);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(170, 21);
            this.txtHost.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(220, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(248, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(27, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(246, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Host";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(15, 25);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(175, 21);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Type of eTiming server";
            // 
            // wizardPage5
            // 
            this.wizardPage5.Controls.Add(this.txtSleepTime);
            this.wizardPage5.Controls.Add(this.label8);
            this.wizardPage5.Controls.Add(this.chkTwoEcards);
            this.wizardPage5.Controls.Add(this.chkOneLineRelayRes);
            this.wizardPage5.Controls.Add(this.label14);
            this.wizardPage5.Controls.Add(this.txtCompDate);
            this.wizardPage5.Controls.Add(this.label13);
            this.wizardPage5.Controls.Add(this.txtOrgName);
            this.wizardPage5.Controls.Add(this.label7);
            this.wizardPage5.Controls.Add(this.chkDeleteEmmaIDs);
            this.wizardPage5.Controls.Add(this.label12);
            this.wizardPage5.Controls.Add(this.chkCreateRadioControls);
            this.wizardPage5.Controls.Add(this.txtCompName);
            this.wizardPage5.Controls.Add(this.label10);
            this.wizardPage5.Controls.Add(this.txtCompID);
            this.wizardPage5.Controls.Add(this.label9);
            this.wizardPage5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage5.IsFinishPage = false;
            this.wizardPage5.Location = new System.Drawing.Point(0, 0);
            this.wizardPage5.Name = "wizardPage5";
            this.wizardPage5.Size = new System.Drawing.Size(506, 229);
            this.wizardPage5.TabIndex = 5;
            this.wizardPage5.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wizardPage5_CloseFromNext);
            this.wizardPage5.ShowFromNext += new System.EventHandler(this.wizardPage5_ShowFromNext);
            // 
            // txtSleepTime
            // 
            this.txtSleepTime.Location = new System.Drawing.Point(160, 57);
            this.txtSleepTime.Name = "txtSleepTime";
            this.txtSleepTime.Size = new System.Drawing.Size(81, 21);
            this.txtSleepTime.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(82, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(128, 13);
            this.label8.TabIndex = 15;
            this.label8.Text = "Options for live event";
            this.label8.Click += new System.EventHandler(this.label8_Click);
            // 
            // chkTwoEcards
            // 
            this.chkTwoEcards.AutoSize = true;
            this.chkTwoEcards.Location = new System.Drawing.Point(12, 172);
            this.chkTwoEcards.Name = "chkTwoEcards";
            this.chkTwoEcards.Size = new System.Drawing.Size(222, 17);
            this.chkTwoEcards.TabIndex = 14;
            this.chkTwoEcards.Text = "Require two ecards to set status Started";
            this.chkTwoEcards.UseVisualStyleBackColor = true;
            this.chkTwoEcards.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // chkOneLineRelayRes
            // 
            this.chkOneLineRelayRes.AutoSize = true;
            this.chkOneLineRelayRes.Location = new System.Drawing.Point(12, 126);
            this.chkOneLineRelayRes.Name = "chkOneLineRelayRes";
            this.chkOneLineRelayRes.Size = new System.Drawing.Size(208, 17);
            this.chkOneLineRelayRes.TabIndex = 13;
            this.chkOneLineRelayRes.Text = "Make one-line results for relay classes";
            this.chkOneLineRelayRes.UseVisualStyleBackColor = true;
            this.chkOneLineRelayRes.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(338, 9);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(132, 13);
            this.label14.TabIndex = 12;
            this.label14.Text = "eTiming database info";
            // 
            // txtCompDate
            // 
            this.txtCompDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCompDate.Location = new System.Drawing.Point(319, 33);
            this.txtCompDate.Name = "txtCompDate";
            this.txtCompDate.ReadOnly = true;
            this.txtCompDate.Size = new System.Drawing.Size(175, 21);
            this.txtCompDate.TabIndex = 11;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(287, 35);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(30, 13);
            this.label13.TabIndex = 10;
            this.label13.Text = "Date";
            // 
            // txtOrgName
            // 
            this.txtOrgName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOrgName.Location = new System.Drawing.Point(319, 58);
            this.txtOrgName.Name = "txtOrgName";
            this.txtOrgName.ReadOnly = true;
            this.txtOrgName.Size = new System.Drawing.Size(175, 21);
            this.txtOrgName.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(263, 60);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Organizer";
            // 
            // chkDeleteEmmaIDs
            // 
            this.chkDeleteEmmaIDs.AutoSize = true;
            this.chkDeleteEmmaIDs.Location = new System.Drawing.Point(12, 149);
            this.chkDeleteEmmaIDs.Name = "chkDeleteEmmaIDs";
            this.chkDeleteEmmaIDs.Size = new System.Drawing.Size(194, 17);
            this.chkDeleteEmmaIDs.TabIndex = 7;
            this.chkDeleteEmmaIDs.Text = "Delete unused IDs on Emma server";
            this.chkDeleteEmmaIDs.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 60);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(147, 13);
            this.label12.TabIndex = 6;
            this.label12.Text = "Sleep time between scans [s]";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // chkCreateRadioControls
            // 
            this.chkCreateRadioControls.AutoSize = true;
            this.chkCreateRadioControls.Checked = true;
            this.chkCreateRadioControls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateRadioControls.Location = new System.Drawing.Point(12, 103);
            this.chkCreateRadioControls.Name = "chkCreateRadioControls";
            this.chkCreateRadioControls.Size = new System.Drawing.Size(124, 17);
            this.chkCreateRadioControls.TabIndex = 4;
            this.chkCreateRadioControls.Text = "Create radiocontrols";
            this.chkCreateRadioControls.UseVisualStyleBackColor = true;
            // 
            // txtCompName
            // 
            this.txtCompName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCompName.Location = new System.Drawing.Point(319, 84);
            this.txtCompName.Name = "txtCompName";
            this.txtCompName.ReadOnly = true;
            this.txtCompName.Size = new System.Drawing.Size(175, 21);
            this.txtCompName.TabIndex = 3;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(253, 86);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(64, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Event name";
            // 
            // txtCompID
            // 
            this.txtCompID.Location = new System.Drawing.Point(160, 32);
            this.txtCompID.Name = "txtCompID";
            this.txtCompID.Size = new System.Drawing.Size(81, 21);
            this.txtCompID.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 35);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(76, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Competiton ID";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // wizardPage2
            // 
            this.wizardPage2.Controls.Add(this.lstDB);
            this.wizardPage2.Controls.Add(this.label6);
            this.wizardPage2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage2.IsFinishPage = false;
            this.wizardPage2.Location = new System.Drawing.Point(0, 0);
            this.wizardPage2.Name = "wizardPage2";
            this.wizardPage2.Size = new System.Drawing.Size(506, 229);
            this.wizardPage2.TabIndex = 2;
            this.wizardPage2.ShowFromBack += new System.EventHandler(this.wizardPage2_ShowFromBack);
            this.wizardPage2.ShowFromNext += new System.EventHandler(this.wizardPage2_ShowFromNext);
            // 
            // lstDB
            // 
            this.lstDB.FormattingEnabled = true;
            this.lstDB.Location = new System.Drawing.Point(13, 26);
            this.lstDB.Name = "lstDB";
            this.lstDB.Size = new System.Drawing.Size(218, 82);
            this.lstDB.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Database";
            // 
            // NewETimingComp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 277);
            this.Controls.Add(this.wizard1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewETimingComp";
            this.Text = "New eTiming connection";
            this.wizard1.ResumeLayout(false);
            this.wizardPage1.ResumeLayout(false);
            this.wizardPage1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.wizardPage5.ResumeLayout(false);
            this.wizardPage5.PerformLayout();
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
        private Gui.Wizard.WizardPage wizardPage5;
        private System.Windows.Forms.TextBox txtCompID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCompName;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.CheckBox chkCreateRadioControls;
        private Gui.Wizard.WizardPage wizardPage2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtETimingDb;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ListBox lstDB;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtSleepTime;
        public System.Windows.Forms.CheckBox chkDeleteEmmaIDs;
        private System.Windows.Forms.TextBox txtOrgName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtCompDate;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        public System.Windows.Forms.CheckBox chkOneLineRelayRes;
        public System.Windows.Forms.CheckBox chkTwoEcards;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label15;
    }
}