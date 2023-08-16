using LiveResults.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LiveResults.Client.Parsers
{
    public class RacomFileSetParser : IExternalSystemResultParser
    {
        private readonly string m_startListFile;
        private readonly bool m_useCsvStartlist;
        private readonly string m_splitsFile;
        private readonly string m_finishFile;
        private readonly string m_radioDefinitionFile;
        private readonly DateTime m_zeroTime;
        private readonly bool m_isRelay;
        private readonly int m_finishCode;
        public RacomFileSetParser()
        {
        }

        public RacomFileSetParser(string startlistFile, bool useCsvStartlist, string splitsFile, string finishFile, string radioDefinitionFile, DateTime zeroTime, bool isRelay, int finishCode)
        {
            m_startListFile = startlistFile;
            m_useCsvStartlist = useCsvStartlist;
            m_splitsFile = splitsFile;
            m_finishFile = finishFile;
            m_radioDefinitionFile = radioDefinitionFile;
            m_zeroTime = zeroTime;
            m_isRelay = isRelay;
            m_finishCode = finishCode;
        }

        public class RacomRunner : Runner
        {
            public int BibNo { get; set; }
            public int? LegNo { get; set; }
            public string ClassWithoutLeg { get; set; }
            public RacomRunner(int bibNo, int dbID, string name, string club, string Class, int? legNo = null ) : base(dbID, name, club, Class)
            {
                BibNo = bibNo;
                LegNo = legNo;
                if (bibNo > 0)
                    Bib = bibNo.ToString();
            }

        }



        public Runner[] ParseFiles(DateTime zeroTime, string startlistFile, string splitsFile, string finFile, bool useCsvStartlist, int finishCode)
        {
            var ret = new List<Runner>();
            var siToRunner = new Dictionary<int, Runner>();
            var enc = Encoding.GetEncoding("Windows-1250");
            if (useCsvStartlist)
                ReadStartListCsvUtf8(zeroTime, startlistFile, ret, siToRunner);
            else
                ReadStartList(zeroTime, startlistFile, ret, siToRunner, enc);
            //ReadAndApplyRaceFile(raceFile, ret, enc);
            ReadAndApplyFINFile(finFile, siToRunner, enc);
            ReadAndApplySplitsFile(splitsFile, siToRunner, finishCode, enc);

            if (m_isRelay)
            {
                foreach (var classCategory in ret.GroupBy(x => (x as RacomRunner).ClassWithoutLeg))
                {
                    /*Update runners for teams according to status on earlier legs*/
                    foreach (var team in classCategory.GroupBy(x => (x as RacomRunner).BibNo))
                    {
                        var runners = new List<Runner>(team).OrderBy(x => (x as RacomRunner).LegNo).ToList();

                        for (int i = 1; i < runners.Count(); i++)
                        {
                            if (runners[i-1].Status != 0 && runners[i-1].Status < 9)
                            {
                                runners[i].SetResultStatus(runners[i - 1].Status);
                            }
                            if (runners[i - 1].Time > 0)
                                runners[i].SetStartTime(runners[0].StartTime + runners[i - 1].Time);
                        }
                    }
                }
            }
            

            return ret.ToArray();
        }

        private void ReadAndApplyFINFile(string finFile, Dictionary<int,Runner> siToRunner , Encoding enc)
        {
            if (!System.IO.File.Exists(finFile))
            {
                if (OnLogMessage != null)
                    OnLogMessage("Read FIN file - file not found");
                return;
            }

            using (var sr = new StreamReader(finFile, enc))
            {
                string tmp;
                while ((tmp = sr.ReadLine()) != null)
                {
//                    Dictionary<int,int> teamstatus = new Dictionary<int, int>();
                    if (!string.IsNullOrEmpty(tmp) && !string.IsNullOrEmpty(tmp.Trim()))
                    {
                        int idxColon = tmp.IndexOf(":", StringComparison.Ordinal);
                        int idxSlash = tmp.IndexOf("/", StringComparison.Ordinal);
                        int idxSlash2 = tmp.IndexOf("/", idxSlash + 1, StringComparison.Ordinal);
                        var si = int.Parse(tmp.Substring(0, idxColon).Trim());

                        var code = tmp.Substring(idxColon + 1, idxSlash - idxColon - 1).Trim();
                        if (code != "FIN")
                            continue;

                        string time = tmp.Substring(idxSlash + 1, idxSlash2 - idxSlash - 1).Trim();
                        string status = tmp.Substring(idxSlash2 + 1).Trim();

                        if (!siToRunner.ContainsKey(si))
                        {
                            continue;
                        }

                        var runner = siToRunner[si];
                        int rtime = -10;
                        int rstatus = 10;

                        switch (status)
                        {
                            case "MP":
                                rstatus = 3;
                                rtime = -3;
                                break;
                            case "DISQ":
                                rstatus = 4;
                                break;
                            case "DNF":
                                rstatus = 2;
                                rtime = -3;
                                break;
                            case "DNS":
                                rstatus = 1;
                                rtime = -3;
                                break;
                            case "OVRT":
                                rstatus = 5;
                                break;
                            case "NC":
                                rstatus = 11;
                                break;
                            case "O.K.":
                                rstatus = 0;
                                break;
                        }

                        if (rstatus == 0)
                        {
                            var passTime = DateTime.ParseExact(time, "HH:mm:ss.ffff", CultureInfo.InvariantCulture);
                            var asTime = passTime.Hour*360000 + passTime.Minute*6000 + passTime.Second*100 + passTime.Millisecond/10;
                            runner.SetResult(asTime - runner.StartTime, rstatus);
                        }
                        else
                        {
                            runner.SetResult(rtime, rstatus);
                        }
                    }
                }
            }
        }

        
        private static
             void ReadAndApplySplitsFile(string splitsFile, Dictionary<int, Runner> siToRunner, int finishCode, Encoding enc)
        {
            if (!File.Exists(splitsFile))
                return;
            using (var sr = new StreamReader(splitsFile, enc))
            {
                var siSplitPunches = new Dictionary<int, List<CodeTimeHolder>>();
                string tmp;
                while ((tmp = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(tmp) && !string.IsNullOrEmpty(tmp.Trim()))
                    {
                        int idxColon = tmp.IndexOf(":", StringComparison.Ordinal);
                        int idxSlash = tmp.IndexOf("/", StringComparison.Ordinal);
                        if (idxColon == -1 || idxSlash == -1)
                            continue;
                        var si = int.Parse(tmp.Substring(0, idxColon).Trim());

                        var code = int.Parse(tmp.Substring(idxColon + 1, idxSlash - idxColon-1));
                        string time = tmp.Substring(idxSlash + 1);

                        if (!siToRunner.ContainsKey(si) || time == "No time ")
                        {
                            continue;
                        }

                        var runner = siToRunner[si];

                        var passTime = DateTime.ParseExact(time, "HH:mm:ss.f", CultureInfo.InvariantCulture);
                        var asTime = passTime.Hour * 360000 + passTime.Minute * 6000 + passTime.Second * 100 + passTime.Millisecond / 10;

                        if (code == finishCode)
                            runner.SetResult(asTime - runner.StartTime, (runner.Status == 9) ? 0  : runner.Status);
                        else
                        {
                            if (!siSplitPunches.ContainsKey(si))
                                siSplitPunches.Add(si, new List<CodeTimeHolder>());

                            siSplitPunches[si].Add(new CodeTimeHolder
                            {
                                Code = code,
                                Time = asTime - runner.StartTime
                            });
                        }
                    }
                }

                //Do processing (remove double punches within 15sek) and add as runner splits
                foreach (var kvp in siSplitPunches)
                {
                    var punches = FilterPunchesAgainstDoubles(kvp.Value);

                    var r = siToRunner[kvp.Key];
                    var splitCodeCounter = new Dictionary<int, int>();
                    foreach (CodeTimeHolder codeTimeHolder in punches)
                    {
                        if (!splitCodeCounter.ContainsKey(codeTimeHolder.Code))
                        {
                            splitCodeCounter.Add(codeTimeHolder.Code, 1);
                        }
                        r.SetSplitTime(splitCodeCounter[codeTimeHolder.Code]*1000 + codeTimeHolder.Code, codeTimeHolder.Time);

                        splitCodeCounter[codeTimeHolder.Code]++;
                    }
                }
            }
        }

        private static IEnumerable<CodeTimeHolder> FilterPunchesAgainstDoubles(IEnumerable<CodeTimeHolder> orginalPunches)
        {
            var punches = orginalPunches.OrderBy(x => x.Time).ToList();
            for (int i = 1; i < punches.Count; i++)
            {
                if (punches[i - 1].Code == punches[i].Code &&
                    (punches[i].Time - punches[i - 1].Time) < 1500)
                {
                    punches.RemoveAt(i);
                    i--;
                }
            }
            return punches;
        }

        private void ReadStartList(DateTime zeroTime, string startlistFile, List<Runner> ret, Dictionary<int, Runner> siToRunner, Encoding enc)
        {
            if (!System.IO.File.Exists(startlistFile))
            {
                if (OnLogMessage != null)
                    OnLogMessage("Read StartList - file not found");
                return;
            }
            using (var sr = new StreamReader(startlistFile, enc))
            {
                string tmp;
                while ((tmp = sr.ReadLine()) != null)
                {
                    if (tmp.Length != 65)
                    {
                        if (OnLogMessage != null)
                            OnLogMessage("Incorrect start line size ! (length is " + tmp.Length + ")");
                    }
                    try
                    {
                        string stnr = tmp.Substring(0, 5).Trim();
                        string sinr = tmp.Substring(6, 8).Trim();
                        string className = tmp.Substring(15, 7).Trim();
                        string name = tmp.Substring(31, 23).Trim();
                        string start = tmp.Substring(54).Trim();
                        string club = tmp.Substring(23, 3);

                        if (string.IsNullOrEmpty(stnr))
                        {
                            if (OnLogMessage != null)
                                OnLogMessage("Startnumber empty: runner: " + name + " in class " + className);
                            continue;
                        }
                        int leg = 0;
                        if (m_isRelay)
                            leg = int.Parse(className.Split(' ').Last());
                        int dbId = m_isRelay ? leg * 100000 + int.Parse(stnr) : int.Parse(stnr);

                        var r = new RacomRunner(int.Parse(stnr), dbId, name, club, className, m_isRelay ? (int?)leg : null);

                        if (m_isRelay)
                        {
                            r.ClassWithoutLeg = className.Substring(0, className.LastIndexOf(' '));
                            if (!string.IsNullOrEmpty(r.ClassWithoutLeg))
                                r.ClassWithoutLeg = r.ClassWithoutLeg.Trim();

                            r.Class = r.ClassWithoutLeg + " " + leg;
                        }


                        var startTime = zeroTime.AddSeconds(parseTime(start));
                        r.SetStartTime(startTime.Hour * 360000 + startTime.Minute * 6000 + startTime.Second * 100 + startTime.Millisecond / 10);
                        r.SetResult(-9, 9);

                        ret.Add(r);
                        if (!siToRunner.ContainsKey(int.Parse(sinr)))
                        {
                            siToRunner.Add(int.Parse(sinr), r);
                        }
                        else
                        {
                            if (OnLogMessage != null)
                                OnLogMessage("Duplicate SI-NO: " + sinr + ", skipping " + name);
                        }
                    }
                    catch (System.Exception)
                    {
                        if (OnLogMessage != null)
                            OnLogMessage("Parsing start list error on line : " + tmp);
                    }
                }
            }
        }

        private void ReadStartListCsvUtf8(DateTime zeroTime, string startlistFile, List<Runner> ret, Dictionary<int, Runner> siToRunner)
        {
            if (!System.IO.File.Exists(startlistFile))
            {
                if (OnLogMessage != null)
                    OnLogMessage("Read StartList - file not found");
                return;
            }
            var csvRows = System.IO.File.ReadAllLines(startlistFile, Encoding.UTF8).ToList();
            foreach (var row in csvRows.Skip(1))    // first row is header
            {
                var columns = row.Split(';');
                if (columns.Length < 7)
                {
                    if (OnLogMessage != null)
                        OnLogMessage("Incorrect start line columns count! (" + columns.Length + ")");
                }
                try
                {
                    // columns are :
                    // ID, Class, SI, Name, Starttime, Club, Bib, [Leg]
                    string id = columns[0].Trim();
                    string stnr = columns[6].Trim();
                    string sinr = columns[2].Trim();
                    string className = columns[1].Trim();
                    string name = columns[3].Trim();
                    string start = columns[4].Trim();
                    string club = columns[5].Trim();
                    int leg = 0;
                    if (columns.Length > 7)
                        leg = int.Parse(columns[7].Trim());

                    if (string.IsNullOrEmpty(id))
                    {
                        if (OnLogMessage != null)
                            OnLogMessage("Startnumber empty: runner: " + name + " in class " + className);
                        continue;
                    }

                    int dbId = m_isRelay ? leg * 100000 + int.Parse(id) : int.Parse(id);
                    var r = new RacomRunner(int.Parse(stnr), dbId, name, club, className, m_isRelay ? (int?)leg : null);

                    if (m_isRelay)
                    {
                        r.ClassWithoutLeg = className;
                        if (!string.IsNullOrEmpty(r.ClassWithoutLeg))
                            r.ClassWithoutLeg = r.ClassWithoutLeg.Trim();

                        r.Class = r.ClassWithoutLeg + " " + leg;
                    }

                    var startTime = zeroTime.AddSeconds(parseTime(start));
                    r.SetStartTime(startTime.Hour * 360000 + startTime.Minute * 6000 + startTime.Second * 100 + startTime.Millisecond / 10);
                    r.SetResult(-9, 9);

                    ret.Add(r);
                    if (!siToRunner.ContainsKey(int.Parse(sinr)))
                    {
                        siToRunner.Add(int.Parse(sinr), r);
                    }
                    else
                    {
                        if (OnLogMessage != null)
                            OnLogMessage("Duplicate SI-NO: " + sinr + ", skipping " + name);
                    }
                }
                catch (System.Exception)
                {
                    if (OnLogMessage != null)
                        OnLogMessage("Parsing start list error on line : " + row);
                }
            }
        }

        private class CodeTimeHolder
        {
            public int Code
            {
                get;
                set;
            }

            public int Time
            {
                get;
                set;
            }
        }


        private double parseTime(string start)
        {
            int iDot = start.IndexOf(".", StringComparison.Ordinal);
            bool bMinus = start.IndexOf("-", StringComparison.Ordinal) == 0; // first in -
            string minutes = start.Substring(bMinus ? 1 : 0, bMinus ? iDot-1: iDot);
            string secs = start.Substring(iDot + 1).Replace(",", ".");
            double time = int.Parse(minutes) * 60 + double.Parse(secs, CultureInfo.InvariantCulture);
            return bMinus ? time *= -1 : time;
        }

        private bool m_continue = false;

        void ProcessRadioDefinitionFile()
        {
            var dlg = OnRadioControl;
            if (dlg != null)
            {
                Dictionary<string, string> radioNamesDictionary = null;
                string splitcodeNamesFile = m_radioDefinitionFile.Replace(".splitcodes.txt", ".splitnames.txt");
                if (File.Exists(splitcodeNamesFile))
                {
                    radioNamesDictionary = new Dictionary<string, string>();
                    using (var sr = new StreamReader(splitcodeNamesFile, Encoding.Default))
                    {
                        string tmp;
                        while ((tmp = sr.ReadLine()) != null)
                        {
                            string[] parts = tmp.Split(new string[]{
                                " "
                            }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 1)
                            {
                                string className = parts[0];
                                if (!string.IsNullOrEmpty(className))
                                {
                                    className = className.Replace("_", " ");
                                }
                                for (int i= 1; i < parts.Length; i++)
                                {
                                    radioNamesDictionary.Add(className + "::" + (i - 1), parts[i]);
                                }
                            }
                        }
                    }
                }

                using (var sr = new StreamReader(m_radioDefinitionFile, Encoding.Default))
                {
                    string tmp;
                    while ((tmp = sr.ReadLine()) != null)
                    {
                        string[] parts = tmp.Split(new string[]{
                            " "
                        }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 2)
                        {
                            var passingDic = new Dictionary<string, int>();
                            string className = parts[0];
                            if (!string.IsNullOrEmpty(className))
                                className = className.Replace("_", " ");
                            for (int spl = 1; spl < parts.Length - 1; spl++)
                            {
                                int passing = 1;
                                if (!passingDic.ContainsKey(parts[spl]))
                                    passingDic.Add(parts[spl], 1);
                                else
                                {
                                    passingDic[parts[spl]]++;
                                    passing = passingDic[parts[spl]];
                                }
                                int rCode = passing*1000 + int.Parse(parts[spl]);
                                string name = parts[spl];
                                if (radioNamesDictionary != null && radioNamesDictionary.ContainsKey(className + "::" + (spl-1)))
                                {
                                    name = radioNamesDictionary[className + "::" + (spl - 1)];
                                }
                                dlg(name, rCode, className, spl - 1);
                            }
                        }

                    }
                }
            }
        }

        public void Start()
        {
        //    var bdir = @"C:\Projekt\opensource\liveresultat-tfs\emmaclient\LiveResults.Client.Tests\";
        //    var runners = ParseFiles(new DateTime(2014, 1, 1, 9, 30, 0), bdir + @"\TestFiles\Racom\Middle\RACOM_UTF.txt",
        //        bdir + @"\TestFiles\Racom\Middle\w_test2\w_test2.rawsplits.txt",
        //        bdir + @"\TestFiles\Racom\Middle\FIN00071.CSV",
        //        bdir + @"\TestFiles\Racom\Middle\w_test2\w_test2.disks.txt");

            if (!string.IsNullOrEmpty(m_radioDefinitionFile) && File.Exists(m_radioDefinitionFile))
            {
                ProcessRadioDefinitionFile();
            }

            m_continue = true;
            var th = new Thread(() =>
            {
                while (m_continue)
                {
                    try
                    {

                    
                    var runners = ParseFiles(m_zeroTime, m_startListFile, m_splitsFile, m_finishFile, m_useCsvStartlist, m_finishCode);
                    if (OnResult != null)
                    {
                        foreach (var r in runners)
                        {
                            OnResult(new Result{
                                RunnerName = r.Name,
                                Class = r.Class,
                                ID = r.ID,
                                RunnerClub = r.Club,
                                SplitTimes = r.SplitTimes.Select(x => new ResultStruct{
                                    ControlCode = x.Control,
                                    ControlNo = x.Control,
                                    Place = 0,
                                    Time = x.Time
                                }).ToList(),
                                StartTime = r.StartTime,
                                Status = r.Status,
                                Time = r.Time,
                                bib = r.Bib
                            });
                        }
                    }
                    }
                    catch (Exception ee)
                    {
                        if (OnLogMessage != null)
                            OnLogMessage(ee.Message);
                    }

                    Thread.Sleep(15000);
                }
            });
            th.Start();
        }

       

        public void Stop()
        {
            m_continue = false;
        }



        public event ResultDelegate OnResult;

        public event LogMessageDelegate OnLogMessage;

        public event RadioControlDelegate OnRadioControl;
    }
}
