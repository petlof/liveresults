using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using LiveResults.Client.Parsers;

namespace LiveResults.Client
{
    public partial class OEForm : Form
    {
        readonly List<EmmaMysqlClient> m_clients;
        OSParser m_osParser;
        OEParser m_oeParser;

        enum Format { Iofxml, Oecsv, Oscsv, Oecsvteam }
        class FormatItem
        {
            public string Name { get; private set; }
            public string Description { get; private set; }
            public Format Format { get; private set; }
            public FormatItem(string name, string description, Format format)
            {
                Name = name;
                Description = description;
                Format = format;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        readonly List<FormatItem> m_supportedFormats = new List<FormatItem>();

        public OEForm()
        {
            InitializeComponent();
            Text = Text + @", " + Encoding.Default.EncodingName + @"," + Encoding.Default.CodePage;
            fileSystemWatcher1.Changed += new FileSystemEventHandler(fileSystemWatcher1_Changed);
            m_clients = new List<EmmaMysqlClient>();

            m_supportedFormats.Add(new FormatItem("IOF-XML", "Export files in IOF-XML (version 2 supported)", Format.Iofxml));
            m_supportedFormats.Add(new FormatItem("OE-csv", "CSV files exported from OLEinzel 10.3 and 11", Format.Oecsv));
            m_supportedFormats.Add(new FormatItem("OS-csv", "CSV files exported from OLStaffel 10.3 and 11", Format.Oscsv));
            m_supportedFormats.Add(new FormatItem("OS-csv (Team)", "Team-CSV files exported from OSStaffel 10.3", Format.Oecsvteam));
            cmbFormat.DataSource = m_supportedFormats;
            cmbFormat.SelectedIndex = 0;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmmaClient");
            

            string file = Path.Combine(path, "oesetts.xml");
            if (File.Exists(file))
            {
                try
                {
                    var fs = File.OpenRead(file);
                    var ser = new XmlSerializer(typeof(Settings));
                    var s = ser.Deserialize(fs) as Settings;
                    fs.Close();
                    if (s != null)
                    {
                        txtOEDirectory.Text = s.Location;
                        txtExtension.Text = s.extension;
                        txtCompID.Text = s.CompID.ToString(CultureInfo.InvariantCulture);
                        for (int i = 0; i < m_supportedFormats.Count; i++)
                        {
                            if (m_supportedFormats[i].Name == s.Format)
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

            if (m_clients != null)
            {
                foreach (EmmaMysqlClient c in m_clients)
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
                MessageBox.Show(this, @"Please select an existing OE Export directory", @"Start OE Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(txtCompID.Text))
            {
                MessageBox.Show(this, @"You must enter a competition-ID", @"Start OE Monitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listBox1.Items.Clear();
            m_clients.Clear();
            Logit("Reading servers from config (eventually resolving online)");
            Application.DoEvents();
            EmmaMysqlClient.EmmaServer[] servers = EmmaMysqlClient.GetServersFromConfig();
            Logit("Got servers from obasen...");
            Application.DoEvents();
            foreach (EmmaMysqlClient.EmmaServer server in servers)
            {
                var client = new EmmaMysqlClient(server.Host, 3306, server.User, server.Pw, server.DB, Convert.ToInt32(txtCompID.Text));

                client.OnLogMessage += client_OnLogMessage;
                client.Start();
                m_clients.Add(client);
            }

            timer1_Tick(null, null);

            var format = cmbFormat.SelectedItem as FormatItem;


            if ( format.Format == Format.Oecsv || format.Format == Format.Oecsvteam || format.Format == Format.Oscsv)
            {
                m_osParser = new OSParser();
                m_osParser.OnLogMessage += Logit;

                m_osParser.OnResult += m_OSParser_OnResult;

                m_oeParser = new OEParser();
                m_oeParser.OnLogMessage += Logit;

                m_oeParser.OnResult += m_OSParser_OnResult;

                fsWatcherOS.Path = txtOEDirectory.Text;
                fsWatcherOS.Filter = txtExtension.Text;
                fsWatcherOS.EnableRaisingEvents = true;

            }
            else if (format.Format == Format.Iofxml)
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
            foreach (EmmaMysqlClient c in m_clients)
            {
                if (!c.IsRunnerAdded(newResult.ID))
                {
                    c.AddRunner(new Runner(newResult.ID, newResult.RunnerName, newResult.RunnerClub, newResult.Class));
                }
                else
                    c.UpdateRunnerInfo(newResult.ID, newResult.RunnerName, newResult.RunnerClub, newResult.Class);


                if (newResult.StartTime >= 0)
                    c.SetRunnerStartTime(newResult.ID, newResult.StartTime);

                if (newResult is RelayResult)
                {
                    var rr = newResult as RelayResult;
                    c.SetRunnerResult(newResult.ID, rr.OverallTime, rr.OverallStatus);
                }
                else
                {
                    c.SetRunnerResult(newResult.ID, newResult.Time, newResult.Status);
                }

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
            Logit(msg);
        }

        void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            string filename = e.Name;
            string fullFilename = e.FullPath;
            Logit(filename + " changed..");
            bool processed = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var runners = Parsers.IOFXmlV2Parser.ParseFile(fullFilename, Logit);
                    processed = true;
                    foreach (EmmaMysqlClient c in m_clients)
                    {
                        c.MergeRunners(runners);
                    }
                }
                catch (Exception ee)
                {
                    Logit(ee.Message);
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
                Logit("Could not open " + filename + " for processing");
            }
        }

        void Logit(string msg)
        {
            if (!listBox1.IsDisposed)
            {
                listBox1.Invoke(new MethodInvoker(() => listBox1.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") + " " + msg)));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listBox2.BeginUpdate();

            listBox2.Items.Clear();
            if (m_clients != null)
            {
                foreach (EmmaMysqlClient c in m_clients)
                {
                    listBox2.Items.Add(c);
                }
            }
            listBox2.EndUpdate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_clients != null)
            {
                foreach (EmmaMysqlClient c in m_clients)
                {
                    c.Stop();
                }
                m_clients.Clear();
                timer1_Tick(null, null);
            }
            fileSystemWatcher1.EnableRaisingEvents = false;
            fsWatcherOS.EnableRaisingEvents = false;
        }

        private void fsWatcherOS_Changed(object sender, FileSystemEventArgs e)
        {
            string filename = e.Name;
            string fullFilename = e.FullPath;
            Logit(filename + " changed..");
            bool processed = false;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    StreamReader sr;
                    if (!File.Exists(fullFilename))
                    {
                        return;
                    }
                    
                    sr = new StreamReader(fullFilename, Encoding.Default);
                    sr.Close();

                    var format = cmbFormat.SelectedItem as FormatItem;

                    if (format.Format == Format.Oecsv)
                    {
                        m_oeParser.AnalyzeFile(fullFilename);
                    }
                    else if (format.Format == Format.Oscsv)
                    {
                        m_osParser.AnalyzeFile(fullFilename);
                    }
                    else if (format.Format == Format.Oecsvteam)
                    {
                        m_osParser.AnalyzeTeamFile(fullFilename);
                    }

                    File.Delete(fullFilename);
                    processed = true;
                }
                catch (Exception ee)
                {
                    Logit(ee.Message);
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
                Logit("Could not open " + filename + " for processing");
            }
        }


        private void cmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblFormatInfo.Text = "";
            if (cmbFormat.SelectedItem != null)
            {
                lblFormatInfo.Text = (cmbFormat.SelectedItem as FormatItem).Description;


                if ((cmbFormat.SelectedItem as FormatItem).Format == Format.Iofxml)
                {
                    txtExtension.Text = @"*.xml";
                }
                else
                {
                    txtExtension.Text = @"*.csv";
                }
            }
        }

        private void OEForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                var s = new Settings
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
                var ser = new XmlSerializer(typeof(Settings));
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