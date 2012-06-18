using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace WOCEmmaClient
{
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

        private Hashtable m_SplitTimes;
        public Runner(int dbID, string name, string club, string Class)
        {
            RunnerUpdated = true;
            ResultUpdated = false;
            StartTimeUpdated = false;

            m_SplitTimes = new Hashtable();
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
