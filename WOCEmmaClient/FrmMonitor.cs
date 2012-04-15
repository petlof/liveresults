using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WOCEmmaClient
{
    public partial class FrmMonitor : Form
    {
        IExternalSystemResultParser m_Parser;
        List<EmmaMysqlClient> m_Clients = new List<EmmaMysqlClient>();
        public FrmMonitor()
        {
            InitializeComponent();
        }

        private int m_CompetitionID;

        public int CompetitionID
        {
            get { return m_CompetitionID; }
            set { m_CompetitionID = value; }
        }
	

        public void SetParser(IExternalSystemResultParser parser)
        {
            m_Parser = parser;
            m_Parser.OnLogMessage += new LogMessageDelegate(m_Parser_OnLogMessage);
            m_Parser.OnResult += new ResultDelegate(m_Parser_OnResult);
        }

        void m_Parser_OnResult(int id, int SI, string name, string club, string Class, int start, int time, int status, List<ResultStruct> splits)
        {
            foreach (EmmaMysqlClient client in m_Clients)
            {
                if (!client.IsRunnerAdded(id))
                    client.AddRunner(new Runner(id, name, club, Class));
                else
                    client.UpdateRunnerInfo(id, name, club, Class);

                if (time != -2)
                {
                    client.SetRunnerResult(id, time, status);
                }

                foreach (ResultStruct str in splits)
                {
                    client.SetRunnerSplit(id, str.ControlCode, str.Time);
                }
            }
        }

        void m_Parser_OnLogMessage(string msg)
        {
            try
            {
                if (listBox1 != null && !listBox1.IsDisposed)
                {
                    listBox1.Invoke(new MethodInvoker(delegate
                    {
                        listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " " + msg);
                    }));
                }
            }
            catch
            {
            }
        }

        private void btnStartSTop_Click(object sender, EventArgs e)
        {
            if (btnStartSTop.Text == "Start")
            {
                EmmaMysqlClient.EmmaServer[] servers = EmmaMysqlClient.GetServersFromConfig();

                foreach (EmmaMysqlClient.EmmaServer srv in servers)
                {
                    EmmaMysqlClient cli = new EmmaMysqlClient(srv.host, 3309, srv.user, srv.pw, srv.db, m_CompetitionID);
                    m_Clients.Add(cli);
                    cli.OnLogMessage += new LogMessageDelegate(cli_OnLogMessage);
                    cli.Start();
                }
                m_Parser.Start();
                btnStartSTop.Text = "Stop";
            }
            else
            {
                foreach (EmmaMysqlClient cli in m_Clients)
                {
                    cli.Stop();
                }
                m_Parser.Stop();
                btnStartSTop.Text = "Start";
            }
        }

        void cli_OnLogMessage(string msg)
        {
            if (listBox1 != null && !listBox1.IsDisposed)
            {
                listBox1.Invoke(new MethodInvoker(delegate
                {
                    listBox1.Items.Insert(0, DateTime.Now.ToString("hh:mm:ss") + " " + msg);
                }));
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (btnStartSTop.Text == "Stop")
                btnStartSTop_Click(null, new EventArgs());

            this.Close();
        }
    }
}