using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using LiveResults.Client.Parsers;

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
            lblInfo.Text = "Export liveresults by generic IOF-XML and from SportSoftware OE/OS 20003/2010";
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults from OS-Speaker automatic CSV-export";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lblInfo.Text = "Export liveresults from OS-Speaker automatic CSV-export";
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
            OEForm frm = new OEForm();
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
            var monForm = new FrmMonitor();

            using (var frm = new FrmNewRacomComp())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    var rsP = new RacomFileSetParser(frm.txtStartlist.Text, frm.txtRawSplits.Text, frm.txtRaceFile.Text, frm.txtDSQFile.Text,
                        frm.txtRadioControls.Text, frm.dtZeroTime.Value,frm.checkBox1.Checked);

                    monForm.SetParser(rsP);
                    monForm.CompetitionID = int.Parse(frm.txtCompID.Text);
                    monForm.ShowDialog(this);
                }
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

            FrmMonitor monForm = new FrmMonitor();
            //string[] lines = File.ReadAllLines("wocinfo.txt");
            //int compId = int.Parse(lines[0]);
            int compId = 10640;
            List<string> urls = new List<string>();
            /*for (int i = 1; i < lines.Length; i++)
                urls.Add(lines[i]);*/
            urls.Add("http://www.liveresultater.no/includes/individuell/orientering/lister/allelister.php?q=alle&w=alle&s=DESC&c=radiopost.tid&a=350&acc=sek&lang=en_EN&sid=0.7241666284389794");

            LiveResultaterNoParser wp = new LiveResultaterNoParser(urls.ToArray());
            monForm.SetParser(wp as IExternalSystemResultParser);
            monForm.CompetitionID = compId;
            monForm.ShowDialog(this);
        }
    }
}