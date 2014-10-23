using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Xml;

namespace LiveResults.Client.Parsers
{
    public class Jwoc2014Parser : IExternalSystemResultParser
    {
        private bool m_Continue = false;
        private Thread th;
        private string m_url = "http://bgodays.org/xml/event-6.xml";
        public void Start()
        {
            m_Continue = true;
            th = new System.Threading.Thread(new System.Threading.ThreadStart(run));
            th.Start();
        }

        public void Stop()
        {
            m_Continue = false;
        }

        private void run()
        {
            while (m_Continue)
            {
                try
                {
                    var wc = new WebClient();
                    var xml = wc.DownloadString(m_url);

                    var xdoc = new XmlDocument();
                    xdoc.LoadXml(xml);
                    foreach (XmlElement group in xdoc.GetElementsByTagName("group"))
                    {
                        string className = group.Attributes["name"].Value.Trim();
                        foreach (XmlElement competitor in group.GetElementsByTagName("competitor"))
                        {
                            string country = competitor.Attributes["country"].Value;
                            string name = competitor.Attributes["name"].Value;

                            var startNode = competitor.GetElementsByTagName("start")[0];
                            var startTime = startNode.InnerText;

                            if (competitor.GetElementsByTagName("finish").Count > 0)
                            {
                                var fNode = competitor.GetElementsByTagName("finish")[0];
                                var fTime = fNode.InnerText;

                            }


                        }


                    }

                }
                catch(Exception ee)
                {
                    FireLogMsg(ee.Message);
                }
            }
        }

        private void FireOnResult(Result newResult)
        {
            if (OnResult != null)
            {
                OnResult(newResult);
            }
        }
        private void FireLogMsg(string msg)
        {
            if (OnLogMessage != null)
                OnLogMessage(msg);
        }


        public event
              ResultDelegate OnResult;

        public event LogMessageDelegate OnLogMessage;

        public event RadioControlDelegate OnRadioControl;
    }
}
