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
            m_emmaClient.ResultChanged += EmmaClientOnResultChanged;
        }

        private void EmmaClientOnResultChanged(Runner runner, int resultPosition)
        {
            if (!string.IsNullOrEmpty(m_ResultsCurrentClassName) && m_ResultsCurrentPosition != null)
            {
                if (rdoResultTypeLowerThrid.Checked &&  runner.Class == m_ResultsCurrentClassName && resultPosition == m_ResultsCurrentPosition.code)
                {
                    UpdateCurrentResultList();
                    string templateName;
                    var cgData = GetResultsCasparData(out templateName);
                    if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
                    {
                        m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG
                            .Update(Properties.Settings.Default.GraphicsLayerResultList, cgData);
                    }
                }
            }
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
                cmbClass.DataSource = m_emmaClient.GetClasses().OrderBy(x => x).ToList();
            }
        }

        private void cmbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            m_ResultsCurrentClassName = cmbClass.SelectedItem as string;
            List<cmbRadio> radios = new List<cmbRadio>();
            if (m_emmaClient.GetRadioControlsForClass(m_ResultsCurrentClassName) != null)
            {
                radios = m_emmaClient.GetRadioControlsForClass(m_ResultsCurrentClassName)
                    .OrderBy(x => x.Order).Select(x => new cmbRadio()
                    {
                        code = x.Code,
                        Name = x.ControlName
                    }).ToList();
            }
            radios.Insert(0,new cmbRadio
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
                UpdateCurrentResultList();
                string templateName;
                var cgData = GetResultsCasparData(out templateName);
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerResultList, templateName, true, cgData);
            }
        }

        private CasparCGDataCollection GetResultsCasparData(out string templateName)
        {
            CasparCGDataCollection cgData = new CasparCGDataCollection();
            templateName = "";
            if (rdoResultListTypeFF.Checked)
            {
                UpdateResultListPage(cgData);
                templateName = templateFolder + "/ResultList";
            }
            else if (rdoResultTypeLowerThrid.Checked)
            {
                UpdateResultListPagePassing(cgData);
                templateName = templateFolder + "/ResultList_Passing";
            }
            return cgData;
        }

        private DateTime m_nextForcedUpdate = DateTime.MaxValue;
        private void UpdateResultListPagePassing(CasparCGDataCollection cgData)
        {
            cgData.SetData("title_class", m_ResultsCurrentClassName + " - " + m_ResultsCurrentPosition.Name);

            var list = m_currentResultList;
            int lastTime = -1;
            int pos = 1;

            lstBoxItem selItem = null;
            int selItemEstimated_Place = -1;

            listBox2.Invoke(new MethodInvoker(() =>
            {
                selItem = listBox2.SelectedItem as lstBoxItem;
            }));

            if (selItem != null)
            {
                cgData.SetData("follow_name", selItem.runner.Name);
                cgData.SetData("follow_club", selItem.runner.Club);

                var runnerInResults = m_currentResultList.FirstOrDefault(x => x.runner.ID == selItem.runner.ID);
                if (runnerInResults == null)
                {

                    int h = (int)(selItem.runner.StartTime / (100.0 * 60 * 60));
                    int m = (int)((selItem.runner.StartTime - h * 60 * 60 * 100) / (100.0 * 60));
                    int s = (int)((selItem.runner.StartTime - h * 60 * 60 * 100 - m * 60 * 100) / (100.0));

                    var now = DateTime.Now;
                    var startDate = new DateTime(now.Year, now.Month, now.Day, h, m, s);
                    var runnerCurrentTime = (DateTime.Now - startDate).TotalSeconds * 100;
                    var runnerCurrentPlace = m_currentResultList.Count(x => x.Status == 0 && x.Time > 0 && x.Time < runnerCurrentTime) + 1;
                    selItemEstimated_Place = runnerCurrentPlace;
                    cgData.SetData("follow_place", "(" + runnerCurrentPlace + ")");
                    cgData.SetData("follow_starttime", h + "," + m + "," + s);

                    if (m_currentResultList.Length > 0)
                    {
                        cgData.SetData("follow_tref", "" + (int)((runnerCurrentTime - m_currentResultList[0].Time) / 100));
                    }
                    else {
                        cgData.SetData("follow_tref", "" + (int)((runnerCurrentTime) / 100));
                    }
                    var nextResultItem = m_currentResultList.FirstOrDefault(x => x.Time > runnerCurrentTime);
                    if (nextResultItem != null)
                    {
                        m_nextForcedUpdate = DateTime.Now.AddSeconds((nextResultItem.Time - runnerCurrentTime) / 100);
                    }
                    else
                        m_nextForcedUpdate = DateTime.MaxValue;
                }
                else
                {
                    int place = m_currentResultList.Count(x => x.Status == 0 && x.Time > 0 && x.Time < runnerInResults.Time) + 1;
                    selItemEstimated_Place = place;
                    cgData.SetData("follow_place", place.ToString());
                    if (place == 1)
                    {
                        cgData.SetData("follow_time", formatTime(runnerInResults.Time, 0, false, true, true));
                    }
                    else
                    {
                        cgData.SetData("follow_time", "+" + formatTime(runnerInResults.Time - m_currentResultList[0].Time, 0, false, true, true));
                    }
                }


            }


            if (m_currentResultList.Length > 0)
            {
                int leaderTime = m_currentResultList[0].Time;
                int p = 0;
                for (int i = 0; i < list.Count(); i++)
                {
                    string sPos = list[i].Time != lastTime ? pos.ToString() : "=";
                    pos++;
                    lastTime = list[i].Time;
                    cgData.SetData("res_name_" +p, list[i].runner.Name);
                    cgData.SetData("res_club_" + p, list[i].runner.Club);
                    cgData.SetData("res_place_" + p, list[i].Status == 0 ? sPos : "-");

                    var time = i == 0 ? list[i].Time : list[i].Time - leaderTime;

                    cgData.SetData("res_time_" + p, (i > 0 ? "+" : "") + formatTime(time, list[i].Status, false, true, false));

                    if (i == 0 && selItemEstimated_Place > 3 && m_currentResultList.Length > 5)
                    {
                        i = Math.Min(selItemEstimated_Place - 3, list.Length - 5);
                      
                        pos = i+2;
                    }
                    p++;
                    if (p > 4)
                        break;
                }
            }

           


        }

        private void UpdateResultListPage(CasparCGDataCollection cgData)
        {
            cgData.SetData("title_class", m_ResultsCurrentClassName);

            var list = m_currentResultList.Skip((m_ResultCurrentPage - 1)*12).Take(12).ToList();
            cgData.SetData("title_class_description", "Standings at " + m_ResultsCurrentPosition.Name);
            int lastTime = -1;
            int pos = (m_ResultCurrentPage - 1)*12 + 1;
            if (m_currentResultList.Length > 0)
            {
                int winnerTime = m_currentResultList[0].Time;
                for (int i = 0; i < list.Count(); i++)
                {
                    string sPos = list[i].Time != lastTime ? pos.ToString() : "=";
                    pos++;
                    lastTime = list[i].Time;
                    cgData.SetData("res_name_" + i, list[i].runner.Name);
                    cgData.SetData("res_club_" + i, list[i].runner.Club);
                    cgData.SetData("res_place_" + i, list[i].Status == 0 ? sPos : "-");
                    cgData.SetData("res_time_" + i, formatTime(list[i].Time, list[i].Status, false, true, false));
                    if (list[i].Status == 0)
                    {
                        cgData.SetData("res_timeplus_" + i,
                            "+" + formatTime(list[i].Time - winnerTime, list[i].Status, false, true, false));
                    }
                    else
                    {
                        cgData.SetData("res_timeplus_" + i, "-");
                    }
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
            var radios = m_emmaClient.GetAllRadioControls();
            lstRadioControls.DataSource = radios.Select(x => x).ToArray();
            lstRadioControls.DisplayMember = "Code";

            cmbPre1.DataSource = radios.Select(x => x).ToArray();
            cmbPre1.DisplayMember = "Code";
            cmbPre2.DataSource = radios.Select(x => x).ToArray();
            cmbPre2.DisplayMember = "Code";
            cmbPre3.DataSource = radios.Select(x => x).ToArray();
            cmbPre3.DisplayMember = "Code";
            cmbPre4.DataSource = radios.Select(x => x).ToArray();
            cmbPre4.DisplayMember = "Code";

            var classes = m_emmaClient.GetClasses().OrderBy(x => x).ToArray();
            var classesToChoose = new string[classes.Length + 1];
            classes.CopyTo(classesToChoose, 1);
            classesToChoose[0] = "None";
            cmbPreClass1.DataSource = classesToChoose.Select(x => x).ToArray();
            cmbPreClass2.DataSource = classesToChoose.Select(x => x).ToArray();
            cmbPreClass3.DataSource = classesToChoose.Select(x => x).ToArray();
            cmbPreClass4.DataSource = classesToChoose.Select(x => x).ToArray();
        }

        private bool showingPrewarned = false;
        private void btnStartPrewarning_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                
                CasparCGDataCollection cgData = new CasparCGDataCollection();

                UpdatePrewarnedRunners(cgData);

                //UpdateResultListPage(cgData);

                string templateName = templateFolder + "/ResultList_Q";
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Add(Properties.Settings.Default.GraphicsLayerPrewarnedRunners, templateName, true, cgData);
                showingPrewarned = true;
            }
        }

        private void UpdatePrewarnedRunners(CasparCGDataCollection cgData)
        {
            int follIdx = 0;
            if (cmbPreClass1.SelectedIndex > 0)
            {
                SetCGDataForPrewarnedClass(cgData, cmbPreClass1,cmbPre1,int.Parse(txtNumQ1.Text), 1, ref follIdx);
            }
            if (cmbPreClass2.SelectedIndex > 0)
            {
                SetCGDataForPrewarnedClass(cgData, cmbPreClass2, cmbPre2, int.Parse(txtNumQ2.Text), 2, ref follIdx);
            }
            if (cmbPreClass3.SelectedIndex > 0)
            {
                SetCGDataForPrewarnedClass(cgData, cmbPreClass3, cmbPre3, int.Parse(txtNumQ3.Text), 3, ref follIdx);
            }
            if (cmbPreClass4.SelectedIndex > 0)
            {
                SetCGDataForPrewarnedClass(cgData, cmbPreClass4, cmbPre4, int.Parse(txtNumQ4.Text), 4, ref follIdx);
            }
            /*if (cmbPreClass2.SelectedIndex > 0)
            {
                cgData.SetData("title_class_2", cmbPreClass2.SelectedItem as string);
                var leader = m_emmaClient.GetRunnersInClass(cmbPreClass2.SelectedItem as string)
                    .Where(x => x.Time > 0 && x.Status == 0).OrderBy(x => x.Time).FirstOrDefault();
                if (leader != null)
                {
                    cgData.SetData("res2_name_0", leader.Name);
                    cgData.SetData("res2_club_0", leader.Club);
                    cgData.SetData("res2_place_0", "1");
                    cgData.SetData("res2_time_0", formatTime(leader.Time, leader.Status, false, true, true));
                }
            }
            if (cmbPreClass3.SelectedIndex > 0)
            {
                cgData.SetData("title_class_3", cmbPreClass3.SelectedItem as string);
                var leader = m_emmaClient.GetRunnersInClass(cmbPreClass3.SelectedItem as string)
                    .Where(x => x.Time > 0 && x.Status == 0).OrderBy(x => x.Time).FirstOrDefault();
                if (leader != null)
                {
                    cgData.SetData("res3_name_0", leader.Name);
                    cgData.SetData("res3_club_0", leader.Club);
                    cgData.SetData("res3_place_0", "1");
                    cgData.SetData("res3_time_0", formatTime(leader.Time, leader.Status, false, true, true));
                }
            }
            if (cmbPreClass4.SelectedIndex > 0)
            {
                cgData.SetData("title_class_4", cmbPreClass4.SelectedItem as string);
                var leader = m_emmaClient.GetRunnersInClass(cmbPreClass4.SelectedItem as string)
                    .Where(x => x.Time > 0 && x.Status == 0).OrderBy(x => x.Time).FirstOrDefault();
                if (leader != null)
                {
                    cgData.SetData("res4_name_0", leader.Name);
                    cgData.SetData("res4_club_0", leader.Club);
                    cgData.SetData("res4_place_0", "1");
                    cgData.SetData("res4_time_0", formatTime(leader.Time, leader.Status, false, true, true));
                }
            }
            */


            /*
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

            }*/
        }

        private void SetCGDataForPrewarnedClass(CasparCGDataCollection cgData, ComboBox cmbBox, ComboBox preWarnControl,int numQualifiers, int resIdx, ref int follIdx)
        {
            cgData.SetData("title_class_" + resIdx, cmbBox.SelectedItem as string);
            var res = m_emmaClient.GetRunnersInClass(cmbBox.SelectedItem as string)
                .Where(x => x.Time > 0 && (x.Status == 0|| x.Status == 10)).OrderBy(x => x.Time).ToList();

            Runner leader = null;
            if (res.Count > 0)
            {
                leader = res[0];
                cgData.SetData("res" + resIdx + "_name_0", leader.Name);
                cgData.SetData("res" + resIdx + "_club_0", leader.Club);
                cgData.SetData("res" + resIdx + "_place_0", "1");
                cgData.SetData("res" + resIdx + "_time_0", formatTime(leader.Time, leader.Status, false, true, true));
            }

            //Get Prewarned
                int preWarnCode = (preWarnControl.SelectedItem as RadioControl).Code;
            foreach (var prewarned in m_emmaClient.GetRunnersInClass(cmbBox.SelectedItem as string)
                .Where(x => x.Time <= 0 && (x.Status == 0 || x.Status == 9 || x.Status == 10) && x.SplitTimes != null &&
                            x.SplitTimes.Any(y => y.Control == preWarnCode)))
            {
                var curTime = DateTime.Now.Hour * 360000 + DateTime.Now.Minute * 6000 + DateTime.Now.Second * 100 - prewarned.StartTime;
                cgData.SetData("foll_name_" + follIdx, prewarned.Name);
                cgData.SetData("foll_club_" + follIdx, prewarned.Club);
                cgData.SetData("foll_place_" + follIdx, "");
                cgData.SetData("foll_timeref_" + follIdx, (leader != null && curTime < leader.Time ? "" : "+") +  (leader != null ? curTime - leader.Time : curTime)/100 + ".0");
                follIdx++;
            }

            //Finished within 10sek
            var curRealTime = DateTime.Now.Hour * 360000 + DateTime.Now.Minute * 6000 + DateTime.Now.Second * 100;
            foreach (var prewarned in m_emmaClient.GetRunnersInClass(cmbBox.SelectedItem as string)
                .Where(x => x.Time > 0 /*&& (x.StartTime+x.Time) < curRealTime */&& (x.StartTime+x.Time) > curRealTime-10*100 && x.Status == 0))
            {
                if (prewarned != leader)
                {
                    // var curTime = DateTime.Now.Hour * 360000 + DateTime.Now.Minute * 6000 + DateTime.Now.Second * 100 - prewarned.StartTime;
                    cgData.SetData("foll_name_" + follIdx, prewarned.Name);
                    cgData.SetData("foll_club_" + follIdx, prewarned.Club);
                    var pTime = prewarned.Time;
                    cgData.SetData("foll_place_" + follIdx, res.Count(x => x.Time > 0 && x.Status == 0 && x.Time < pTime)+1 + "");
                    cgData.SetData("foll_time_" + follIdx,
                        "+" + formatTime(prewarned.Time - leader.Time, prewarned.Status, false, true, true));
                    follIdx++;
                }
            }


            if (leader != null)
            {

                int place = 1;
                int lastTime = res[0].Time;
                for (int i = 1; i < res.Count; i++)
                {
                    if (place == numQualifiers)
                    {
                        cgData.SetData("res" + resIdx + "_name_1", res[i].Name);
                        cgData.SetData("res" + resIdx + "_club_1", res[i].Club);
                        cgData.SetData("res" + resIdx + "_place_1", "" + place);
                        cgData.SetData("res" + resIdx + "_time_1",
                            "+" + formatTime(res[i].Time - leader.Time, res[i].Status, false, true, true));
                    }
                    if (res[i].Time == lastTime)
                    {
                    }
                    else
                    {
                        place = i + 2;
                        lastTime = res[i].Time;
                    }
                }
            }
        }

        private void btnStopPrewarning_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(Properties.Settings.Default.GraphicsLayerPrewarnedRunners);
            }
            showingPrewarned = false;
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
            public Runner runner;
            public int Time;
            public int Status;
        }

        class lstBoxItem
        {
            public Runner runner;
            
            public override string ToString()
            {
                int h = (int)(runner.StartTime / (100.0 * 60 * 60));
                int m = (int)((runner.StartTime - h * 60 * 60 * 100) / (100.0 * 60));
                int s = (int)((runner.StartTime - h * 60 * 60 * 100 - m * 60 * 100) / (100.0));
                return h.ToString().PadLeft(2, '0') + ":" + m.ToString().PadLeft(2,'0') + ":" + s.ToString().PadLeft(2,'0') + " - " +  runner.Name + " " + runner.Club;
            }
        }

        private void cmbResultListClassPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selItem = cmbResultListClassPosition.SelectedItem as cmbRadio;
            m_ResultsCurrentPosition = selItem;
            UpdateCurrentResultList();
            var runners = m_emmaClient.GetRunnersInClass(m_ResultsCurrentClassName).OrderBy(x => x.StartTime).Select(x => new lstBoxItem { runner = x }).ToList();
            if (!chkShowAlsoAlreadyPassed.Checked && m_currentResultList != null)
            {
                var idsInResults = m_currentResultList.Select(x => x.runner.ID).ToArray();
                runners = runners.Where(x => Array.IndexOf(idsInResults, x.runner.ID) < 0).ToList();
            }
            listBox2.DataSource = runners;
            m_ResultCurrentPage = 1;
            updateResultPageXOfX();
        }

        private void UpdateCurrentResultList()
        {
            if (m_ResultsCurrentPosition.code == 1000)
            {
                m_currentResultList = m_emmaClient.GetRunnersInClass(m_ResultsCurrentClassName)
                    .Where(x => x.Status != 9 && x.Status != 10 && x.Time != 0).OrderBy(x => x.Status).ThenBy(x => x.Time).Select(
                        x => new resultListItem
                        {
                            runner= x,
                            Status = x.Status,
                            Time = x.Time
                        }).ToArray();
            }
            else
            {
                m_currentResultList = m_emmaClient.GetRunnersInClass(m_ResultsCurrentClassName)
                    .Where(x => x.SplitTimes != null && x.SplitTimes.Any(y => y.Control == m_ResultsCurrentPosition.code)).OrderBy(x => x.SplitTimes.First(y => y.Control == m_ResultsCurrentPosition.code).Time).Select(x => new resultListItem() { runner = x, Status = 0, Time = x.SplitTimes.First(y => y.Control == m_ResultsCurrentPosition.code).Time }).ToArray();

            }
        }

        private void rdoResultTypeLowerThrid_CheckedChanged(object sender, EventArgs e)
        {
            button3.Visible = lblResultNumPages.Visible = button4.Visible = rdoResultListTypeFF.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                UpdateCurrentResultList();
                string templateName;
                var cgData = GetResultsCasparData(out templateName);
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG
                    .Update(Properties.Settings.Default.GraphicsLayerResultList, cgData);
            }
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                listBox2.SelectedItem = null;
            }
        }

        private DateTime pwUpdate = DateTime.MinValue;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (DateTime.Now > m_nextForcedUpdate)
            {
                m_nextForcedUpdate = DateTime.MaxValue;
                button2_Click(sender, e);
            }
            if (showingPrewarned && (DateTime.Now - pwUpdate).TotalSeconds > 1)
            {
                btnPrewarningForceUpdate_Click(sender, e);
                pwUpdate = DateTime.Now;
            }
        }

        private void btnPrewarningForceUpdate_Click(object sender, EventArgs e)
        {
            if (m_Caspar.IsConnected && m_Caspar.Channels.Count > 0)
            {
                CasparCGDataCollection cgData = new CasparCGDataCollection();
                UpdatePrewarnedRunners(cgData);
                m_Caspar.Channels[Properties.Settings.Default.CasparChannel].CG
                    .Update(Properties.Settings.Default.GraphicsLayerPrewarnedRunners, cgData);
            }
        }

        private void chkShowAlsoAlreadyPassed_CheckedChanged(object sender, EventArgs e)
        {
            var runners = m_emmaClient.GetRunnersInClass(m_ResultsCurrentClassName).OrderBy(x => x.StartTime).Select(x => new lstBoxItem { runner = x }).ToList();
            if (!chkShowAlsoAlreadyPassed.Checked && m_currentResultList != null)
            {
                var idsInResults = m_currentResultList.Select(x => x.runner.ID).ToArray();
                runners = runners.Where(x => Array.IndexOf(idsInResults, x.runner.ID) < 0).ToList();
            }
            listBox2.DataSource = runners;
        }
    }
}
