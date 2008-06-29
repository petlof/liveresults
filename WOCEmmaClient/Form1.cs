using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WOCEmmaClient
{
    public delegate void LogDelegate(string msg);
    public partial class Form1 : Form
    {
        private EmmaMysqlClient m_Client = null;
        private OEParser m_Parser = null;

        private LogDelegate m_LogDelegate;
        public Form1()
        {
            InitializeComponent();
            m_LogDelegate = new LogDelegate(LogMsg);
            loadSettings();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                m_Client = new EmmaMysqlClient(txtServer.Text, 3306, txtUser.Text, txtPass.Text, txtDB.Text,Convert.ToInt32(txtCompID.Text));
                m_Client.OnLogMessage += new LogMessageDelegate(OnLogMessage);
                m_Client.Start();
                m_Parser = new OEParser(txtOEDir.Text);
                m_Parser.OnLogMessage += new LogMessageDelegate(OnLogMessage);
                m_Parser.OnResult += new ResultDelegate(pars_OnResult);
                m_Parser.Start();

                saveSettings();

                //m_Client.AddRunner(new Runner(0, "Peter Löfås", "Stora Tuna IK", "H21 Elit"));
            }
            catch (Exception ee)
            {
                m_LogDelegate(ee.Message);
            }
        }

        private void loadSettings()
        {
            string filename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\settings.xml");
            if (System.IO.File.Exists(filename))
            {
                DataSet ds = new DataSet("Settings");
                ds.ReadXml(filename);
                DataTable dt = ds.Tables["OE"];
                DataRow r = dt.Rows[0];
                txtServer.Text = r["server"] as string;
                txtUser.Text = r["user"] as string;
                txtPass.Text = r["password"] as string;
                txtOEDir.Text = r["dir"] as string;
                txtDB.Text = r["db"] as string;
                txtCompID.Text = r["compid"] as string;
                ds.Dispose();
            }
        }

        private void saveSettings()
        {
            DataSet ds = new DataSet("Settings");
            DataTable dt = new DataTable("OE");
            dt.Columns.Add("server");
            dt.Columns.Add("user");
            dt.Columns.Add("password");
            dt.Columns.Add("dir");
            dt.Columns.Add("db");
            dt.Columns.Add("compid");

            DataRow r = dt.NewRow();
            r["server"] = txtServer.Text;
            r["user"] = txtUser.Text;
            r["password"] = txtPass.Text;
            r["dir"] = txtOEDir.Text;
            r["db"] = txtDB.Text;
            r["compid"] = txtCompID.Text;
            dt.Rows.Add(r);
            ds.Tables.Add(dt);
            ds.WriteXml(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\settings.xml"));
            ds.Dispose();
        }

        void pars_OnResult(int id,int si, string name, string club, string Class, int start, int time, int status, List<ResultStruct> results)
        {
            if (m_Client != null)
            {
                Runner r = null;
                if (!m_Client.IsRunnerAdded(id))
                {
                    r = new Runner(id, name, club, Class);
                    m_Client.AddRunner(r);
                }

                m_Client.SetRunnerResult(id, time, status);
                foreach (ResultStruct s in results)
                {
                    m_Client.SetRunnerSplit(id, s.ControlCode, s.Time);
                }
            }
        }

        void OnLogMessage(string msg)
        {
            listBox1.Invoke(m_LogDelegate,new object[] {msg});   
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_Client != null)
            {
                m_Client.Stop();
                m_Parser.OnLogMessage -= new LogMessageDelegate(OnLogMessage);
                m_Parser.OnResult -= new ResultDelegate(pars_OnResult);
            }
        }

        private void LogMsg(string msg)
        {
            listBox1.Items.Insert(0,DateTime.Now.ToString("yyyy-MM-dd HH:mm") + " " + msg);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (m_Client != null)
                lblCount.Text = m_Client.UpdatesPending.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog diag = new FolderBrowserDialog();
            diag.Description = "Select folder where exported OE files are copied";
            diag.ShowDialog(this);
            txtOEDir.Text = diag.SelectedPath;
        }
    }
}