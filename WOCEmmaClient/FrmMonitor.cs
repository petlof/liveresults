using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using LiveResults.Client.Parsers;

namespace LiveResults.Client
{
    public partial class FrmMonitor : Form
    {
        IExternalSystemResultParser m_Parser;
        List<EmmaMysqlClient> m_Clients = new List<EmmaMysqlClient>();
        public FrmMonitor()
        {
            InitializeComponent();
            Text = Text += ", " + Encoding.Default.EncodingName + "," + Encoding.Default.CodePage;
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
            m_Parser.OnRadioControl += (name, code, className, order) =>
            {
                foreach (EmmaMysqlClient client in m_Clients)
                {
                    client.SetRadioControl(className, code, name, order);
                }
            };
        }

        void m_Parser_OnResult(Result newResult)
        {
            foreach (EmmaMysqlClient client in m_Clients)
            {
                if (!client.IsRunnerAdded(newResult.ID))
                    client.AddRunner(new Runner(newResult.ID, newResult.RunnerName, newResult.RunnerClub, newResult.Class));
                else
                    client.UpdateRunnerInfo(newResult.ID, newResult.RunnerName, newResult.RunnerClub, newResult.Class);

                if (newResult.StartTime > 0)
                    client.SetRunnerStartTime(newResult.ID, newResult.StartTime);


                if (newResult.Time != -2)
                {
                    client.SetRunnerResult(newResult.ID, newResult.Time, newResult.Status);
                }

                if (newResult.SplitTimes != null)
                {
                    foreach (ResultStruct str in newResult.SplitTimes)
                    {
                        client.SetRunnerSplit(newResult.ID, str.ControlCode, str.Time);
                    }
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
                    EmmaMysqlClient cli = new EmmaMysqlClient(srv.Host, 3309, srv.User, srv.Pw, srv.DB, m_CompetitionID);
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