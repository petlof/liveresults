using LiveResults.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveResults.Client.Model
{
    public enum SummaryMethod
    {
        FirstRunnerOnPreviousLeg,
        LastRunnerOnPreviousLeg
    }

    public class LegConfiguration
    {
        public int Leg { get; set; }
        public SummaryMethod SummaryMethod { get; set; }
    }

    public class ClassConfiguration
    {
        public LegConfiguration[] LegConfigurations { get; set; }

        public SummaryMethod GetStartTimeAllocationMethodForLeg(int leg)
        {
            var conf = LegConfigurations.FirstOrDefault(x => x.Leg == leg);
            return conf != null ? conf.SummaryMethod : SummaryMethod.FirstRunnerOnPreviousLeg;

        }
    }

    public class EventConfiguration
    {
        readonly Dictionary<string,ClassConfiguration> m_classes = new Dictionary<string, ClassConfiguration>(); 
        public void AddClassLeg(string className, int leg, SummaryMethod summaryMethod)
        {
            if (!m_classes.ContainsKey(className))
                m_classes.Add(className, new ClassConfiguration{
                    LegConfigurations = new LegConfiguration[0]
                });

            m_classes[className].LegConfigurations = m_classes[className].LegConfigurations.Append(new LegConfiguration{
                Leg = leg,
                SummaryMethod = summaryMethod
            }).OrderBy(x => x.Leg).ToArray();

        }

        public ClassConfiguration GetClassConfiguration(string className)
        {
            return m_classes.ContainsKey(className) ? m_classes[className] : null;
        }
    }

    public class RelayEventCache
    {
        public EventConfiguration Configuration { get; set; }
        public event ResultDelegate OnResultChanged;
        public event LogMessageDelegate OnLogMessage;

       
        private readonly Dictionary<string,Dictionary<string,TeamHolder>> m_cache = new Dictionary<string, Dictionary<string, TeamHolder>>();

        public void SetTeamLegSplitResult(int dbId, string className, string teamName,string bib, string runnerName, int leg, int startTime, int controlCode, int time, int passCounter)
        {
            if (!m_cache.ContainsKey(className))
            {
                m_cache.Add(className, new Dictionary<string, TeamHolder>());
            }
            if (!m_cache[className].ContainsKey(teamName))
            {
                m_cache[className].Add(teamName, new TeamHolder
                {
                    ClassName = className,
                    Legs = new TeamLegHolder[0],
                    TeamName = teamName,
                    Bib = bib
                });
            }

            var curLegRes = m_cache[className][teamName].Legs.FirstOrDefault(x => x.Leg == leg && x.DbId == dbId);
            if (curLegRes == null)
            {
                m_cache[className][teamName].Legs = m_cache[className][teamName].Legs.Append(new TeamLegHolder
                {
                    Leg = leg,
                    RunnerName = runnerName,
                    StartTime = startTime,
                    Status = 10,
                    Time = -10,
                    DbId = dbId,
                    Splits = new TeamLegSplitHolder[] { new TeamLegSplitHolder { controlCode =controlCode, passingNumber = passCounter, time = time} }
                }).OrderBy(x => x.Leg).ToArray();


            }
            else
            {
                if (curLegRes.Splits == null)
                {
                    curLegRes.Splits = new TeamLegSplitHolder[] { new TeamLegSplitHolder { controlCode = controlCode, passingNumber = passCounter, time = time } };
                }
                else
                {
                    var curSplitItem = curLegRes.Splits.Where(x => x.controlCode == controlCode && x.passingNumber == passCounter).FirstOrDefault();
                    if (curSplitItem != null)
                    {
                        curSplitItem.time = time;
                    }
                    else
                    {
                        var newSplits = new TeamLegSplitHolder[curLegRes.Splits.Count() + 1];
                        curLegRes.Splits.CopyTo(newSplits, 0);
                        newSplits[newSplits.Length - 1] = new TeamLegSplitHolder { controlCode = controlCode, passingNumber = passCounter, time = time };
                        curLegRes.Splits = newSplits;
                    }
                }
            }

            TriggerUpdateOnTeamRunners(className, teamName, leg);
        }

        public void SetTeamLegResult(int dbId, string className, string teamName,string bib, string runnerName, int leg, int startTime, int time, int status)
        {
            if (!m_cache.ContainsKey(className))
            {
                m_cache.Add(className, new Dictionary<string, TeamHolder>());
            }
            if (!m_cache[className].ContainsKey(teamName))
            {
                m_cache[className].Add(teamName, new TeamHolder{
                    ClassName = className,
                    Legs = new TeamLegHolder[0],
                    TeamName = teamName,
                    Bib = bib
                });
            }

            var curLegRes = m_cache[className][teamName].Legs.FirstOrDefault(x => x.Leg == leg && x.DbId == dbId);
            if (curLegRes == null)
            {
                m_cache[className][teamName].Legs = m_cache[className][teamName].Legs.Append(new TeamLegHolder{
                    Leg = leg,
                    RunnerName = runnerName,
                    StartTime = startTime,
                    Status = status,
                    Time = time,
                    DbId = dbId
                }).OrderBy(x => x.Leg).ToArray();

                
            }
            else
            {
                curLegRes.Time = time;
                curLegRes.Status = status;
                curLegRes.StartTime = startTime;
                curLegRes.RunnerName = runnerName;
            }

            //trigga uppdatering av alla efterföljande sträckor
            TriggerUpdateOnTeamRunners(className, teamName, leg);
        }

        private void TriggerUpdateOnTeamRunners(string className, string teamName, int leg)
        {
            TeamHolder team = m_cache[className][teamName];
            ClassConfiguration classConfig = Configuration.GetClassConfiguration(className);
            foreach (var legItem in team.Legs.Where(x => x.Leg >= leg))
            {
                var res = new Result
                {
                    Class = className + "-" + legItem.Leg,
                    ID = legItem.DbId,
                    RunnerClub = teamName,
                    RunnerName = legItem.RunnerName,
                    SplitTimes = GetSplitTimes(team,legItem,classConfig),
                    StartTime = legItem.StartTime,
                    bib = team.Bib
                };
                res.Status = m_cache[className][teamName].GetTeamTotalStatusAfterLeg(legItem.Leg);
                if (legItem.Status == 10)
                    res.Status = 10;

                if (res.Status != 0)
                    res.Time = -1 * res.Status;
                else
                {
                    res.Time = team.GetTeamTotalTimeAfterLeg(legItem.Leg, classConfig);
                    var legRunners = team.Legs.Where(x => x.Leg == legItem.Leg && x.Status == 0);
                    if (legRunners.Count() > 1)
                    {
                        res.Time += (legItem.Time - legRunners.Min(x => x.Time));
                    }
                }

                OnResultChanged(res);
            }
        }

        private List<ResultStruct> GetSplitTimes(TeamHolder team, TeamLegHolder legItem, ClassConfiguration classConfig)
        {
            if (legItem.Splits == null)
                return null;

            List<ResultStruct> ret = new List<ResultStruct>();
            for (int i = 0; i < legItem.Splits.Count(); i++)
            {
                ret.Add(new ResultStruct() { ControlCode = legItem.Splits[i].controlCode + 1000 * legItem.Splits[i].passingNumber, Time = legItem.Splits[i].time + team.GetTeamTotalTimeAfterLeg(legItem.Leg-1,classConfig) });
            }
            return ret;
        }
    }

   
    public class TeamHolder
    {
        public string ClassName { get; set; }
        public string TeamName { get; set; }
        public string Bib { get; set; }
        public TeamLegHolder[] Legs { get; set; }

        public int GetTeamTotalStatusAfterLeg(int leg)
        {
            for (int l = 1; l <= leg; l++)
            {
                var legRunners = Legs.Where(x => x.Leg == l).Select(x => x.Status).ToList();
                if (legRunners.Count() > 1)
                {
                    //Check if we have any that is OK..
                    if (legRunners.Where(x => x == 0).Count() > 0)
                    {

                    }
                    else
                    {
                        return legRunners.Count() > 0 ? legRunners.Min() : 0;
                    }
                }
                else if (legRunners.Count() == 1 && legRunners[0] != 0)
                    return legRunners[0];

            }
            return 0;
        }

        public int GetTeamTotalTimeAfterLeg(int leg, ClassConfiguration config)
        {
            int totaltime = 0;
            for (int i = 1; i <=leg; i++)
            {
                var times = Legs.Where(x => x.Leg == i && x.Status == 0).ToList();
                if (times.Count > 0)
                {
                    if (config.GetStartTimeAllocationMethodForLeg(i) == SummaryMethod.FirstRunnerOnPreviousLeg)
                    {
                        totaltime += times.Count > 0 ? times.Min(x => x.Time) : 0;
                    }
                    else
                    {
                        totaltime += times.Count > 0 ? times.Max(x => x.Time) : 0;
                    }
                }
            }
            return totaltime;
        }
    }

    public class TeamLegHolder
    {
        public int DbId { get; set; }
        public int Leg { get; set; }
        public string RunnerName { get; set; }
        public int StartTime { get; set; }
        public int Time { get; set; }
        public int Status { get; set; }
        public TeamLegSplitHolder[] Splits {get;set;}
        
    }

    public class TeamLegSplitHolder
    {
        public int controlCode {get;set;}
        public int time {get;set;}
        public int passingNumber {get;set;}
    }

    public static class ArrayExtensions
    {
        public static T[] Append<T>(this T[] originalArray, T addItem)
        {
            if (addItem == null)
            {
                throw new ArgumentNullException("addItem");
            }
            var arr = new[] { addItem };
            return originalArray == null ? arr : originalArray.Concat(arr).ToArray();
        }
    }
}
