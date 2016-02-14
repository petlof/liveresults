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

        private bool m_continue;

        public SSFTimingParser(IDbConnection conn, int eventID)
        {
            m_connection = conn;
            m_eventID = eventID;
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

                    cmd.CommandText = "select max(logid) from dbLog where raceid=" + m_eventID;
                    int maxLogId = Convert.ToInt32(cmd.ExecuteScalar());

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
                    cmd.CommandText = initialCommand;
                    string lastRunner = "";
                    ParseReader(cmd, out lastRunner);

                    int lastId = maxLogId;

                    FireLogMsg("SSFTiming Monitor thread started");
                    while (m_continue)
                    {
                        
                        try
                        {
                            /*Kontrollera om nya klasser*/
                            /*Kontrollera om nya resultat*/
                            cmd.CommandText = "select max(logid) from dbLog where raceid=" + m_eventID;
                            maxLogId = Convert.ToInt32(cmd.ExecuteScalar());
                            cmd.CommandText = initialCommand + string.Format(@" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", m_eventID, lastId);
                            ParseReader(cmd, out lastRunner);
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
                        //int logid = Convert.ToInt32(reader[0]);
                        //lastId = (logid > lastId ? logid : lastId);
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
