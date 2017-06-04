using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using LiveResults.Client.Model;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using LiveResults.Model;
namespace LiveResults.Client
{
    
    public class MeOsParser : IExternalSystemResultParser
    {
        private readonly IDbConnection m_connection;
        private readonly int m_eventID;
        private readonly int m_eventRaceId;
        private readonly bool m_recreateRadioControls;
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;
        private bool m_isRelay = false;
        private bool m_continue;
        
        public MeOsParser(IDbConnection conn, bool recreateRadioControls = true, bool isRelay = false)
        {
            m_connection = conn;
            m_recreateRadioControls = recreateRadioControls;
            m_isRelay = isRelay;
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
                    cmd.CommandText = "select zerotime from oevent";
                    int zeroTime = Convert.ToInt32(cmd.ExecuteScalar());
                    string paramOper = "?counter";

                    string baseCommand =
                        "select r.id,r.bib, r.name,c.name as clubname,r.starttime, r.Status, cl.name as classname, r.FinishTime,r.counter, r.removed from orunner r, oclub c, oclass cl where r.class = cl.id and c.ID = r.club and r.counter > " + paramOper + " order by r.counter"; 
                   
                    string splitbaseCommand = "select p.counter, p.time, p.type,r.id,r.bib, r.name,c.name as clubname,r.starttime, r.Status, cl.name as classname from opunch p, orunner r, oclub c, oclass cl where p.CardNo = r.CardNo AND r.class = cl.id and c.ID = r.club and p.counter > " + paramOper + " order by p.counter";

                    if (m_isRelay)
                    {
                        baseCommand =
                        "select r.id,r.bib, r.name,r.starttime, r.Status, cl.name as classname, r.FinishTime,r.counter, r.removed from orunner r, oclass cl where r.class = cl.id and r.counter > " + paramOper + " order by r.counter";
                        splitbaseCommand = "select p.counter, p.time, p.type,r.id,r.bib, r.name,r.starttime, r.Status, cl.name as classname from opunch p, orunner r, oclass cl where p.CardNo = r.CardNo AND r.class = cl.id  and p.counter > " + paramOper + " order by p.counter";
                    }
                    string teamCommand = "select id, name, runners, (select legmethod from oclass oc where oc.id = class) as legmethod, counter from oTeam where counter > " + paramOper + " order by counter";

                    if (m_recreateRadioControls)
                    {
                        ReadRadioControls();
                    }

                    cmd.CommandText = baseCommand;
                    IDbCommand cmdSplits = m_connection.CreateCommand();
                    IDbCommand cmdSplitTimes = m_connection.CreateCommand();
                    IDbCommand cmdTeams = null;
                    if (m_isRelay)
                    {
                        cmdTeams = m_connection.CreateCommand();
                        cmdTeams.CommandText = teamCommand;
                    }
                    cmdSplits.CommandText = splitbaseCommand;
                    IDbDataParameter param = cmd.CreateParameter();
                    param.ParameterName = "counter";
                  
                        param.DbType = DbType.Int32;
                    param.Value = -1;
                
                    

                    IDbDataParameter splitparam = cmdSplits.CreateParameter();
                    splitparam.ParameterName = "counter";
                    
                        splitparam.DbType = DbType.Int32;
                        splitparam.Value = -1;

                    if (m_isRelay)
                    {
                        IDbDataParameter teamparam = cmdTeams.CreateParameter();
                        teamparam.ParameterName = "counter";

                        teamparam.DbType = DbType.Int32;
                        teamparam.Value = -1;
                        cmdTeams.Parameters.Add(teamparam);
                    }

                    
                    cmd.Parameters.Add(param);
                    cmdSplits.Parameters.Add(splitparam);
                    

                    int lastCounter = -1;
                    int lastTeamCounter = -1;
                    int lastSplitCounter = -1;

                    FireLogMsg("MeOS Monitor thread started");
                    IDataReader reader = null;
                    Dictionary<int, int> teamStarttimes = null;
                    Dictionary<int, int> runnerToTeamMap = null;
                    Dictionary<int, string> teamNames = null;
                    Dictionary<int, int> runnerLeg= null;
                    if (m_isRelay)
                    {
                        teamStarttimes = new Dictionary<int, int>();
                        runnerToTeamMap = new Dictionary<int, int>();
                        runnerLeg = new Dictionary<int, int>();
                        teamNames = new Dictionary<int, string>();

                    }
                    while (m_continue)
                    {
                        string lastRunner = "";
                        try
                        {
                            /*Kontrollera om nya klasser*/
                            /*Kontrollera om nya resultat*/

                            (cmd.Parameters["counter"] as IDbDataParameter).Value = lastCounter;
                            
                            (cmdSplits.Parameters["counter"] as IDbDataParameter).Value = lastSplitCounter;
                            cmd.Prepare();

                            if (m_isRelay)
                            {
                                (cmdTeams.Parameters["counter"] as IDbDataParameter).Value = lastTeamCounter;
                                reader = cmdTeams.ExecuteReader();
                                while (reader.Read())
                                {
                                    string name = reader["name"] as string;
                                    int counter = Convert.ToInt32(reader["counter"]);
                                    if (counter > lastTeamCounter)
                                        lastTeamCounter = counter;

                                    int id = Convert.ToInt32(reader["iD"]);
                                    if (!teamNames.ContainsKey(id))
                                        teamNames.Add(id, name);
                                    else
                                        teamNames[id] = name;

                                    string[] legSetup = (reader["legmethod"] as string).Split('*');
                                    string[] leg1Parts = legSetup[0].Split(':');
                                    int leg1StartTime = (Convert.ToInt32(leg1Parts[2]) + zeroTime) * 100;


                                    if (!teamStarttimes.ContainsKey(id))
                                        teamStarttimes.Add(id, leg1StartTime);
                                    else
                                        teamStarttimes[id] = leg1StartTime;

                                    string runners = reader["runners"] as string;
                                    string[] arunners = runners.Split(';');
                                    for (int i = 0; i < arunners.Length; i++)
                                    {
                                        var arunn = arunners[i];
                                        if (string.IsNullOrEmpty(arunn))
                                            continue;
                                        int idRunner = Convert.ToInt32(arunn);
                                        if (!runnerToTeamMap.ContainsKey(idRunner))
                                            runnerToTeamMap.Add(idRunner, id);
                                        else
                                            runnerToTeamMap[idRunner] = id;

                                        if (!runnerLeg.ContainsKey(idRunner))
                                        {
                                            runnerLeg.Add(idRunner, i + 1);
                                        }
                                        else
                                        {
                                            runnerLeg[idRunner] = i + 1;
                                        }
                                    }
                                }

                                reader.Close();
                            }

                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                int time = 0, runnerID = 0, iStartTime = 0;
                                string runnerName = "", club = "", classN = "";
                                int status = 0;

                                try
                                {
                                    int counter = Convert.ToInt32(reader["counter"]);
                                    lastCounter = counter > lastCounter ? counter : lastCounter;
                                    runnerID = Convert.ToInt32(reader["id"]);

                                    int teamId = runnerToTeamMap[runnerID];

                                    time = -9;
                                    iStartTime = m_isRelay ? teamStarttimes[teamId] : -1;
                                    if (m_isRelay)
                                    {
                                        if (reader["finishtime"] != null && reader["finishtime"] != DBNull.Value && Convert.ToInt32(reader["finishtime"]) > 0)
                                        {
                                            time = (Convert.ToInt32(reader["finishtime"])+zeroTime) * 100 - iStartTime;
                                        }
                                    }

                                    runnerName = reader["name"] as string;

                                    lastRunner = runnerName;

                                    club = m_isRelay ? teamNames[teamId] : (reader["clubname"] as string);
                                    classN = (reader["classname"] as string);
                                    if (m_isRelay)
                                    {
                                        classN += "-" + runnerLeg[runnerID];
                                    }
                                    status = Convert.ToInt32(reader["status"]);
                                }
                                catch (Exception ee)
                                {
                                    FireLogMsg(ee.Message);
                                }


                                /*
                                    time is seconds * 100
                                 * 
                                 * valid status is
                                    0 - notActivated...
                                    1 - OK

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
                                int rstatus = 0;
                                switch (status)
                                {
                                    case 0:
                                        rstatus = time > 0 ? 0 : 10;
                                        break;
                                    case 1: //OK
                                        rstatus = 0;
                                        break;
                                    case 20:
                                    case 2: //Ej start
                                        rstatus = 1;
                                        break;
                                    case 3: //Felst.
                                        rstatus = 3;
                                        break;
                                    case 4: //Utgått.
                                        rstatus = 3;
                                        break;
                                    case 5: //Dsq.
                                        rstatus = 4;
                                        break;
                                    case 6: //Maxtid.
                                        rstatus = 4;
                                        break;
                                    case 99:
                                    case 7: //Deltar ej.
                                        rstatus = 999;
                                        break;
                                    default:
                                        rstatus = 3;
                                        Debug.WriteLine("Unknwon status: " + status);
                                        break;
                                }
                                if (rstatus != 999)
                                {
                                    var res = new Result
                                    {
                                        ID = runnerID,
                                        RunnerName = runnerName,
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

                            reader = cmdSplits.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    int counter = Convert.ToInt32(reader["counter"]);
                                    lastSplitCounter = (counter > lastSplitCounter ? counter : lastSplitCounter);

                                    int runnerID = Convert.ToInt32(reader["id"]);

                                    int time = Convert.ToInt32(reader["time"]);
                                    int iStartTime = 0;
                                    if (m_isRelay)
                                    {
                                        int teamId = runnerToTeamMap[runnerID];
                                        iStartTime = teamStarttimes[teamId];
                                    }
                                    else
                                    {
                                        if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                                        {
                                            iStartTime = Convert.ToInt32(reader["starttime"]) * 100 + zeroTime * 100;
                                            //time = (Convert.ToInt32(reader["finishtime"]) - Convert.ToInt32(reader["starttime"])) * 100;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    time = (time * 100 + zeroTime * 100) - iStartTime;
                                    string runnerName = reader["name"] as string;

                                    lastRunner = runnerName;

                                    string club =  m_isRelay ? "" : (reader["clubname"] as string);
                                    string classN = (reader["classname"] as string);
                                    if (m_isRelay)
                                    {
                                        int teamId = runnerToTeamMap[runnerID];
                                        club = teamNames[teamId];
                                        classN += "-" + runnerLeg[runnerID];
                                    }

                                    int sCont = Convert.ToInt32(reader["Type"]);


                                    int passedCount = 1; //Convert.ToInt32(reader["passedCount"].ToString());

                                    var times = new List<ResultStruct>();
                                    var t = new ResultStruct
                                    {
                                        ControlCode = sCont + 1000 * passedCount,
                                        ControlNo = 0,
                                        Time = (int) time
                                    };
                                    times.Add(t);

                                    var res = new Result
                                    {
                                        ID = runnerID,
                                        RunnerName = runnerName,
                                        RunnerClub = club,
                                        Class = classN,
                                        StartTime = iStartTime,
                                        Time = -2,
                                        Status = 0,
                                        SplitTimes = times
                                    };
                                    FireOnResult(res);

                                }
                                catch (Exception ee)
                                {
                                    FireLogMsg(ee.Message);
                                }
                            }

                            reader.Close();

                            Thread.Sleep(1000);
                        }
                        catch (Exception ee)
                        {
                            if (reader != null)
                                reader.Close();
                            FireLogMsg("MeoS Parser: " + ee.Message + " {parsing: " + lastRunner);

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
                    FireLogMsg("MeOs Parser: " +ee.Message);
                }
                finally
                {
                    if (m_connection != null)
                    {
                        m_connection.Close();
                    }
                    FireLogMsg("Disconnected");
                    FireLogMsg("OLA Monitor thread stopped");

                }
            }
        }

        private void ReadRadioControls()
        {
            using (var cmd = m_connection.CreateCommand())
            {
                cmd.CommandText = "select Id,Name,Numbers from ocontrol where name <> \"\"";
                Dictionary<int,string> radiocontrols = new Dictionary<int, string>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        radiocontrols.Add(Convert.ToInt32(reader["id"]), reader["name"] as string);
                    }
                    reader.Close();
                }

                cmd.CommandText = "select cl.Name,co.Controls from oclass cl, ocourse co where co.Id = cl.Course";
                
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string className = reader["name"] as string;
                        string scontrols = reader["controls"].ToString();
                        if (!string.IsNullOrEmpty(scontrols))
                        {
                            int[] controls = scontrols.Split(new char[] { ';'},StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x)).ToArray();
                            Dictionary<int, int> passPerCode = new Dictionary<int, int>();
                            for (int i = 0; i < controls.Length; i++)
                            {
                                if (radiocontrols.ContainsKey(controls[i]))
                                {
                                    if (!passPerCode.ContainsKey(controls[i]))
                                        passPerCode.Add(controls[i], 0);

                                    int controlCode = (++passPerCode[controls[i]]) * 1000 + controls[i];

                                    var dlg = OnRadioControl;
                                    if (dlg != null)
                                        OnRadioControl(radiocontrols[controls[i]], controlCode, className, i);
                                }
                            }
                        }
                    }
                    reader.Close();
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
