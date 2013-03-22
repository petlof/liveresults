using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace LiveResults.Client
{
    [Obsolete("The SITimec parser is not maintained, probably won't work either but kept here for historic reasons")]
    public class SiTimecParser
    {
        private OleDbConnection m_Connection;

        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;

        private bool m_Continue = false;
        public SiTimecParser(OleDbConnection conn)
        {
            m_Connection = conn;
        }

        //private void FireOnResult(int id, int SI, string name, string club, string Class, int start, int time, int status, List<ResultStruct> results)
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
                    string baseCommand = "select CompetitorRaces.RowVersion,CompetitorRaces.Time, CompetitorRaces.Status, CompetitorRaces.CompetitorID, Competitors.FirstName, Competitors.LastName, Clubs.Name as ClubName, Classes.Name as ClassName from CompetitorRaces, Competitors, Clubs, CompetitionEntries,Classes where CompetitorRaces.CompetitionEntryId = CompetitionEntries.CompetitionEntryId and CompetitionEntries.ClassID = Classes.ClassID and CompetitorRaces.CompetitorId = Competitors.CompetitorId and  Competitors.ClubId = Clubs.ClubId and CompetitorRaces.RowVersion> ? order by CompetitorRaces.RowVersion";
                    //string splitbaseCommand = "select splittimes.modifyDate, splittimes.passedTime, Controls.ID, results.entryid, results.allocatedStartTime, persons.familyname as lastname, persons.firstname as firstname, clubs.name as clubname, eventclasses.shortName, splittimes.passedCount from splittimes, results, SplitTimeControls, Controls, eventClasses, raceClasses, Persons, Clubs, entries where splittimes.resultraceindividualnumber = results.resultid and SplitTimes.splitTimeControlID = SplitTimeControls.splitTimeControlID and SplitTimeControls.timingControl = Controls.controlid and Controls.eventRaceId = " + m_EventRaceId + " and raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_EventRaceId + " and eventclasses.eventid = " + m_EventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.clubid = clubs.clubid and splitTimes.modifyDate > ?";
                    OleDbCommand cmd = new OleDbCommand(baseCommand, m_Connection);

                    cmd.CommandText = "select Value from CompetitionProperties where Name = 'TimeAccurancy'";
                    int timeAccurancy = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.CommandText = baseCommand;
                    //OleDbCommand cmdSplits = new OleDbCommand(splitbaseCommand, m_Connection);
                    OleDbParameter param = new OleDbParameter("@date", OleDbType.Binary);
                    //OleDbParameter splitparam = new OleDbParameter("@date", DateTime.Now);
                    //param.DbType = DbType.DateTime;
                    //splitparam.DbType = DbType.DateTime;

                    //param.OleDbType = OleDbType.DBTimeStamp;
                    //DateTime lastDateTime = DateTime.MinValue;
                    object lastRowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                    //DateTime lastSplitDateTime = DateTime.Now.AddMonths(-120);
                    param.Value = lastRowVersion;
                    //splitparam.Value = lastSplitDateTime;

                    cmd.Parameters.Add(param);
                    //cmdSplits.Parameters.Add(splitparam);

                    FireLogMsg("SiTimec Monitor thread started");
                    while (m_Continue)
                    {
                        OleDbDataReader reader = null;
                        string lastRunner = "";
                        try
                        {
                            /*Kontrollera om nya klasser*/
                            /*Kontrollera om nya resultat*/
                            //cmd.CommandText = baseCommand + "" + lastDateTime.ToString("yyyyMMddhhmmss");
                            cmd.Parameters["@date"].Value = lastRowVersion; //lastDateTime == DateTime.MinValue ? (object)DBNull.Value : (object)lastDateTime;
                            //cmdSplits.Parameters["@date"].Value = lastSplitDateTime;

                            string command = cmd.CommandText;
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                DateTime modDate = DateTime.MinValue;
                                int time = 0, position = 0, runnerID = 0;
                                string famName = "", fName = "", club = "", classN = "";
                                int status = -1;
                                try
                                {

                                    object rowVersion = reader["RowVersion"];
                                    lastRowVersion = rowVersion;
                                    runnerID = Convert.ToInt32(reader["CompetitorID"]);
                                    time = -9;
                                    //SITimec seems to store times as 1000*numseconds
                                    if (reader["Time"] != DBNull.Value)
                                        time = Convert.ToInt32(reader["Time"]) / 10;
                                    famName = reader["LastName"] as string;
                                    fName = reader["FirstName"] as string;
                                    lastRunner = fName + " " + famName;
                                    club = reader["ClubName"] as string;
                                    classN = reader["ClassName"] as string;
                                    status = Convert.ToInt32(reader["Status"]);
                                    
                                }
                                catch (Exception ee)
                                {
                                    FireLogMsg(ee.Message);
                                }

                                int rstatus = 0;
                                switch (status)
                                {
                                    case 0: //"notActivated":
                                        rstatus = 10;
                                        //rstatus = EMMAClient.RunnerStatus.NotStartedYet;
                                        break;
                                    case 100: //OK
                                        rstatus = 0;
                                        break;
                                    case 1:
                                        rstatus = 1;
                                        break;
                                    case 11:
                                    case 6:
                                        rstatus = 3;
                                        break;
                                    case 5:
                                        rstatus = 3;
                                        break;
                                    case 7:
                                        rstatus = 4;
                                        break;
                                    case 9:
                                        rstatus = 11;
                                        break;
                                    case 8:
                                        //utom tävlan;
                                        rstatus = 9;
                                        break;
                                    default:
                                        rstatus = 10;
                                        break;


                                }
                                if (rstatus != 9 && rstatus != 10)
                                    FireOnResult(new Result()
                                    {
                                        ID = runnerID,
                                        RunnerName = fName + " " + famName,
                                        RunnerClub = club,
                                        Class = classN,
                                        StartTime = 0,
                                        Time = time,
                                        Status = rstatus,
                                        SplitTimes = new List<ResultStruct>()
                                    });
                            }
                            reader.Close();

                            System.Threading.Thread.Sleep(1000);
                        }
                        catch (Exception ee)
                        {
                            if (reader != null)
                                reader.Close();
                            FireLogMsg("SiTimec Parser: " + ee.Message + " {parsing: " + lastRunner);
                        }
                    }
                }
                catch (Exception ee)
                {
                    FireLogMsg("SiTimec Parser: " +ee.Message);
                }
                finally
                {
                    if (m_Connection != null)
                    {
                        m_Connection.Close();
                    }
                    FireLogMsg("Disconnected");
                    FireLogMsg("SiTimec Monitor thread stopped");

                }
            }
        }
    }
}
