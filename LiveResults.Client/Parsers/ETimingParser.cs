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
    
    public class ETimingParser : IExternalSystemResultParser
    {
        private readonly IDbConnection m_connection;
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;
        private bool m_createRadioControls;
        private bool m_continue;
        private int  m_sleepTime;
        private bool m_oneLineRelayRes;
        private bool m_MSSQL;

        public ETimingParser(IDbConnection conn, int sleepTime, bool recreateRadioControls = true, bool oneLineRelayRes = false,bool MSSQL = false)
        {
            m_connection = conn;
            m_createRadioControls = recreateRadioControls;
            m_sleepTime = sleepTime;
            m_oneLineRelayRes = oneLineRelayRes;
            m_MSSQL = MSSQL;
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

        public struct SplitRawStruct
        {
            public int controlCode;
            public int passTime;
            public int netTime;
            public int changedTime;
        }
                
        public class RelayLegInfo
        {
            public string LegName;
            public string LegStatus;
            public int LegTime;
        }

        private class RelayTeam
        {
            public string ClassName;
            public string TeamName;
            public string TeamStatus;
            public int StartTime;
            public int TotalTime;
            public int TeamBib;
            public List<ResultStruct> SplitTimes;
            public Dictionary<int,RelayLegInfo> TeamMembers;
        }
        
        private class IntermediateTime
        {
            public string ClassName { get; set; }
            public string IntermediateName { get; set; }
            public int Position { get; set; }
            public int Order { get; set; }
        }

        public struct ClassStruct
        {
            public int Cource; 
            public string ClassName;
            public int NumLegs;
        };

        public struct RadioStruct
        {
            public int Code;
            public string Description;
            public int RadioType;
            public int Leg;
            public int Order;
        };

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

                    IDbCommand cmd       = m_connection.CreateCommand();
                    IDbCommand cmdInd    = m_connection.CreateCommand();
                    IDbCommand cmdRelay  = m_connection.CreateCommand();
                    IDbCommand cmdSplits = m_connection.CreateCommand();
                    IDbCommand cmdLastUpdateName  = m_connection.CreateCommand();
                    IDbCommand cmdLastUpdateSplit = m_connection.CreateCommand();
                    
                    /*Detect event type*/
                    cmd.CommandText = "SELECT kid FROM arr";
                    var reader = cmd.ExecuteReader();
                    bool isRelay = false;
                    while (reader.Read())
                    {
                        if (reader[0] != null && reader[0] != DBNull.Value)
                        {
                            int kid = Convert.ToInt16(reader[0]);
                            FireLogMsg("Event type is " + kid);
                            if (kid == 3)
                            {
                                isRelay = true;
                            }
                        }
                    }
                    reader.Close();


                    // *** Set up radiocontrols ***
                    /* ****************************
                     *  ordinary controls  = code + 1000*nc
                     *  relay controls     = code + 1000*nc + 10000*leg 
                     *  ordinary pass time = code + 1000*nc             + 100000
                     *  relay pass time    = code + 1000*nc + 10000*leg + 100000
                     *  change-over code   = 999  + 1000    + 10000*leg
                     * 
                     *  nc  = number of occurrence
                     *  leg = leg number
                     */
                     

                    if (m_createRadioControls)
                    {
                        List<IntermediateTime> intermediates = new List<IntermediateTime>();
                        var dlg = OnRadioControl;
                        if (dlg != null)
                        {
                            // Radio controltype, 4 = normal, 10 = exchange
                            cmd.CommandText = (@"SELECT code, radiocourceno, radiotype, description, etappe, radiorundenr FROM radiopost");
                            var RadioPosts = new Dictionary<int, List<RadioStruct>>();

                            using (reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int cource = 0, code = 0, radiotype = 0, leg = 0, order=0;

                                    if (reader["code"] != null && reader["code"] != DBNull.Value)
                                        code = Convert.ToInt32(reader["code"].ToString());

                                    if (code > 1000)
                                        code = code / 100; // Take away last to digits

                                    if (reader["radiocourceno"] != null && reader["radiocourceno"] != DBNull.Value)
                                        cource = Convert.ToInt32(reader["radiocourceno"].ToString());
                                   
                                    if (reader["radiotype"] != null && reader["radiotype"] != DBNull.Value)
                                        radiotype = Convert.ToInt32(reader["radiotype"].ToString());

                                    if (reader["etappe"] != null && reader["etappe"] != DBNull.Value)
                                        leg = Convert.ToInt32(reader["etappe"].ToString());

                                    if (reader["radiorundenr"] != null && reader["radiorundenr"] != DBNull.Value)
                                        order = Convert.ToInt32(reader["radiorundenr"].ToString());

                                    string description = reader["description"] as string;
                                    if (!string.IsNullOrEmpty(description))
                                        description = description.Trim();

                                    var radioControl = new RadioStruct
                                    {
                                        Code        = code,         
                                        Description = description,
                                        RadioType   = radiotype,
                                        Order       = order,
                                        Leg         = leg
                                    };

                                    if (!RadioPosts.ContainsKey(cource))
                                    {
                                        RadioPosts.Add(cource, new List<RadioStruct>());
                                    };
                                    RadioPosts[cource].Add(radioControl);
                                }
                                reader.Close();
                            }

                            // Class table
                            cmd.CommandText = @"SELECT code, cource, class, purmin, timingtype FROM class";
                            var classTable = new Dictionary<string,ClassStruct>();
                            
                            using (reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int cource = 0, numLegs = 0, timingType = 0;

                                    string classCode = reader["code"] as string;
                                    if (!string.IsNullOrEmpty(classCode))
                                        classCode = classCode.Trim();
                                    else
                                        continue;
                                   
                                    string className = reader["class"] as string;
                                    if (!string.IsNullOrEmpty(className))
                                        className = className.Trim();
                                    if (className == "NOCLAS") continue; // Skip if NOCLAS

                                    if (reader["cource"] != null && reader["cource"] != DBNull.Value)
                                        cource = Convert.ToInt32(reader["cource"].ToString());

                                    if (reader["purmin"] != null && reader["purmin"] != DBNull.Value)
                                        numLegs = Convert.ToInt32(reader["purmin"].ToString());

                                    if (reader["timingtype"] != null && reader["timingtype"] != DBNull.Value)
                                        timingType = Convert.ToInt32(reader["timingtype"].ToString());

                                    if (timingType == 1) // Add finish passing for Not-ranked class
                                    {
                                        intermediates.Add(new IntermediateTime
                                        {
                                            ClassName = className,
                                            IntermediateName = "Time",
                                            Position = 998,
                                            Order = 100
                                        });
                                    }

                                    if (RadioPosts.ContainsKey(cource))
                                    {
                                        Dictionary<int, int> radioCnt = new Dictionary<int, int>();
                                        foreach (var radioControl in RadioPosts[cource])
                                        {
                                            string classN = className; 
                                            int Code = radioControl.Code;
                                            int CodeforCnt = 0; // Code for counter
                                            int AddforLeg = 0;  // Addition for relay legs
                                            if (numLegs == 0 && (Code == 999 || Code == 0))
                                                continue;    // Skip if not relay and finish or start code
                                            if (radioControl.RadioType == 2) // Finish
                                                continue;
                                            if (numLegs > 0) // Relay
                                            {                                                
                                                if (!classN.EndsWith("-"))
                                                    classN += "-";
                                                classN += Convert.ToString(radioControl.Leg);
                                                AddforLeg = 10000 * radioControl.Leg;
                                            }

                                            CodeforCnt = Code + AddforLeg;
                                            if (!radioCnt.ContainsKey(CodeforCnt))
                                                radioCnt.Add(CodeforCnt, 0);
                                            radioCnt[CodeforCnt]++;

                                            if (Code < 999)
                                            {                                             
                                                // Add codes for ordinary classes and leg based classes
                                                intermediates.Add(new IntermediateTime
                                                {
                                                    ClassName = classN,
                                                    IntermediateName = radioControl.Description,
                                                    Position = Code + radioCnt[CodeforCnt] * 1000 + AddforLeg,
                                                    Order = radioControl.Order
                                                });
                                            }

                                            // Add codes for one-line relay classes
                                            if (numLegs > 0 && m_oneLineRelayRes)  
                                            {
                                                string classAll = className;
                                                if (!classAll.EndsWith("-"))
                                                    classAll += "-";
                                                classAll += "All";
                                                string Description = Convert.ToString(radioControl.Leg) +") "+ radioControl.Description;
                                                intermediates.Add(new IntermediateTime
                                                {
                                                    ClassName = classAll,
                                                    IntermediateName = Description,
                                                    Position = Code + radioCnt[CodeforCnt] * 1000 + AddforLeg,
                                                    Order = radioControl.Order
                                                });
                                            }
                                        }
                                    }
                                }
                                reader.Close();
                            }

                            foreach (var itime in intermediates)
                            {
                                dlg(itime.IntermediateName, itime.Position, itime.ClassName, itime.Order);
                            }

                        }
                    }

                    string baseCommandInd;
                    string baseCommandRelay; 
                    string baseSplitCommand;
                    string lastUpdateName;
                    string lastUpdateSplit;

                    string modulus; // integer_div, //integer_div = "/"; //integer_div = "\\";
                    if (m_MSSQL)
                        modulus = "%";
                    else
                        modulus = "MOD";
                    
                    baseCommandRelay = string.Format(@"SELECT N.timechanged, N.id, N.startno, N.ename, N.name, N.times, N.intime,
                            N.place, N.status, N.cource, N.starttime, N.ecard, N.ecard2,
                            T.name AS tname, C.class AS cclass, C.timingtype, C.freestart, C.cource AS ccource, 
                            C.firststart AS cfirststart, C.purmin AS cpurmin,
                            R.lgstartno, R.teamno, R.lgclass, R.lgtotaltime, R.lglegno, R.lgstatus, R.lgteam  
                            FROM Name N, Class C, Team T, Relay R
                            WHERE N.class=C.code AND T.code=R.lgteam AND N.rank=R.lgstartno AND (N.startno {0} 100)<=C.purmin 
                            ORDER BY N.startno", modulus);
                    //AND N.timechanged>? 

                    baseCommandInd = string.Format(@"SELECT N.timechanged, N.id, N.startno, N.ename, N.name, N.times, N.intime, 
                            N.place, N.status, N.cource, N.starttime, N.ecard, N.ecard2,
                            T.name AS tname, C.class AS cclass, C.timingtype, C.freestart, C.cource AS ccource
                            FROM Name N, Class C, Team T
                            WHERE N.class=C.code AND T.code=N.team AND (C.purmin IS NULL OR C.purmin<2) 
                            ");
                    //AND N.timechanged>? 

                    baseSplitCommand = string.Format(@"SELECT timechanged, mellomid, iplace, stasjon, mintime, nettotid, mecard 
                            FROM mellom 
                            WHERE stasjon>0 AND stasjon<240 AND iplace>100 AND mecard>0  
                            ORDER BY timechanged");

                    lastUpdateName  = string.Format(@"SELECT TOP 1 changed     FROM Name   ORDER BY changed     DESC"); 
                    lastUpdateSplit = string.Format(@"SELECT TOP 1 timechanged FROM mellom ORDER BY timechanged DESC");

                    Dictionary<int, List<SplitRawStruct>> splitList = null;

                    DateTime lastNameTime, lastNameTimePrev;
                    double lastSplitTime, lastSplitTimePrev;
                    
                    string lastRunner = "";

                    cmdLastUpdateName.CommandText = lastUpdateName;
                    cmdLastUpdateSplit.CommandText = lastUpdateSplit;

                    GetLastUpdates(cmdLastUpdateName, cmdLastUpdateSplit, out lastNameTimePrev, out lastSplitTimePrev);
                    //lastDatePrev = Math.Max(lastDateName, lastDateSplit);
                                       
                    cmdSplits.CommandText = baseSplitCommand;
                    IDbDataParameter paramSplit = cmd.CreateParameter();
                    paramSplit.ParameterName = "date";
                    paramSplit.Value = 0.0;
                    paramSplit.DbType = DbType.Double;
                    //cmdSplits.Parameters.Add(paramSplit);
                    ParseReaderSplits(cmdSplits, out splitList, out lastRunner);

                    cmdInd.CommandText = baseCommandInd;
                    IDbDataParameter paramInd = cmdInd.CreateParameter();
                    paramInd.ParameterName = "date";
                    paramInd.Value = 0.0;
                    paramInd.DbType = DbType.Double;
                    //cmdInd.Parameters.Add(paramInd);
                    ParseReader(cmdInd, splitList, false, out lastRunner);

                    if (isRelay)
                    {
                        cmdRelay.CommandText = baseCommandRelay;
                        IDbDataParameter paramRelay = cmdInd.CreateParameter();
                        paramRelay.ParameterName = "date";
                        paramRelay.Value = 0.0;
                        paramRelay.DbType = DbType.Double;
                        //cmdRelay.Parameters.Add(paramRelay);
                        ParseReader(cmdRelay, splitList, true, out lastRunner);
                    }

                    FireLogMsg("eTiming Monitor thread started");
                    while (m_continue)
                    {
                        try
                        {
                            if (true)
                            {
                                // Check for updates and parse if changes
                                GetLastUpdates(cmdLastUpdateName, cmdLastUpdateSplit, out lastNameTime, out lastSplitTime);
                                
                                if (lastNameTime > lastNameTimePrev || lastSplitTime > lastSplitTimePrev)
                                {
                                    lastNameTimePrev  = lastNameTime;
                                    lastSplitTimePrev = lastSplitTime;
                                    ParseReaderSplits(cmdSplits, out splitList, out lastRunner);
                                    ParseReader(cmdInd, splitList, false, out lastRunner);
                                    if (isRelay)
                                        ParseReader(cmdRelay, splitList, true, out lastRunner);
                                }
                            }
                            Thread.Sleep(1000*m_sleepTime);
                        }
                        catch (Exception ee)
                        {
                            FireLogMsg("eTiming Parser: " + ee.Message + " {parsing: " + lastRunner);
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
                    FireLogMsg("eTiming Parser: " +ee.Message);
                }
                finally
                {
                    if (m_connection != null)
                        m_connection.Close();                    
                    FireLogMsg("Disconnected");
                    FireLogMsg("eTiming Monitor thread stopped");
                }
            }
        }

        private void GetLastUpdates(IDbCommand cmdName, IDbCommand cmdSplit, out DateTime lastNameTime, out double lastSplitTime)
        {
            lastNameTime = DateTime.MinValue;
            using (IDataReader reader = cmdName.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        if (reader[0] != null && reader[0] != DBNull.Value)
                        {
                            lastNameTime = Convert.ToDateTime(reader[0]);
                        }
                    }
                    catch (Exception ee)
                    {
                        FireLogMsg("eTiming Parser: " + ee.Message);
                    }
                }
            }
            lastSplitTime = 0.0;
            using (IDataReader reader = cmdSplit.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        if (reader[0] != null && reader[0] != DBNull.Value)
                        {
                            lastSplitTime = Convert.ToDouble(reader[0]);
                        }
                    }
                    catch (Exception ee)
                    {
                        FireLogMsg("eTiming Parser: " + ee.Message);
                    }
                }
            }
        }
        

        private void ParseReader(IDbCommand cmd, Dictionary<int, List<SplitRawStruct>> splitList, bool isRelay, out string lastRunner)
        {
            lastRunner = "";

            Dictionary<int, RelayTeam> RelayTeams;
            RelayTeams = new Dictionary<int, RelayTeam>();

            using (IDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int time = 0, runnerID = 0, iStartTime = 0, iStartClass = 0,  bib = 0, teambib = 0, leg = 0, numlegs = 0, intime = -1, timingType = 0;
                    string famName = "", givName = "", club = "", classN = "", status = "", bibread = "", bibstr="", name = "", shortName="-";

                    var SplitTimes = new List<ResultStruct>();

                    try
                    {
                        runnerID = Convert.ToInt32(reader["id"].ToString());

                        status = reader["status"] as string;
                        if ((status == "V") || (status == "C")) // Skip if free or not entered  
                            continue;
                        
                        club = (reader["tname"] as string);
                        if (!string.IsNullOrEmpty(club))
                            club = club.Trim();

                        classN = (reader["cclass"] as string);
                        if (!string.IsNullOrEmpty(classN))
                            classN = classN.Trim();
                        if (classN == "NOCLAS") continue; // Skip runner if in NOCLAS

                        famName = (reader["ename"] as string);
                        if (!string.IsNullOrEmpty(famName))
                            famName = famName.Trim();                           

                        givName = (reader["name"] as string);
                        if (!string.IsNullOrEmpty(famName))
                            givName = givName.Trim();

                        bibread = (reader["startno"].ToString()).Trim();
                        bib = string.IsNullOrEmpty(bibread) ? 0 : Convert.ToInt32(bibread);

                        time = -2;
                        if (reader["times"] != null && reader["times"] != DBNull.Value)
                            time = GetRunTime((reader["times"].ToString()).Trim());

                        if (reader["timingtype"] != null && reader["timingtype"] != DBNull.Value)
                            timingType = Convert.ToInt32(reader["timingtype"].ToString());
                            // 0=normal, 1=not ranked, 2=not show times
                        
                        iStartTime = 0;
                        if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                            iStartTime = ConvertFromDay2cs(Convert.ToDouble(reader["starttime"]));

                        if (isRelay)
                        {
                            teambib = bib / 100; // Team bib no
                            bibstr = Convert.ToString(teambib);
                        }
                        else
                            bibstr = bibread;
                                                
                        name = ((bib > 0) ? "(" + bibstr + ") " : "") + givName + " " + famName;
                        lastRunner = name;

                        if (isRelay)
                        { //RelayTeams
                            leg = bib % 100;     // Leg number

                            if (!RelayTeams.ContainsKey(teambib))
                            {
                                RelayTeams.Add(teambib, new RelayTeam());
                                RelayTeams[teambib].TeamMembers = new Dictionary<int, RelayLegInfo>();
                                RelayTeams[teambib].SplitTimes = new List<ResultStruct>();
                            }                            
                            RelayTeams[teambib].TeamBib = teambib;
                            RelayTeams[teambib].ClassName = classN;
                            if (leg == 1)
                                RelayTeams[teambib].StartTime = iStartTime;

                            if (!(RelayTeams[teambib].TeamMembers).ContainsKey(leg))
                                RelayTeams[teambib].TeamMembers.Add(leg, new RelayLegInfo());

                            if (givName != null)
                            {
                                if (givName.Length == 0) givName = "-";
                                shortName = givName[0] + "." + famName;
                            }

                            RelayTeams[teambib].TeamMembers[leg].LegName = shortName;
                            
                            if (!classN.EndsWith("-"))
                                classN += "-";
                            classN += Convert.ToString(leg);
                            bibstr = Convert.ToString(teambib);

                            if (reader["teamno"] != null && reader["teamno"] != DBNull.Value)
                                club += "-" + Convert.ToString(reader["teamno"]);
                            RelayTeams[teambib].TeamName = club;

                            if (reader["cfirststart"] != null && reader["cfirststart"] != DBNull.Value)
                                iStartClass = ConvertFromDay2cs(Convert.ToDouble(reader["cfirststart"]));

                            if (reader["cpurmin"] != null && reader["cpurmin"] != DBNull.Value)
                                numlegs = Convert.ToInt16(reader["cpurmin"]);

                            if (reader["intime"] != null && reader["intime"] != DBNull.Value)
                                intime = ConvertFromDay2cs(Convert.ToDouble(reader["intime"]));

                            if (time > 0)
                                RelayTeams[teambib].TeamMembers[leg].LegTime = time;

                            RelayTeams[teambib].TeamMembers[leg].LegStatus = status;

                            int TeamTime = 0;
                            RelayTeams[teambib].TotalTime = -2;
                            string TeamStatus = "I";
                            bool TeamOK = true;
                            for (int legs = 1; legs <= leg; legs++)
                            {
                                if (RelayTeams[teambib].TeamMembers[legs].LegTime > 0)
                                {
                                    TeamTime += RelayTeams[teambib].TeamMembers[legs].LegTime;

                                    var SplitTime = new ResultStruct
                                    {
                                        ControlCode = 999 + 1000 + 10000 * legs,              // Note code 999 for change-over!
                                        Time = TeamTime
                                    };
                                    RelayTeams[teambib].SplitTimes.Add(SplitTime);

                                    var LegTime = new ResultStruct
                                    {
                                        ControlCode = 999 + 1000 + +10000 * legs + 100000,  // Note code 999 for change-over!
                                        Time = RelayTeams[teambib].TeamMembers[legs].LegTime
                                    };
                                    RelayTeams[teambib].SplitTimes.Add(LegTime);
                                }

                                // Accumulated status
                                if (TeamOK && (RelayTeams[teambib].TeamMembers[legs].LegStatus == "A"))
                                    TeamOK = true;
                                else
                                    TeamOK = false;
                                if (legs == numlegs && TeamOK)
                                {
                                    TeamStatus = "A";
                                    status = "A";
                                    RelayTeams[teambib].TotalTime = TeamTime;
                                    break;
                                }

                                if (RelayTeams[teambib].TeamMembers[legs].LegStatus == "D")
                                {
                                    TeamStatus = "D";
                                    status = "D";
                                }
                                else if (RelayTeams[teambib].TeamMembers[legs].LegStatus == "B")
                                {
                                    TeamStatus = "B";
                                    status = "B";
                                }
                                else if (RelayTeams[teambib].TeamMembers[legs].LegStatus == "N")
                                {
                                    if (legs == 1)
                                        TeamStatus = "N";
                                    else if (!(TeamStatus == "N"))
                                        TeamStatus = "B";
                                }

                            }
                            RelayTeams[teambib].TeamStatus = TeamStatus;

                            if (intime > 0)
                                time = Math.Max(intime - iStartClass, TeamTime);
                        }
                                                
                        bool freestart = Convert.ToBoolean(reader["freestart"].ToString());

                        // Add split times
                        int ecard1 = 0, ecard2 = 0;
                        if (reader["ecard"] != null && reader["ecard"] != DBNull.Value)
                            ecard1 = Convert.ToInt32(reader["ecard"].ToString());
                        if (reader["ecard2"] != null && reader["ecard2"] != DBNull.Value)
                            ecard2 = Convert.ToInt32(reader["ecard2"].ToString());

                        var splits = new List<SplitRawStruct>();
                        if (splitList.ContainsKey(ecard1))
                            splits = splitList[ecard1];
                        else if (splitList.ContainsKey(ecard2))
                            splits = splitList[ecard2];

                        var lsplitCodes = new List<int>();
                        int calcStartTime = -2;
                        foreach (var split in splits)
                        {
                            int passTime = -2;    // Total time at passing
                            int passLegTime = -2; // Time used on leg at passing
                            if (split.passTime < iStartTime) continue;   // Neglect passing before starttime
                            if (freestart)
                                passTime = split.netTime;
                            else if (isRelay)
                            {
                                passTime = split.passTime - iStartClass;   // Absolute pass time
                                if (leg > 1)                               // Leg based pass time
                                    passLegTime = split.passTime - Math.Max(iStartTime, iStartClass); // In case ind. start time not set
                            }
                            else
                                passTime = split.passTime - iStartTime;

                            int iSplitcode = split.controlCode + 1000;
                            while (lsplitCodes.Contains(iSplitcode))
                                iSplitcode += 1000;
                            lsplitCodes.Add(iSplitcode);

                            var SplitTime = new ResultStruct
                            {
                                ControlCode = iSplitcode + 10000 * leg,
                                Time = passTime
                            };
                            SplitTimes.Add(SplitTime);
                            if (isRelay)
                                RelayTeams[teambib].SplitTimes.Add(SplitTime);

                            if (passLegTime > 0)
                            {
                                var LegTime = new ResultStruct
                                {
                                    ControlCode = iSplitcode + 10000 * leg + 100000,
                                    Time = passLegTime
                                };
                                SplitTimes.Add(LegTime);
                                RelayTeams[teambib].SplitTimes.Add(LegTime);
                            }

                            if (freestart)
                                calcStartTime = split.changedTime - passTime;
                        }

                        if (freestart && (calcStartTime > 0) && (Math.Abs(calcStartTime - iStartTime) > 3000))  // Update starttime if deviation more than 30 sec
                            iStartTime = calcStartTime;

                        if (timingType == 1 || timingType == 2) // not ranked or not show times
                        {
                            if (timingType == 1 && time > 0)
                            {
                                var FinishTime = new ResultStruct
                                {
                                    ControlCode = 998, // Note code used for finish passing
                                    Time = time
                                };
                                SplitTimes.Add(FinishTime);
                            }
                            if (time > 0)
                                time = 1; // Set 1 as finish time
                        }
                    }
                    catch (Exception ee)
                    {
                        FireLogMsg("eTiming Parser: " + ee.Message);
                    }

                    int rstatus = GetStatusFromCode(ref time, status);
                    if (rstatus != 999)
                    {
                        var res = new Result
                        {
                            ID         = runnerID,
                            RunnerName = name,
                            RunnerClub = club,
                            Class      = classN,
                            StartTime  = iStartTime,
                            Time       = time,
                            Status     = rstatus,
                            SplitTimes = SplitTimes
                        };

                        FireOnResult(res);
                    }
                }
                reader.Close();

                // Loop through relay teams and add one-line results
                if (isRelay && m_oneLineRelayRes)
                {
                    foreach (var Team in RelayTeams)
                    {
                        int rstatus = GetStatusFromCode(ref Team.Value.TotalTime, Team.Value.TeamStatus);
                        if (rstatus != 999)
                        {

                            const int maxLength = 50; // Max lenght of one-line name
                            int numlegs   = Team.Value.TeamMembers.Count;
                            int legLength = maxLength / numlegs;
                            string name = "", classAll = "";
                            foreach (var Runner in Team.Value.TeamMembers)
                            {
                                int numChars = Math.Min(Runner.Value.LegName.Length, legLength);
                                if (Runner.Key == 1)
                                    name = "(" + Convert.ToString(Team.Value.TeamBib) + ") " + Runner.Value.LegName.Substring(0,numChars);
                                else
                                    name += ", " + Runner.Value.LegName.Substring(0, numChars);
                            }

                            classAll = Team.Value.ClassName;
                            if (!classAll.EndsWith("-"))
                                classAll += "-";
                            classAll += "All";

                            var res = new Result
                            {
                                ID = 100000 + Team.Value.TeamBib,
                                RunnerName = name,
                                RunnerClub = Team.Value.TeamName,
                                Class = classAll,
                                StartTime = Team.Value.StartTime,
                                Time = Team.Value.TotalTime,
                                Status = rstatus,
                                SplitTimes = Team.Value.SplitTimes
                            };

                            FireOnResult(res);
                        }

                    }
                }
            }
        }

        private static int GetStatusFromCode(ref int time, string status)
        {
            int rstatus = 10; //  Default: Started
            switch (status)
            {
                case "S": // Started
                    rstatus = 9;
                    time = -1;
                    break;
                case "I": // Entered 
                    rstatus = 10;
                    time = -1;
                    break;
                case "A": // OK
                    rstatus = 0;
                    break;
                case "N": // Ikke startet
                    rstatus = 1;
                    time = -1;
                    break;
                case "B": // Brutt
                    rstatus = 2;
                    time = -1;
                    break;
                case "D": // Disk / mp
                    rstatus = 3;
                    time = -1;
                    break;
            }
            return rstatus;
        }

        private void ParseReaderSplits(IDbCommand cmd, out Dictionary<int,List<SplitRawStruct>> splitList, out string lastRunner)
        {
            splitList = new Dictionary<int, List<SplitRawStruct>>();
            lastRunner = "";
            using (IDataReader reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    int ecard = 0;
                    try
                    {
                        ecard = Convert.ToInt32(reader["mecard"].ToString());
                        lastRunner = "Ecard no:" + reader["mecard"].ToString();
                        Double ChangedTimeD = Convert.ToDouble(reader["timechanged"].ToString());
                        //lastSplitDateTime   = ChangedTimeD;

                        var res = new SplitRawStruct
                        {
                            controlCode = Convert.ToInt32(reader["stasjon"]),
                            passTime    = ConvertFromDay2cs(Convert.ToDouble(reader["mintime"])),
                            netTime     = ConvertFromDay2cs(Convert.ToDouble(reader["nettotid"])),
                            changedTime = ConvertFromDay2cs(ChangedTimeD % 1)
                        };

                        if (!splitList.ContainsKey(ecard))
                        {
                            splitList.Add(ecard, new List<SplitRawStruct>());
                        };
                        splitList[ecard].Add(res);

                    }
                    catch (Exception ee)
                    {
                        FireLogMsg("eTiming Parser: " + ee.Message);
                    }
                }
                reader.Close();

            }
        }

        
        private static int GetRunTime(string runTime)
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
                            if (!DateTime.TryParseExact(runTime, "H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                            {
                                if (!DateTime.TryParseExact(runTime, "mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                {
                                    if (runTime != "")
                                    {
                                        throw new ApplicationException ("Could not parse Time" + runTime);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return (int)Math.Round(dt.TimeOfDay.TotalSeconds * 100 * factor);
        }

        
        private static int ConvertFromDay2cs(double timeD)
        {
            int timems;
            timems = Convert.ToInt32(100.0 * 86400.0 * timeD);
            return timems;
        }


    public event RadioControlDelegate OnRadioControl;
    }
}
