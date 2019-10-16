using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LiveResults.Model;
using Svt.Caspar;

namespace LiveResults.CasparClient
{
    public class PlayoutJobResultListPrewarned : PlayoutJob
    {
        private DateTime m_nextForcedUpdate = DateTime.MaxValue;
        private readonly EmmaMysqlClient m_emmaMysqlClient;
        private int[] m_prewarningControls;
        public PlayoutJobResultListPrewarned(CasparDevice device, string templateFolder, EmmaMysqlClient emmaClient, 
            int[] prewarningControls) : 
            base(device,templateFolder)
        {

            m_prewarningControls = prewarningControls;
            m_emmaMysqlClient = emmaClient;
            m_emmaMysqlClient.ResultChanged += M_emmaMysqlClient_ResultChanged;
        }

        private void M_emmaMysqlClient_ResultChanged(Runner runner, int position)
        {
            if (m_isPlaying)
            {
                if (m_prewarningControls.Contains(position) || position == 1000)
                {
                    NeedToRefreshGraphics();
                }
            }
        }
        
        public override string Title
        {
            get
            {
                return "Prewarned Runners";
            }
        }

        public override string Description
        {
            get
            {
                return "Controls: " + string.Join(",", m_prewarningControls.Select(x => x.ToString()).ToArray());
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
            m_nextForcedUpdate = DateTime.MaxValue;
            cgData.SetData("title", "Förvarnade");
            int follIdx = 0;

            //Finished within 10sek
            var curRealTime = DateTime.Now.Hour * 360000 + DateTime.Now.Minute * 6000 + DateTime.Now.Second * 100;
            bool hadRunnerFinishedWithin10sek = false;
            foreach (var prewarned in m_emmaMysqlClient.GetAllRunners()
                .Where(x => x.Time > 0 && (x.StartTime + x.Time) > curRealTime - 10 * 100 && x.Status == 0)
                .OrderBy(x => x.StartTime + x.Time))
            {
                int startTime = prewarned.StartTime;
                if ((curRealTime - 10 * 100) > startTime + prewarned.Time)
                    continue;

                var betterResInclass = m_emmaMysqlClient.GetRunnersInClass(prewarned.Class)
                    .Where(x => x.Time > 0 && x.Time < prewarned.Time && x.Status == 0).OrderBy(x => x.Time).ToArray();
                var leader = betterResInclass.Any() ? betterResInclass[0] : null;

                cgData.SetData("res_name_" + follIdx, prewarned.Name);
                cgData.SetData("res_club_" + follIdx, prewarned.Club);
                cgData.SetData("res_class_" + follIdx, prewarned.Class);
                var pTime = prewarned.Time;
                cgData.SetData("res_place_" + follIdx, betterResInclass.Count() + 1 + "");
                cgData.SetData("res_time_" + follIdx,
                    Helpers.FormatTime(prewarned.Time, prewarned.Status, false, true, true));
                cgData.SetData("res_timeplus_" + follIdx,
                    "+" + Helpers.FormatTime(prewarned.Time - (leader != null ? leader.Time : prewarned.Time), prewarned.Status, false, true,
                        true));
                follIdx++;
                hadRunnerFinishedWithin10sek = true;
                if (follIdx > 15)
                    break;

            }

            if (hadRunnerFinishedWithin10sek)
            {
                m_nextForcedUpdate = DateTime.Now.AddSeconds(11);
            }

            double minTimeUntilUpdatePlaceRequired = double.MaxValue;

            foreach (var prewarned in m_emmaMysqlClient.GetAllRunners()
                .Where(x => x.Time <= 0 && (x.Status == 0 || x.Status == 9 || x.Status == 10) && x.SplitTimes != null &&
                            x.SplitTimes.Any(y => Array.IndexOf(m_prewarningControls, y.Control) >= 0))
                .OrderBy(x => x.StartTime + x.SplitTimes.Max(m => m.Time)))
            {
                var classResults = m_emmaMysqlClient.GetRunnersInClass(prewarned.Class).Where(x => x.Time > 0 && x.Status == 0)
                    .OrderBy(x => x.Time).ToArray();
                Runner leader = classResults.Any() ? classResults[0] : null;

                int startTime = prewarned.StartTime;

                var curTime = DateTime.Now.Hour * 360000 + DateTime.Now.Minute * 6000 + DateTime.Now.Second * 100 - startTime;
                cgData.SetData("res_name_" + follIdx, prewarned.Name);
                cgData.SetData("res_class_" + follIdx, prewarned.Class);
                cgData.SetData("res_club_" + follIdx, prewarned.Club);
                cgData.SetData("res_tref_" + follIdx, (curTime / 100).ToString());

                if (classResults.Any())
                {
                    var nextRunner = classResults.Where(x => x.Time > curTime).OrderBy(x => x.Time).FirstOrDefault();
                    if (nextRunner != null)
                    {
                        var timeUntilPlaceChanges = (nextRunner.Time - curTime) / 100;
                        if (timeUntilPlaceChanges < minTimeUntilUpdatePlaceRequired)
                        {
                            minTimeUntilUpdatePlaceRequired = timeUntilPlaceChanges;
                        }
                    }
                }

                if (leader != null)
                {
                    string place =
                    (prewarned.Status != 9 && prewarned.Status != 10 && prewarned.Status != 0 &&
                     Helpers.ContainsRunnerStatus(prewarned.Status))
                        ? Helpers.RunnerStatus(prewarned.Status)
                        : (classResults.Count(x => x.Time <= curTime) + 1).ToString();
                    cgData.SetData("res_tplusref_" + follIdx, ((curTime - leader.Time) / 100).ToString());
                    cgData.SetData("res_place_" + follIdx, "(" + place + ")");
                }
                else
                {
                    string place =
                    (prewarned.Status != 9 && prewarned.Status != 10 && prewarned.Status != 0 &&
                     Helpers.ContainsRunnerStatus(prewarned.Status))
                        ? Helpers.RunnerStatus(prewarned.Status)
                        : "1";
                    cgData.SetData("res_tplusref_" + follIdx, "-999999");
                    cgData.SetData("res_place_" + follIdx, "(" + place + ")");
                }
                follIdx++;
                if (follIdx > 16)
                    break;
            }

            if (minTimeUntilUpdatePlaceRequired < double.MaxValue)
            {
                if (DateTime.Now.AddSeconds(minTimeUntilUpdatePlaceRequired) < m_nextForcedUpdate)
                {
                    m_nextForcedUpdate = DateTime.Now.AddSeconds(minTimeUntilUpdatePlaceRequired);
                }
            }
        }


        protected override string TemplateName
        {
            get
            {
                return "prewarned_runners";
            }
        }

        protected override int GraphicsLayer
        {
            get
            {
                return Properties.Settings.Default.GraphicsLayerPrewarnedRunners;
            }
        }
    }
}
