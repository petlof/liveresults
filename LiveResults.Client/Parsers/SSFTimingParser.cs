using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using LiveResults.Client.Model;

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

                    if (m_createRadioControls)
                    {
                        var dlg = OnRadioControl;
                        if (dlg != null)
                        {
                            cmd.CommandText =
                                "select dbclass.Name as className, dbITimeInfo.Name as intermediateName, dbITimeInfo.Ipos from dbclass, dbITimeInfo where dbclass.RaceId = " +
                                m_eventID + " and dbITimeInfo.RaceID = " + m_eventID + " and dbITimeInfo.Course = dbclass.course  " +
                                " and dbiTimeInfo.PresInternet = 'Ja' order by dbclass.Name, ipos";
                            using (var reader = cmd.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    string className = reader["className"] as string;
                                    string intermediateName = reader["intermediateName"] as string;
                                    int position = Convert.ToInt32(reader["Ipos"]);
                                    dlg(intermediateName, 1000 + position, className, position);
                                }
                                reader.Close();
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

                    string initialCommand = string.Format(@"select  dbName.FirstName, dbName.LastName,
 dbTeam.Name as teamname, dbclass.name as classname,
 dbName.startTime, dbRuns.RaceTime, dbName.Startno, dbRuns.Status
from dbName, dbTeam, dbclass, dbRuns
where dbName.raceId = {0}
and dbTeam.raceId = {0}
and dbclass.raceid={0}
and dbRuns.RaceID = {0}
and dbRuns.StartNo = dbName.Startno
and dbTeam.TeamID = dbName.teamid
and dbclass.classid = dbName.classid", m_eventID);

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

                    cmd.CommandText = initialCommand;
                    cmdSplits.CommandText = initialSplitCommand;

                    string lastRunner = "";
                    ParseReader(cmd, out lastRunner);
                    ParseReaderSplits(cmdSplits, out lastRunner);

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
                            if (maxLogId >= 0)
                            {
                                cmd.CommandText = initialCommand +
                                                  string.Format(
                                                      @" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", m_eventID,
                                                      lastId);
                                cmdSplits.CommandText = initialSplitCommand +
                                                  string.Format(
                                                      @" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", m_eventID,
                                                      lastId);
                                ParseReaderSplits(cmdSplits, out lastRunner);
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

        private void ParseReader(IDbCommand cmd, out string lastRunner)
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

                        famName = (reader["lastname"] as string);
                        fName = (reader["firstname"] as string);
                        lastRunner = (string.IsNullOrEmpty(fName) ? "" : (fName + " ")) + famName;

                        club = (reader["teamname"] as string);
                        classN = (reader["classname"] as string);
                        status = reader["status"] as string;

                        time = -2;
                        DateTime startTime = DateTime.MinValue;

                        if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                        {
                            startTime = ParseDateTime(reader["starttime"].ToString());
                        }


                        if (reader["RaceTime"] != null && reader["RaceTime"] != DBNull.Value)
                        {

                            time = GetSSFRunTime(reader["RaceTime"].ToString());

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
        private void ParseReaderSplits(IDbCommand cmd, out string lastRunner)
        {
            lastRunner = null;
            using (IDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int runnerID = 0;
                    string famName = "", fName = "", club = "", classN = "";
                    var splits = new List<ResultStruct>();
                    try
                    {
                        runnerID = Convert.ToInt32(reader["startno"].ToString());

                        famName = (reader["lastname"] as string);
                        fName = (reader["firstname"] as string);
                        lastRunner = (string.IsNullOrEmpty(fName) ? "" : (fName + " ")) + famName;

                        club = (reader["teamname"] as string);
                        classN = (reader["classname"] as string);


                        if (reader["runtime"] != null && reader["runtime"] != DBNull.Value)
                        {
                            splits.Add(new ResultStruct{
                                ControlCode =  1000 + Convert.ToInt32(reader["ipos"]),
                                Time =  GetSSFRunTime(reader["runtime"].ToString()),
                                ControlNo = Convert.ToInt32(reader["ipos"])
                            });

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
                        Time = -2,
                        Status = 0,
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
            
            return (int)dt.TimeOfDay.TotalSeconds * 100 * factor;
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
