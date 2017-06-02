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

        private bool m_continue;
        
        public MeOsParser(IDbConnection conn, bool recreateRadioControls = true)
        {
            m_connection = conn;
            m_recreateRadioControls = recreateRadioControls;
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

                    if (m_recreateRadioControls)
                    {
                        ReadRadioControls();
                    }

                    cmd.CommandText = baseCommand;
                    IDbCommand cmdSplits = m_connection.CreateCommand();
                    IDbCommand cmdSplitTimes = m_connection.CreateCommand();
                    cmdSplits.CommandText = splitbaseCommand;
                    IDbDataParameter param = cmd.CreateParameter();
                    param.ParameterName = "counter";
                  
                        param.DbType = DbType.Int32;
                    param.Value = -1;
                
                    

                    IDbDataParameter splitparam = cmdSplits.CreateParameter();
                    splitparam.ParameterName = "counter";
                    
                        splitparam.DbType = DbType.Int32;
                        splitparam.Value = -1;

                    int lastCounter = -1;
                    int lastSplitCounter = -1;
                   
                    cmd.Parameters.Add(param);
                    cmdSplits.Parameters.Add(splitparam);

                    FireLogMsg("MeOS Monitor thread started");
                    IDataReader reader = null;
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

                                    time = -9;
                                    iStartTime = 0;
                                    if (reader["finishtime"] != null && reader["finishtime"] != DBNull.Value && Convert.ToInt32(reader["finishtime"]) > 0 &&
                                        reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                                    {
                                        iStartTime = Convert.ToInt32(reader["starttime"]) * 100 + zeroTime * 100;
                                        time = (Convert.ToInt32(reader["finishtime"]) - Convert.ToInt32(reader["starttime"])) * 100;
                                    }

                                    runnerName = reader["name"] as string;

                                    lastRunner = runnerName;

                                    club = (reader["clubname"] as string);
                                    classN = (reader["classname"] as string);
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
                                    if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                                    {
                                        iStartTime = Convert.ToInt32(reader["starttime"]) * 100 + zeroTime * 100;
                                        //time = (Convert.ToInt32(reader["finishtime"]) - Convert.ToInt32(reader["starttime"])) * 100;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    time = (time * 100 + zeroTime * 100) - iStartTime;
                                    string runnerName = reader["name"] as string;

                                    lastRunner = runnerName;

                                    string club = (reader["clubname"] as string);
                                    string classN = (reader["classname"] as string);


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
