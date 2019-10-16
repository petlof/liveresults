using System;
using System.Drawing;
using System.Windows.Forms;
using Svt.Caspar;

namespace LiveResults.CasparClient
{
    public abstract class PlayoutJob
    {
        private readonly CasparDevice m_casparDevice = null;
        protected bool m_isPlaying = false;
        private readonly string m_templateFolder;

        public delegate void PlayingChangedDelegate(bool isPlaying);

        public event PlayingChangedDelegate PlayingChanged;

        protected PlayoutJob(CasparDevice device, string templateFolder)
        {
            m_casparDevice = device;
            m_templateFolder = templateFolder;
        }

        public bool IsPlaying
        {
            get
            {
                return m_isPlaying;
            }
        }

        public virtual void RenderControls(FlowLayoutPanel pnl)
        {
            
        }

        public virtual void RenderPreview(Graphics graphics, int width, int height)
        {
            graphics.DrawString(Title, new Font(FontFamily.GenericSansSerif, 6f), Brushes.GhostWhite, 3, height - 28);
            graphics.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), new Font(FontFamily.GenericSansSerif, 6f), Brushes.GhostWhite, 3, height - 15);
        }

        public void Play()
        {
            if (m_casparDevice.IsConnected && m_casparDevice.Channels.Count > 0)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();
                SetData(cgData);
               
                string templateName = m_templateFolder + "/" + TemplateName;
                m_casparDevice.Channels[Properties.Settings.Default.CasparChannel].CG
                    .Add(GraphicsLayer, templateName, true, cgData);
                
                SetIsPlaying(true);
            }
        }

        private void SetIsPlaying(bool isPlaying)
        {
            m_isPlaying = isPlaying;
            var dlg = PlayingChanged;
            if (dlg != null)
            {
                dlg(m_isPlaying);
            }
        }

        public void Stop()
        {
            if (m_casparDevice != null && m_casparDevice.IsConnected && m_casparDevice.Channels.Count > 0)
            {
                m_casparDevice.Channels[Properties.Settings.Default.CasparChannel].CG.Stop(GraphicsLayer);
                SetIsPlaying(false);
            }
        }

        public void ForceUpdate()
        {
            NeedToRefreshGraphics();
        }

        protected void NeedToRefreshGraphics()
        {
            if (m_casparDevice.IsConnected && m_casparDevice.Channels.Count > 0 && m_isPlaying)
            {

                CasparCGDataCollection cgData = new CasparCGDataCollection();
                SetData(cgData);

                m_casparDevice.Channels[Properties.Settings.Default.CasparChannel].CG
                    .Update(GraphicsLayer, cgData);
            }
        }

        public abstract string Title
        {
            get;
        }

        public abstract string Description
        {
            get;
        }

        protected abstract void SetData(CasparCGDataCollection cgData);
        protected abstract string TemplateName
        {
            get;
        }

        protected abstract int GraphicsLayer
        {
            get;
        }
    }
}
