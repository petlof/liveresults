using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LiveResults.Model;
using Svt.Caspar;

namespace LiveResults.CasparClient
{
    public class PlayoutJobResultListLowerThird : PlayoutJob
    {
        private readonly string m_className;
        private readonly int m_resultPosition;
        private readonly string m_resultPositionName;
        private Runner m_followRunner = null;
        private bool m_followTail = false;
        private DateTime m_nextForcedUpdate = DateTime.MaxValue;
        private readonly EmmaMysqlClient m_emmaMysqlClient;
        private ResultListItem[] m_currentResultList;
        public PlayoutJobResultListLowerThird(CasparDevice device, string templateFolder, EmmaMysqlClient emmaClient, string className, int positionCode, string positionName) : 
            base(device,templateFolder)
        {
            m_className = className;
            m_resultPosition = positionCode;
            m_resultPositionName = positionName;
            m_emmaMysqlClient = emmaClient;
            m_emmaMysqlClient.ResultChanged += M_emmaMysqlClient_ResultChanged;
            UpdateCurrentResultList();
        }

        private void M_emmaMysqlClient_ResultChanged(Runner runner, int position)
        {
            if (m_isPlaying)
            {
                if (position == m_resultPosition)
                {
                    NeedToRefreshGraphics();
                }
            }
        }
        
        public override string Title
        {
            get
            {
                return "Results " + m_className + " (LoTh)";
            }
        }

        public override string Description
        {
            get
            {
                return "Position: " + m_resultPosition;
            }
        }

      

        private void UpdateCurrentResultList()
        {
            if (m_resultPosition == 1000)
            {
                m_currentResultList = m_emmaMysqlClient.GetRunnersInClass(m_className)
                    .Where(x => x.Status != 9 && x.Status != 10 && x.Time > 0).OrderBy(x => x.Status).ThenBy(x => x.Time).Select(
                        x => new ResultListItem
                        {
                            Runner = x,
                            Status = x.Status,
                            Time = x.Time
                        }).ToArray();
            }
            else
            {
                m_currentResultList = m_emmaMysqlClient.GetRunnersInClass(m_className)
                    .Where(x => x.SplitTimes != null && x.SplitTimes.Any(y => y.Control == m_resultPosition))
                    .OrderBy(x => x.SplitTimes.First(y => y.Control == m_resultPosition).Time)
                    .Select(x => new ResultListItem
                    {
                        Runner = x,
                        Status = 0,
                        Time = x.SplitTimes.First(y => y.Control == m_resultPosition).Time
                    }).ToArray();

            }
        }

        

        public override void RenderControls(FlowLayoutPanel pnl)
        {
            base.RenderControls(pnl);

            Label l = new Label();
            l.Text = "Follow runner: ";
            pnl.Controls.Add(l);
            ComboBox cmbFollow = new ComboBox();
            cmbFollow.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFollow.Width = 300;
            System.Collections.Generic.List<Runner> selectableRunners = m_emmaMysqlClient.GetRunnersInClass(m_className).OrderBy(x => x.StartTime).ToList();
            selectableRunners.Insert(0, new Runner(-1, null, null, null));

            cmbFollow.DataSource = selectableRunners;
            cmbFollow.Format += CmbFollow_Format;
            cmbFollow.FormattingEnabled = true;
            cmbFollow.SelectedValueChanged += CmbFollow_SelectedValueChanged;
            pnl.Controls.Add(cmbFollow);

        }

        private void CmbFollow_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            m_followRunner= cmb.SelectedItem as Runner;
            if (m_followRunner.Name == null)
                m_followRunner = null;
            NeedToRefreshGraphics();
            
        }

        private void CmbFollow_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is Runner && ((Runner)e.ListItem).Name != null)
            {
                var runner = ((Runner)e.ListItem);
                e.Value = runner.Name + ", " + runner.Club;
            }
            else
            {
                e.Value = "<none>";
            }
        }

        public override void RenderPreview(Graphics graphics, int width, int height)
        {
            base.RenderPreview(graphics, width, height);

            //w and h in 1920x1080;
            int w = 1300;
            int h = 150;
            float fS = 30f;
            float fSs = 20f;

            int wP = (int)(w * (width / 1920.0));
            int hP = (int)(h * (height / 1080.0));
            float fSP = fS * height / 1080f;
            float fSPs = fSs * height / 1080f;


            //g.DrawString(m_name, new Font(FontFamily.GenericSansSerif, fSP), Brushes.White, width - wP + 10, height - 2 * hP);
            //g.DrawString(m_titleClub, new Font(FontFamily.GenericSansSerif, fSPs), Brushes.White, width - wP + 30, height - 2 * hP + 3 * fSP);

           /* var list = m_currentResultList.Skip((m_currentResultListPage - 1) * NumResultsPerPage).Take(NumResultsPerPage).ToList();


            graphics.FillRectangle(Brushes.Blue, 30, 10, width - 60, 30);
            var hFont = new Font(FontFamily.GenericSansSerif, fSP);
            var sFont = new Font(FontFamily.GenericSansSerif, fSPs);
            graphics.DrawString(m_className, hFont, Brushes.White, 35, 10);
            graphics.DrawString(GetDescription(), hFont, Brushes.White, (int)(width - 30 - graphics.MeasureString(GetDescription(), hFont).Width), 10);


            int lastTime = -1;
            int pos = (m_currentResultListPage - 1) * NumResultsPerPage + 1;
            if (m_currentResultList.Length > 0)
            {
                int winnerTime = m_currentResultList[0].Time;
                for (int i = 0; i < list.Count; i++)
                {
                    graphics.FillRectangle(Brushes.Blue, 30, 52 + 22 * (i), width - 60, 20);


                    string sPos = list[i].Time != lastTime ? pos.ToString() : "=";
                    graphics.DrawString(sPos, hFont, Brushes.White, 35, 52 + 22 * (i));
                    pos++;
                    lastTime = list[i].Time;
                    graphics.DrawString(list[i].Runner.Name, hFont, Brushes.White, 65, 52 + 22 * (i));

                    graphics.DrawString(list[i].Runner.Club, hFont, Brushes.White, 265, 52 + 22 * (i));

                    graphics.DrawString(Helpers.FormatTime(list[i].Time, list[i].Status, false, true, false), hFont, Brushes.White, 435, 52 + 22 * (i));

                    if (list[i].Status == 0)
                    {
                        graphics.DrawString("+" + Helpers.FormatTime(list[i].Time - winnerTime, list[i].Status, false, true, false), hFont, Brushes.White, 500, 52 + 22 * (i));
                        
                    }
                    else
                    {
                        graphics.DrawString("-" , hFont, Brushes.White, 500, 52 + 22 * (i));
                        
                    }
                }
            }*/
        }

        protected override void SetData(CasparCGDataCollection cgData)
        {
            cgData.SetData("title_class", m_className + " - " + m_resultPositionName);
            UpdateCurrentResultList();
            var list = m_currentResultList;
            int lastTime = -1;
            int pos = 1;
            
            int selItemEstimated_Place = -1;

            if (m_followRunner != null)
            {
                cgData.SetData("follow_name", m_followRunner.Name);
                cgData.SetData("follow_club", m_followRunner.Club);

                var runnerInResults = m_currentResultList.FirstOrDefault(x => x.Runner.ID == m_followRunner.ID);
                if (runnerInResults == null)
                {

                    int h = (int)(m_followRunner.StartTime / (100.0 * 60 * 60));
                    int m = (int)((m_followRunner.StartTime - h * 60 * 60 * 100) / (100.0 * 60));
                    int s = (int)((m_followRunner.StartTime - h * 60 * 60 * 100 - m * 60 * 100) / (100.0));

                    var now = DateTime.Now;
                    var startDate = new DateTime(now.Year, now.Month, now.Day, h, m, s);
                    var runnerCurrentTime = (DateTime.Now - startDate).TotalSeconds * 100;
                    var runnerCurrentPlace = m_currentResultList.Count(x => x.Status == 0 && x.Time > 0 && x.Time < runnerCurrentTime) + 1;
                    selItemEstimated_Place = runnerCurrentPlace;
                    cgData.SetData("follow_place", "(" + runnerCurrentPlace + ")");
                    cgData.SetData("follow_starttime", h + "," + m + "," + s);

                    if (m_currentResultList.Length > 0)
                    {
                        cgData.SetData("follow_tref", "" + (int)((runnerCurrentTime - m_currentResultList[0].Time) / 100));
                    }
                    else
                    {
                        cgData.SetData("follow_tref", "" + (int)((runnerCurrentTime) / 100));
                    }
                    var nextResultItem = m_currentResultList.FirstOrDefault(x => x.Time > runnerCurrentTime);
                    if (nextResultItem != null)
                    {
                        m_nextForcedUpdate = DateTime.Now.AddSeconds((nextResultItem.Time - runnerCurrentTime) / 100);
                    }
                    else
                        m_nextForcedUpdate = DateTime.MaxValue;
                }
                else
                {
                    int place = m_currentResultList.Count(x => x.Status == 0 && x.Time > 0 && x.Time < runnerInResults.Time) + 1;
                    selItemEstimated_Place = place;
                    cgData.SetData("follow_place", place.ToString());
                    if (place == 1)
                    {
                        cgData.SetData("follow_time", Helpers.FormatTime(runnerInResults.Time, 0, false, true, true));
                    }
                    else
                    {
                        cgData.SetData("follow_time",
                            "+" + Helpers.FormatTime(runnerInResults.Time - m_currentResultList[0].Time, 0, false, true, true));
                    }
                }
            }


            
            if (m_currentResultList.Length > 0)
            {
                int leaderTime = m_currentResultList[0].Time;
                int p = 0;
                for (int i = 0; i < list.Count(); i++)
                {
                    string sPos = list[i].Time != lastTime ? pos.ToString() : "=";
                    pos++;
                    lastTime = list[i].Time;
                    cgData.SetData("res_name_" + p, list[i].Runner.Name);
                    cgData.SetData("res_club_" + p, list[i].Runner.Club);
                    cgData.SetData("res_place_" + p, list[i].Status == 0 ? sPos : "-");

                    var time = i == 0 ? list[i].Time : list[i].Time - leaderTime;

                    cgData.SetData("res_time_" + p, (i > 0 ? "+" : "") + Helpers.FormatTime(time, list[i].Status, false, true, false));

                    if (i == 0 && selItemEstimated_Place > 3 && m_currentResultList.Length > 5)
                    {
                        i = Math.Min(selItemEstimated_Place - 3, list.Length - 5);

                        pos = i + 2;
                    }
                    p++;

                    if (i == 0 && m_followTail && list.Count() > 5)
                    {
                        i = list.Count() - 5;
                        pos = i + 2;
                    }



                    if (p > 4)
                        break;
                }
                if (m_followTail)
                {
                    cgData.SetData("tail_plus_time",
                        ((int)(DateTime.Now.TimeOfDay.TotalSeconds -
                                (m_currentResultList[0].Runner.StartTime + m_currentResultList[0].Time) / 100)) + "");
                }
            }
            else
            {
                if (m_followTail)
                {
                    cgData.SetData("tail_plus_time", ((int)(DateTime.Now.TimeOfDay.TotalSeconds - (10 * 3600))) + "");
                }
            }
        }

        

        protected override string TemplateName
        {
            get
            {
                return "ResultList_Passing";
            }
        }

        protected override int GraphicsLayer
        {
            get
            {
                return Properties.Settings.Default.GraphicsLayerResultList;
            }
        }
    }
}
