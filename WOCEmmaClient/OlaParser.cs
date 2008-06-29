using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace WOCEmmaClient
{
    public class OlaParser
    {
        private OleDbConnection m_Connection;
        private int m_EventID;
        private int m_EventRaceId;

        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;

        private bool m_Continue = false;
        public OlaParser(OleDbConnection conn, int eventID, int eventRaceId)
        {
            m_Connection = conn;
            m_EventID = eventID;
            m_EventRaceId = eventRaceId;
        }

        private void FireOnResult(int id, int SI, string name, string club, string Class, int start, int time, int status, List<ResultStruct> results)
        {
            if (OnResult != null)
            {
                OnResult(id, SI, name, club, Class, start, time, status, results);
            }
        }
        private void FireLogMsg(string msg)
        {
            if (OnLogMessage != null)
                OnLogMessage(msg);
        }

        System.Threading.Thread th;

        public void Start()
        {
            m_Continue = true;
            th = new System.Threading.Thread(new System.Threading.ThreadStart(run));
            th.Start();
        }

        public void Stop()
        {
            m_Continue = false;
        }

        private void run()
        {
            while (m_Continue)
            {
                try
                {
                    if (m_Connection.State != System.Data.ConnectionState.Open)
                    {
                        m_Connection.Open();
                    }
                    string baseCommand = "select results.modifyDate, results.totalTime, results.position, persons.familyname as lastname, persons.firstname as firstname, clubs.name as clubname, eventclasses.shortName, results.runnerStatus, results.entryid from results, entries, Persons, Clubs, raceclasses,eventclasses where raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.clubid = clubs.clubid and results.runnerStatus != 'notActivated' and results.modifyDate > ?";
                    string splitbaseCommand = "select splittimes.modifyDate, splittimes.passedTime, Controls.ID, results.entryid, results.allocatedStartTime, persons.familyname as lastname, persons.firstname as firstname, clubs.name as clubname, eventclasses.shortName, splittimes.passedCount from splittimes, results, SplitTimeControls, Controls, eventClasses, raceClasses, Persons, Clubs, entries where splittimes.resultraceindividualnumber = results.resultid and SplitTimes.splitTimeControlID = SplitTimeControls.splitTimeControlID and SplitTimeControls.timingControl = Controls.controlid and Controls.eventRaceId = " + m_EventRaceId + " and raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.clubid = clubs.clubid and splitTimes.modifyDate > ?";
                    OleDbCommand cmd = new OleDbCommand(baseCommand, m_Connection);
                    OleDbCommand cmdSplits = new OleDbCommand(splitbaseCommand, m_Connection);
                    OleDbParameter param = new OleDbParameter("@date", DateTime.Now);
                    OleDbParameter splitparam = new OleDbParameter("@date", DateTime.Now);
                    param.DbType = DbType.DateTime;
                    splitparam.DbType = DbType.DateTime;

                    //param.OleDbType = OleDbType.DBTimeStamp;
                    DateTime lastDateTime = DateTime.Now.AddMonths(-120);
                    DateTime lastSplitDateTime = DateTime.Now.AddMonths(-120);
                    param.Value = lastDateTime;
                    splitparam.Value = lastSplitDateTime;

                    cmd.Parameters.Add(param);
                    cmdSplits.Parameters.Add(splitparam);

                    FireLogMsg("OLA Monitor thread started");
                    while (m_Continue)
                    {
                        OleDbDataReader reader = null;
                        string lastRunner = "";
                        try
                        {
                            /*Kontrollera om nya klasser*/
                            /*Kontrollera om nya resultat*/
                            //cmd.CommandText = baseCommand + "" + lastDateTime.ToString("yyyyMMddhhmmss");
                            cmd.Parameters["@date"].Value = lastDateTime;
                            cmdSplits.Parameters["@date"].Value = lastSplitDateTime;

                            string command = cmd.CommandText;
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                DateTime modDate = DateTime.MinValue;
                                int time = 0, position = 0, runnerID = 0;
                                string famName = "", fName = "", club = "", classN = "", status = "";
                                try
                                {
                                    modDate = reader.GetDateTime(0);
                                    lastDateTime = (modDate > lastDateTime ? modDate : lastDateTime);
                                    runnerID = reader.GetInt32(8);
                                    if (runnerID == 4579)
                                    {
                                        int test = 1;
                                    }
                                    //logit("got result {" + reader.GetDateTime(0).ToString("yyyy-MM-dd hh:mm:ss") + "}");
                                    time = -9;
                                    if (!reader.IsDBNull(1))
                                        time = reader.GetInt32(1);

                                    //position = -1;
                                    //if (!reader.IsDBNull(2) && reader[2] != null)
                                    //    position = reader.GetInt32(2);
                                    famName = reader.GetString(3);
                                    fName = reader.GetString(4);
                                    lastRunner = fName + " " + famName;
                                    club = reader.GetString(5);
                                    classN = reader.GetString(6);
                                    status = reader.GetString(7);
                                    
                                }
                                catch (Exception ee)
                                {
                                    FireLogMsg(ee.Message);
                                }


                                /*
                                    time is seconds * 100
                                 * 
                                 * valid status is
                                    notStarted
                                    finishedTimeOk
                                    disqualified
                                    finished
                                    movedUp
                                    walkOver
                                    started
                                    passed
                                    notValid
                                    notActivated
                                 */
                                //EMMAClient.RunnerStatus rstatus = EMMAClient.RunnerStatus.Passed;
                                int rstatus = 0;
                                switch (status)
                                {
                                    case "started":
                                        rstatus = 9;
                                        break;
                                    case "notActivated":
                                        rstatus = 10;
                                        //rstatus = EMMAClient.RunnerStatus.NotStartedYet;
                                        break;
                                    case "notStarted":
                                        rstatus = 1;
                                        //rstatus = EMMAClient.RunnerStatus.NotStarted;
                                        break;
                                    case "disqualified":
                                        rstatus = 4;
                                        break;
                                    case "notValid":
                                        rstatus = 3;
                                        //rstatus = EMMAClient.RunnerStatus.MissingPunch;
                                        break;
                                    case "walkOver":
                                        rstatus = 11;
                                        //rstatus = EMMAClient.RunnerStatus.WalkOver;
                                        break;
                                    case "movedUp":
                                        rstatus = 12;
                                        //rstatus = EMMAClient.RunnerStatus.MovedUp;
                                        break;
                                }
                                if (rstatus != 9 && rstatus != 10)
                                    FireOnResult(runnerID, 0, fName + " " + famName, club, classN, 0, time, rstatus, new List<ResultStruct>());

                                /*if (!client.IsClassAdded(classN))
                                {
                                    client.AddClass(classN);
                                }
                                if (!client.IsRunnerAdded(runnerID))
                                {
                                    client.AddRunner(runnerID, fName + " " + famName, club, classN);
                                }
                                if (client.HasResultChanged(runnerID, time, rstatus, 1000))
                                {
                                    client.SetResult(runnerID, time, rstatus, 1000);
                                }*/

                                //logit(status + "," +time + ", " + position + "," + famName + "," + fName + "," + club + "," + classN);
                            }
                            reader.Close();

                            reader = cmdSplits.ExecuteReader();
                            while (reader.Read())
                            {
                                //splittimes.modifyDate, splittimes.passedTime, splittimes.splitTimeControlID , results.entryid 
                                DateTime mod = reader.GetDateTime(0);
                                lastSplitDateTime = (mod > lastSplitDateTime ? mod : lastSplitDateTime);
                                DateTime pTime = reader.GetDateTime(1);
                                int sCont = reader.GetInt32(2);
                                int entryid = reader.GetInt32(3);
                                DateTime startTime = reader.GetDateTime(4);
                                int passedCount = reader.GetInt32(reader.GetOrdinal("passedCount"));
                                TimeSpan rTid = pTime - startTime;
                                double time = rTid.TotalMilliseconds / 10;
                                List<ResultStruct> times = new List<ResultStruct>();
                                ResultStruct t = new ResultStruct();
                                t.ControlCode = sCont + 1000*passedCount;
                                t.ControlNo = 0;
                                t.Time = (int)time;
                                times.Add(t);
                                string name = reader.GetString(6) + " " + reader.GetString(5);
                                string club = reader.GetString(7);
                                string classn = reader.GetString(8);
                                FireOnResult(entryid, 0,name,club,classn, 0, -2, 0, times);
                            }
                            reader.Close();

                            System.Threading.Thread.Sleep(1000);
                        }
                        catch (Exception ee)
                        {
                            if (reader != null)
                                reader.Close();
                            FireLogMsg("OLA Parser: " + ee.Message + " {parsing: " + lastRunner);
                        }
                    }
                }
                catch (Exception ee)
                {
                    FireLogMsg("OLA Parser: " +ee.Message);
                }
                finally
                {
                    if (m_Connection != null)
                    {
                        m_Connection.Close();
                    }
                    FireLogMsg("Disconnected");
                    FireLogMsg("OLA Monitor thread stopped");

                }
            }
        }
    }
}
