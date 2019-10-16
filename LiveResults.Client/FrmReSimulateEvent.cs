using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
#if _CASPARCG_
using LiveResults.CasparClient;
#endif
using LiveResults.Model;

namespace LiveResults.Client
{
    public partial class FrmReSimulateEvent : Form
    {
        private EmmaMysqlClient m_client = null;
        private DateTime m_StartTime = DateTime.MaxValue;
        private DateTime m_CurrentTime = DateTime.MaxValue;

        private List<Event> eventsToSimulate;
        public FrmReSimulateEvent()
        {
            InitializeComponent();
        }

        class Event
        {
            public DateTime occurs;
            public ResultStruct Data;
            public Runner runner;
            public int RunnerStatus;

            public override string ToString()
            {
                return occurs.ToString("HH:mm:ss") + ": CN-" + Data.ControlCode + ", R-" + runner.Name + "(" + runner.Class + ")";
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            m_client = new EmmaMysqlClient("52.16.131.75", 3006, "liveresultat", "web", "liveresultat", int.Parse(textBox1.Text));
            m_client.Start();
            m_client.OnLogMessage += M_client_OnLogMessage;

            List<Event> events = new List<Event>();
            foreach (var runner in m_client.GetAllRunners())
            {
                int hour = (int) (runner.StartTime / (100.0 * 60 * 60));
                int minute = (int)((runner.StartTime-hour*60*60*100) / (100.0 * 60));
                int second = (int) ((runner.StartTime - hour * 60*60*100 - minute * 60*100) / (100.0));
                DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                if (startTime < m_StartTime)
                    m_StartTime = startTime;

                foreach (var splitTime in runner.SplitTimes)
                {
                    DateTime eventTime = startTime.AddSeconds(splitTime.Time / 100.0);
                    events.Add(new Event
                    {
                        occurs = eventTime,
                        Data = new ResultStruct
                        {
                            ControlCode = splitTime.Control,
                            Time = splitTime.Time,
                        },
                        runner = runner,
                        RunnerStatus = 0
                    });
                }

                if (runner.Time > 0)
                {
                    DateTime eventTime = startTime.AddSeconds(runner.Time / 100.0);
                    events.Add(new Event
                    {
                        occurs = eventTime,
                        Data = new ResultStruct
                        {
                            ControlCode = 1000,
                            Time = runner.Time
                        },
                        runner = runner,
                        RunnerStatus = runner.StageStatus
                    });
                }

                runner.ClearTimeAndSplits();

                eventsToSimulate = events.OrderBy(x => x.occurs).ToList();
                listBox1.DataSource = eventsToSimulate.Take(20).ToList();
                lblTime.Text = m_StartTime.ToString("HH:mm:ss");
            }

            m_client.SetCompetitionId(int.Parse(textBox2.Text));

        }

        private void M_client_OnLogMessage(string msg)
        {
            listBox2.Invoke(new MethodInvoker(() =>
            {
                listBox2.Items.Insert(0,msg);
            }));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            m_CurrentTime = m_CurrentTime.AddSeconds(1);
            //if (m_CurrentTime. > 0)
            //    m_CurrentTime = m_CurrentTime.AddMilliseconds(-1 * m_CurrentTime.Millisecond);
            lblTime.Text = m_CurrentTime.ToString("HH:mm:ss");
            while (eventsToSimulate.Count > 0 && (eventsToSimulate[0]).occurs == m_CurrentTime)
            {
                var ev = eventsToSimulate[0];
                if (ev.Data.ControlCode == 1000)
                {
                    m_client.SetRunnerResult(ev.runner.ID, ev.Data.Time,ev.RunnerStatus);
                }
                else
                {
                    m_client.SetRunnerSplit(ev.runner.ID, ev.Data.ControlCode, ev.Data.Time);
                }
                eventsToSimulate.RemoveAt(0);
                listBox1.DataSource = eventsToSimulate.Take(20).ToList();
            }

            if (eventsToSimulate.Count == 0)
            {
                timer1.Enabled = false;
                timer1.Stop();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_CurrentTime = m_StartTime;
            timer1.Enabled = true;
            timer1.Start();
            
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown1.Value > 0 ? 1000 / (int)numericUpDown1.Value : int.MaxValue;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (eventsToSimulate.Count > 0)
            {
                m_CurrentTime = eventsToSimulate[0].occurs.AddSeconds(-1);
                timer1_Tick(sender, e);
            }
        }

        private void FrmReSimulateEvent_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
#if _CASPARCG_
                CasparControlFrm frm =new CasparControlFrm();
                frm.SetEmmaClient(m_client);
                frm.Show();
#endif
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox2.Text = textBox1.Text;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            while (eventsToSimulate.Count > 0 && (eventsToSimulate[0]).occurs < DateTime.Now)
            {
                var ev = eventsToSimulate[0];
                if (ev.Data.ControlCode == 1000)
                {
                    m_client.SetRunnerResult(ev.runner.ID, ev.Data.Time, ev.RunnerStatus);
                }
                else
                {
                    m_client.SetRunnerSplit(ev.runner.ID, ev.Data.ControlCode, ev.Data.Time);
                }
                eventsToSimulate.RemoveAt(0);
                listBox1.DataSource = eventsToSimulate.Take(20).ToList();
            }
            m_CurrentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                DateTime.Now.Second);
        }
    }
}
