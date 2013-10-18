using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;

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
                

                List<ResultStruct> combinedSplits = new List<ResultStruct>();
                if (Runner1.SplitTimes != null)
                {
                    if (Runner2.SplitTimes == null)
                        combinedSplits = Runner1.SplitTimes;
                    else
                    {
                        foreach (var spl in Runner1.SplitTimes)
                        {
                            var os = Runner2.SplitTimes.Where(x => x.ControlCode == spl.ControlCode).FirstOrDefault();
                            if (os.ControlCode == 0)
                                combinedSplits.Add(spl);
                            else
                                combinedSplits.Add(spl.Time > os.Time ? spl : os);
                        }

                        //Add those in Runner2 that are not in Runner1
                        foreach (var spl in Runner2.SplitTimes)
                        {
                            var os = Runner1.SplitTimes.Where(x => x.ControlCode == spl.ControlCode).FirstOrDefault();
                            if (os.ControlCode == 0)
                                combinedSplits.Add(spl);
                        }
                    }
                }
                else if (Runner2.SplitTimes != null)
                    combinedSplits = Runner2.SplitTimes;

                var res = new Result()
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

    public class Runner
    {
        private int m_id;
        private string m_name;
        private string m_club;
        private string m_Class;
        private int m_Start;
        private int m_Time;
        private int m_Status;

        public bool RunnerUpdated;
        public bool ResultUpdated;
        public bool StartTimeUpdated;

        private Dictionary<int,SplitTime> m_SplitTimes;
        public Runner(int dbID, string name, string club, string Class)
        {
            RunnerUpdated = true;
            ResultUpdated = false;
            StartTimeUpdated = false;

            m_SplitTimes = new Dictionary<int, SplitTime>();
            m_id = dbID;
            m_name = name;
            m_club = club;
            m_Class = Class;
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
            foreach (SplitTime t in m_SplitTimes.Values)
            {
                if (t.Updated)
                    return true;
            }
            return false;
        }

        public void ResetUpdatedSplits()
        {
            foreach (SplitTime t in m_SplitTimes.Values)
            {
                t.Updated = false;
            }
        }
        public List<SplitTime> GetUpdatedSplitTimes()
        {
            List<SplitTime> ret = new List<SplitTime>();
            foreach (SplitTime t in m_SplitTimes.Values)
            {
                if (t.Updated)
                {
                    ret.Add(t);
                }
            }
            return ret;
        }

        public bool HasStartTimeChanged(int starttime)
        {
            return m_Start != starttime;
        }

        public void SetStartTime(int starttime)
        {
            if (m_Start != starttime)
            {
                m_Start = starttime;
                StartTimeUpdated = true;
            }
        }

        public int Time
        {
            get
            {
                return m_Time;
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
                return m_SplitTimes.Values.ToArray();
            }
        }

        public string Class
        {
            get
            {
                return m_Class;
            }
            set
            {
                m_Class = value;
                RunnerUpdated = true;
            }
        }

        public int Status
        {
            get
            {
                return m_Status;
            }
        }

        public int StartTime
        {
            get
            {
                return m_Start;
            }
        }

        public bool HasSplitChanged(int controlCode, int time)
        {
            return !(m_SplitTimes.ContainsKey(controlCode) && ((SplitTime)m_SplitTimes[controlCode]).Time == time);
            
        }

        public void SetSplitTime(int controlCode, int time)
        {
            if (HasSplitChanged(controlCode, time))
            {
                if (m_SplitTimes.ContainsKey(controlCode))
                {
                    SplitTime t = (SplitTime)m_SplitTimes[controlCode];
                    t.Time = time;
                    t.Updated = true;
                }
                else
                {
                    SplitTime t = new SplitTime();
                    t.Control = controlCode;
                    t.Time = time;
                    t.Updated = true;
                    m_SplitTimes.Add(controlCode, t);
                }

            }
        }

        public bool HasResultChanged(int time, int status)
        {
            return m_Time != time || m_Status != status;
        }

        public void SetResult(int time, int status)
        {
            if (HasResultChanged(time,status))
            {
                m_Time = time;
                m_Status = status;
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
