using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiveResults.CasparClient
{
    public partial class PlayoutItem : UserControl
    {
        public PlayoutItem()
        {
            InitializeComponent();

        }

        public void RenderPreview(Graphics g, int width, int height)
        {
            m_job.RenderPreview(g, width, height);
        }

        private bool m_Selected = false;
        
        public bool Selected
        {
            get
            {
                return m_Selected;
            }
            set
            {
                m_Selected = value;
            }
        }

        public void SetupControlPanel(FlowLayoutPanel pnl)
        {
            m_job.RenderControls(pnl);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(BackColor))
                e.Graphics.FillRectangle(brush, ClientRectangle);
            e.Graphics.DrawRectangle(Selected ? Pens.Red : Pens.Transparent, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        private PlayoutJob m_job = null;

        public bool IsPlaying
        {
            get
            {
                return m_job.IsPlaying;
            }
        }

        public PlayoutItem(PlayoutJob job) : this()
        {
            m_job = job;
            lblTitle.Text = job.Title;
            lblDescription.Text = job.Description;
            btnStop.Enabled = false;
            this.BackColor = Color.LightGray;
            m_job.PlayingChanged += playing =>
            {
                this.Invoke((MethodInvoker) delegate
                {
                    this.BackColor = playing ? Color.LightGreen : Color.LightGray;
                    this.btnStop.Enabled = playing;
                    this.btnPlay.Enabled = !playing;
                });
            };
        }

        public void ForceUpdate()
        {
            m_job.ForceUpdate();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (m_job != null)
            {
                m_job.Play();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (m_job != null)
            {
                m_job.Stop();
            }
        }
    }
}
