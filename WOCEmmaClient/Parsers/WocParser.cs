using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Web.Script.Serialization;

namespace LiveResults.Client.Parsers
{
    public class WocParser : IExternalSystemResultParser
    {
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;

        private bool m_Continue = false;
        string[] urls;
        public WocParser(string[] urls)
        {
            this.urls = urls;
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

        System.Threading.Thread th;

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

        class WocJson
        {
            public WocJsonRow[] rows { get; set; }
        }
        class WocJsonRow
        {
            public int id { get; set; }
            public WocJsonRowCell cell { get; set; }
        }
        class WocJsonRowCell
        {
            public int id { get; set; }
            public string name { get; set; }
            public string federation { get; set; }
            public string Class { get; set; }
            public string startTime { get; set; }
            public int? raceTime { get; set; }
            public int classification { get; set; }
        }

        private void run()
        {
            while (m_Continue)
            {
                try
                {
                    WebClient wc = new WebClient();
                    foreach (var url in urls)
                    {
                        var json = wc.DownloadString(url);
                        JavaScriptSerializer ser = new JavaScriptSerializer();
                        var wocJson = ser.Deserialize<WocJson>(json);
                        Dictionary<string, object> kvps = ser.Deserialize<Dictionary<string, object>>(json);
                        //foreach (var r in wocJson.rows)
                        for (int i = 0; i < wocJson.rows.Length; i++)
                        {
                            var r = wocJson.rows[i];
                            var dic = ((kvps["rows"] as System.Collections.ArrayList)[i] as Dictionary<string, object>)["cell"] as Dictionary<string, object>;
                            int time = -4;
                            //DateTime startTime = DateTime.ParseExact(r.cell.startTime, new string[] { "H:mm:ss", "HH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None);
                            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(int.Parse(r.cell.startTime));
                            int start = startTime.Hour * 3600 * 100 + startTime.Minute * 60 * 100 + startTime.Second * 100;
                            int status = 10;
                            if (r.cell.raceTime.HasValue)
                            {
                                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(r.cell.raceTime.Value);
                                time = dt.Hour * 3600 * 100 + dt.Minute * 60 * 100 + dt.Second * 100;
                                status = r.cell.classification == 0 ? 0 : 4;
                            }

                            if (r.cell.classification != 0)
                                status = 4;
                            if (r.cell.classification == 1)
                                status = 1;
                            if (r.cell.classification == 2)
                                status = 2;
                            if (r.cell.classification == 3)
                                status = 3;
                            var splits = new List<ResultStruct>();
                            foreach (var kvp in dic)
                            {
                                if (kvp.Key.StartsWith("100") && kvp.Value != null && !string.IsNullOrEmpty(kvp.Value.ToString()))
                                {
                                    int code = int.Parse(kvp.Key) + 1000;
                                    int stime = int.Parse(kvp.Value.ToString());
                                    code = code - 10000;
                                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(int.Parse(kvp.Value.ToString()));
                                    stime = dt.Hour * 3600 * 100 + dt.Minute * 60 * 100 + dt.Second * 100;
                                    splits.Add(new ResultStruct() { ControlCode = code, Time = stime });
                                }
                            }
                            FireOnResult(new Result()
                            {
                                ID = r.id,
                                RunnerName = r.cell.name,
                                RunnerClub = r.cell.federation,
                                Class = r.cell.Class,
                                StartTime = start,
                                Time = time,
                                Status = status,
                                SplitTimes = splits
                            });
                        }
                    }
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
                        }
                    }
                }
            }
            return startTime;
        }


        public event RadioControlDelegate OnRadioControl;
    }
}
