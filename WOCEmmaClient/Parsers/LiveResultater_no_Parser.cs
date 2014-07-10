using System;
using System.Collections.Generic;
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
    public class LiveResultaterNoParser : IExternalSystemResultParser
    {
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;

        private bool m_continue;
        readonly string[] m_urls;
        public LiveResultaterNoParser(string[] urls)
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

                    var runnerIds = new Dictionary<string, int>();
                    var bf = new BinaryFormatter();
                    if (File.Exists("runnerids.data"))
                    {
                        var st = File.OpenRead("runnerids.data");
                        runnerIds = bf.Deserialize(st) as Dictionary<string, int>;
                        st.Close();
                    }


                    int nextRid = runnerIds.Max(x => x.Value) + 1;
                    var wc = new WebClient();
                    wc.Encoding = Encoding.UTF8;
                    foreach (var url in m_urls)
                    {
                        var html = wc.DownloadString(url);
                        var rex = new Regex("<td valign=\"top\">.*?<h3>(?<passing>.*?)</h3>.*?<table cellpadding='1' cellspacing='0' class='list2'>(?<data>.*?)</table></td>", RegexOptions.Singleline);
                        var matches = rex.Matches(html);
                        var results = new Dictionary<int, List<ResultStruct>>();

                        var idToRunners = new Dictionary<int, Runner>();

                        foreach (Match m in matches)
                        {
                            
                            var data = m.Groups["data"].Value;
                            var passing = m.Groups["passing"].Value;
                            
                            data = data.Replace("class=siste", "class=\"siste\"");
                            data = data.Replace("class=odd", "class=\"odd\"");
                            data = data.Replace("class=even", "class=\"even\"");
                            var xml = new XmlDocument();
                            xml.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><table>" + data + "</table>");
                            var trs = xml.GetElementsByTagName("tr");

                            for (int i = 1; i < trs.Count; i++)
                            {
                                string place = trs[i].ChildNodes[0].InnerText;
                                //string starttime = trs[i].ChildNodes[1].InnerText;

                                string name = trs[i].ChildNodes[2].InnerText;
                                string className = trs[i].ChildNodes[3].InnerText;
                                string club = trs[i].ChildNodes[4].InnerText;

                                string raceTime = trs[i].ChildNodes[5].InnerText;

                                //string raceTime = trs[i].ChildNodes[trs[i].ChildNodes.Count - 1].InnerText;
                                //DateTime startTime = DateTime.ParseExact(starttime, new string[] { "H:mm:ss", "HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                                int time = -4;
                                int status = 10;

                                if (!runnerIds.ContainsKey(club + "-" + name))
                                    runnerIds.Add(club + "-" + name, nextRid++);

                                int id = runnerIds[club + "-" + name];

                                if (!results.ContainsKey(id))
                                    results.Add(id, new List<ResultStruct>());

                                if (place=="DNF" || place=="DSQ")
                                {
                                    status = 4;
                                }
                                else if (place=="DNS")
                                {
                                    status = 1;
                                }
                                else if (raceTime.Length > 0)
                                {
                                    var tid = ParseDateTime(raceTime);
                                    time = tid.Hour*360000 + tid.Minute*6000 + tid.Second*100;
                                    if (passing == "Finish")
                                        status = 0;
                                }
                                if (passing == "Finish")
                                {
                                    if (!radios.ContainsKey(className) || radios[className].Count < results[id].Count)
                                    {
                                        UpdateRadiocontrols(radios, className, results, id);
                                    }
                                    FireOnResult(new Result
                                    {
                                        ID = id,
                                        Class = className,
                                        RunnerClub = club,
                                        RunnerName = name,
                                        SplitTimes = results[id].OrderBy(x=>x.Time).ToList(),
                                        Status = status,
                                        Time = time

                                    });

                                    results.Remove(id);
                                }
                                else
                                {

                                    if (!idToRunners.ContainsKey(id))
                                        idToRunners.Add(id, new Runner(id, name, club, className));

                                    results[id].Add(new ResultStruct
                                    {
                                        ControlCode = int.Parse(passing),
                                        Time = time
                                    });
                                }

                                //                            int start = startTime.Hour * 3600 * 100 + startTime.Minute * 60 * 100 + startTime.Second * 100;


                            }

                            

                            //                        data = data.Replace("&nbsp;", " ");
 //                       data = data.Replace("&nbsp", "");

                        }

                        foreach (var res in results)
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
                        }



                       
                    }

                    var stream = File.Create("runnerids.data");
                    bf.Serialize(stream, runnerIds);
                    stream.Close();

                    System.Threading.Thread.Sleep(15000);

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
                                if (!DateTime.TryParseExact(tTime, "mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                                {
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
