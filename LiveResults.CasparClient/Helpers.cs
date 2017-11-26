using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveResults.CasparClient
{
    public static class Helpers
    {
        static Dictionary<int, string> m_runnerStatus = new Dictionary<int, string>();

        static Helpers()
        {
            m_runnerStatus.Add(1, "DNS");
            m_runnerStatus.Add(2, "DNF");
            m_runnerStatus.Add(11, "WO");
            m_runnerStatus.Add(12, "MO");
            m_runnerStatus.Add(0, "OK");
            m_runnerStatus.Add(3, "MP");
            m_runnerStatus.Add(4, "DSQ");
            m_runnerStatus.Add(5, "OT");
            m_runnerStatus.Add(9, "");
            m_runnerStatus.Add(10, "");
        }

        public static string RunnerStatus(int status)
        {
            return m_runnerStatus[status];
        }
        public static bool ContainsRunnerStatus(int status)
        {
            return m_runnerStatus.ContainsKey(status);
        }

        public static string FormatTime(int time, int status, bool showTenthOs, bool showHours, bool padZeros)
        {

            if (status != 0)
            {
                return m_runnerStatus[status];
            }
            else
            {
                if (showHours)
                {
                    int hours = ((int)Math.Floor(time / 360000.0));
                    int minutes = ((int)Math.Floor((time - hours * 360000d) / 6000d));
                    int seconds = ((int)Math.Floor((time - minutes * 6000d - hours * 360000) / 100));
                    int tenth = ((int)Math.Floor((time - minutes * 6000d - hours * 360000 - seconds * 100) / 10));
                    if (hours > 0)
                    {
                        string sHours = hours.ToString();
                        if (padZeros)
                            sHours = sHours.ToString().PadLeft(2, '0');

                        return sHours + ":" + minutes.ToString().PadLeft(2, '0') + ":" + seconds.ToString().PadLeft(2, '0') +
                               (showTenthOs ? "." + tenth.ToString() : "");
                    }
                    else
                    {


                        return (padZeros ? minutes.ToString().PadLeft(2, '0') : minutes.ToString()) + ":" +
                               seconds.ToString().PadLeft(2, '0') + (showTenthOs ? "." + tenth : "");
                    }

                }
                else
                {

                    int minutes = (int)Math.Floor(time / 6000d);
                    int seconds = (int)Math.Floor((time - minutes * 6000d) / 100);
                    int tenth = (int)Math.Floor((time - minutes * 6000d - seconds * 100) / 10);
                    if (padZeros)
                    {
                        return minutes.ToString().PadLeft(2, '0') + ":" + seconds.ToString().PadLeft(2, '0') +
                               (showTenthOs ? "." + tenth : "");
                    }
                    else
                    {
                        return minutes + ":" + seconds.ToString().PadLeft(2, '0') + (showTenthOs ? "." + tenth : "");
                    }
                }
            }
        }
    }
}
