using LiveResults.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace LiveResults.Client.Parsers
{
    public class TulospalveluParser : IExternalSystemResultParser
    {
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;

        private bool m_continue;
        readonly string[] m_urls;
        public TulospalveluParser(string[] urls)
        {
            m_urls = urls;
        }

        //private void FireOnResult(int id, int SI, string name, string club, string Class, int start, int time, int status, List<ResultStruct> results)
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

        System.Threading.Thread m_th;

        public void Start()
        {
            m_continue = true;
            m_th = new System.Threading.Thread(Run);
            m_th.Start();
        }

        public void Stop()
        {
            m_continue = false;
        }

       
        private void Run()
        {
            var radios = new Dictionary<string, List<RadioControl>>();
            while (m_continue)
            {
                try
                {
                    var wc = new WebClient
                    {
                        Encoding = Encoding.GetEncoding("iso-8859-1")
                    };

                    var teamTotalTimes = new Dictionary<string, int>();
                    var teamTotalStatus = new Dictionary<string, int>();

                    var radioControlsRex = new Regex("<a href='/tulokset/en/.*?/tilanne/\\d/(?<control>[1-9])/'>(?<name>.*?)</a>");
                    var rex = new Regex("<h\\d>(?<classname>.*?)</h\\d>.*?<table class=\'tuloslista\' cellspacing=0>(?<data>.*?)</table>",
                        RegexOptions.Singleline);
                    var rexBibClub = new Regex("(?<club>.*) \\((?<bibNo>\\d+)\\)", RegexOptions.Singleline);
                    foreach (var url in m_urls)
                    {
                        string html = wc.DownloadString(url);
                        string className;
                        var urlParts = url.Split(new string[]
                        {
                            "/"
                        }, StringSplitOptions.RemoveEmptyEntries);


                        bool isSmartView = urlParts[urlParts.Length - 2] == "smart";

                        if (!isSmartView)
                        {
                            string relayLeg = null;
                            if (!isSmartView)
                                relayLeg = urlParts[urlParts.Length - 2];

                            ParseResultList(html, teamTotalTimes, teamTotalStatus, rex, rexBibClub, relayLeg, out className, 1000);

                            var radioMatches = radioControlsRex.Matches(html);
                            foreach (Match rm in radioMatches)
                            {
                                string name = rm.Groups["name"].Value;
                                string control = rm.Groups["control"].Value;
                                int code = 1000 + Convert.ToInt32(control);
                                OnRadioControl(name, code, className, Convert.ToInt32(control));

                                string rUrl = "http://" + string.Join("/", urlParts.Skip(1).Take(urlParts.Length - 2)) + "/" + control +
                                              "/";
                                html = wc.DownloadString(rUrl);
                                ParseResultList(html, teamTotalTimes, teamTotalStatus, rex, rexBibClub, relayLeg, out className, code);
                            }
                        }
                        else
                        {
                            ParseSmartViewResultList(html, out className);
                        }
                    }

                    System.Threading.Thread.Sleep(5000);

                    /*
                        time is seconds * 100
                     * 
                     * valid status is
                        notStarted
                        finishedTimeOk
                        finishedPunchOk
                        disqualified
                        finished
                        movedUp
                        walkOver
                        started
                        passed
                        notValid
                        notActivated
                        notParticipating
                     */
                    //EMMAClient.RunnerStatus rstatus = EMMAClient.RunnerStatus.Passed;
                    /*switch (status)
                    {
                        case "started":
                            rstatus = 9;
                            break;
                        case "notActivated":
                            rstatus = 10;
                            //rstatus = EMMAClient.RunnerStatus.NotStartedYet;
                            break;
                        case "notStarted":
                            rstatus = 1;
                            //rstatus = EMMAClient.RunnerStatus.NotStarted;
                            break;
                        case "disqualified":
                            rstatus = 4;
                            break;
                        case "notValid":
                            rstatus = 3;
                            //rstatus = EMMAClient.RunnerStatus.MissingPunch;
                            break;
                        case "notParticipating":
                            rstatus = 999;
                            break;
                        case "walkOver":
                            rstatus = 11;
                            //rstatus = EMMAClient.RunnerStatus.WalkOver;
                            break;
                        case "movedUp":
                            rstatus = 12;
                            break;
                    }
                    if (rstatus != 999)
                        FireOnResult(runnerID, 0, fName + " " + famName, club, classN, iStartTime, time, rstatus, new List<ResultStruct>());
                     */


                }
                catch (Exception ee)
                {
                    Debug.WriteLine("Exception fetching data from remote host");
                }
            }
        }

        private void ParseSmartViewResultList(string html, out string className)
        {
            var rex = new Regex("<h2>(?<classname>.*?)</h2>.*?<table class='rva_hk' cellpadding=2 cellspacing=0>(?<data>.*?)</table>",
                RegexOptions.Singleline);
            var m = rex.Match(html);
            var data = m.Groups["data"].Value.Trim();
            className = m.Groups["classname"].Value.Trim();

            var xml = new XmlDocument();
            var rexReplace = new Regex("<img border=1 src='.*?' height='11' alt='.*?' title='.*?'>");
            data = rexReplace.Replace(data, "");
            data = data.Replace("&nbsp;", " ");
            xml.LoadXml("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><table>" + data + "</table>");
            var trs = xml.GetElementsByTagName("tr");

            var rexRunnerId = new Regex("<a href=.*kilpailijat/(?<id>\\d+)/\\d/\">(?<name>.*?)</a>");

            for (int i = 5; i < trs[0].ChildNodes.Count - 2; i++)
            {
                string name = trs[0].ChildNodes[i].InnerXml.Split(new string[] {"<br />"},StringSplitOptions.None)[1];
                int code = 1001 + i-5;
                int order = i - 5;
                OnRadioControl(name, code, className, order);
            }

            for (int i = 1; i < trs.Count; i++)
            {
                string place = trs[i].ChildNodes[0].InnerText.Trim();
                string name = trs[i].ChildNodes[1].InnerText.Trim();
                string club = trs[i].ChildNodes[3].InnerText.Trim();

                var ridM = rexRunnerId.Match(trs[i].ChildNodes[1].InnerXml);
                int id = Convert.ToInt32(ridM.Groups["id"].Value);
                

                string raceTime = trs[i].ChildNodes[trs[i].ChildNodes.Count-1].InnerText.Trim();
                int time = -4;
                int status = 10;

                string startTime = trs[i].ChildNodes[4].InnerText.Trim();
                DateTime dStartTime = ParseDateTime(startTime);
                int iStartTime = dStartTime.Hour * 360000 + dStartTime.Minute * 6000 + dStartTime.Second * 100;

                List<ResultStruct> splits = new List<ResultStruct>();
                for (int split = 5; split < trs[i].ChildNodes.Count - 1; split++)
                {
                    var splitCode = 1001 + split - 5;
                    var splittime = trs[i].ChildNodes[split].InnerText.Trim();
                    if (!string.IsNullOrEmpty(splittime))
                    {
                        splittime = splittime.Substring(0, splittime.IndexOf("(")).Trim();
                        DateTime dSplitTime = ParseDateTime(splittime);
                        splits.Add(new ResultStruct()
                        {
                            ControlCode = splitCode,
                            Time = dSplitTime.Hour * 360000 + dSplitTime.Minute * 6000 + dSplitTime.Second * 100
                        });
                    }
                }

                if (raceTime == "DQ" || raceTime == "DNF")
                {
                    status = 4;
                }
                else if (raceTime == "DNS")
                {
                    status = 1;
                }
                else if (raceTime.Length > 0)
                {
                   /* if (raceTime.StartsWith("+", StringComparison.Ordinal))
                    {
                        raceTime = raceTime.Substring(1);
                        var tid = ParseTimePlus(raceTime);
                        time = (int) (tid.TotalSeconds * 100);
                    }
                    else
                    {*/
                        var tid = ParseDateTime(raceTime);
                        time = tid.Hour * 360000 + tid.Minute * 6000 + tid.Second * 100;
                    //}
                    

                    //if (place.Length > 0)
                    status = 0;
                }

                FireOnResult(new Result
                {
                    ID = id,
                    Class = className,
                    RunnerClub = club,
                    RunnerName = name,
                    Status = status,
                    Time = time,
                    StartTime =  iStartTime,
                    SplitTimes = splits

                });
            }
        }

        private void ParseResultList(string html, Dictionary<string, int> teamTotalTimes, Dictionary<string, int> teamTotalStatus, Regex rex, Regex rexBibClub, string relayLeg, out string className, int split)
        {
            var m = rex.Match(html);
            var data = m.Groups["data"].Value.Trim();
            className = m.Groups["classname"].Value.Trim() + (!string.IsNullOrEmpty(relayLeg) ? "-" + relayLeg : "");
            var xml = new XmlDocument();
            var rexReplace = new Regex("<img border=1 src='.*?' height='11' alt='.*?' title='.*?'>");
            data = rexReplace.Replace(data, "");
            xml.LoadXml("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><table>" + data + "</table>");
            var trs = xml.GetElementsByTagName("tr");

            int bestTime = 0;
            for (int i = 0; i < trs.Count; i++)
            {
                string place = trs[i].ChildNodes[0].InnerText;
                string name = trs[i].ChildNodes[4].InnerText;
                string club = trs[i].ChildNodes[2].InnerText;

                
                string bibNo = trs[i].ChildNodes[1].InnerText;
                var ms = rexBibClub.Match(club);
                club = ms.Groups["club"].Value;
                bibNo = ms.Groups["bibNo"].Value;


                string raceTime = trs[i].ChildNodes[3].InnerText.Trim();
                int time = -4;
                int status = 10;

                int id = (!string.IsNullOrEmpty(relayLeg) ?  Convert.ToInt32(relayLeg) * 1000000 : 0) + Convert.ToInt32(bibNo);

                if (raceTime == "DQ" || raceTime == "DNF" || raceTime == "hylätty" || raceTime== "keskeytti")
                {
                    status = 4;
                }
                else if (raceTime == "DNS")
                {
                    status = 1;
                }
                else if (raceTime.Length > 0)
                {
                    if (raceTime.StartsWith("+", StringComparison.Ordinal))
                    {
                        raceTime = raceTime.Substring(1);
                        var tid = ParseTimePlus(raceTime);
                        time = (int)(tid.TotalSeconds * 100);
                    }
                    else
                    {
                        var tid = ParseDateTime(raceTime);
                        time = tid.Hour * 360000 + tid.Minute * 6000 + tid.Second * 100;
                    }
                    if (i == 0)
                    {
                        bestTime = time;
                    }
                    else
                    {
                        time = bestTime + time;
                    }

                    //if (place.Length > 0)
                        status = 0;
                }

                if (!string.IsNullOrEmpty(relayLeg))
                {
                    if (relayLeg == "1")
                    {

                    }
                    else
                    {
                        if (status == 0 && teamTotalStatus[bibNo + "-" + (Convert.ToInt32(relayLeg) - 1)] != 0)
                        {
                            status = teamTotalStatus[bibNo + "-" + (Convert.ToInt32(relayLeg) - 1)];
                        }
                    }
                }

                string key = bibNo + (!string.IsNullOrEmpty(relayLeg) ? "-" + relayLeg : "");
                if (!teamTotalTimes.ContainsKey(key))
                    teamTotalTimes.Add(key, time);
                else
                    teamTotalTimes[key] = time;

                if (!teamTotalStatus.ContainsKey(key))
                    teamTotalStatus.Add(key, status);
                else
                    teamTotalStatus[key] = status;

                if (split == 1000)
                {
                    FireOnResult(new Result
                    {
                        ID = id,
                        Class = className,
                        RunnerClub = club,
                        RunnerName = name,
                        Status = status,
                        Time = time

                    });
                }
                else
                {
                    FireOnResult(new Result
                    {
                        ID = id,
                        Class = className,
                        RunnerClub = club,
                        RunnerName = name,
                        SplitTimes = new ResultStruct[]
                        {
                            new ResultStruct
                            {
                                ControlCode = split,
                                ControlNo = split - 1000,
                                Place = -1,
                                Time = time
                            }
                        }.ToList(),
                        Status = 10,
                        Time = -2

                    });

                }
            }
        }

        private void UpdateRadiocontrols(Dictionary<string, List<RadioControl>> radios, string className, Dictionary<int, List<ResultStruct>> results, int id)
        {
            int ri = 0;
            radios[className] = new List<RadioControl>(results[id].OrderBy(x => x.Time).Select(x => new RadioControl
            {
                ClassName = className,
                Code = x.ControlCode,
                ControlName = x.ControlCode.ToString(CultureInfo.InvariantCulture),
                Order = ri++
            }));
            foreach (var r in radios[className])
            {
                OnRadioControl(r.ControlName, r.Code, r.ClassName, r.Order);
            }
        }

        private static TimeSpan ParseTimePlus(string time)
        {
            TimeSpan ts = new TimeSpan(0);
            string[] parts = time.Split(':');
            if (parts.Length == 1)
                ts = new TimeSpan(0, 0, 0, Convert.ToInt32(parts[0]));
            else if (parts.Length == 2)
                ts = new TimeSpan(0, 0, Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]));
            else if (parts.Length == 3)
                ts = new TimeSpan(0, Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]));
            
            return ts;
        }
        private static DateTime ParseDateTime(string tTime)
        {
            DateTime startTime;
            if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
            {
                if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                {
                    if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                    {
                        if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                        {
                            if (!DateTime.TryParseExact(tTime, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                            {
                                if (!DateTime.TryParseExact(tTime, "H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                    out startTime))
                                {
                                    if (!DateTime.TryParseExact(tTime, "mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                        out startTime))
                                    {
                                        if (!DateTime.TryParseExact(tTime, "m:ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                            out startTime))
                                        {
                                            if (!DateTime.TryParseExact(tTime, "ss", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                out startTime))
                                            {
                                                if (!DateTime.TryParseExact(tTime, "s", CultureInfo.InvariantCulture, DateTimeStyles.None,
                                                    out startTime))
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return startTime;
        }


        public event RadioControlDelegate OnRadioControl;
    }
}
