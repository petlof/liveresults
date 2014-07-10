using System;
using System.Collections.Generic;
using System.Linq;
using LiveResults.Client.Parsers;

namespace LiveResults.Client
{
    public class RunnerPair
    {
        public Result Runner1;
        public Result Runner2;

        public Result CombinedRunner
        {
            get
            {
                if (Runner1 == null || Runner2 == null)
                    return null;
                else if (Runner1.Status == 999 || Runner2.Status == 999)
                    return null;

                string club = Runner1.RunnerClub;
                if (club != Runner2.RunnerClub)
                    club += "/" + Runner2.RunnerClub;

                int totalStatus = Runner1.Status;
                if (totalStatus == 0 && Runner2.Status != 0)
                    totalStatus = Runner2.Status;

                int time = Math.Max(Runner1.Time, Runner2.Time);
                

                var combinedSplits = new List<ResultStruct>();
                if (Runner1.SplitTimes != null)
                {
                    if (Runner2.SplitTimes == null)
                        combinedSplits = Runner1.SplitTimes;
                    else
                    {
                        foreach (var spl in Runner1.SplitTimes)
                        {
                            var os = Runner2.SplitTimes.FirstOrDefault(x => x.ControlCode == spl.ControlCode);
                            if (os.ControlCode == 0)
                                combinedSplits.Add(spl);
                            else
                                combinedSplits.Add(spl.Time > os.Time ? spl : os);
                        }

                        //Add those in Runner2 that are not in Runner1
                        foreach (var spl in Runner2.SplitTimes)
                        {
                            var os = Runner1.SplitTimes.FirstOrDefault(x => x.ControlCode == spl.ControlCode);
                            if (os.ControlCode == 0)
                                combinedSplits.Add(spl);
                        }
                    }
                }
                else if (Runner2.SplitTimes != null)
                    combinedSplits = Runner2.SplitTimes;

                var res = new Result
                {
                    ID = Math.Min(Runner1.ID,Runner2.ID),
                    Class = Runner1.Class,
                    Time = time,
                    RunnerClub = club,
                    StartTime = Runner1.StartTime,
                    SplitTimes = combinedSplits,
                    RunnerName = Runner1.RunnerName + "/" + Runner2.RunnerName,
                    Status = totalStatus
                };

                return res;
            }
        }
    }

    public class RadioControl : DbItem
    {
        public string ClassName
        {
            get;
            set;
        }

        public int Code
        {
            get;
            set;
        }

        public string ControlName
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }
    }

    public class DbItem
    {
    }

    public class Runner : DbItem
    {
        private readonly int m_id;
        private string m_name;
        private string m_club;
        private string m_class;
        private int m_start;
        private int m_time;
        private int m_status;

        public bool RunnerUpdated;
        public bool ResultUpdated;
        public bool StartTimeUpdated;

        private readonly Dictionary<int,SplitTime> m_splitTimes;
        public Runner(int dbID, string name, string club, string Class)
        {
            RunnerUpdated = true;
            ResultUpdated = false;
            StartTimeUpdated = false;

            m_splitTimes = new Dictionary<int, SplitTime>();
            m_id = dbID;
            m_name = name;
            m_club = club;
            m_class = Class;
        }


        public int ID
        {
            get
            {
                return m_id;
            }
        }

        public bool HasUpdatedSplitTimes()
        {
            return m_splitTimes.Values.Any(t => t.Updated);
        }

        public void ResetUpdatedSplits()
        {
            foreach (SplitTime t in m_splitTimes.Values)
            {
                t.Updated = false;
            }
        }
        public List<SplitTime> GetUpdatedSplitTimes()
        {
            return m_splitTimes.Values.Where(t => t.Updated).ToList();
        }

        public bool HasStartTimeChanged(int starttime)
        {
            return m_start != starttime;
        }

        public void SetStartTime(int starttime)
        {
            if (m_start != starttime)
            {
                m_start = starttime;
                StartTimeUpdated = true;
            }
        }

        public int Time
        {
            get
            {
                return m_time;
            }
        }

        public string Club
        {
            get
            {
                return m_club;
            }
            set
            {
                m_club = value;
                RunnerUpdated = true;
            }
        }

        public SplitTime[] SplitTimes
        {
            get
            {
                return m_splitTimes.Values.ToArray();
            }
        }

        public string Class
        {
            get
            {
                return m_class;
            }
            set
            {
                m_class = value;
                RunnerUpdated = true;
            }
        }

        public int Status
        {
            get
            {
                return m_status;
            }
        }

        public int StartTime
        {
            get
            {
                return m_start;
            }
        }

        public bool HasSplitChanged(int controlCode, int time)
        {
            return !(m_splitTimes.ContainsKey(controlCode) && m_splitTimes[controlCode].Time == time);
            
        }

        public void SetSplitTime(int controlCode, int time)
        {
            if (HasSplitChanged(controlCode, time))
            {
                if (m_splitTimes.ContainsKey(controlCode))
                {
                    SplitTime t = m_splitTimes[controlCode];
                    t.Time = time;
                    t.Updated = true;
                }
                else
                {
                    var t = new SplitTime();
                    t.Control = controlCode;
                    t.Time = time;
                    t.Updated = true;
                    m_splitTimes.Add(controlCode, t);
                }

            }
        }

        public bool HasResultChanged(int time, int status)
        {
            return m_time != time || m_status != status;
        }

        public void SetResultStatus(int status)
        {
            if (m_status != status)
            {
                m_status = status;
                ResultUpdated = true;
            }
        }

        public void SetResult(int time, int status)
        {
            if (HasResultChanged(time,status))
            {
                m_time = time;
                m_status = status;
                ResultUpdated = true;
            }
        }

        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
                RunnerUpdated = true;
            }
        }
    }

    public class SplitTime
    {
        public int Control;
        public int Time;
        public bool Updated;
    }
}
