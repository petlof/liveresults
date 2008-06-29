using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Data.OleDb;
using System.Configuration;

namespace WOCEmmaClient
{
    public partial class SITimecForm : Form
    {
        private List<EmmaMysqlClient> m_Clients = new List<EmmaMysqlClient>();
        //private EmmaMysqlClient m_Client = null;
        SiTimecParser m_Parser;

        private LogDelegate m_LogDelegate;
        public SITimecForm()
        {
            InitializeComponent();
            m_LogDelegate = new LogDelegate(LogMsg);
            loadSettings();           
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            button3_Click(null, null);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                saveSettings();
                EmmaMysqlClient.EmmaServer[] servers = EmmaMysqlClient.GetServersFromConfig();
                foreach (EmmaMysqlClient.EmmaServer server in servers)
                {
                    EmmaMysqlClient client = new EmmaMysqlClient(server.host, 3306, server.user, server.pw, server.db, Convert.ToInt32(txtCompID.Text));
                    
                    client.OnLogMessage += new LogMessageDelegate(OnLogMessage);
                    client.Start();
                    m_Clients.Add(client);
                    lstServers.Items.Add(client);
                    //m_Client = new EmmaMysqlClient(ConfigurationSettings.AppSettings.Get("emmaServer"), 3306, ConfigurationSettings.AppSettings.Get("emmaUser"), ConfigurationSettings.AppSettings.Get("emmaPw"), ConfigurationSettings.AppSettings.Get("emmaDB"), Convert.ToInt32(txtCompID.Text));
                    //m_Client.OnLogMessage += new LogMessageDelegate(OnLogMessage);
                    //m_Client.Start();
                }
                listBox1.Items.Clear();

                m_Parser = new SiTimecParser(GetOlaConnection());
                m_Parser.OnLogMessage += new LogMessageDelegate(OnLogMessage);
                m_Parser.OnResult += new ResultDelegate(pars_OnResult);
                m_Parser.Start();                               

                //m_Client.AddRunner(new Runner(0, "Peter Löfås", "Stora Tuna IK", "H21 Elit"));
            }
            catch (Exception ee)
            {
                m_LogDelegate(ee.Message);
            }
        }

        private void loadSettings()
        {
            string filename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\settingsSITimec.xml");
            if (System.IO.File.Exists(filename))
            {
                DataSet ds = new DataSet("Settings");
                ds.ReadXml(filename);
                DataTable dt = ds.Tables["SITimec"];
                DataRow r = dt.Rows[0];
                txtCompID.Text = r["compid"] as string;

                if (dt.Columns.Contains("sitimecuser") && r["sitimecuser"] != null)
                {
                    txtOlaUser.Text = r["sitimecuser"] as string;
                }
                if (dt.Columns.Contains("sitimecpw") &&  r["sitimecpw"] != null)
                {
                    txtOlaPass.Text = r["sitimecpw"] as string;
                }

                txtOlaDB.Text = r["sitimecserver"] as string;

                ds.Dispose();
            }
        }

        private void saveSettings()
        {
            DataSet ds = new DataSet("Settings");
            DataTable dt = new DataTable("SITimec");
            dt.Columns.Add("compid");

            dt.Columns.Add("SITimecserver");
            dt.Columns.Add("SITimecdb");

            dt.Columns.Add("SITimecuser");
            dt.Columns.Add("SITimecpw");

            DataRow r = dt.NewRow();
            r["compid"] = txtCompID.Text;
            r["SITimecserver"] = txtOlaDB.Text;
            r["SITimecdb"] = cmbTavling.SelectedItem;
            r["SITimecuser"] = txtOlaUser.Text;
            r["SITimecpw"] = txtOlaPass.Text;
            dt.Rows.Add(r);
            ds.Tables.Add(dt);
            if (!System.IO.Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\")))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\"));
            }
            ds.WriteXml(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\settingsSITimec.xml"));
            ds.Dispose();
        }

        void pars_OnResult(int id,int si, string name, string club, string Class, int start, int time, int status, List<ResultStruct> results)
        {
            if (m_Clients != null && m_Clients.Count > 0)
            {
                foreach (EmmaMysqlClient client in m_Clients)
                {
                    Runner r = null;
                    if (!client.IsRunnerAdded(id))
                    {
                        if (name.Length > 0 && Class.Length > 0)
                        {
                            r = new Runner(id, name, club, Class);
                            client.AddRunner(r);
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (time != -2)
                    {
                        client.SetRunnerResult(id, time, status);
                    }
                    foreach (ResultStruct s in results)
                    {
                        client.SetRunnerSplit(id, s.ControlCode, s.Time);
                    }
                }
            }
        }

        void OnLogMessage(string msg)
        {
            if (!listBox1.IsDisposed)
            {
                listBox1.Invoke(m_LogDelegate, new object[] { msg });
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_Clients != null)
            {
                while (m_Clients.Count > 0)
                {
                    EmmaMysqlClient client = m_Clients[0];
                    lstServers.Items.Remove(client);
                    client.Stop();
                    m_Clients.Remove(client);
                }
            }

            if (m_Parser != null)
            {
                m_Parser.Stop();
                m_Parser = null;
            }
        }

        private void LogMsg(string msg)
        {
            listBox1.Items.Insert(0,DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + msg);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int numPending = 0;
            //if (m_Clients != null)
            //{
            //    foreach (EmmaMysqlClient client in m_Clients)
            //    {
            //        numPending += client.UpdatesPending;
            //    }
            //}
            //lblCount.Text = numPending.ToString();
            lstServers.BeginUpdate();
            lstServers.Items.Clear();
            foreach (EmmaMysqlClient c in m_Clients)
            {
                lstServers.Items.Add(c);
            }
            lstServers.EndUpdate();
        }

        private OleDbConnection GetOlaConnection()
        {
            OleDbConnection conn = new OleDbConnection("Provider=SQLOLEDB.1;Persist Security Info=False;User ID=" + txtOlaUser.Text + ";Password=" +txtOlaPass.Text +";Initial Catalog=" + (cmbTavling.SelectedItem != null ? (cmbTavling.SelectedItem as SiTimecComp).DbName : "") + ";Data Source=" + txtOlaDB.Text);
            return conn;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OleDbConnection conn = GetOlaConnection();
            try
            {
                cmbTavling.Items.Clear();
                conn.Open();
                DataTable schemas = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null);
                OleDbCommand cmd = conn.CreateCommand();
                //dataGridView1.DataSource = schemas;
                foreach (DataRow r in schemas.Rows)
                {
                    string dbName = r["CATALOG_NAME"] as string;
                    if (dbName.StartsWith("SiTimec"))
                    {
                        cmd.CommandText = "select Value from " + dbName + ".dbo.CompetitionProperties where Name = 'Name'";
                        string compname = cmd.ExecuteScalar() as string;
                        cmbTavling.Items.Add(new SiTimecComp(compname,dbName));
                    }
                }
                if (cmbTavling.Items.Count > 0)
                    cmbTavling.SelectedIndex = 0;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, ee.Message);
            }
            finally
            {
                conn.Close();
            }
        }
    }

    class SiTimecComp
    {
        public string Name;
        public string DbName;
        public SiTimecComp(string name,string dbname)
        {
            Name = name;
            DbName = dbname;
        }

        public override string ToString()
        {
            return Name + " [" + DbName + "]";
        }
    }
}