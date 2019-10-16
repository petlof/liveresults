using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LiveResults.Model;
using Svt.Caspar;

namespace LiveResults.CasparClient
{
    public class PlayoutJobResultList : PlayoutJob
    {
        private readonly string m_className;
        private const int NumResultsPerPage = 10;
        private readonly int m_resultPosition;
        private readonly string m_resultPositionName;
        private int m_currentResultListPage = 1;
        private readonly EmmaMysqlClient m_emmaMysqlClient;
        private ResultListItem[] m_currentResultList;
        public PlayoutJobResultList(CasparDevice device, string templateFolder, EmmaMysqlClient emmaClient, string className, int positionCode, string positionName) : 
            base(device,templateFolder)
        {
            m_className = className;
            m_resultPosition = positionCode;
            m_resultPositionName = positionName;
            m_emmaMysqlClient = emmaClient;
            UpdateCurrentResultList();
        }

        public override string Title
        {
            get
            {
                return "Results " + m_className;
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

        private void NextPage()
        {
            m_currentResultListPage++;
            UpdateResultPageXofX();
            NeedToRefreshGraphics();
        }

        private void PreviousPage()
        {
            m_currentResultListPage--;
            if (m_currentResultListPage < 1)
            {
                m_currentResultListPage = 1;
            }

            
            UpdateResultPageXofX();
            NeedToRefreshGraphics();
        }

        void UpdateResultPageXofX()
        {
            int numPages = 0;
            if (m_currentResultList != null)
            {
                numPages = (int) Math.Ceiling(m_currentResultList.Length / 10.0);
            }
            if (lblResultNumPages != null)
            {
                lblResultNumPages.Text = "Page " + m_currentResultListPage + " / " + numPages;
            }
        }

        private Label lblResultNumPages = null;

        public override void RenderControls(FlowLayoutPanel pnl)
        {
            base.RenderControls(pnl);

            var btnPrevious = new Button();
            btnPrevious.Text = " < ";
            btnPrevious.BackColor = Color.FromKnownColor(KnownColor.Control);
            btnPrevious.TextAlign = ContentAlignment.MiddleCenter;
            btnPrevious.Click += (sender, args) => PreviousPage();
            pnl.Controls.Add(btnPrevious);

            lblResultNumPages = new Label();
            lblResultNumPages.TextAlign = ContentAlignment.MiddleCenter;
            pnl.Controls.Add(lblResultNumPages);

            var btnNext = new Button();
            btnNext.Text = " > ";
            btnNext.BackColor = Color.FromKnownColor(KnownColor.Control);
            btnNext.TextAlign = ContentAlignment.MiddleCenter;
            btnNext.Click += (sender, args) => NextPage();
            pnl.Controls.Add(btnNext);


            UpdateResultPageXofX();
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

            var list = m_currentResultList.Skip((m_currentResultListPage - 1) * NumResultsPerPage).Take(NumResultsPerPage).ToList();


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
            }
        }

        private string GetDescription()
        {
            return "Ställning vid " + (m_resultPositionName == "Finish" ? "Mål" : m_resultPositionName);
        }

        protected override void SetData(CasparCGDataCollection cgData)
        {
            cgData.SetData("title_class", m_className);

            //UpdateCurrentResultList();

            var list = m_currentResultList.Skip((m_currentResultListPage - 1) * NumResultsPerPage).Take(NumResultsPerPage).ToList();
            cgData.SetData("title_class_description",
               GetDescription());
            int lastTime = -1;
            int pos = (m_currentResultListPage - 1) * NumResultsPerPage+ 1;
            if (m_currentResultList.Length > 0)
            {
                int winnerTime = m_currentResultList[0].Time;
                for (int i = 0; i < list.Count; i++)
                {
                    string sPos = list[i].Time != lastTime ? pos.ToString() : "=";
                    pos++;
                    lastTime = list[i].Time;
                    cgData.SetData("res_name_" + i, list[i].Runner.Name);
                    cgData.SetData("res_club_" + i, list[i].Runner.Club);
                    cgData.SetData("res_place_" + i, list[i].Status == 0 ? sPos : "-");
                    cgData.SetData("res_time_" + i, Helpers.FormatTime(list[i].Time, list[i].Status, false, true, false));
                    if (list[i].Status == 0)
                    {
                        cgData.SetData("res_timeplus_" + i,
                            "+" + Helpers.FormatTime(list[i].Time - winnerTime, list[i].Status, false, true, false));
                    }
                    else
                    {
                        cgData.SetData("res_timeplus_" + i, "-");
                    }
                }
            }
        }

        

        protected override string TemplateName
        {
            get
            {
                return "ResultList";
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
