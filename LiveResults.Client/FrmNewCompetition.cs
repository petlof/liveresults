using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LiveResults.CasparClient;
using LiveResults.Client.Parsers;
using LiveResults.Model;

namespace LiveResults.Client
{
    public partial class FrmNewCompetition : Form
    {
        public FrmNewCompetition()
        {
            InitializeComponent();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnOLA_MouseHover(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults from SOFTs OLA-system";
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults by generic IOF-XML v2 and v3 (SportSoftware 2010, MeOS, Helga, AutoDownload,...)";
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults from SSF-Timing (BETA)";
        }
        private void button4_MouseEnter(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults from Sportsoftware OE/OS with CSV-format. Historic reasons, please use IOF XML V3 when possible";
        }

        private void buttonRacom_MouseEnter(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults from RaCom special fileset format";
        }

        private void btn_MouseLeave(object sender, EventArgs e)
        {
            lblInfo.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults from OL-Staffel automatic CSV-export";
        }

        private void btnOLA_Click(object sender, EventArgs e)
        {
            NewOLAComp cmp = new NewOLAComp();
            cmp.ShowDialog(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OEForm frm = new OEForm(false);
            frm.ShowDialog();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FrmMonitor monForm = new FrmMonitor();
            string[] lines = File.ReadAllLines("wocinfo.txt");
            int compId = int.Parse(lines[0]);
            List<string> urls = new List<string>();
            for (int i = 1; i < lines.Length; i++)
                urls.Add(lines[i]);
            /*WocParser wp = new WocParser(new string[] {
                "http://www.woc2012.ch/results/live/" + comp + "-women-heat-a.json",
                "http://www.woc2012.ch/results/live/" + comp + "-women-heat-b.json",
                "http://www.woc2012.ch/results/live/" + comp + "-women-heat-c.json",
            "http://www.woc2012.ch/results/live/" + comp + "-men-heat-a.json",
                "http://www.woc2012.ch/results/live/" + comp + "-men-heat-b.json",
                "http://www.woc2012.ch/results/live/" + comp + "-men-heat-c.json"});*/
            WocParser wp = new WocParser(urls.ToArray());
            monForm.SetParser(wp as IExternalSystemResultParser);
            monForm.CompetitionID = compId;
            monForm.ShowDialog(this);
        }



        private void button1_Click_2(object sender, EventArgs e)
        {
            FrmMonitor monForm = new FrmMonitor();
            //string[] lines = File.ReadAllLines("wocinfo.txt");
            //int compId = int.Parse(lines[0]);
            int compId = -127;
            List<string> urls = new List<string>();
            /*for (int i = 1; i < lines.Length; i++)
                urls.Add(lines[i]);*/
            urls.Add("http://www.ori-live.com/special/go-live-eoc2014-6/EOC2014-6-Men%20A.html");
            urls.Add("http://www.ori-live.com/special/go-live-eoc2014-6/EOC2014-6-Woman%20A.html");

            OriLiveParser wp = new OriLiveParser(urls.ToArray());
            monForm.SetParser(wp as IExternalSystemResultParser);
            monForm.CompetitionID = compId;
            monForm.ShowDialog(this);
        }

        private void button1_Click_3(object sender, EventArgs e)
        {
            FrmNewRacomComp newRacomComp = new FrmNewRacomComp();
            if (newRacomComp.ShowDialog(this) == DialogResult.OK)
            {
                FrmMonitor monForm = new FrmMonitor();
                monForm.SetParser(new RacomFileSetParser(newRacomComp.txtStartlist.Text, newRacomComp.txtRawSplits.Text, newRacomComp.txtRaceFile.Text,
                    newRacomComp.txtDSQFile.Text, newRacomComp.txtRadioControls.Text, newRacomComp.dtZeroTime.Value, newRacomComp.checkBox1.Checked));
                monForm.CompetitionID = int.Parse(newRacomComp.txtCompID.Text);
                monForm.ShowDialog(this);
            }
            //FrmMonitor monForm = new FrmMonitor();
            ////string[] lines = File.ReadAllLines("wocinfo.txt");
            ////int compId = int.Parse(lines[0]);
            //int compId = -50;
            //List<string> urls = new List<string>();
            ///*for (int i = 1; i < lines.Length; i++)
            //    urls.Add(lines[i]);*/
            //urls.Add("http://www.liveresultater.no/includes/individuell/orientering/lister/allelister.php?q=alle&w=alle&s=DESC&c=radiopost.tid&a=439&acc=sek&lang=en&sid=0.3296072955708951");

            //var wp = new LiveResultaterNoParser(urls.ToArray());
            //monForm.SetParser(wp as IExternalSystemResultParser);
            //monForm.CompetitionID = compId;
            //monForm.ShowDialog(this);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            NewSSFTimingComp cmp = new NewSSFTimingComp();
            cmp.ShowDialog(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OEForm frm = new OEForm();
            frm.ShowDialog();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            CasparClient.CasparControlFrm frm = new CasparControlFrm();
            frm.Show();
        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FrmNewCompetition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.R)
            {
                var parser = new RaceTimerParser(new string[]
                {
                    "http://www.racetimer.se/sv/race/resultlist/3647?race_id=3646&layout=racetimer&rc_id=14200&per_page=2500&commit=Visa+resultat+%3E%3E",
                    "http://www.racetimer.se/sv/race/resultlist/3647?race_id=3646&layout=racetimer&rc_id=14417&per_page=2500&commit=Visa+resultat+%3E%3E",
                    "http://www.racetimer.se/sv/race/resultlist/3647?race_id=3646&layout=racetimer&rc_id=14418&per_page=2005&commit=Visa+resultat+%3E%3E",
                    "http://www.racetimer.se/sv/race/resultlist/3647?race_id=3646&layout=racetimer&rc_id=14419&per_page=2500&commit=Visa+resultat+%3E%3E"

                });
                var mon = new FrmMonitor();
                mon.CompetitionID = -112;
                mon.SetParser(parser);
                mon.ShowDialog(this);
            }

            if (e.KeyCode == Keys.W)
            {
                var parser = new TulospalveluParser(new string[]
                {
                    //"http://online4.tulospalvelu.fi/tulokset/en/2017_wrelay/str8w/tilanne/1/0/",
                    //"http://online4.tulospalvelu.fi/tulokset/en/2017_wrelay/str8m/tilanne/1/0/",
                    "http://online4.tulospalvelu.fi/tulokset/en/2017_wrelay/SPRINT/tilanne/1/0/",
                    "http://online4.tulospalvelu.fi/tulokset/en/2017_wrelay/SPRINT/tilanne/2/0/",
                    "http://online4.tulospalvelu.fi/tulokset/en/2017_wrelay/SPRINT/tilanne/3/0/",
                    "http://online4.tulospalvelu.fi/tulokset/en/2017_wrelay/SPRINT/tilanne/4/0/"

                });
                var mon = new FrmMonitor();
                mon.CompetitionID = 12496;
                mon.SetParser(parser);
                mon.ShowDialog(this);
            }

        }

        private void FrmNewCompetition_KeyPress(object sender, KeyPressEventArgs e)
        {

          
        }
    }
}