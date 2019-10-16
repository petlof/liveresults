namespace LiveResults.Client
{
    partial class NewMeosComp
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
            this.wizard1 = new Gui.Wizard.Wizard();
            this.wizardPage3 = new Gui.Wizard.WizardPage();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbMeosComp = new System.Windows.Forms.ComboBox();
            this.wizardPage1 = new Gui.Wizard.WizardPage();
            this.txtPw = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.wizardPage5 = new Gui.Wizard.WizardPage();
            this.chkCreateRadioControls = new System.Windows.Forms.CheckBox();
            this.txtCompName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtCompID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chkIsRelay = new System.Windows.Forms.CheckBox();
            this.wizard1.SuspendLayout();
            this.wizardPage3.SuspendLayout();
            this.wizardPage1.SuspendLayout();
            this.wizardPage5.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard1
            // 
            this.wizard1.Controls.Add(this.wizardPage5);
            this.wizard1.Controls.Add(this.wizardPage3);
            this.wizard1.Controls.Add(this.wizardPage1);
            this.wizard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizard1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wizard1.Location = new System.Drawing.Point(0, 0);
            this.wizard1.Margin = new System.Windows.Forms.Padding(4);
            this.wizard1.Name = "wizard1";
            this.wizard1.Pages.AddRange(new Gui.Wizard.WizardPage[] {
            this.wizardPage1,
            this.wizardPage3,
            this.wizardPage5});
            this.wizard1.Size = new System.Drawing.Size(675, 223);
            this.wizard1.TabIndex = 0;
            
            // 
            // wizardPage3
            // 
            this.wizardPage3.Controls.Add(this.label7);
            this.wizardPage3.Controls.Add(this.cmbMeosComp);
            this.wizardPage3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage3.IsFinishPage = false;
            this.wizardPage3.Location = new System.Drawing.Point(0, 0);
            this.wizardPage3.Margin = new System.Windows.Forms.Padding(4);
            this.wizardPage3.Name = "wizardPage3";
            this.wizardPage3.Size = new System.Drawing.Size(675, 175);
            this.wizardPage3.TabIndex = 3;
            this.wizardPage3.ShowFromNext += new System.EventHandler(this.wizardPage3_ShowFromNext);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 4);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 17);
            this.label7.TabIndex = 1;
            this.label7.Text = "Event";
            // 
            // cmbMeosComp
            // 
            this.cmbMeosComp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMeosComp.FormattingEnabled = true;
            this.cmbMeosComp.Location = new System.Drawing.Point(16, 25);
            this.cmbMeosComp.Margin = new System.Windows.Forms.Padding(4);
            this.cmbMeosComp.Name = "cmbMeosComp";
            this.cmbMeosComp.Size = new System.Drawing.Size(473, 25);
            this.cmbMeosComp.TabIndex = 0;
            // 
            // wizardPage1
            // 
            this.wizardPage1.Controls.Add(this.chkIsRelay);
            this.wizardPage1.Controls.Add(this.txtPw);
            this.wizardPage1.Controls.Add(this.label5);
            this.wizardPage1.Controls.Add(this.txtUser);
            this.wizardPage1.Controls.Add(this.txtPort);
            this.wizardPage1.Controls.Add(this.txtHost);
            this.wizardPage1.Controls.Add(this.label4);
            this.wizardPage1.Controls.Add(this.label3);
            this.wizardPage1.Controls.Add(this.label2);
            this.wizardPage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wizardPage1.IsFinishPage = false;
            this.wizardPage1.Location = new System.Drawing.Point(0, 0);
            this.wizardPage1.Margin = new System.Windows.Forms.Padding(4);
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.Size = new System.Drawing.Size(675, 175);
            this.wizardPage1.TabIndex = 1;
            // 
            // txtPw
            // 
            this.txtPw.Location = new System.Drawing.Point(96, 109);
            this.txtPw.Margin = new System.Windows.Forms.Padding(4);
            this.txtPw.Name = "txtPw";
            this.txtPw.Size = new System.Drawing.Size(227, 24);
            this.txtPw.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 112);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(66, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "Password";
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(96, 76);
            this.txtUser.Margin = new System.Windows.Forms.Padding(4);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(227, 24);
            this.txtUser.TabIndex = 7;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(96, 42);
            this.txtPort.Margin = new System.Windows.Forms.Padding(4);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(227, 24);
            this.txtPort.TabIndex = 6;
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(97, 9);
            this.txtHost.Margin = new System.Windows.Forms.Padding(4);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(225, 24);
            this.txtHost.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 79);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 17);
            this.label4.TabIndex = 4;
            this.label4.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 46);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 13);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Host";
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
            this.wizardPage5.Margin = new System.Windows.Forms.Padding(4);
            this.wizardPage5.Name = "wizardPage5";
            this.wizardPage5.Size = new System.Drawing.Size(675, 175);
            this.wizardPage5.TabIndex = 5;
            this.wizardPage5.CloseFromNext += new Gui.Wizard.PageEventHandler(this.wizardPage5_CloseFromNext);
            // 
            // chkCreateRadioControls
            // 
            this.chkCreateRadioControls.AutoSize = true;
            this.chkCreateRadioControls.Checked = true;
            this.chkCreateRadioControls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateRadioControls.Location = new System.Drawing.Point(295, 33);
            this.chkCreateRadioControls.Margin = new System.Windows.Forms.Padding(4);
            this.chkCreateRadioControls.Name = "chkCreateRadioControls";
            this.chkCreateRadioControls.Size = new System.Drawing.Size(236, 21);
            this.chkCreateRadioControls.TabIndex = 4;
            this.chkCreateRadioControls.Text = "Automatically create radiocontrols";
            this.chkCreateRadioControls.UseVisualStyleBackColor = true;
            // 
            // txtCompName
            // 
            this.txtCompName.Location = new System.Drawing.Point(20, 80);
            this.txtCompName.Margin = new System.Windows.Forms.Padding(4);
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
            this.txtCompID.Margin = new System.Windows.Forms.Padding(4);
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
            // chkIsRelay
            // 
            this.chkIsRelay.AutoSize = true;
            this.chkIsRelay.Location = new System.Drawing.Point(96, 140);
            this.chkIsRelay.Name = "chkIsRelay";
            this.chkIsRelay.Size = new System.Drawing.Size(73, 21);
            this.chkIsRelay.TabIndex = 10;
            this.chkIsRelay.Text = "IsRelay";
            this.chkIsRelay.UseVisualStyleBackColor = true;
            // 
            // NewMeosComp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 223);
            this.Controls.Add(this.wizard1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "NewMeosComp";
            this.Text = "New Meos-connection";
            this.wizard1.ResumeLayout(false);
            this.wizardPage3.ResumeLayout(false);
            this.wizardPage3.PerformLayout();
            this.wizardPage1.ResumeLayout(false);
            this.wizardPage1.PerformLayout();
            this.wizardPage5.ResumeLayout(false);
            this.wizardPage5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Gui.Wizard.Wizard wizard1;
        private Gui.Wizard.WizardPage wizardPage1;
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
        private System.Windows.Forms.ComboBox cmbMeosComp;
        private Gui.Wizard.WizardPage wizardPage5;
        private System.Windows.Forms.TextBox txtCompID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtCompName;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.CheckBox chkCreateRadioControls;
        private System.Windows.Forms.CheckBox chkIsRelay;
    }
}