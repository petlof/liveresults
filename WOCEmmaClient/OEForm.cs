using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace LiveResults.Client
{
    public partial class OEForm : Form
    {
        List<EmmaMysqlClient> m_Clients;
        OSParser m_OSParser;
        OEParser m_OEParser;

        enum Format { IOFXML, OECSV, OSCSV, OECSVTEAM, OECSVPAR }
        class FormatItem
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public Format Format { get; set; }
            public FormatItem(string Name, string Description, Format format)
            {
                this.Name = Name;
                this.Description = Description;
                this.Format = format;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        List<FormatItem> supportedFormats = new List<FormatItem>();

        public OEForm()
        {
            InitializeComponent();
            Text = Text += ", " + Encoding.Default.EncodingName + "," + Encoding.Default.CodePage;
            fileSystemWatcher1.Changed += new FileSystemEventHandler(fileSystemWatcher1_Changed);
            m_Clients = new List<EmmaMysqlClient>();

            supportedFormats.Add(new FormatItem("IOF-XML", "Export files in IOF-XML (version 2 supported)", Format.IOFXML));
            supportedFormats.Add(new FormatItem("OE-csv", "CSV files exported from OLEinzel 10.3 and 11", Format.OECSV));
            supportedFormats.Add(new FormatItem("OS-csv", "CSV files exported from OLStaffel 10.3 and 11", Format.OSCSV));
            supportedFormats.Add(new FormatItem("OS-csv (Team)", "Team-CSV files exported from OSStaffel 10.3", Format.OECSVTEAM));
            supportedFormats.Add(new FormatItem("OE-csv (Par)", "Special format for DalaDubbeln from OLEinzel 10.3", Format.OECSVPAR));
            cmbFormat.DataSource = supportedFormats;
            cmbFormat.SelectedIndex = 0;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmmaClient");
            

            string file = Path.Combine(path, "oesetts.xml");
            if (File.Exists(file))
            {
                try
                {
                    var fs = File.OpenRead(file);
                    XmlSerializer ser = new XmlSerializer(typeof(Settings));
                    Settings s = ser.Deserialize(fs) as Settings;
                    fs.Close();
                    if (s != null)
                    {
                        txtOEDirectory.Text = s.Location;
                        txtExtension.Text = s.extension;
                        txtCompID.Text = s.CompID.ToString();
                        for (int i = 0; i < supportedFormats.Count; i++)
                        {
                            if (supportedFormats[i].Name == s.Format)
                                cmbFormat.SelectedIndex = i;
                        }

                    }
                }
                catch
                {
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            fileSystemWatcher1.EnableRaisingEvents = false;
            fileSystemWatcher1.Dispose();
            timer1.Enabled = false;
            timer1.Dispose();

            if (m_Clients != null)
            {
                foreach (EmmaMysqlClient c in m_Clients)
                {
                    c.Stop();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                txtOEDirectory.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtOEDirectory.Text))
            {
                MessageBox.Show(this, "Please select an existing OE Export directory", "Start OE Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtCompID.Text))
            {
                MessageBox.Show(this, "You must enter a competition-ID", "Start OE Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listBox1.Items.Clear();
            m_Clients.Clear();
            logit("Reading servers from config (eventually resolving online)");
            Application.DoEvents();
            EmmaMysqlClient.EmmaServer[] servers = EmmaMysqlClient.GetServersFromConfig();
            logit("Got servers from obasen...");
            Application.DoEvents();
            foreach (EmmaMysqlClient.EmmaServer server in servers)
            {
                EmmaMysqlClient client = new EmmaMysqlClient(server.host, 3306, server.user, server.pw, server.db, Convert.ToInt32(txtCompID.Text));

                client.OnLogMessage += new LogMessageDelegate(client_OnLogMessage);
                client.Start();
                m_Clients.Add(client);
            }

            timer1_Tick(null, null);

            FormatItem format = cmbFormat.SelectedItem as FormatItem;


            if ( format.Format == Format.OECSV || format.Format == Format.OECSVPAR || format.Format == Format.OECSVTEAM || format.Format == Format.OSCSV)
            {
                m_OSParser = new OSParser();
                m_OSParser.OnLogMessage +=
                delegate (string msg)
                {
                    logit(msg);
                };

                m_OSParser.OnResult += new ResultDelegate(m_OSParser_OnResult);

                m_OEParser = new OEParser();
                m_OEParser.OnLogMessage +=
                delegate(string msg)
                {
                    logit(msg);
                };

                m_OEParser.OnResult += new ResultDelegate(m_OSParser_OnResult);

                fsWatcherOS.Path = txtOEDirectory.Text;
                fsWatcherOS.Filter = txtExtension.Text;
                fsWatcherOS.EnableRaisingEvents = true;

            }
            else if (format.Format == Format.IOFXML)
            {
                fileSystemWatcher1.Path = txtOEDirectory.Text;
                fileSystemWatcher1.Filter = txtExtension.Text;
                fileSystemWatcher1.NotifyFilter = NotifyFilters.LastWrite;
                fileSystemWatcher1.IncludeSubdirectories = false;
                fileSystemWatcher1.EnableRaisingEvents = true;
            }
        }

        void m_OSParser_OnResult(Result newResult)
        {
            foreach (EmmaMysqlClient c in m_Clients)
            {
                if (!c.IsRunnerAdded(newResult.ID))
                {
                    c.AddRunner(new Runner(newResult.ID, newResult.RunnerName, newResult.RunnerClub, newResult.Class));
                }
                else
                    c.UpdateRunnerInfo(newResult.ID, newResult.RunnerName, newResult.RunnerClub, newResult.Class);


                if (newResult.StartTime >= 0)
                    c.SetRunnerStartTime(newResult.ID, newResult.StartTime);

                c.SetRunnerResult(newResult.ID, newResult.Time, newResult.Status);
                if (newResult.SplitTimes != null)
                {
                    foreach (ResultStruct r in newResult.SplitTimes)
                    {
                        c.SetRunnerSplit(newResult.ID, r.ControlCode, r.Time);
                    }
                }
            }
        }

        void client_OnLogMessage(string msg)
        {
            logit(msg);
        }

        void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            string filename = e.Name;
            string fullFilename = e.FullPath;
            logit(filename + " changed..");
            bool processed = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var runners = Parsers.IOFXmlV2Parser.ParseFile(fullFilename, new LogMessageDelegate(delegate(string msg) { logit(msg); }));
                    processed = true;
                    foreach (EmmaMysqlClient c in m_Clients)
                    {
                        c.MergeRunners(runners);
                    }
                }
                catch (Exception ee)
                {
                    logit(ee.Message);
                }

                if (!processed)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }
            if (!processed)
            {
                logit("Could not open " + filename + " for processing");
            }
        }

        private static int ParseTime(string time)
        {
            int itime = -9;
            string[] timeParts = time.Split(':');
            if (timeParts.Length == 3)
            {
                //HH:MM:SS
                itime = Convert.ToInt32(timeParts[0]) * 360000 + Convert.ToInt32(timeParts[1]) * 6000 + Convert.ToInt32(timeParts[2]) * 100;
            }
            else if (timeParts.Length == 2)
            {
                //MM:SS
                itime = Convert.ToInt32(timeParts[0]) * 6000 + Convert.ToInt32(timeParts[1]) * 100;
            }
            return itime;
        }

        void logit(string msg)
        {
            if (!listBox1.IsDisposed)
            {
                listBox1.Invoke(new MethodInvoker(delegate
                {
                    listBox1.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") + " " + msg);
                }));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listBox2.BeginUpdate();

            listBox2.Items.Clear();
            if (m_Clients != null)
            {
                foreach (EmmaMysqlClient c in m_Clients)
                {
                    listBox2.Items.Add(c);
                }
            }
            listBox2.EndUpdate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_Clients != null)
            {
                foreach (EmmaMysqlClient c in m_Clients)
                {
                    c.Stop();
                }
                m_Clients.Clear();
                timer1_Tick(null, null);
            }
            fileSystemWatcher1.EnableRaisingEvents = false;
            fsWatcherOS.EnableRaisingEvents = false;
        }

        private void fsWatcherOS_Changed(object sender, FileSystemEventArgs e)
        {
            string filename = e.Name;
            string fullFilename = e.FullPath;
            logit(filename + " changed..");
            bool processed = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    StreamReader sr = null;
                    if (!File.Exists(fullFilename))
                    {
                        return;
                    }
                    else
                    {
                        sr = new StreamReader(fullFilename, Encoding.Default);
                    }
                    //string tmp = sr.ReadToEnd();
                    sr.Close();

                    FormatItem format = cmbFormat.SelectedItem as FormatItem;

                    if (format.Format == Format.OECSV)
                    {
                        m_OEParser.AnalyzeFile(fullFilename);
                    }
                    else if (format.Format == Format.OSCSV)
                    {
                        m_OSParser.AnalyzeFile(fullFilename);
                    }
                    else if (format.Format == Format.OECSVTEAM)
                    {
                        m_OSParser.AnalyzeTeamFile(fullFilename);
                    }
                    else if (format.Format == Format.OECSVPAR)
                    {
                        m_OEParser.AnalyzeFile(fullFilename,true);
                    }

                    File.Delete(fullFilename);
                    processed = true;
                }
                catch (Exception ee)
                {
                    logit(ee.Message);
                }

                if (!processed)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }
            if (!processed)
            {
                logit("Could not open " + filename + " for processing");
            }
        }


        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblFormatInfo.Text = "";
            if (cmbFormat.SelectedItem != null)
            {
                lblFormatInfo.Text = (cmbFormat.SelectedItem as FormatItem).Description;


                if ((cmbFormat.SelectedItem as FormatItem).Format == Format.IOFXML)
                {
                    txtExtension.Text = "*.xml";
                }
                else
                {
                    txtExtension.Text = "*.csv";
                }
            }
        }

        private void OEForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Settings s = new Settings()
                {
                    Location = txtOEDirectory.Text,
                    CompID = int.Parse(txtCompID.Text),
                    extension = txtExtension.Text,
                    Format = (cmbFormat.SelectedItem as FormatItem).Name

                };

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmmaClient");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string file = Path.Combine(path, "oesetts.xml");
                var fs = File.Create(file);
                XmlSerializer ser = new XmlSerializer(typeof(Settings));
                ser.Serialize(fs, s);
                fs.Close();
            }
            catch
            {
            }
        }

        [Serializable]
        public class Settings
        {
            public string Location { get; set; }
            public int  CompID { get; set; }
            public string extension { get; set; }
            public string Format { get; set; }
        }
    }
}