﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace LiveResults.Client
{
    public partial class FrmNewRacomComp : Form
    {
        public FrmNewRacomComp()
        {
            InitializeComponent();
            this.FormClosing += FrmNewRacomComp_FormClosing;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmmaClient");
            string file = Path.Combine(path, "racomsetts.xml");
            this.FrmNewRacomComp_LoadSettingsFromFile(file);

        }

        private void FrmNewRacomComp_LoadSettingsFromFile(string file)
        {
            if (File.Exists(file))
            {
                FileStream fs = null;
                try
                {
                    fs = File.OpenRead(file);
                    var ser = new XmlSerializer(typeof(Settings));
                    var s = ser.Deserialize(fs) as Settings;

                    if (s != null)
                    {
                        txtStartlist.Text = s.StartlistFile;
                        txtRawSplits.Text = s.RawSplitsFile;
                        txtRadioControls.Text = s.RadioControlFile;
                        txtRaceFile.Text = s.RaceFile;
                        txtCompID.Text = s.CompetitionID;
                        dtZeroTime.Value = s.zeroTime;
                        checkBox1.Checked = s.IsRelay;

                    }
                }
                catch
                {
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }

        }

        void FrmNewRacomComp_FormClosing(object sender, FormClosingEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmmaClient");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string file = Path.Combine(path, "racomsetts.xml");
            SaveSettingsToFile(file);
        }

        private void SaveSettingsToFile(string fileName)
        {
            try
            {
                var s = new Settings{
                    StartlistFile = txtStartlist.Text,
                    CompetitionID = txtCompID.Text,
                    RaceFile = txtRaceFile.Text,
                    RadioControlFile = txtRadioControls.Text,
                    RawSplitsFile = txtRawSplits.Text
                    ,
                    zeroTime = dtZeroTime.Value,
                    IsRelay = checkBox1.Checked
                };

                
                var fs = File.Create(fileName);
                var ser = new XmlSerializer(typeof (Settings));
                ser.Serialize(fs, s);
                fs.Close();
            }
            catch
            {
            }
        }

        public class Settings
        {
            public DateTime zeroTime { get; set; }
            public string StartlistFile { get; set; }
            public string RaceFile { get; set; }
            public string RawSplitsFile { get; set; }
            public string RadioControlFile { get; set; }
            public string CompetitionID { get; set; }
            public bool IsRelay { get; set; }
        }

        private void btn_loadsetting_Click(object sender, EventArgs e)
        {
            if (openSettingsDialog.ShowDialog(this) == DialogResult.OK)
            {
                FrmNewRacomComp_LoadSettingsFromFile(openSettingsDialog.FileName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "XML-files|*.xml";
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                SaveSettingsToFile(sfd.FileName);
                MessageBox.Show(this, "File saved!", "Save settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
       
    }
}
