using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Svt.Caspar;

namespace LiveResults.CasparClient
{
    public class PlayoutJobClock : PlayoutJob
    {
        private readonly string m_refTime = null;
        private readonly bool m_showTenth = false;
        public PlayoutJobClock(CasparDevice device, string templateFolder, string refTime, bool showTenth) : 
            base(device,templateFolder)
        {
            m_refTime = refTime;
            m_showTenth = showTenth;
        }

        public override string Title
        {
            get
            {
                return "Clock";
            }
        }

        public override string Description
        {
            get
            {
                return "RefTime: " + m_refTime + ", showTenth: " + m_showTenth;
            }
        }

        protected override void SetData(CasparCGDataCollection cgData)
        {
            if (!string.IsNullOrEmpty(m_refTime))
            {
                cgData.SetData("ref_time", m_refTime);
            }
            cgData.SetData("show_tenth", m_showTenth ? "true" : "false");
        }

        protected override string TemplateName
        {
            get
            {
                return "clock";
            }
        }

        protected override int GraphicsLayer
        {
            get
            {
                return Properties.Settings.Default.GraphicsLayersClock;
            }
        }
    }
}
