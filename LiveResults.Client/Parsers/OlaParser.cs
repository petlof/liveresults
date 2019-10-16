using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using LiveResults.Client.Model;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using LiveResults.Model;
namespace LiveResults.Client
{
    
    public class OlaParser : IExternalSystemResultParser
    {
        private readonly IDbConnection m_connection;
        private readonly int m_eventID;
        private readonly int m_eventRaceId;
        private readonly bool m_recreateRadioControls;
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;

        private bool m_continue;
        
        public OlaParser(IDbConnection conn, int eventID, int eventRaceId, bool recreateRadioControls = true)
        {
            m_connection = conn;
            m_eventID = eventID;
            m_eventRaceId = eventRaceId;
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
            string splitsPaths = ConfigurationManager.AppSettings["splitspath"];
            while (m_continue)
            {
                try
                {
                    if (m_connection.State != ConnectionState.Open)
                    {
                        if (m_connection is System.Data.H2.H2Connection)
                        {
                            (m_connection as System.Data.H2.H2Connection).Open("root", "");
                        }
                        else
                        {
                            m_connection.Open();
                        }
                    }

                    string paramOper = "?";
                    if (m_connection is MySql.Data.MySqlClient.MySqlConnection)
                    {
                        paramOper = "?date";
                    }

                    /*Detect eventtype*/

                    string scmd = "select eventForm from events where eventid = " + m_eventID;
                    IDbCommand cmd = m_connection.CreateCommand();
                    cmd.CommandText = scmd;

                    var form = cmd.ExecuteScalar() as string;
                    bool isRelay = form.ToLower().Contains("relay");

                    /* detect ola version*/
                    scmd = "select versionNumber from Version";
                    cmd = m_connection.CreateCommand();
                    cmd.CommandText = scmd;

                    int version = Convert.ToInt32(cmd.ExecuteScalar());

                    string baseCommand = "select results.bibNumber, results.individualCourseId, results.rawDataFromElectronicPunchingCardsId, results.modifyDate, results.totalTime, results.position, persons.familyname as lastname, persons.firstname as firstname, organisations.shortname as clubname, eventclasses.shortName, results.runnerStatus, results.entryid, results.allocatedStartTime, results.starttime, entries.allocationControl, entries.allocationEntryId from results, entries, Persons, organisations, raceclasses,eventclasses where raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_eventRaceId + " and eventclasses.eventid = " + m_eventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.defaultorganisationid = organisations.organisationid and raceClasses.raceClassStatus <> 'notUsed' and results.modifyDate > " + paramOper;
                    string splitbaseCommand = "select splittimes.modifyDate, splittimes.passedTime, Controls.ID, results.entryid, results.allocatedStartTime, results.starttime, persons.familyname as lastname, persons.firstname as firstname, organisations.shortname as clubname, eventclasses.shortName, splittimes.passedCount,entries.allocationControl, entries.allocationEntryId from splittimes, results, SplitTimeControls, Controls, eventClasses, raceClasses, Persons, organisations, entries where splittimes.resultraceindividualnumber = results.resultid and SplitTimes.splitTimeControlID = SplitTimeControls.splitTimeControlID and SplitTimeControls.timingControl = Controls.controlid and Controls.eventRaceId = " + m_eventRaceId + " and raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_eventRaceId + " and eventclasses.eventid = " + m_eventID + " and results.entryid = entries.entryid and entries.competitorid = persons.personid and persons.defaultorganisationid = organisations.organisationid and raceClasses.raceClassStatus <> 'notUsed' and  splitTimes.modifyDate > " + paramOper;

                    RelayEventCache relayEventCache = null;
                    

                    if (isRelay)
                    {
                        relayEventCache = new RelayEventCache{
                            Configuration = new EventConfiguration()
                        };
                        relayEventCache.OnLogMessage += FireLogMsg;
                        relayEventCache.OnResultChanged += FireOnResult;

                        /*Detect changeover-types for all classes/legs*/
                        cmd.CommandText =
                            "select ec.shortName, rc.relayLeg, rc.allocationMethod from raceclasses rc, eventClasses ec where ec.eventclassId = rc.eventClassId and rc.eventRaceId=" +
                            m_eventRaceId;

                        using (var classReader = cmd.ExecuteReader())
                        {
                            while (classReader.Read())
                            {
                                relayEventCache.Configuration.AddClassLeg(classReader["shortName"].ToString(),
                                    Convert.ToInt32(classReader["relayLeg"].ToString()),
                                    String.Compare(classReader["allocationMethod"].ToString(), "relayLastRunner", StringComparison.OrdinalIgnoreCase) == 0
                                        ? SummaryMethod.LastRunnerOnPreviousLeg
                                        : SummaryMethod.FirstRunnerOnPreviousLeg);

                            }
                            classReader.Close();
                        }


                        baseCommand = "select results.bibNumber, results.individualCourseId, results.rawDataFromElectronicPunchingCardsId, results.modifyDate,results.totalTime, results.position, persons.familyname as lastname, persons.firstname as firstname, entries.teamName as clubname, eventclasses.shortName, raceclasses.relayleg, results.runnerStatus, results.resultId as entryId, results.finishTime, results.allocatedStartTime, results.starttime from results, entries, Persons, raceclasses,eventclasses where raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_eventRaceId + " and eventclasses.eventid = " + m_eventID + " and results.entryid = entries.entryid and results.relaypersonid = persons.personid and raceClasses.raceClassStatus <> 'notUsed' and  results.modifyDate > " + paramOper + " order by relayLeg";
                        splitbaseCommand = "select splittimes.modifyDate, splittimes.passedTime, Controls.ID, results.resultId as entryId, results.allocatedStartTime, persons.familyname as lastname, persons.firstname as firstname, entries.teamName as clubname, eventclasses.shortName,raceclasses.relayleg, splittimes.passedCount,results.allocatedStartTime, results.starttime from splittimes, results, SplitTimeControls, Controls, eventClasses, raceClasses, Persons, entries where splittimes.resultraceindividualnumber = results.resultid and SplitTimes.splitTimeControlID = SplitTimeControls.splitTimeControlID and SplitTimeControls.timingControl = Controls.controlid and Controls.eventRaceId = " + m_eventRaceId + " and raceclasses.eventClassID = eventClasses.eventClassID and results.raceClassID = raceclasses.raceclassid and raceClasses.eventRaceId = " + m_eventRaceId + " and eventclasses.eventid = " + m_eventID + " and results.entryid = entries.entryid and results.relaypersonid = persons.personid and raceClasses.raceClassStatus <> 'notUsed' and splitTimes.modifyDate > " + paramOper;
                    }

                    if (m_recreateRadioControls)
                    {
                        ReadRadioControls();
                    }

                    cmd.CommandText = baseCommand;
                    IDbCommand cmdSplits = m_connection.CreateCommand();
                    IDbCommand cmdSplitTimes = m_connection.CreateCommand();
                    cmdSplits.CommandText = splitbaseCommand;
                    IDbDataParameter param = cmd.CreateParameter();
                    param.ParameterName = "date";
                    if (m_connection is MySql.Data.MySqlClient.MySqlConnection || m_connection is System.Data.H2.H2Connection)
                    {
                        param.DbType = DbType.String;
                        param.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        param.DbType = DbType.DateTime;
                        param.Value = DateTime.Now;
                    }
                    

                    IDbDataParameter splitparam = cmdSplits.CreateParameter();
                    splitparam.ParameterName = "date";
                    if (m_connection is MySql.Data.MySqlClient.MySqlConnection || m_connection is System.Data.H2.H2Connection)
                    {
                        splitparam.DbType = DbType.String;
                        splitparam.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        splitparam.DbType = DbType.DateTime;
                        splitparam.Value = DateTime.Now;
                    }
                   
                    DateTime lastDateTime = DateTime.Now.AddMonths(-120);
                    DateTime lastSplitDateTime = DateTime.Now.AddMonths(-120);
                    param.Value = lastDateTime;
                    splitparam.Value = lastSplitDateTime;

                    cmd.Parameters.Add(param);
                    cmdSplits.Parameters.Add(splitparam);

                    FireLogMsg("OLA Monitor thread started");
                    IDataReader reader = null;
                    var runnerPairs = new Dictionary<int, RunnerPair>();
                    while (m_continue)
                    {
                        string lastRunner = "";
                        try
                        {
                            /*Kontrollera om nya klasser*/
                            /*Kontrollera om nya resultat*/
                            if (cmd is MySql.Data.MySqlClient.MySqlCommand || m_connection is System.Data.H2.H2Connection)
                            {
                                (cmd.Parameters["date"] as IDbDataParameter).Value = lastDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                (cmdSplits.Parameters["date"] as IDbDataParameter).Value = lastSplitDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else
                            {
                                (cmd.Parameters["date"] as IDbDataParameter).Value = lastDateTime;
                                (cmdSplits.Parameters["date"] as IDbDataParameter).Value = lastSplitDateTime;
                            }

                            List<object[]> splitsToRead = null;
                            if (!string.IsNullOrEmpty(splitsPaths))
                             splitsToRead = new List<object[]>();

                            cmd.Prepare();
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                int time = 0, runnerID = 0, iStartTime = 0;
                                string famName = "", fName = "", club = "", classN = "", status = "";
                                
                                try
                                {

                                    DateTime modDate = GetDbDateTime(reader, "modifyDate");
                                    lastDateTime = (modDate > lastDateTime ? modDate : lastDateTime);
                                    runnerID = Convert.ToInt32(reader["entryid"].ToString());

                                    time = -9;
                                    if (reader["totaltime"] != null && reader["totaltime"] != DBNull.Value)
                                        time = Convert.ToInt32(reader["totalTime"].ToString());

                                    famName = (reader["lastname"] as string);
                                    fName = (reader["firstname"] as string);

                                    lastRunner = (string.IsNullOrEmpty(fName) ? "" : (fName + " ")) + famName;

                                    club = (reader["clubname"] as string);
                                    classN = (reader["shortname"] as string);
                                    status = reader["runnerStatus"] as string; // reader.GetString(7);

                                    DateTime startTime = DateTime.MinValue;

                                    if (reader["allocatedStartTime"] != null && reader["allocatedStartTime"] != DBNull.Value)
                                    {
                                        startTime = GetDbDateTime(reader, "allocatedStartTime");
                                    }
                                    if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                                    {
                                        startTime = GetDbDateTime(reader, "starttime");
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


                                /*
                                    time is seconds * 100
                                 * 
                                 * valid status is
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
                                    case "notParticipating":
                                        rstatus = 999;
                                        break;
                                    case "walkOver":
                                        rstatus = 11;
                                        //rstatus = EMMAClient.RunnerStatus.WalkOver;
                                        break;
                                    case "movedUp":
                                        rstatus = 12;
                                        break;
                                }
                                if (rstatus != 999)
                                {

                                    if (isRelay)
                                    {
                                        relayEventCache.SetTeamLegResult(runnerID, classN, club, fName + " " + famName, Convert.ToInt32(reader["relayLeg"].ToString()),
                                            iStartTime, time, rstatus);
                                    }
                                    else
                                    {


                                        var res = new Result{
                                            ID = runnerID,
                                            RunnerName = fName + " " + famName,
                                            RunnerClub = club,
                                            Class = classN,
                                            StartTime = iStartTime,
                                            Time = time,
                                            Status = rstatus

                                        };

                                        CheckAndCreatePairRunner(isRelay, reader, runnerPairs, runnerID, res);
                                    }
                                }
                            }
                            reader.Close();
                            #region readsplits
                            if (splitsToRead != null && splitsToRead.Count > 0)
                            {
                                while (splitsToRead.Count > 0)
                                {
                                    object[] toRead = splitsToRead[0];
                                    splitsToRead.RemoveAt(0);
                                    int bibNumber = (int)toRead[0];
                                    int courseId = (int)toRead[1];
                                    int rawDataId = (int)toRead[2];
                                    DateTime startTime = (DateTime)toRead[3];
                                    int time = (int)toRead[4];

                                    cmdSplitTimes.CommandText = @"select ordered, punchingCode from punchingUnits pu, controlspunchingunits cpu, coursesWayPointControls cwp where
 cwp.controlId = cpu.control and cpu.punchingUnit=pu.punchingUnitId and cwp.courseId=" + courseId + " order by ordered";

                                    List<int[]> courseWayPointCodes = new List<int[]>();
                                    using (var splreader = cmdSplitTimes.ExecuteReader())
                                    {
                                        int lastOrder = -1;
                                        while (splreader.Read())
                                        {
                                            int code = Convert.ToInt32(splreader["punchingCode"]);
                                            int order = Convert.ToInt32(splreader["ordered"]);
                                            if (courseWayPointCodes.Count == 0 || order > lastOrder)
                                            {
                                                courseWayPointCodes.Add(new int[] { code });
                                            }
                                            else
                                            {
                                                var l = new List<int>(courseWayPointCodes[courseWayPointCodes.Count-1]);
                                                l.Add( code);
                                                courseWayPointCodes[courseWayPointCodes.Count - 1] = l.ToArray();
                                            }
                                        }
                                        splreader.Close();
                                    }

                                    DateTime cardReadTime = DateTime.MinValue;
                                    cmdSplitTimes.CommandText = @"select readInTime from rawdatafromelectronicpunchingcards where Id=" + rawDataId;
                                    using (var splreader = cmdSplitTimes.ExecuteReader())
                                    {
                                        int lastOrder = -1;
                                        if (splreader.Read())
                                        {
                                            cardReadTime = Convert.ToDateTime(splreader["readInTime"]);
                                        }
                                        splreader.Close();
                                    }

                                    cmdSplitTimes.CommandText = @"select punchingCode, punchingTime from rawpunches where rawCardId=" + rawDataId + " order by controlNUmber";
                                    List<int[]> punches = new List<int[]>();
                                    using (var splreader = cmdSplitTimes.ExecuteReader())
                                    {
                                        while (splreader.Read())
                                        {
                                            punches.Add(new int[] { Convert.ToInt32(splreader["punchingCode"]), Convert.ToInt32(splreader["punchingTime"]) });
                                        }
                                        splreader.Close();
                                    }


                                    int curIdx = punches.Count - 1;
                                    int timeAtReadOut = -1;
                                    while (curIdx > 0)
                                    {
                                        if (punches[curIdx][0] >= 250 && punches[curIdx][0] <= 254)
                                        {
                                            timeAtReadOut = punches[curIdx][1];
                                            break;
                                        }
                                        curIdx--;
                                    }

                                    DateTime[] splits = new DateTime[courseWayPointCodes.Count];
                                    
                                    int wayPointIx = courseWayPointCodes.Count - 1;
                                    if (timeAtReadOut > 0)
                                    {
                                        curIdx--;
                                        while (curIdx >= 0 && wayPointIx >= 0)
                                        {
                                            int punch = punches[curIdx][0];
                                            if (Array.IndexOf<int>(courseWayPointCodes[wayPointIx], punch) >= 0)
                                            {
                                                DateTime punchTime = cardReadTime.AddSeconds(-1*(timeAtReadOut - punches[curIdx][1]));
                                                //splits.Add(new object[] { punches[curIdx]);
                                                splits[wayPointIx] = punchTime;
                                                wayPointIx--;
                                            }
                                            curIdx--;
                                        }
                                    }

                                    using (var xml = XmlWriter.Create(Path.Combine(splitsPaths, bibNumber + ".xml"), new XmlWriterSettings() { Indent = true }))
                                    {
                                        xml.WriteStartDocument();
                                        xml.WriteStartElement("splits");
                                        xml.WriteAttributeString("bibNumber",bibNumber.ToString());
                                        xml.WriteAttributeString("startTime", startTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                        xml.WriteAttributeString("totalTimeSeconds", time.ToString());
                                        for (int i = 0; i < splits.Length; i++)
                                        {
                                            xml.WriteStartElement("split");
                                            xml.WriteAttributeString("control", (i+1).ToString());
                                            xml.WriteAttributeString("punchTime", splits[i].ToString("yyyy-MM-dd HH:mm:ss"));
                                            xml.WriteAttributeString("splitTimeSeconds", ((int)(i > 0 ? (splits[i] - splits[i - 1]).TotalSeconds : 0)).ToString());
                                            xml.WriteEndElement();
                                        }
                                        xml.WriteEndElement();
                                    }
                                }
                            }
                            #endregion

                            reader = cmdSplits.ExecuteReader();
                            while (reader.Read())
                            {
                                try
                                {
                                    DateTime mod = GetDbDateTime(reader, "modifyDate");

                                    lastSplitDateTime = (mod > lastSplitDateTime ? mod : lastSplitDateTime);

                                    string tTime = Convert.ToString(reader[1]);
                                    DateTime pTime = GetDbDateTime(reader, "passedTime");
                                    int sCont = reader.GetInt32(2);
                                    int entryid = Convert.ToInt32(reader["entryid"].ToString());
                                    DateTime startTime;

                                    if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                                    {
                                        startTime = GetDbDateTime(reader, "starttime");
                                    }
                                    else if (reader["allocatedStartTime"] != null && reader["allocatedStartTime"] != DBNull.Value)
                                    {
                                        startTime = GetDbDateTime(reader, "allocatedStartTime");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    int passedCount = Convert.ToInt32(reader["passedCount"].ToString());

                                    TimeSpan rTid = pTime - startTime;

                                    double time = rTid.TotalMilliseconds / 10;
                                    var times = new List<ResultStruct>();
                                    var t = new ResultStruct{
                                        ControlCode = sCont + 1000*passedCount,
                                        ControlNo = 0,
                                        Time = (int) time
                                    };
                                    times.Add(t);

                                    var sfamName = reader["lastname"] as string;
                                    var sfName = reader["firstname"] as string;
                                    string name = (string.IsNullOrEmpty(sfName) ? "" : (sfName + " ")) + sfamName;

                                    var club = reader["clubname"] as string; 
                                    var classn = reader["shortname"] as string;

                                    if (isRelay)
                                    {
                                        relayEventCache.SetTeamLegSplitResult(entryid, classn, club, name, Convert.ToInt32(reader["relayLeg"].ToString()), (int)startTime.TimeOfDay.TotalSeconds * 100, sCont, (int)time, passedCount);
                                    }
                                    else
                                    {
                                        var res = new Result
                                        {
                                            ID = entryid,
                                            RunnerName = name,
                                            RunnerClub = club,
                                            Class = classn,
                                            StartTime = 0,
                                            Time = -2,
                                            Status = 0,
                                            SplitTimes = times
                                        };
                                        CheckAndCreatePairRunner(isRelay, reader, runnerPairs, entryid, res);
                                    }
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
                            FireLogMsg("OLA Parser: " + ee.Message + " {parsing: " + lastRunner);

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
                    FireLogMsg("OLA Parser: " +ee.Message);
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
                cmd.CommandText =
                    @"select stc.name, c.ID, ec.shortName as className, rc.relayLeg,rcsttc.ordered from 
                            raceclasssplittimecontrols rcsttc, controls c, 
                            splittimecontrols stc, raceClasses rc, eventClasses ec
                WHERE rcsttc.splittimecontrolId = stc.splitTimeControlId AND
                        stc.timingControl = c.controlId AND
                        rcsttc.raceClassId = rc.raceClassId AND 
                        rc.eventClassId = ec.eventClassId AND
                        rc.eventRaceId=" + m_eventRaceId + @" AND 
                        ec.eventId = " + m_eventID + @" 
                ORDER BY rcsttc.ordered";
                using (var reader = cmd.ExecuteReader())
                {
                    var dlg = OnRadioControl;
                    var passCount = new Dictionary<string, int>();
                    while (reader.Read())
                    {
                        var name = reader["name"] as string;
                        
                        int code = Convert.ToInt32(reader["ID"].ToString());
                        
                        var className = reader["className"] as string;

                        if (reader["relayLeg"] != null && reader["relayLeg"] != DBNull.Value)
                            className += "-" + reader["relayLeg"].ToString();

                        int order = Convert.ToInt32(reader["ordered"].ToString());

                        string key = className + "-" + code;
                        if (!passCount.ContainsKey(key))
                            passCount.Add(key, 0);

                        int passing = passCount[key] + 1;
                        passCount[key]++;

                        if (passing > 1)
                            name += " (" + passing + ")";

                        int rcode = 1000*passing + code;

                        if (dlg != null)
                            dlg(name, rcode, className, order);

                    }
                    reader.Close();
                }
                


            }
        }

        private void CheckAndCreatePairRunner(bool isRelay, IDataReader reader, Dictionary<int, RunnerPair> runnerPairs, int runnerID, Result res)
        {
            // is this a pair-runner?
            if (!isRelay && reader["allocationControl"] != null && reader["allocationControl"] as string == "groupedWithRef" && reader["allocationEntryId"] != DBNull.Value)
            {
                int otherRunnerId = Convert.ToInt32(reader["allocationEntryId"].ToString());
                RunnerPair rp;

                if (runnerPairs.ContainsKey(runnerID))
                {
                    rp = runnerPairs[runnerID];

                }
                else if (runnerPairs.ContainsKey(otherRunnerId))
                {
                    rp = runnerPairs[otherRunnerId];
                    rp.Runner2 = res;
                    runnerPairs.Add(runnerID, rp);
                }
                else
                {
                    rp = new RunnerPair{
                        Runner1 = res
                    };
                    runnerPairs.Add(runnerID, rp);
                }

                if (rp.Runner1 != null && rp.Runner1.ID == res.ID)
                {
                    rp.Runner1 = res;
                }
                else if (rp.Runner2 != null && rp.Runner2.ID == res.ID)
                {
                    rp.Runner2 = res;
                }

                var comb = rp.CombinedRunner;
                if (comb != null && comb.Status != 999)
                    FireOnResult(comb);

            }
            else
            {
                FireOnResult(res);
            }
        }

        private DateTime GetDbDateTime(IDataReader reader, string columnName)
        {
            DateTime ret = DateTime.MinValue;
            object value = reader[columnName];
            if (value != null && value != DBNull.Value)
            {
                if (m_connection is System.Data.H2.H2Connection)
                {
                    string dateTimeString = value.ToString();
                    ret = ParseDateTime(dateTimeString);
                }
                else
                {
                    ret = reader.GetDateTime(reader.GetOrdinal(columnName));
                }
            }
            return ret;
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
