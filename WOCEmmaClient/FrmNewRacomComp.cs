using System;
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
                        txtDSQFile.Text = s.DSQFile;
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
            try
            {
                var s = new Settings
                {
                    StartlistFile = txtStartlist.Text,
                    CompetitionID =  txtCompID.Text,
                    DSQFile =  txtDSQFile.Text,
                    RaceFile = txtRaceFile.Text,
                    RadioControlFile = txtRadioControls.Text,
                    RawSplitsFile = txtRawSplits.Text
                    , zeroTime =  dtZeroTime.Value,
                    IsRelay = checkBox1.Checked
                    
                    
                };

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EmmaClient");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string file = Path.Combine(path, "racomsetts.xml");
                var fs = File.Create(file);
                var ser = new XmlSerializer(typeof(Settings));
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
            public string DSQFile { get; set; }
            public string RawSplitsFile { get; set; }
            public string RadioControlFile { get; set; }
            public string CompetitionID { get; set; }
            public bool IsRelay { get; set; }
        }
    }
}
