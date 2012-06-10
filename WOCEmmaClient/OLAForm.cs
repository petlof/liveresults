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
    public partial class OlaForm : Form
    {
        private List<EmmaMysqlClient> m_Clients = new List<EmmaMysqlClient>();
        //private EmmaMysqlClient m_Client = null;
        private OlaParser m_Parser = null;

        private LogMessageDelegate m_LogDelegate;
        public OlaForm()
        {
            InitializeComponent();
            m_LogDelegate = new LogMessageDelegate(LogMsg);
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
                

                OleDbConnection conn = GetOlaConnection();
                m_Parser = new OlaParser(conn, (cmbOLAComp.SelectedItem as OlaComp).Id, (cmbOLAEtapp.SelectedItem as OlaComp).Id);
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

        void m_Parser_OnLogMessage(string msg)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        private void loadSettings()
        {
            string filename = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\settingsOLA.xml");
            if (System.IO.File.Exists(filename))
            {
                DataSet ds = new DataSet("Settings");
                ds.ReadXml(filename);
                DataTable dt = ds.Tables["Ola"];
                DataRow r = dt.Rows[0];
                txtServer.Text = r["server"] as string;
                txtUser.Text = r["user"] as string;
                txtPass.Text = r["password"] as string;
                txtDB.Text = r["db"] as string;
                txtCompID.Text = r["compid"] as string;

                if (dt.Columns.Contains("olauser") && r["olauser"] != null)
                {
                    txtOlaUser.Text = r["olauser"] as string;
                }
                if (dt.Columns.Contains("olapw") &&  r["olapw"] != null)
                {
                    txtOlaPass.Text = r["olapw"] as string;
                }

                txtOlaDB.Text = r["olaserver"] as string;
                cmbOlaDB.Items.Add(r["oladb"] as string);

                cmbOlaDB.SelectedIndex = 0;

                if (r["olaevent"] == null)
                    return;

                OlaComp cmp = new OlaComp();
                string[] comp = (r["olaevent"] as string).Split(';');
                for (int i = 0; i < cmbOLAComp.Items.Count; i++)
                {
                    OlaComp c = cmbOLAComp.Items[i] as OlaComp;
                    if (c.Id == Convert.ToInt32(comp[0]))
                    {
                        cmbOLAComp.SelectedIndex = i;
                        break;
                    }
                    
                }

                cmp = new OlaComp();
                comp = (r["olarace"] as string).Split(';');
                cmp.Id = Convert.ToInt32(comp[0]);
                cmp.Name = comp[1];

                for (int i = 0; i < cmbOLAEtapp.Items.Count; i++)
                {
                    if (((OlaComp)cmbOLAEtapp.Items[i]).Id == cmp.Id)
                    {
                        cmbOLAEtapp.SelectedIndex = i;
                        break;
                    }
                }
                ds.Dispose();
            }
        }

        private void saveSettings()
        {
            DataSet ds = new DataSet("Settings");
            DataTable dt = new DataTable("Ola");
            dt.Columns.Add("server");
            dt.Columns.Add("user");
            dt.Columns.Add("password");
            dt.Columns.Add("db");
            dt.Columns.Add("compid");

            dt.Columns.Add("olaserver");
            dt.Columns.Add("oladb");
            dt.Columns.Add("olaevent");
            dt.Columns.Add("olarace");

            dt.Columns.Add("olauser");
            dt.Columns.Add("olapw");

            DataRow r = dt.NewRow();
            r["server"] = txtServer.Text;
            r["user"] = txtUser.Text;
            r["password"] = txtPass.Text;
            r["db"] = txtDB.Text;
            r["compid"] = txtCompID.Text;
            r["olaserver"] = txtOlaDB.Text;
            r["oladb"] = cmbOlaDB.SelectedItem;
            r["olaevent"] = ((OlaComp)cmbOLAComp.SelectedItem).Id + ";" + ((OlaComp)cmbOLAComp.SelectedItem).Name;
            r["olarace"] = ((OlaComp)cmbOLAEtapp.SelectedItem).Id + ";" + ((OlaComp)cmbOLAEtapp.SelectedItem).Name;
            r["olauser"] = txtOlaUser.Text;
            r["olapw"] = txtOlaPass.Text;
            dt.Rows.Add(r);
            ds.Tables.Add(dt);
            if (!System.IO.Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\")))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\"));
            }
            ds.WriteXml(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lofas\\EMMA\\settingsOLA.xml"));
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
            listBox1.Invoke(m_LogDelegate,new object[] {msg});   
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
                m_Parser.OnLogMessage -= new LogMessageDelegate(OnLogMessage);
                m_Parser.OnResult -= new ResultDelegate(pars_OnResult);
                m_Parser.Stop();
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
            OleDbConnection conn = new OleDbConnection("Provider=SQLOLEDB.1;Persist Security Info=False;User ID=" + txtOlaUser.Text + ";Password=" +txtOlaPass.Text +";Initial Catalog=" + (cmbOlaDB.SelectedIndex >= 0 ? cmbOlaDB.SelectedItem : "") + ";Data Source=" + txtOlaDB.Text);
            return conn;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OleDbConnection conn = GetOlaConnection();
            try
            {
                cmbOlaDB.Items.Clear();
                conn.Open();
                DataTable schemas = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null);
                //dataGridView1.DataSource = schemas;
                foreach (DataRow r in schemas.Rows)
                {
                    cmbOlaDB.Items.Add(r["CATALOG_NAME"]);
                }
                if (cmbOlaDB.Items.Count > 0)
                    cmbOlaDB.SelectedIndex = 0;
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

        private void OnOlaDbSelIndexChanged(object sender, EventArgs e)
        {
            OleDbConnection conn = GetOlaConnection();
            try
            {
                cmbOLAComp.Items.Clear();
                conn.Open();
                OleDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select eventid, name from Events";
                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    OlaComp cmp = new OlaComp();
                    cmp.Id = Convert.ToInt32(reader["eventid"]);
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmbOLAComp.Items.Add(cmp);
                }
                reader.Close();
                cmd.Dispose();

                if (cmbOLAComp.Items.Count > 0)
                   cmbOLAComp.SelectedIndex = 0;
            }
            catch (Exception ee)
            {
            }
            finally
            {
                conn.Close();
            }
        }

        private class OlaComp
        {
            public string Name;
            public int Id;

            public override string ToString()
            {
                return Name;
            }
        }

        private void OnOlaCompSelIndxChanged(object sender, EventArgs e)
        {
            OleDbConnection conn = GetOlaConnection();
            try
            {
                cmbOLAEtapp.Items.Clear();
                conn.Open();
                OleDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select eventraceid, name from EventRaces where eventid = " + ((OlaComp)cmbOLAComp.SelectedItem).Id;
                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    OlaComp cmp = new OlaComp();
                    cmp.Id = Convert.ToInt32(reader["eventraceid"]);
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmbOLAEtapp.Items.Add(cmp);
                }
                reader.Close();
                cmd.Dispose();

                if (cmbOLAEtapp.Items.Count > 0)
                    cmbOLAEtapp.SelectedIndex = 0;
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
}