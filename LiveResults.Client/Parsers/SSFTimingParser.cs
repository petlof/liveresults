using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using LiveResults.Client.Model;
using System.Linq;
using LiveResults.Model;

namespace LiveResults.Client
{
    
    public class SSFTimingParser : IExternalSystemResultParser
    {
        private readonly IDbConnection m_connection;
        private readonly int m_eventID;
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;
        private bool m_createRadioControls;
        private bool m_continue;
        private bool m_useTenth = false;

        public SSFTimingParser(IDbConnection conn, int eventID, bool recreateRadioControls = true)
        {
            m_connection = conn;
            m_eventID = eventID;
            m_createRadioControls = recreateRadioControls;
        }


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

        Thread m_monitorThread;

        public void Start()
        {
            m_continue = true;
            m_monitorThread = new Thread(Run);
            m_monitorThread.Start();
        }

        public void Stop()
        {
            m_continue = false;
        }

        private class RelayLegInfo
        {
            public int ITimeForFinish { get; set; }
            public bool IsLastLeg { get; set; }
        }

        private class IntermediateTime
        {
            public string ClassName { get; set; }
            public string IntermediateName { get; set; }
            public int Position { get; set; }
        }

       
        private void Run()
        {
            while (m_continue)
            {
                try
                {
                    if (m_connection.State != ConnectionState.Open)
                    {
                        m_connection.Open();
                    }

                   
                    IDbCommand cmd = m_connection.CreateCommand();
                    IDbCommand cmdSplits = m_connection.CreateCommand();

                    /*Detect event type*/
                    cmd.CommandText = "select TOP 1 dbClass.disciplin from dbClass where raceId=" + m_eventID;
                    var disciplin = cmd.ExecuteScalar() as string;
                    bool isRelay = false;
                    Dictionary<string, RelayLegInfo> relayLegs = null;
                    if (disciplin != null && disciplin.StartsWith("Stafett"))
                    {
                        isRelay = true;
                        relayLegs = new Dictionary<string, RelayLegInfo>();
                    }

                    

                    if (m_createRadioControls)
                    {
                        var dlg = OnRadioControl;
                        if (dlg != null)
                        {
                            cmd.CommandText =
                                "select dbclass.Name as className, dbITimeInfo.Name as intermediateName, dbITimeInfo.Ipos, dbITimeInfo.Leg from dbclass, dbITimeInfo where dbclass.RaceId = " +
                                m_eventID + " and dbITimeInfo.RaceID = " + m_eventID + " and dbITimeInfo.Course = dbclass.course  " +
                                " and dbiTimeInfo.PresInternet = 'Ja' order by dbclass.Name, ipos";

                            Dictionary<string, int> classMaxLeg = new Dictionary<string, int>();
                            List<IntermediateTime> intermediates = new List<IntermediateTime>();
                            using (var reader = cmd.ExecuteReader())
                            {
                                
                                while (reader.Read())
                                {
                                    string className = reader["className"] as string;
                                    if (!string.IsNullOrEmpty(className))
                                        className = className.Trim();

                                    string cleanClassName = className;
                                    
                                    if (isRelay)
                                    {
                                        if (!className.EndsWith("-"))
                                            className += "-";

                                        className +=reader["leg"].ToString().Trim();
                                    }

                                    
                                    string intermediateName =  reader["intermediateName"] as string;
                                    if (isRelay)
                                    {
                                        if (intermediateName.Contains("-"))
                                        {
                                            intermediateName = intermediateName.Split(new string[] {"-"},2,StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(intermediateName))
                                        intermediateName = intermediateName.Trim();
                                    int position = Convert.ToInt32(reader["Ipos"]);

                                    if (isRelay)
                                    {
                                        if (!relayLegs.ContainsKey(className))
                                        {
                                            relayLegs.Add(className, new RelayLegInfo
                                            {
                                                ITimeForFinish = position
                                            });
                                        }
                                        else
                                        {
                                            if (position > relayLegs[className].ITimeForFinish)
                                                relayLegs[className].ITimeForFinish = position;
                                        }
                                        if (!classMaxLeg.ContainsKey(cleanClassName))
                                        {
                                            classMaxLeg.Add(cleanClassName, Convert.ToInt32(reader["leg"]));
                                        }
                                        else
                                        {
                                            if (Convert.ToInt32(reader["leg"]) > classMaxLeg[cleanClassName])
                                                classMaxLeg[cleanClassName] = Convert.ToInt32(reader["leg"]);
                                        }
                                    }

                                    
                                    intermediates.Add(new IntermediateTime { ClassName = className, IntermediateName = intermediateName, Position = position });
                                }
                                reader.Close();
                            }

                            if (isRelay)
                            {
                                foreach (var kvp in classMaxLeg)
                                {
                                    var cName = kvp.Key;
                                    if (!cName.EndsWith("-"))
                                    {
                                        cName += "-";
                                    }
                                    cName += kvp.Value;
                                    relayLegs[cName].IsLastLeg = true;
                                }

                                foreach (var relayLeg in relayLegs)
                                {
                                    if (!relayLeg.Value.IsLastLeg)
                                    {
                                        var toDelete = intermediates.FirstOrDefault(x => x.ClassName == relayLeg.Key && x.Position == relayLeg.Value.ITimeForFinish);
                                        if (toDelete != null)
                                            intermediates.Remove(toDelete);
                                    }
                                }
                            }

                            foreach (var itime in intermediates)
                            {
                                dlg(itime.IntermediateName, 1000 + itime.Position, itime.ClassName, itime.Position);
                            }

                        }
                    }


                    cmd.CommandText = "select max(logid) from dbLog where raceid=" + m_eventID;
                    var oval = cmd.ExecuteScalar();
                    int maxLogId = -1;
                    if (oval != null && oval != DBNull.Value)
                    {
                        maxLogId = Convert.ToInt32(oval);
                    }

//                    string initialCommand = string.Format(@"select  dbName.FirstName, dbName.LastName,
// dbTeam.Name as teamname, dbclass.name as classname,
// dbName.startTime, dbRuns.RaceTime, dbName.Startno, dbRuns.Status
//from dbName, dbTeam, dbclass, dbRuns
//where dbName.raceId = {0}
//and dbTeam.raceId = {0}
//and dbclass.raceid={0}
//and dbRuns.RaceID = {0}
//and dbRuns.StartNo = dbName.Startno
//and dbTeam.TeamID = dbName.teamid
//and dbclass.classid = dbName.classid", m_eventID);

                    string initialCommand = string.Format(@"select  dbName.FirstName, dbName.LastName,
 dbTeam.Name as teamname, dbclass.name as classname,
 dbName.startTime, dbRuns.RaceTime, dbName.Startno, dbRuns.Status, dbRuns.Starttime as actualstarttime
from dbName 
inner join dbTeam on (dbTeam.raceId={0} and dbTeam.TeamID=dbName.TeamID)
inner join dbclass on (dbClass.RaceId={0} and dbclass.classId=dbName.ClassID)
left outer join dbRuns on (dbRuns.RaceID = {0} and dbRuns.StartNo = dbName.Startno)
where dbName.raceId = {0}", m_eventID);




                    string initialSplitCommand = string.Format(@"select  dbName.FirstName, dbName.LastName,
 dbTeam.Name as teamname, dbclass.name as classname, dbName.Startno, dbITime.runtime, dbITime.IPos
from dbName, dbTeam, dbclass, dbITime
where dbName.raceId = {0}
and dbTeam.raceId = {0}
and dbclass.raceid={0}
and dbITime.RaceID = {0}
and dbItime.StartNo = dbName.Startno
and dbTeam.TeamID = dbName.teamid
and dbclass.classid = dbName.classid", m_eventID);

                    if (isRelay)
                    {
                            initialCommand = string.Format(@"select  dbName.FirstName as teamname,dbRelay.Firstname, dbRelay.LastName, 
                             dbRelay.Leg, dbclass.name as classname,
                             dbName.startTime, dbRelay.LegTime, dbRuns.RaceTime, dbName.Startno, dbRuns.Status
                            from dbName 
                            inner join dbclass on (dbClass.RaceId={0} and dbclass.classId=dbName.ClassID)
                            inner join dbRelay on (dbRelay.RaceID={0} and dbRelay.NameID=dbName.ID)
                            left outer join dbRuns on (dbRuns.RaceID = {0} and dbRuns.StartNo = dbName.Startno)
                            where dbName.raceId = {0}", m_eventID);

                            initialSplitCommand = string.Format(@"select  dbName.FirstName as teamname, dbRelay.Firstname, dbRelay.LastName, dbRelay.Leg, 
                                dbclass.name as classname, dbName.Startno, dbITime.runtime, dbITime.IPos,
                                (select status from dbRuns where RaceId={0} and startNo = dbName.StartNo) as status
                                from dbName, dbclass, dbITime, dbRelay, dbITimeInfo
                                where dbName.raceId = {0}
                                and dbRelay.RaceID = {0}
                                and dbclass.raceid={0}
                                and dbITime.RaceID = {0}
                                and dbITimeInfo.RaceID={0}
                                and dbItime.StartNo = dbName.Startno
                                and dbclass.classid = dbName.classid
                                and dbRelay.NameID = dbName.ID
                                and dbclass.course = dbITimeInfo.Course
                                and dbITimeInfo.Ipos = dbITime.IPos
                                and dbITimeInfo.Leg=dbRelay.Leg", m_eventID);
                    }

                    cmd.CommandText = initialCommand;
                    cmdSplits.CommandText = initialSplitCommand;

                    string lastRunner = "";
                    ParseReader(cmd, out lastRunner, isRelay, relayLegs);
                    ParseReaderSplits(cmdSplits, out lastRunner, isRelay, relayLegs);

                    int lastId = maxLogId;

                    FireLogMsg("SSFTiming Monitor thread started");
                    while (m_continue)
                    {
                        
                        try
                        {
                            /*Kontrollera om nya klasser*/
                            /*Kontrollera om nya resultat*/
                            cmd.CommandText = "select max(logid) from dbLog where raceid=" + m_eventID;
                            oval = cmd.ExecuteScalar();
                            maxLogId = -1;
                            
                            if (oval != null && oval != DBNull.Value)
                            {
                                maxLogId = Convert.ToInt32(oval);
                            }
                            if (maxLogId >= 0 && maxLogId > lastId)
                            {
                                cmd.CommandText = initialCommand +
                                                  string.Format(
                                                      @" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", m_eventID,
                                                      lastId);
                                ParseReader(cmd, out lastRunner, isRelay, relayLegs);

                                cmdSplits.CommandText = initialSplitCommand +
                                                  string.Format(
                                                      @" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", m_eventID,
                                                      lastId);
                                ParseReaderSplits(cmdSplits, out lastRunner, isRelay, relayLegs);
                            }
                            lastId = maxLogId;

                            Thread.Sleep(1000);
                        }
                        catch (Exception ee)
                        {
                            FireLogMsg("SSFTiming Parser: " + ee.Message + " {parsing: " + lastRunner);

                            Thread.Sleep(100);

                            switch (m_connection.State)
                            {
                                case ConnectionState.Broken:
                                case ConnectionState.Closed:
                                    m_connection.Close();
                                    m_connection.Open();
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    FireLogMsg("SSFTIming Parser: " +ee.Message);
                }
                finally
                {
                    if (m_connection != null)
                    {
                        m_connection.Close();
                    }
                    FireLogMsg("Disconnected");
                    FireLogMsg("SSFTIMING Monitor thread stopped");

                }
            }
        }

        private void ParseReader(IDbCommand cmd, out string lastRunner, bool isRelay, Dictionary<string,RelayLegInfo> relayLegs)
        {
            lastRunner = null;
            using (IDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int time = 0, runnerID = 0, iStartTime = 0;
                    string famName = "", fName = "", club = "", classN = "", status = "";

                    try
                    {
                       runnerID = Convert.ToInt32(reader["startno"].ToString());

                        famName = (reader["lastname"] as string).Trim();
                        fName = (reader["firstname"] as string).Trim();
                        lastRunner = (string.IsNullOrEmpty(fName) ? "" : (fName + " ")) + famName;

                        club = (reader["teamname"] as string);
                        if (!string.IsNullOrEmpty(club))
                            club = club.Trim();
                        classN = (reader["classname"] as string);
                        if (!string.IsNullOrEmpty(classN))
                            classN = classN.Trim();

                        if (isRelay)
                        {
                            if (reader["leg"] != null)
                            { 
                                if (!classN.EndsWith("-"))
                                    classN +="-";

                                classN += reader["leg"].ToString().Trim();
                            }

                            runnerID = Convert.ToInt32(reader["leg"]) * 1000000 + runnerID;
                        }

                        status = reader["status"] as string;

                        time = -2;
                        DateTime startTime = DateTime.MinValue;

                        if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                        {
                            startTime = ParseDateTime(reader["starttime"].ToString());
                        }

                        if (!isRelay)
                        {
                            if (reader["actualstarttime"] != null && reader["actualstarttime"] != DBNull.Value)
                            {
                                startTime = ParseDateTime(reader["actualstarttime"].ToString());
                            }
                        }


                        if (!isRelay || relayLegs[classN].IsLastLeg)
                        {
                            if (reader["RaceTime"] != null && reader["RaceTime"] != DBNull.Value)
                            {

                                time = GetSSFRunTime(reader["RaceTime"].ToString());
								//Round to whole seconds
                                if (m_useTenth)
                                {
                                    time = ((int)(Math.Floor(time / 10d)) * 10);
                                }
                                else
                                {
                                    time = ((int) (Math.Floor(time / 100d)) * 100);
                                }
                            }
                        }

                        iStartTime = 0;
                        if (startTime > DateTime.MinValue)
                        {
                            iStartTime = (int)(startTime.TimeOfDay.TotalSeconds * 100);
                        }

                    }
                    catch (Exception ee)
                    {
                        FireLogMsg(ee.Message);
                    }

                    int rstatus = GetStatusFromCode(ref time, status);

                    if (rstatus != 999)
                    {
                        var res = new Result
                        {
                            ID = runnerID,
                            RunnerName = fName + " " + famName,
                            RunnerClub = club,
                            Class = classN,
                            StartTime = iStartTime,
                            Time = time,
                            Status = rstatus

                        };
                        FireOnResult(res);
                    }
                }

                reader.Close();
            }
        }

        private static int GetStatusFromCode(ref int time, string status)
        {
            int rstatus = 0;
            switch (status)
            {
                case "Start":
                    rstatus = 9;
                    break;
                case "DNS":
                    rstatus = 1;
                    time = -1;
                    break;
                case "DNF":
                    time = -3;
                    rstatus = 3;
                    break;
                case "DSQ":
                    time = -4;
                    rstatus = 4;
                    break;
            }
            return rstatus;
        }
        private void ParseReaderSplits(IDbCommand cmd, out string lastRunner, bool isRelay, Dictionary<string, RelayLegInfo> relayLegs)
        {
            lastRunner = null;
            using (IDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int runnerID = 0;
                    string famName = "", fName = "", club = "", classN = "";
                    int time = -2;
                    int status = 0;
                    var splits = new List<ResultStruct>();
                    try
                    {
                        runnerID = Convert.ToInt32(reader["startno"].ToString());

                        famName = (reader["lastname"] as string).Trim();
                        fName = (reader["firstname"] as string).Trim();
                        lastRunner = (string.IsNullOrEmpty(fName) ? "" : (fName + " ")) + famName;

                        club = (reader["teamname"] as string).Trim();
                        classN = (reader["classname"] as string).Trim();

                        if (isRelay)
                        {
                            if (reader["leg"] != null)
                            { 
                                if (!classN.EndsWith("-"))
                                    classN +="-";

                                classN += reader["leg"].ToString().Trim();
                            }

                            runnerID = Convert.ToInt32(reader["leg"]) * 1000000 + runnerID;
                         }

                        if (reader["runtime"] != null && reader["runtime"] != DBNull.Value)
                        {
                            int stime = GetSSFRunTime(reader["runtime"].ToString());
                            //Round to whole seconds
                            if (m_useTenth)
                            {
                                stime = ((int) (Math.Floor(stime / 10d)) * 10);
                            }
                            else
                            {
                                stime = ((int) (Math.Floor(stime / 100d)) * 100);
                             
                            }

                            if (isRelay && !relayLegs[classN].IsLastLeg && relayLegs[classN].ITimeForFinish == Convert.ToInt32(reader["ipos"]))
                            {
                                   time = stime;
                                   status = GetStatusFromCode(ref time, reader["status"].ToString().Trim());
                                   if (status == 9)
                                       status = 0;
                            }
                            else
                            {
                                splits.Add(new ResultStruct
                                {
                                    ControlCode = 1000 + Convert.ToInt32(reader["ipos"]),
                                    Time = stime,
                                    ControlNo = Convert.ToInt32(reader["ipos"])
                                });

                            }
                        }

                    }
                    catch (Exception ee)
                    {
                        FireLogMsg(ee.Message);
                    }


                    var res = new Result{
                        ID = runnerID,
                        RunnerName = fName + " " + famName,
                        RunnerClub = club,
                        Class = classN,
                        StartTime = 0,
                        Time = time,
                        Status = status,
                        SplitTimes =  splits
                    };
                    FireOnResult(res);

                }

                reader.Close();
            }
        }

        
        private static int GetSSFRunTime(string runTime)
        {
            int factor = 1;
            if (runTime.StartsWith("-"))
            {
                factor = -1;
                runTime = runTime.Substring(1);
            }
            
            DateTime dt;
            if (!DateTime.TryParseExact(runTime, "HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                if (!DateTime.TryParseExact(runTime, "HH:mm:ss.ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if (!DateTime.TryParseExact(runTime, "HH:mm:ss.f", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    {
                        if (!DateTime.TryParseExact(runTime, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            throw new ApplicationException ("Could not parse Time" + runTime);
                        }
                    }
                }
            }
            
            return (int)Math.Round(dt.TimeOfDay.TotalSeconds * 100 * factor);
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
