namespace LiveResults.CasparClient
{
    partial class CasparControlFrm
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
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.txtCGServer = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtNameFinder = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.btnHideNameLabel = new System.Windows.Forms.Button();
            this.btnShowNameLabel = new System.Windows.Forms.Button();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSaveToList = new System.Windows.Forms.Button();
            this.lstNameTemplates = new System.Windows.Forms.ListBox();
            this.txtTitleClub = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.lblResultNumPages = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.btnShowResultList = new System.Windows.Forms.Button();
            this.btnRefreshResultListClasses = new System.Windows.Forms.Button();
            this.cmbClass = new System.Windows.Forms.ComboBox();
            this.lblClass = new System.Windows.Forms.Label();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnPrewarningForceUpdate = new System.Windows.Forms.Button();
            this.btnStopPrewarning = new System.Windows.Forms.Button();
            this.btnStartPrewarning = new System.Windows.Forms.Button();
            this.btnRefreshPrewarningControls = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.lstRadioControls = new System.Windows.Forms.CheckedListBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.btnStopClock = new System.Windows.Forms.Button();
            this.btnStartClock = new System.Windows.Forms.Button();
            this.chkClockShowTenth = new System.Windows.Forms.CheckBox();
            this.txtClockRefTime = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnClockUpdate = new System.Windows.Forms.Button();
            this.cmbResultListClassPosition = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 27);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.lblStatus);
            this.groupBox1.Controls.Add(this.txtCGServer);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(11, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(864, 65);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(237, 23);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(100, 28);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(345, 30);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            this.lblStatus.TabIndex = 2;
            // 
            // txtCGServer
            // 
            this.txtCGServer.Location = new System.Drawing.Point(67, 23);
            this.txtCGServer.Margin = new System.Windows.Forms.Padding(4);
            this.txtCGServer.Name = "txtCGServer";
            this.txtCGServer.Size = new System.Drawing.Size(161, 22);
            this.txtCGServer.TabIndex = 1;
            this.txtCGServer.Text = "127.0.0.1";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(11, 87);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(864, 430);
            this.tabControl1.TabIndex = 2;
            this.tabControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tabControl1_KeyDown);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.txtNameFinder);
            this.tabPage1.Controls.Add(this.listBox1);
            this.tabPage1.Controls.Add(this.btnHideNameLabel);
            this.tabPage1.Controls.Add(this.btnShowNameLabel);
            this.tabPage1.Controls.Add(this.radioButton1);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.btnSaveToList);
            this.tabPage1.Controls.Add(this.lstNameTemplates);
            this.tabPage1.Controls.Add(this.txtTitleClub);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.txtName);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(856, 401);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Titles";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 147);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(259, 17);
            this.label8.TabIndex = 14;
            this.label8.Text = "Matches (doubleclick to load to Row1/2)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 100);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(135, 17);
            this.label7.TabIndex = 13;
            this.label7.Text = "Find runner in event";
            // 
            // txtNameFinder
            // 
            this.txtNameFinder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNameFinder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.txtNameFinder.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtNameFinder.Location = new System.Drawing.Point(8, 121);
            this.txtNameFinder.Margin = new System.Windows.Forms.Padding(4);
            this.txtNameFinder.Name = "txtNameFinder";
            this.txtNameFinder.Size = new System.Drawing.Size(364, 22);
            this.txtNameFinder.TabIndex = 12;
            this.txtNameFinder.TextChanged += new System.EventHandler(this.txtNameFinder_TextChanged);
            this.txtNameFinder.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNameFinder_KeyDown);
            this.txtNameFinder.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtNameFinder_KeyUp);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 16;
            this.listBox1.Location = new System.Drawing.Point(12, 165);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(364, 212);
            this.listBox1.TabIndex = 11;
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // btnHideNameLabel
            // 
            this.btnHideNameLabel.Location = new System.Drawing.Point(120, 68);
            this.btnHideNameLabel.Margin = new System.Windows.Forms.Padding(4);
            this.btnHideNameLabel.Name = "btnHideNameLabel";
            this.btnHideNameLabel.Size = new System.Drawing.Size(100, 28);
            this.btnHideNameLabel.TabIndex = 10;
            this.btnHideNameLabel.Text = "Hide";
            this.btnHideNameLabel.UseVisualStyleBackColor = true;
            this.btnHideNameLabel.Click += new System.EventHandler(this.btnHideNameLabel_Click);
            // 
            // btnShowNameLabel
            // 
            this.btnShowNameLabel.Location = new System.Drawing.Point(12, 68);
            this.btnShowNameLabel.Margin = new System.Windows.Forms.Padding(4);
            this.btnShowNameLabel.Name = "btnShowNameLabel";
            this.btnShowNameLabel.Size = new System.Drawing.Size(100, 28);
            this.btnShowNameLabel.TabIndex = 9;
            this.btnShowNameLabel.Text = "Show";
            this.btnShowNameLabel.UseVisualStyleBackColor = true;
            this.btnShowNameLabel.Click += new System.EventHandler(this.btnShowNameLabel_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(12, 34);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(4);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(158, 21);
            this.radioButton1.TabIndex = 8;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Name-label (bottom)";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 16);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 17);
            this.label5.TabIndex = 7;
            this.label5.Text = "Type of label";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(457, 74);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(196, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "double-click item in list to load";
            // 
            // btnSaveToList
            // 
            this.btnSaveToList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveToList.Location = new System.Drawing.Point(773, 4);
            this.btnSaveToList.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveToList.Name = "btnSaveToList";
            this.btnSaveToList.Size = new System.Drawing.Size(59, 50);
            this.btnSaveToList.TabIndex = 5;
            this.btnSaveToList.Text = "Save V";
            this.btnSaveToList.UseVisualStyleBackColor = true;
            this.btnSaveToList.Click += new System.EventHandler(this.button1_Click);
            // 
            // lstNameTemplates
            // 
            this.lstNameTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstNameTemplates.FormattingEnabled = true;
            this.lstNameTemplates.ItemHeight = 16;
            this.lstNameTemplates.Location = new System.Drawing.Point(457, 97);
            this.lstNameTemplates.Margin = new System.Windows.Forms.Padding(4);
            this.lstNameTemplates.Name = "lstNameTemplates";
            this.lstNameTemplates.Size = new System.Drawing.Size(387, 292);
            this.lstNameTemplates.TabIndex = 4;
            this.lstNameTemplates.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstNameTemplates_MouseDoubleClick);
            // 
            // txtTitleClub
            // 
            this.txtTitleClub.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTitleClub.Location = new System.Drawing.Point(563, 34);
            this.txtTitleClub.Margin = new System.Windows.Forms.Padding(4);
            this.txtTitleClub.Name = "txtTitleClub";
            this.txtTitleClub.Size = new System.Drawing.Size(203, 22);
            this.txtTitleClub.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(435, 38);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Row2 (Title/Club)";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(561, 7);
            this.txtName.Margin = new System.Windows.Forms.Padding(4);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(203, 22);
            this.txtName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(453, 11);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 17);
            this.label2.TabIndex = 0;
            this.label2.Text = "Row1 (NAME)";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.cmbResultListClassPosition);
            this.tabPage2.Controls.Add(this.label10);
            this.tabPage2.Controls.Add(this.button4);
            this.tabPage2.Controls.Add(this.button3);
            this.tabPage2.Controls.Add(this.lblResultNumPages);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.btnShowResultList);
            this.tabPage2.Controls.Add(this.btnRefreshResultListClasses);
            this.tabPage2.Controls.Add(this.cmbClass);
            this.tabPage2.Controls.Add(this.lblClass);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(856, 401);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Resultlist";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(167, 169);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(60, 28);
            this.button4.TabIndex = 16;
            this.button4.Text = ">";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(11, 169);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(60, 28);
            this.button3.TabIndex = 15;
            this.button3.Text = "<";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // lblResultNumPages
            // 
            this.lblResultNumPages.AutoSize = true;
            this.lblResultNumPages.Location = new System.Drawing.Point(79, 175);
            this.lblResultNumPages.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResultNumPages.Name = "lblResultNumPages";
            this.lblResultNumPages.Size = new System.Drawing.Size(77, 17);
            this.lblResultNumPages.TabIndex = 14;
            this.lblResultNumPages.Text = "Page x of x";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(156, 117);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 12;
            this.button1.Text = "Hide";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnShowResultList
            // 
            this.btnShowResultList.Location = new System.Drawing.Point(11, 117);
            this.btnShowResultList.Margin = new System.Windows.Forms.Padding(4);
            this.btnShowResultList.Name = "btnShowResultList";
            this.btnShowResultList.Size = new System.Drawing.Size(100, 28);
            this.btnShowResultList.TabIndex = 11;
            this.btnShowResultList.Text = "Show";
            this.btnShowResultList.UseVisualStyleBackColor = true;
            this.btnShowResultList.Click += new System.EventHandler(this.btnShowResultList_Click);
            // 
            // btnRefreshResultListClasses
            // 
            this.btnRefreshResultListClasses.Location = new System.Drawing.Point(344, 17);
            this.btnRefreshResultListClasses.Margin = new System.Windows.Forms.Padding(4);
            this.btnRefreshResultListClasses.Name = "btnRefreshResultListClasses";
            this.btnRefreshResultListClasses.Size = new System.Drawing.Size(100, 28);
            this.btnRefreshResultListClasses.TabIndex = 2;
            this.btnRefreshResultListClasses.Text = "Refresh";
            this.btnRefreshResultListClasses.UseVisualStyleBackColor = true;
            this.btnRefreshResultListClasses.Click += new System.EventHandler(this.btnRefreshResultListClasses_Click);
            // 
            // cmbClass
            // 
            this.cmbClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbClass.FormattingEnabled = true;
            this.cmbClass.Location = new System.Drawing.Point(72, 20);
            this.cmbClass.Margin = new System.Windows.Forms.Padding(4);
            this.cmbClass.Name = "cmbClass";
            this.cmbClass.Size = new System.Drawing.Size(261, 24);
            this.cmbClass.TabIndex = 1;
            this.cmbClass.SelectedIndexChanged += new System.EventHandler(this.cmbClass_SelectedIndexChanged);
            // 
            // lblClass
            // 
            this.lblClass.AutoSize = true;
            this.lblClass.Location = new System.Drawing.Point(8, 20);
            this.lblClass.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblClass.Name = "lblClass";
            this.lblClass.Size = new System.Drawing.Size(42, 17);
            this.lblClass.TabIndex = 0;
            this.lblClass.Text = "Class";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnPrewarningForceUpdate);
            this.tabPage3.Controls.Add(this.btnStopPrewarning);
            this.tabPage3.Controls.Add(this.btnStartPrewarning);
            this.tabPage3.Controls.Add(this.btnRefreshPrewarningControls);
            this.tabPage3.Controls.Add(this.label6);
            this.tabPage3.Controls.Add(this.lstRadioControls);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(856, 401);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Prewarned runners";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnPrewarningForceUpdate
            // 
            this.btnPrewarningForceUpdate.Location = new System.Drawing.Point(240, 169);
            this.btnPrewarningForceUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.btnPrewarningForceUpdate.Name = "btnPrewarningForceUpdate";
            this.btnPrewarningForceUpdate.Size = new System.Drawing.Size(205, 28);
            this.btnPrewarningForceUpdate.TabIndex = 5;
            this.btnPrewarningForceUpdate.Text = "Force Update";
            this.btnPrewarningForceUpdate.UseVisualStyleBackColor = true;
            // 
            // btnStopPrewarning
            // 
            this.btnStopPrewarning.Location = new System.Drawing.Point(127, 169);
            this.btnStopPrewarning.Margin = new System.Windows.Forms.Padding(4);
            this.btnStopPrewarning.Name = "btnStopPrewarning";
            this.btnStopPrewarning.Size = new System.Drawing.Size(100, 28);
            this.btnStopPrewarning.TabIndex = 4;
            this.btnStopPrewarning.Text = "Stop";
            this.btnStopPrewarning.UseVisualStyleBackColor = true;
            this.btnStopPrewarning.Click += new System.EventHandler(this.btnStopPrewarning_Click);
            // 
            // btnStartPrewarning
            // 
            this.btnStartPrewarning.Location = new System.Drawing.Point(19, 169);
            this.btnStartPrewarning.Margin = new System.Windows.Forms.Padding(4);
            this.btnStartPrewarning.Name = "btnStartPrewarning";
            this.btnStartPrewarning.Size = new System.Drawing.Size(100, 28);
            this.btnStartPrewarning.TabIndex = 3;
            this.btnStartPrewarning.Text = "Start";
            this.btnStartPrewarning.UseVisualStyleBackColor = true;
            this.btnStartPrewarning.Click += new System.EventHandler(this.btnStartPrewarning_Click);
            // 
            // btnRefreshPrewarningControls
            // 
            this.btnRefreshPrewarningControls.Location = new System.Drawing.Point(232, 34);
            this.btnRefreshPrewarningControls.Margin = new System.Windows.Forms.Padding(4);
            this.btnRefreshPrewarningControls.Name = "btnRefreshPrewarningControls";
            this.btnRefreshPrewarningControls.Size = new System.Drawing.Size(100, 28);
            this.btnRefreshPrewarningControls.TabIndex = 2;
            this.btnRefreshPrewarningControls.Text = "Refresh";
            this.btnRefreshPrewarningControls.UseVisualStyleBackColor = true;
            this.btnRefreshPrewarningControls.Click += new System.EventHandler(this.btnRefreshPrewarningControls_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 15);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(134, 17);
            this.label6.TabIndex = 1;
            this.label6.Text = "Prewarning-controls";
            // 
            // lstRadioControls
            // 
            this.lstRadioControls.FormattingEnabled = true;
            this.lstRadioControls.Location = new System.Drawing.Point(19, 34);
            this.lstRadioControls.Margin = new System.Windows.Forms.Padding(4);
            this.lstRadioControls.Name = "lstRadioControls";
            this.lstRadioControls.Size = new System.Drawing.Size(204, 106);
            this.lstRadioControls.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.btnClockUpdate);
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.txtClockRefTime);
            this.tabPage4.Controls.Add(this.chkClockShowTenth);
            this.tabPage4.Controls.Add(this.btnStopClock);
            this.tabPage4.Controls.Add(this.btnStartClock);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(856, 401);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Clock";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // btnStopClock
            // 
            this.btnStopClock.Location = new System.Drawing.Point(117, 110);
            this.btnStopClock.Margin = new System.Windows.Forms.Padding(4);
            this.btnStopClock.Name = "btnStopClock";
            this.btnStopClock.Size = new System.Drawing.Size(100, 28);
            this.btnStopClock.TabIndex = 12;
            this.btnStopClock.Text = "Hide";
            this.btnStopClock.UseVisualStyleBackColor = true;
            this.btnStopClock.Click += new System.EventHandler(this.btnStopClock_Click);
            // 
            // btnStartClock
            // 
            this.btnStartClock.Location = new System.Drawing.Point(9, 110);
            this.btnStartClock.Margin = new System.Windows.Forms.Padding(4);
            this.btnStartClock.Name = "btnStartClock";
            this.btnStartClock.Size = new System.Drawing.Size(100, 28);
            this.btnStartClock.TabIndex = 11;
            this.btnStartClock.Text = "Show";
            this.btnStartClock.UseVisualStyleBackColor = true;
            this.btnStartClock.Click += new System.EventHandler(this.btnStartClock_Click);
            // 
            // chkClockShowTenth
            // 
            this.chkClockShowTenth.AutoSize = true;
            this.chkClockShowTenth.Location = new System.Drawing.Point(9, 21);
            this.chkClockShowTenth.Name = "chkClockShowTenth";
            this.chkClockShowTenth.Size = new System.Drawing.Size(173, 21);
            this.chkClockShowTenth.TabIndex = 13;
            this.chkClockShowTenth.Text = "Show tenth of seconds";
            this.chkClockShowTenth.UseVisualStyleBackColor = true;
            // 
            // txtClockRefTime
            // 
            this.txtClockRefTime.Location = new System.Drawing.Point(11, 68);
            this.txtClockRefTime.Name = "txtClockRefTime";
            this.txtClockRefTime.Size = new System.Drawing.Size(195, 22);
            this.txtClockRefTime.TabIndex = 14;
            this.txtClockRefTime.Text = "00:00:00";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 48);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(202, 17);
            this.label9.TabIndex = 15;
            this.label9.Text = "Reference time (HH24:MM:SS)";
            // 
            // btnClockUpdate
            // 
            this.btnClockUpdate.Location = new System.Drawing.Point(225, 110);
            this.btnClockUpdate.Margin = new System.Windows.Forms.Padding(4);
            this.btnClockUpdate.Name = "btnClockUpdate";
            this.btnClockUpdate.Size = new System.Drawing.Size(143, 28);
            this.btnClockUpdate.TabIndex = 16;
            this.btnClockUpdate.Text = "Update settings";
            this.btnClockUpdate.UseVisualStyleBackColor = true;
            this.btnClockUpdate.Click += new System.EventHandler(this.btnClockUpdate_Click);
            // 
            // cmbResultListClassPosition
            // 
            this.cmbResultListClassPosition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbResultListClassPosition.FormattingEnabled = true;
            this.cmbResultListClassPosition.Location = new System.Drawing.Point(72, 51);
            this.cmbResultListClassPosition.Margin = new System.Windows.Forms.Padding(4);
            this.cmbResultListClassPosition.Name = "cmbResultListClassPosition";
            this.cmbResultListClassPosition.Size = new System.Drawing.Size(261, 24);
            this.cmbResultListClassPosition.TabIndex = 18;
            this.cmbResultListClassPosition.SelectedIndexChanged += new System.EventHandler(this.cmbResultListClassPosition_SelectedIndexChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 54);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(58, 17);
            this.label10.TabIndex = 17;
            this.label10.Text = "Position";
            // 
            // CasparControlFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 554);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "CasparControlFrm";
            this.Text = "CasparControlFrm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TextBox txtCGServer;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox txtTitleClub;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSaveToList;
        private System.Windows.Forms.ListBox lstNameTemplates;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnHideNameLabel;
        private System.Windows.Forms.Button btnShowNameLabel;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnRefreshResultListClasses;
        private System.Windows.Forms.ComboBox cmbClass;
        private System.Windows.Forms.Label lblClass;
        private System.Windows.Forms.Label lblResultNumPages;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnShowResultList;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnPrewarningForceUpdate;
        private System.Windows.Forms.Button btnStopPrewarning;
        private System.Windows.Forms.Button btnStartPrewarning;
        private System.Windows.Forms.Button btnRefreshPrewarningControls;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckedListBox lstRadioControls;
        private System.Windows.Forms.TextBox txtNameFinder;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnStopClock;
        private System.Windows.Forms.Button btnStartClock;
        private System.Windows.Forms.Button btnClockUpdate;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtClockRefTime;
        private System.Windows.Forms.CheckBox chkClockShowTenth;
        private System.Windows.Forms.ComboBox cmbResultListClassPosition;
        private System.Windows.Forms.Label label10;
    }
}