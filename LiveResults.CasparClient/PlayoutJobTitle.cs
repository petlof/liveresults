using System.Drawing;
using System.Windows.Forms;
using Svt.Caspar;

namespace LiveResults.CasparClient
{
    public class PlayoutJobTitle : PlayoutJob
    {
        private readonly string m_name;
        private readonly string m_titleClub;
        public PlayoutJobTitle(CasparDevice device, string templateFolder, string name, string titleClub) : 
            base(device,templateFolder)
        {
            m_name = name;
            m_titleClub = titleClub;

        }

        public override string Title
        {
            get
            {
                return "Title: " + m_name;
            }
        }

        public override string Description
        {
            get
            {
                return m_titleClub;
            }
        }

        public override void RenderPreview(Graphics g, int width, int height)
        {
            base.RenderPreview(g, width, height);

            //w and h in 1920x1080;
            int w = 1300;
            int h = 150;
            float fS = 30f;
            float fSs = 20f;

            int wP = (int)(w * (width / 1920.0));
            int hP = (int)(h * (height / 1080.0));
            float fSP = fS * height / 1080f;
            float fSPs = fSs * height / 1080f;

            g.FillRectangle(Brushes.Blue, width - wP, height - 2 * hP, wP, hP);
            g.DrawString(m_name, new Font(FontFamily.GenericSansSerif, fSP), Brushes.White, width - wP + 10, height - 2 * hP);
            g.DrawString(m_titleClub, new Font(FontFamily.GenericSansSerif, fSPs), Brushes.White, width - wP + 30, height - 2 * hP + 3 * fSP);

        }

        protected override void SetData(CasparCGDataCollection cgData)
        {
            cgData.SetData("label_name", m_name);
            cgData.SetData("label_title", m_titleClub);
        }

        protected override string TemplateName
        {
            get
            {
                return "Title";
            }
        }

        protected override int GraphicsLayer
        {
            get
            {
                return Properties.Settings.Default.GraphicsLayerNaming;
            }
        }
    }
}
