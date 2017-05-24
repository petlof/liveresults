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

                    /*var runnerIds = new Dictionary<string, int>();
                    var bf = new BinaryFormatter();
                    if (File.Exists("racetimerrunnerids.data"))
                    {
                        var st = File.OpenRead("runnerids.data");
                        runnerIds = bf.Deserialize(st) as Dictionary<string, int>;
                        st.Close();
                    }


                    int nextRid = runnerIds.Count > 0 ? runnerIds.Max(x => x.Value) + 1 : 0;*/
                    var wc = new WebClient();
                    wc.Encoding = Encoding.GetEncoding("iso-8859-1");

                    Dictionary<string, int> teamTotalTimes = new Dictionary<string, int>();
                    Dictionary<string, int> teamTotalStatus = new Dictionary<string, int>();

                    var radioControlsRex = new Regex("<a href='/tulokset/en/.*?/tilanne/\\d/(?<control>[1-9])/'>(?<name>.*?)</a>");
                    var rex = new Regex("<h2>(?<classname>.*?)</h2>.*?<table class=\'tuloslista\' cellspacing=0>(?<data>.*?)</table>",
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
                        string relayLeg = urlParts[urlParts.Length - 2];
                        ParseResultList(html, teamTotalTimes, teamTotalStatus, rex, rexBibClub, relayLeg, out className,1000);

                        var radioMatches = radioControlsRex.Matches(html);
                        foreach (Match rm in radioMatches)
                        {
                            string name = rm.Groups["name"].Value;
                            string control = rm.Groups["control"].Value;
                            int code = 1000 + Convert.ToInt32(control);
                            OnRadioControl(name,code, className, Convert.ToInt32(control));

                            string rUrl = "http://" + string.Join("/", urlParts.Skip(1).Take(urlParts.Length - 2)) + "/" + control + "/";
                            html = wc.DownloadString(rUrl);
                            ParseResultList(html, teamTotalTimes, teamTotalStatus, rex, rexBibClub, relayLeg, out className,code);
                        }

                        /*foreach (var res in results)
                        {
                            var r = idToRunners[res.Key];

                            if (!radios.ContainsKey(r.Class) || radios[r.Class].Count < results[r.ID].Count)
                            {
                                UpdateRadiocontrols(radios, r.Class, results, r.ID);
                            }

                            FireOnResult(new Result
                            {
                                ID = res.Key,
                                Class = r.Class,
                                RunnerClub = r.Club,
                                RunnerName = r.Name,
                                SplitTimes = results[r.ID].OrderBy(x => x.Time).ToList(),
                                Status = 10,
                                Time = -10

                            });
                        }*/




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

        private void ParseResultList(string html, Dictionary<string, int> teamTotalTimes, Dictionary<string, int> teamTotalStatus, Regex rex, Regex rexBibClub, string relayLeg, out string className, int split)
        {
            var m = rex.Match(html);
            var data = m.Groups["data"].Value.Trim();
            className = m.Groups["classname"].Value.Trim() + "-" + relayLeg;
            var xml = new XmlDocument();
            var rexReplace = new Regex("<img border=1 src='.*?' height='11' alt='.*?' title='.*?'>");
            data = rexReplace.Replace(data, "");
            xml.LoadXml("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?><table>" + data + "</table>");
            var trs = xml.GetElementsByTagName("tr");

            int bestTime = 0;
            for (int i = 0; i < trs.Count; i++)
            {
                string place = trs[i].ChildNodes[0].InnerText;
                string name = trs[i].ChildNodes[5].InnerText;
                string club = trs[i].ChildNodes[3].InnerText;

                //var ms = rexBibClub.Match(club);
                string bibNo = trs[i].ChildNodes[1].InnerText;
                //club = ms.Groups["club"].Value;


                string raceTime = trs[i].ChildNodes[4].InnerText.Trim();
                int time = -4;
                int status = 10;

                int id = Convert.ToInt32(relayLeg) * 1000000 + Convert.ToInt32(bibNo);

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

                    if (place.Length > 0)
                        status = 0;
                }


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

                string key = bibNo + "-" + relayLeg;
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
