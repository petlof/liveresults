using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LiveResults.Model;
using Svt.Caspar;
using Svt.Network;

namespace LiveResults.CasparClient
{
    public partial class CasparControlFrm : Form
    {

        private delegate void NetworkCasparEvent(NetworkEventArgs parameter);
        private delegate void UnTypedCasparEvent(EventArgs parameter);

        private const string templateFolder = "sm2017";

        CasparDevice m_Caspar = new CasparDevice();

        private EmmaMysqlClient m_emmaClient = null;

        private resultListItem[] m_currentResultList;
        private int m_ResultCurrentPage = 1;
        private string m_ResultsCurrentClassName = "";
        private cmbRadio m_ResultsCurrentPosition = null;

        Dictionary<int,string> runnerStatus = new Dictionary<int, string>();


        public CasparControlFrm()
        {
            InitializeComponent();
            DisableControls();

            runnerStatus.Add(1,"DNS");
            runnerStatus.Add(2,"DNF");
            runnerStatus.Add(11,"WO");
            runnerStatus.Add(12,"MO");
            runnerStatus.Add(0,"OK");
            runnerStatus.Add(3,"MP");
            runnerStatus.Add(4,"DSQ");
             runnerStatus.Add(5,"OT");
            runnerStatus.Add(9,"");
            runnerStatus.Add(10,"");


            m_Caspar.Connected += m_Caspar_Connected;
            m_Caspar.FailedConnect += m_Caspar_FailedConnect;
            m_Caspar.Disconnected += m_Caspar_Disconnected;
            m_Caspar.DataRetrieved += m_Caspar_DataRetrieved;
            m_Caspar.UpdatedDatafiles += m_Caspar_UpdatedDatafiles;

            UpdateConnectButtonText();
        }

        public void SetEmmaClient(EmmaMysqlClient client)
        {
            m_emmaClient = client;
            m_emmaClient.RunnerChanged += m_emmaClient_RunnerChanged;
        }

        void m_emmaClient_RunnerChanged(Runner runner)
        {
        }

        // update text on button
        private void UpdateConnectButtonText()
        {
            if (!m_Caspar.IsConnected)
            {
                btnConnect.Text = "Connect";// to " + Properties.Settings.Default.Hostname;
            }
            else
            {
                btnConnect.Text = "Disconnect"; // from " + Properties.Settings.Default.Hostname;
            }
        }

        void m_Caspar_UpdatedDatafiles(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<EventArgs>(m_Caspar_UpdatedDatafiles), sender, e);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnDataFilesUpdated");
                System.Diagnostics.Debug.WriteLine(e.ToString());


                //List<CGDataItem> dataFileItems = new List<CGDataItem>();
                
                //cbTableOldSavings.DataSource = dataFileItems;
                //gpExistingTimeTables.Enabled = true;
            }
        }

        void m_Caspar_DataRetrieved(object sender, DataEventArgs e)
        {

        }

        void m_Caspar_Disconnected(object sender, Svt.Network.NetworkEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<NetworkEventArgs>(m_Caspar_Disconnected), sender, e);
            }
            else
            {
                btnConnect.Enabled = true;
                UpdateConnectButtonText();

                lblStatus.BackColor = Color.LightCoral;
                lblStatus.Text = "Disconnected from " + m_Caspar.Settings.Hostname; // Properties.Settings.Default.Hostname;

                DisableControls();
            }
        }

        void m_Caspar_FailedConnect(object sender, Svt.Network.NetworkEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<NetworkEventArgs>(m_Caspar_FailedConnect), sender, e);
            }
            else
            {
                btnConnect.Enabled = true;
                UpdateConnectButtonText();

                lblStatus.BackColor = Color.LightCoral;
                lblStatus.Text = "Failed to connect to " + m_Caspar.Settings.Hostname; // Properties.Settings.Default.Hostname;

                DisableControls();
            }
        }

        void m_Caspar_Connected(object sender, Svt.Network.NetworkEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler<NetworkEventArgs>(m_Caspar_Connected), sender, e);
            }
            else
            {

                btnConnect.Enabled = true;
                UpdateConnectButtonText();

                m_Caspar.RefreshMediafiles();
                m_Caspar.RefreshDatalist();


                lblStatus.BackColor = Color.LightGreen;
                lblStatus.Text = "Connected to " + m_Caspar.Settings.Hostname; // Properties.Settings.Default.Hostname;

                EnableControls();
            }
        }

        private void EnableControls()
        {
            tabControl1.Enabled = true;
        }
        private void DisableControls()
        {
           // tabControl1.Enabled = false;
        }


        private void btnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;

            if (!m_Caspar.IsConnected)
            {
                m_Caspar.Settings.Hostname = txtCGServer.Text; // Properties.Settings.Default.Hostname;
                m_Caspar.Settings.Port = 5250;
                m_Caspar.Connect();
            }
            else
            {
                m_Caspar.Disconnect();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lstNameTemplates.Items.Add(txtName.Text + ";" + txtTitleClub.Text);
        }

        private void lstNameTemplates_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstNameTemplates.SelectedItem != null)
            {
                string[] parts = (lstNameTemplates.SelectedItem as string).Split(';');
                txtName.Text = parts[0];
                txtTitleClub.Text = parts[1];
            }
        }

        private void btnShowNameLabel_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();
                cgData.SetData("label_name", txtName.Text);
                cgData.SetData("label_title", txtTitleClub.Text);
                string templateName = templateFolder+ "/Title";
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerNaming, templateName, true, cgData);
                System.Diagnostics.Debug.WriteLine("Add");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
                System.Diagnostics.Debug.WriteLine(templateName);
                System.Diagnostics.Debug.WriteLine(cgData.ToXml());
            }
        }

        private void btnHideNameLabel_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(Properties.Settings.Default.GraphicsLayerNaming);
                System.Diagnostics.Debug.WriteLine("Stop");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
            }
        }

        private void btnRefreshResultListClasses_Click(object sender, EventArgs e)
        {
            if (m_emmaClient != null)
            {
                cmbClass.DataSource = m_emmaClient.GetClasses();
            }
        }

        private void cmbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            m_ResultsCurrentClassName = cmbClass.SelectedItem as string;
            List <cmbRadio>radios  = m_emmaClient.GetRadioControlsForClass(m_ResultsCurrentClassName)
                .OrderBy(x => x.Order).Select(x => new cmbRadio()
                {
                    code = x.Code,
                    Name = x.ControlName + "(" + x.Code + ")"
                }).ToList();

            radios.Insert(0,new cmbRadio()
            {
                code = 1000,
                Name = "Finish"
            });

            
            cmbResultListClassPosition.DataSource = radios;
            cmbResultListClassPosition.DisplayMember = "Name";

        }

        class cmbRadio
        {
            public string Name;
            public int code;

            public override string ToString()
            {
                return Name;
            }
        }

        void updateResultPageXOfX()
        {
            int numPages = (int)Math.Ceiling(m_currentResultList.Length/12.0);
            lblResultNumPages.Text = "Page " + m_ResultCurrentPage + " / " + numPages;
        }

        private void btnShowResultList_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();
                

                UpdateResultListPage(cgData);

                string templateName = templateFolder + "/ResultList";
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerResultList, templateName, true, cgData);
                System.Diagnostics.Debug.WriteLine("Add");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
                System.Diagnostics.Debug.WriteLine(templateName);
                System.Diagnostics.Debug.WriteLine(cgData.ToXml());
            }
        }

        private void UpdateResultListPage(CasparCGDataCollection cgData)
        {
            cgData.SetData("title_class", m_ResultsCurrentClassName);

            var list = m_currentResultList.Skip((m_ResultCurrentPage - 1)*12).Take(12).ToList();
            cgData.SetData("title_class_description", "Standings at " + m_ResultsCurrentPosition.Name);
            int lastTime = -1;
            int pos = (m_ResultCurrentPage - 1)*12 + 1;
            int winnerTime = m_currentResultList[0].Time;
            for (int i = 0; i < list.Count(); i++)
            {
                string sPos = list[i].Time != lastTime ? pos.ToString() : "=";
                pos++;
                lastTime = list[i].Time;
                cgData.SetData("res_name_" + i, list[i].Name);
                cgData.SetData("res_club_" + i, list[i].Club);
                cgData.SetData("res_place_" + i, list[i].Status == 0 ? sPos : "-");
                cgData.SetData("res_time_" + i, formatTime(list[i].Time,list[i].Status,false,true,false));
                if (list[i].Status == 0)
                {
                    cgData.SetData("res_timeplus_" + i, "+" + formatTime(list[i].Time - winnerTime, list[i].Status, false, true, false));
                }
                else
                {
                    cgData.SetData("res_timeplus_" + i, "-");
                }
            }

            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(Properties.Settings.Default.GraphicsLayerResultList);
                System.Diagnostics.Debug.WriteLine("Stop");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            m_ResultCurrentPage++;
            updateResultPageXOfX();
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();


                UpdateResultListPage(cgData);

                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerResultList, cgData);
                System.Diagnostics.Debug.WriteLine("Update");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
                System.Diagnostics.Debug.WriteLine(cgData.ToXml());
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_ResultCurrentPage < 1)
                m_ResultCurrentPage = 1;

            m_ResultCurrentPage--;
            updateResultPageXOfX();
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();


                UpdateResultListPage(cgData);

                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Update(Properties.Settings.Default.GraphicsLayerResultList, cgData);
                System.Diagnostics.Debug.WriteLine("Update");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
                System.Diagnostics.Debug.WriteLine(cgData.ToXml());
            }
        }


        private string formatTime(int time, int status, bool showTenthOs, bool showHours, bool padZeros) {

            if (status != 0) {
                return this.runnerStatus[status];
            } else {
                if (showHours)
                {
                    int hours = ((int) Math.Floor(time/360000.0));
                    int minutes = ((int)Math.Floor((time - hours * 360000d) / 6000d));
                    int seconds = ((int)Math.Floor((time - minutes * 6000d - hours * 360000) / 100));
                    int tenth = ((int)Math.Floor((time - minutes * 6000d - hours * 360000 - seconds * 100) / 10));
                    if (hours > 0)
                    {
                        string sHours = hours.ToString();
                        if (padZeros)
                            sHours = sHours.ToString().PadLeft(2, '0');

                        return sHours + ":" + minutes.ToString().PadLeft(2,'0') + ":" + seconds.ToString().PadLeft(2,'0') + (showTenthOs ? "." + tenth.ToString() : "");
                    } else {
                       

                        return (padZeros ? minutes.ToString().PadLeft(2,'0') : minutes.ToString()) + ":" + seconds.ToString().PadLeft(2,'0') + (showTenthOs ? "." + tenth : "");
                    }

                } else {

                    int minutes = (int)Math.Floor(time / 6000d);
                    int seconds = (int)Math.Floor((time - minutes * 6000d) / 100);
                    int tenth = (int)Math.Floor((time - minutes * 6000d - seconds * 100) / 10);
                    if (padZeros) {
                        return minutes.ToString().PadLeft(2,'0') + ":" + seconds.ToString().PadLeft(2,'0') + (showTenthOs ? "." + tenth : "");
                    } else {
                        return minutes + ":" + seconds.ToString().PadLeft(2,'0') + (showTenthOs ? "." + tenth : "");
                    }
                }
            }
        }

        private void btnRefreshPrewarningControls_Click(object sender, EventArgs e)
        {
            lstRadioControls.DataSource = m_emmaClient.GetAllRadioControls();
            lstRadioControls.DisplayMember = "Code";
            
        }

        private void btnStartPrewarning_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();

                UpdatePrewarnedRunners(cgData);

                //UpdateResultListPage(cgData);

                string templateName = templateFolder + "/prewarned_runners";
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerPrewarnedRunners, templateName, true, cgData);
                System.Diagnostics.Debug.WriteLine("Add");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
                System.Diagnostics.Debug.WriteLine(templateName);
                System.Diagnostics.Debug.WriteLine(cgData.ToXml());
            }
        }

        private void UpdatePrewarnedRunners(CasparCGDataCollection cgData)
        {
            cgData.SetData("title", "Prewarned runners");
            int pos = 0;
            foreach (var runner in m_emmaClient.GetAllRunners().OrderByDescending(x => x.StartTime + x.Time))
            {
                cgData.SetData("res_name_" + pos, runner.Name);
                cgData.SetData("res_club_" + pos, runner.Club);
                cgData.SetData("res_class_" + pos, runner.Class);
                if (runner.Time > 0)
                    cgData.SetData("res_time_" + pos, formatTime(runner.Time,runner.Status,false,true,true));

                pos++;
                if (pos > 2)
                    break;

            }
        }

        private void btnStopPrewarning_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(Properties.Settings.Default.GraphicsLayerPrewarnedRunners);
                System.Diagnostics.Debug.WriteLine("Stop");
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.GraphicsLayerNaming);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

           
        }

        private void txtNameFinder_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

        private void txtNameFinder_KeyUp(object sender, KeyEventArgs e)
        {
            if (txtNameFinder.Text.Length > 0)
            {
                listBox1.DisplayMember = "Name";
                listBox1.DataSource = m_emmaClient.GetAllRunners().Where(x => x.Name.IndexOf(txtNameFinder.Text) >= 0 || x.Club.IndexOf(txtNameFinder.Text) >= 0)
                    .ToArray();
            }
            else
            {
                listBox1.DataSource = null;
            }
        }

        private void txtNameFinder_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            Runner r = listBox1.SelectedItem as Runner;
            txtName.Text = r.Name;
            txtTitleClub.Text = r.Club;
        }

        private void btnStartClock_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();

                cgData.SetData("ref_time", txtClockRefTime.Text);
                cgData.SetData("show_tenth", chkClockShowTenth.Checked.ToString().ToLower());


                string templateName = templateFolder + "/clock";
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayersClock, templateName, true, cgData);
            }
        }

        private void btnStopClock_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(Properties.Settings.Default.GraphicsLayersClock);
                
            }
        }

        private void btnClockUpdate_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                CasparCGDataCollection cgData = new CasparCGDataCollection();

                cgData.SetData("ref_time", txtClockRefTime.Text);
                cgData.SetData("show_tenth", chkClockShowTenth.Checked.ToString().ToLower());

                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG
                    .Update(Properties.Settings.Default.GraphicsLayersClock, cgData);
            }
        }

        private void tabControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                if (tabControl1.SelectedTab == tabPage1)
                {
                    btnHideNameLabel_Click(sender,new EventArgs());
                }
                if (tabControl1.SelectedTab == tabPage2)
                {
                    button1_Click_1(sender, new EventArgs());
                }
                if (tabControl1.SelectedTab == tabPage4)
                {
                    btnStopClock_Click(sender,new EventArgs());
                }
            }
            if (e.KeyCode == Keys.F2)
            {
                if (tabControl1.SelectedTab == tabPage1)
                {
                    btnShowNameLabel_Click(sender,new EventArgs());
                }
                if (tabControl1.SelectedTab == tabPage2)
                {
                    btnShowResultList_Click(sender, new EventArgs());
                }
                if (tabControl1.SelectedTab == tabPage4)
                {
                    btnStartClock_Click(sender, new EventArgs());
                }
            }
        }

        class resultListItem
        {
            public string Name;
            public string Club;
            public int Time;
            public int Status;
        }

        private void cmbResultListClassPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selItem = cmbResultListClassPosition.SelectedItem as cmbRadio;
            m_ResultsCurrentPosition = selItem;
            if (selItem.code == 1000)
            {
                m_currentResultList = m_emmaClient.GetRunnersInClass(m_ResultsCurrentClassName)
                    .Where(x => x.Status != 9 && x.Status != 10 && x.Time != 0).OrderBy(x => x.Status).ThenBy(x => x.Time).Select(x=>new resultListItem { Club = x.Club, Name = x.Name, Status = x.Status, Time = x.Time}).ToArray();
            }
            else
            {
                m_currentResultList = m_emmaClient.GetRunnersInClass(m_ResultsCurrentClassName)
                    .Where(x => x.SplitTimes != null && x.SplitTimes.Any(y=>y.Control ==selItem.code)).OrderBy(x=>x.SplitTimes.First(y=>y.Control==selItem.code).Time).Select(x=>new resultListItem() {Club = x.Club, Name =  x.Name, Status = 0, Time = x.SplitTimes.First(y=>y.Control == selItem.code).Time}).ToArray();
            }
            m_ResultCurrentPage = 1;
            updateResultPageXOfX();
        }
    }
}
