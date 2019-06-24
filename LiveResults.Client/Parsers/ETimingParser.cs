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
        public event DeleteIDDelegate OnDeleteID;
        private bool m_createRadioControls;
        private bool m_continue;
        private int  m_sleepTime;
        private bool m_oneLineRelayRes;
        private bool m_MSSQL;
        private bool m_twoEcards;

        public ETimingParser(IDbConnection conn, int sleepTime, bool recreateRadioControls = true, bool oneLineRelayRes = false, 
            bool MSSQL = false, bool twoEcards = false)
        {
            m_connection = conn;
            m_createRadioControls = recreateRadioControls;
            m_sleepTime = sleepTime;
            m_oneLineRelayRes = oneLineRelayRes;
            m_MSSQL = MSSQL;
            m_twoEcards = twoEcards;
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
        private void FireOnDeleteID(int runnerID)
        {
            if (OnDeleteID != null)
                OnDeleteID(runnerID);
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
            public int station;
            public int passTime;
            public int netTime;
            public int changedTime;
        }
                
        public class RelayLegInfo
        {
            public string LegName;
            public string LegStatus;
            public int LegTime;
            public int TotalTime;
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
                    
                    /*Detect event type*/
                    cmd.CommandText = "SELECT kid, sub FROM arr";
                    var reader = cmd.ExecuteReader();
                    bool isRelay = false;
                    int day = 1;
                    while (reader.Read())
                    {
                        if (reader[0] != null && reader[0] != DBNull.Value)
                        {
                            int kid = Convert.ToInt16(reader["kid"]);
                            FireLogMsg("Event type is " + kid);
                            if (kid == 3)
                            {
                                isRelay = true;
                            }
                            day = Convert.ToInt16(reader["sub"]);
                        }
                    }
                    reader.Close();


                    // *** Set up radiocontrols ***
                    /* ****************************
                     *  Ordinary controls       =  code + 1000*N
                     *  Pass/leg time           =  code + 1000*N           + 100000
                     *  Relay controls          =  code + 1000*N + 10000*L 
                     *  Pass/leg time for relay =  code + 1000*N + 10000*L + 100000
                     *  Change-over code        =  999  + 1000   + 10000*L
                     *  Exchange time code      =  0
                     *  Leg time code           =  999
                     *  Unranked fin. time code = -999
                     *  Unranked ord. controls  = -(code + 1000*N)
                     * 
                     *  N = number of occurrence
                     *  L = leg number
                     */


                    if (m_createRadioControls)
                    {
                        List<IntermediateTime> intermediates = new List<IntermediateTime>();
                        var dlg = OnRadioControl;
                        if (dlg != null)
                        {
                            // radiotype, 2=finish/finish-passing, 4 = normal, 10 = exchange
                            cmd.CommandText = (@"SELECT code, radiocourceno, radiotype, description, etappe, radiorundenr, live, radioday FROM radiopost");
                            var RadioPosts = new Dictionary<int, List<RadioStruct>>();

                            using (reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int cource = 0, code = 0, radiotype = 0, leg = 0, order = 0, radioday = 0;
                                    bool live = false;

                                    if (reader["live"] != null && reader["live"] != DBNull.Value)
                                        live = Convert.ToBoolean(reader["live"].ToString());
                                    if (!live) continue;

                                    if(reader["radioday"] != null && reader["radioday"] != DBNull.Value)
                                        radioday = Convert.ToInt32(reader["radioday"].ToString());
                                    if (radioday!=day) continue;

                                    if (reader["code"] != null && reader["code"] != DBNull.Value)
                                        code = Convert.ToInt32(reader["code"].ToString());

                                    if (code > 1000)
                                        code = code / 100; // Take away last to digits if code 1000+

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
                            cmd.CommandText = @"SELECT code, cource, class, purmin, timingtype, cheaseing FROM class";
                            var classTable = new Dictionary<string,ClassStruct>();
                            
                            using (reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int cource = 0, numLegs = 0, timingType = 0, sign = 1;

                                    bool chaseStart = Convert.ToBoolean(reader["cheaseing"].ToString());

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

                                    if (timingType == 1 || timingType == 2) // 0 = normal, 1 = not ranked, 2 = not show times
                                        sign = -1; // Use negative sign for these timing types
                                    if (timingType == 1) // Add neg finish passing for not-ranked class
                                        intermediates.Add(new IntermediateTime
                                        {
                                            ClassName = className,
                                            IntermediateName = "Tid",
                                            Position = -999,
                                            Order = 999
                                        });

                                    
                                    // Add starttime and leg times for chase start
                                    if (chaseStart)
                                    {
                                        intermediates.Add(new IntermediateTime
                                        {
                                            ClassName = className,
                                            IntermediateName = "Start",
                                            Position = 0,
                                            Order = 0
                                        });

                                        intermediates.Add(new IntermediateTime
                                        {
                                            ClassName = className,
                                            IntermediateName = "Leg",
                                            Position = 999,
                                            Order = 999
                                        });
                                    }
                                    // Add exchange and leg times for legs 2 and up
                                    for (int i = 2; i <= numLegs; i++) 
                                    {
                                        string classN = " " + className;
                                        if (!classN.EndsWith("-"))
                                                classN += "-";
                                        classN += Convert.ToString(i);

                                        intermediates.Add(new IntermediateTime
                                        {
                                            ClassName = classN,
                                            IntermediateName = "Exchange",
                                            Position = 0,
                                            Order = 0
                                        });

                                        intermediates.Add(new IntermediateTime
                                        {
                                             ClassName = classN,
                                             IntermediateName = "Leg",
                                             Position = 999,
                                             Order = 999
                                        });
                                    }

                                    string classAll = " " + className;
                                    if (!classAll.EndsWith("-"))
                                        classAll += "-";
                                    classAll += "All";
                                    if (numLegs > 0 && m_oneLineRelayRes)
                                    { // Add leg time for last leg in one-line results
                                        intermediates.Add(new IntermediateTime
                                        {
                                            ClassName = classAll,
                                            IntermediateName = "Leg",
                                            Position = 999,
                                            Order = 999
                                        });
                                    }


                                        if (RadioPosts.ContainsKey(cource))
                                    {   // Add radio controls to course
                                        Dictionary<int, int> radioCnt = new Dictionary<int, int>();
                                        foreach (var radioControl in RadioPosts[cource])
                                        {
                                            string classN = className; 
                                            int Code = radioControl.Code;
                                            int CodeforCnt = 0; // Code for counter
                                            int AddforLeg = 0;  // Addition for relay legs
                                            if (numLegs == 0 && (Code == 999 || Code == 0))
                                                continue;       // Skip if not relay and finish or start code
                                            if (numLegs > 0)    // Relay
                                            {
                                                classN = " " + classN;
                                                if (!classN.EndsWith("-"))
                                                    classN += "-";
                                                classN += Convert.ToString(radioControl.Leg);
                                                AddforLeg = 10000 * radioControl.Leg;
                                            }
                                            
                                            if (Code < 999 && radioControl.RadioType != 10) // Not 999 and not exchange)
                                            {
                                                CodeforCnt = Code + AddforLeg;
                                                if (!radioCnt.ContainsKey(CodeforCnt))
                                                    radioCnt.Add(CodeforCnt, 0);
                                                radioCnt[CodeforCnt]++;

                                                // Add codes for ordinary classes and leg based classes
                                                // sign = -1 for unranked classes
                                                intermediates.Add(new IntermediateTime
                                                {
                                                    ClassName = classN,
                                                    IntermediateName = radioControl.Description,
                                                    Position = sign*(Code + radioCnt[CodeforCnt] * 1000 + AddforLeg),
                                                    Order = radioControl.Order
                                                });

                                                // Add leg passing time for relay and chase start
                                                if (numLegs > 0 || chaseStart) 
                                                {
                                                    intermediates.Add(new IntermediateTime
                                                    {
                                                        ClassName = classN,
                                                        IntermediateName = radioControl.Description + "PassTime",
                                                        Position = Code + radioCnt[CodeforCnt] * 1000 + AddforLeg + 100000,
                                                        Order = radioControl.Order
                                                    });
                                                }

                                            }
                                            
                                            // Add codes for one-line relay classes
                                            if (numLegs > 0 && m_oneLineRelayRes)  
                                            {
                                                string Description = Convert.ToString(radioControl.Leg) +":"+ radioControl.Description;
                                                int position = 0;
                                                if (radioControl.RadioType == 10) // Exchange
                                                    position = 999 + 1000 + AddforLeg;
                                                else // Normal radio control
                                                    position = Code + radioCnt[CodeforCnt] * 1000 + AddforLeg;

                                                intermediates.Add(new IntermediateTime
                                                {
                                                    ClassName = classAll,
                                                    IntermediateName = Description,
                                                    Position = position,
                                                    Order = radioControl.Order
                                                });

                                                // Passing time
                                                intermediates.Add(new IntermediateTime
                                                {
                                                    ClassName = classAll,
                                                    IntermediateName = Description + "PassTime",
                                                    Position = position + 100000,
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

                    string modulus; // integer_div, //integer_div = "/"; //integer_div = "\\";
                    if (m_MSSQL)
                        modulus = "%";
                    else
                        modulus = "MOD";
                    
                    baseCommandRelay = string.Format(@"SELECT N.id, N.startno, N.ename, N.name, N.times, N.intime,
                            N.place, N.status, N.cource, N.starttime, N.ecard, N.ecard2, N.ecard3, N.ecard4,
                            T.name AS tname, C.class AS cclass, C.timingtype, C.freestart, C.cource AS ccource, 
                            C.firststart AS cfirststart, C.purmin AS cpurmin,
                            R.lgstartno, R.teamno, R.lgclass, R.lgtotaltime, R.lglegno, R.lgstatus, R.lgteam  
                            FROM Name N, Class C, Team T, Relay R
                            WHERE N.class=C.code AND T.code=R.lgteam AND N.rank=R.lgstartno AND (N.startno {0} 100)<=C.purmin 
                            ORDER BY N.startno", modulus);

                    baseCommandInd = string.Format(@"SELECT N.id, N.startno, N.ename, N.name, N.times, N.intime, N.totaltime,
                            N.place, N.status, N.cource, N.starttime, N.ecard, N.ecard2, N.ecard3, N.ecard4,
                            T.name AS tname, C.class AS cclass, C.timingtype, C.freestart, C.cource AS ccource, C.cheaseing
                            FROM Name N, Class C, Team T
                            WHERE N.class=C.code AND T.code=N.team AND (C.purmin IS NULL OR C.purmin<2)");

                    baseSplitCommand = string.Format(@"SELECT mellomid, iplace, stasjon, mintime, nettotid, timechanged, mecard 
                            FROM mellom 
                            WHERE stasjon>=0 AND stasjon<250 AND mecard>0  
                            ORDER BY mintime");

                    
                    Dictionary<int, List<SplitRawStruct>> splitList = null;
                    List<int> unknownRunners = new List<int>();

                    string lastRunner = "";
                                      
                    cmdSplits.CommandText = baseSplitCommand;
                    ParseReaderSplits(cmdSplits, out splitList, out lastRunner);

                    cmdInd.CommandText = baseCommandInd;
                    ParseReader(cmdInd, ref splitList, false, out lastRunner);

                    if (isRelay)
                    {
                        cmdRelay.CommandText = baseCommandRelay;
                        ParseReader(cmdRelay, ref splitList, true, out lastRunner);
                    }

                    FireLogMsg("eTiming Monitor thread started");
                    while (m_continue)
                    {
                        try
                        {
                            ParseReaderSplits(cmdSplits, out splitList, out lastRunner);
                            ParseReader(cmdInd, ref splitList, false, out lastRunner);
                            if (isRelay)
                                ParseReader(cmdRelay, ref splitList, true, out lastRunner);
                            handleUnknowns(splitList, ref unknownRunners);
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
        

        private void ParseReader(IDbCommand cmd, ref Dictionary<int, List<SplitRawStruct>> splitList, bool isRelay, out string lastRunner)
        {
            lastRunner = "";

            Dictionary<int, RelayTeam> RelayTeams;
            RelayTeams = new Dictionary<int, RelayTeam>();

            using (IDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int time = 0, totalTime = 0, runnerID = 0, iStartTime = 0, iStartClass = 0, bib = 0, teambib = 0, leg = 0, numlegs = 0, intime = -1, timingType = 0, sign = 1;
                    string famName = "", givName = "", club = "", classN = "", status = "", bibread = "", bibstr = "", name = "", shortName = "-";
                    bool chaseStart = false, freeStart = false;
                    var SplitTimes = new List<ResultStruct>();

                    try
                    {
                        runnerID = Convert.ToInt32(reader["id"].ToString());

                        status = reader["status"] as string;
                        if ((status == "V") || (status == "C")) // Skip if free or not entered  
                            continue;

                        classN = (reader["cclass"] as string);
                        if (!string.IsNullOrEmpty(classN))
                            classN = classN.Trim();
                        if (classN == "NOCLAS") continue;   // Skip runner if in NOCLAS

                        club = (reader["tname"] as string);
                        if (!string.IsNullOrEmpty(club))
                            club = club.Trim();

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

                        totalTime = 0;
                        if (reader["totaltime"] != null && reader["totaltime"] != DBNull.Value)
                            totalTime = ConvertFromDay2cs(Convert.ToDouble(reader["totaltime"]));

                        chaseStart = Convert.ToBoolean(reader["cheaseing"].ToString());

                        freeStart = Convert.ToBoolean(reader["freestart"].ToString());

                        if (reader["timingtype"] != null && reader["timingtype"] != DBNull.Value)
                            timingType = Convert.ToInt32(reader["timingtype"].ToString());
                        if (timingType == 1 || timingType == 2)  // 0=normal, 1=not ranked, 2=not show times
                            sign = -1;

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
                        {   //RelayTeams
                            classN = " " + classN;
                            leg = bib % 100;   // Leg number

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
                            {
                                RelayTeams[teambib].TeamMembers[leg].LegTime = time;
                                if (leg >= 2) // Add leg time for relay runners at leg 2 and above
                                {
                                    var LegTime = new ResultStruct
                                    {
                                        ControlCode = 999,
                                        Time = time
                                    };
                                    SplitTimes.Add(LegTime);
                                }
                            }
                            RelayTeams[teambib].TeamMembers[leg].LegStatus = status;

                            int TeamTime = 0;
                            RelayTeams[teambib].TotalTime = -2;
                            RelayTeams[teambib].TeamMembers[leg].TotalTime = 0;
                            string TeamStatus = "I";
                            bool TeamOK = true;
                            for (int legs = 1; legs <= leg; legs++)
                            {
                                if (RelayTeams[teambib].TeamMembers[legs].LegTime > 0)
                                {
                                    TeamTime += RelayTeams[teambib].TeamMembers[legs].LegTime;
                                    RelayTeams[teambib].TeamMembers[legs].TotalTime = TeamTime;

                                    var SplitTime = new ResultStruct
                                    {
                                        ControlCode = 999 + 1000 + 10000 * legs,           // Note code 999 for change-over!
                                        Time = TeamTime
                                    };
                                    RelayTeams[teambib].SplitTimes.Add(SplitTime);

                                    int controlCode = 0;
                                    if (legs == numlegs)
                                        controlCode = 999;
                                    else
                                        controlCode = 999 + 1000 + 10000 * legs + 100000;

                                    var LegTime = new ResultStruct
                                    {
                                        ControlCode = controlCode,
                                        Time = RelayTeams[teambib].TeamMembers[legs].LegTime
                                    };
                                    RelayTeams[teambib].SplitTimes.Add(LegTime);


                                }

                                // Accumulated status
                                if (TeamOK && (RelayTeams[teambib].TeamMembers[legs].LegStatus == "A"))
                                {
                                    TeamOK = true;
                                    TeamStatus = "A";
                                }
                                else
                                    TeamOK = false;

                                if (legs == numlegs && TeamOK)
                                {
                                    TeamStatus = "A";
                                    status = "A";
                                    RelayTeams[teambib].TotalTime = TeamTime;
                                    continue;
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

                            if (leg > 1 && RelayTeams[teambib].TeamMembers[leg - 1].TotalTime > 0)
                            {
                                var ExchangeTime = new ResultStruct
                                {
                                    ControlCode = 0,  // Note code 0 for change-over!
                                    Time = RelayTeams[teambib].TeamMembers[leg - 1].TotalTime
                                };
                                SplitTimes.Add(ExchangeTime);
                            }
                        }

                        if (chaseStart)
                        {
                            // Set starttime split to get starting order based on total time
                            var totalTimeStart = new ResultStruct
                            {
                                ControlCode = 0,  // Note code 0 for change-over!
                                Time = totalTime
                            };
                            SplitTimes.Add(totalTimeStart);

                            // Add time as leg time and add totalTime to time for finished runners
                            if (time > 0)
                            {
                                var LegTime = new ResultStruct
                                {
                                    ControlCode = 999,
                                    Time = time
                                };
                                SplitTimes.Add(LegTime);
                                time += totalTime;
                            }
                            // Set status for runners without total time to not classified NC
                            if (totalTime<=0 && (status == "S" || status == "I" || status == "A"))
                                status = "NC";
                        }

                        // Add split times
                        int ecard1 = 0, ecard2 = 0, ecard3 = 0, ecard4 = 0;
                        if (reader["ecard"] != null && reader["ecard"] != DBNull.Value)
                            ecard1 = Convert.ToInt32(reader["ecard"].ToString());
                        if (reader["ecard2"] != null && reader["ecard2"] != DBNull.Value)
                            ecard2 = Convert.ToInt32(reader["ecard2"].ToString());
                        if (reader["ecard3"] != null && reader["ecard3"] != DBNull.Value)
                            ecard3 = Convert.ToInt32(reader["ecard3"].ToString());
                        if (reader["ecard4"] != null && reader["ecard4"] != DBNull.Value)
                            ecard4 = Convert.ToInt32(reader["ecard4"].ToString());

                        var splits = new List<SplitRawStruct>();
                        var numEcards = 0; // Number of ecards with splits
                        var numStart = 0;  // Number of ecards with 0 (start) registered
                        if (splitList.ContainsKey(ecard1))
                        {
                            if (splitList[ecard1].Any(s => s.controlCode == 0))
                                numStart += 1;
                            splits.AddRange(splitList[ecard1]);
                            splitList.Remove(ecard1);
                            numEcards += 1;
                        }
                        if (splitList.ContainsKey(ecard2))
                        {
                            if (splitList[ecard2].Any(s => s.controlCode == 0))
                                numStart += 1;
                            splits.AddRange(splitList[ecard2]);
                            splitList.Remove(ecard2);
                            numEcards += 1;
                        }
                        if (splitList.ContainsKey(ecard3))
                        {
                            splits.AddRange(splitList[ecard3]);
                            splitList.Remove(ecard3);
                        }
                        if (splitList.ContainsKey(ecard4))
                        {
                            splits.AddRange(splitList[ecard4]);
                            splitList.Remove(ecard4);
                        }
                        if (numEcards > 1)
                            splits = splits.OrderBy(s => s.passTime).ToList();

                        var lsplitCodes = new List<int>();
                        int calcStartTime = -2;
                        int iSplitcode = 0;
                        int lastSplitTime = -1;
                        foreach (var split in splits)
                        {
                            if (split.controlCode == 0)
                            {
                                if (status == "I") // Change code of only entered runners
                                    status = "S";
                                if (freeStart)
                                    calcStartTime = split.passTime;
                                continue;         
                            }
                                
                            if (split.passTime < iStartTime)
                                continue;         // Neglect passing before starttime
                            if (split.passTime - lastSplitTime < 3000)
                                continue;         // Neglect passing less than 3 s from last

                            int passTime = -2;    // Total time at passing
                            int passLegTime = -2; // Time used on leg at passing
                            if (freeStart)
                                passTime = split.netTime;
                            else if (isRelay)
                            {
                                passTime = split.passTime - iStartClass;     // Absolute pass time
                                //if (leg > 1)                               // Leg based pass time
                                passLegTime = split.passTime - Math.Max(iStartTime, iStartClass); // In case ind. start time not set
                            }
                            else if (chaseStart)
                            {
                                passTime    = split.passTime - iStartTime + totalTime;  
                                passLegTime = split.passTime - iStartTime; 
                            }
                            else
                                passTime = split.passTime - iStartTime;
                            if (passTime < 3000)  // Neglect pass times less than 3 s from start
                                continue;
                            lastSplitTime = split.passTime;


                            // Add split code to list
                            if (split.controlCode > 0)
                                iSplitcode = sign * (split.controlCode + 1000);
                            else
                                iSplitcode = sign * (split.station + 1000);

                            while (lsplitCodes.Contains(iSplitcode))
                                iSplitcode += sign * 1000;
                            lsplitCodes.Add(iSplitcode);

                            // Add split time to SplitTime struct
                            if (timingType == 2) // Not show times
                                passTime = -10;
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
                                var passLegTimeStruct = new ResultStruct
                                {
                                    ControlCode = iSplitcode + 10000 * leg + 100000,
                                    Time = passLegTime
                                };
                                if (leg > 1 || chaseStart)
                                    SplitTimes.Add(passLegTimeStruct);
                                if (isRelay)
                                    RelayTeams[teambib].SplitTimes.Add(passLegTimeStruct);
                            }

                            if (freeStart && calcStartTime<0)
                                calcStartTime = split.changedTime - split.netTime;
                        }
                        if (m_twoEcards && numEcards < 2 && (status == "S"))
                            status = "I"; // Set status to "Entered" if only one eCard at start when 2 required 

                        if (freeStart && (calcStartTime > 0) && (Math.Abs(calcStartTime - iStartTime) > 3000))  // Update starttime if deviation more than 30 sec
                            iStartTime = calcStartTime;

                        if (time > 0 && (timingType == 1 || timingType == 2)) // Not ranked or not show times
                        {
                            if (timingType == 2) // Do not show times
                                time = -10;
                            else // Not ranked
                            {
                                var FinishTime = new ResultStruct
                                {
                                    ControlCode = -999, // Code used for finish passing
                                    Time = time
                                };
                                SplitTimes.Add(FinishTime);
                            }

                            if (status == "A")
                            {
                                status = "F";    // Finished
                                time = runnerID; // Sets a "random", but unique time for each competitor
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        FireLogMsg("eTiming Parser. Runner ID:" + runnerID + " Error: " + ee.Message);
                    }

                    int rstatus = GetStatusFromCode(ref time, status);
                    if (rstatus != 999)
                    {
                        
                        if (rstatus == 9 || rstatus == 1)    // Modify starttime if "started" or "DNS" to force update with new status
                            iStartTime += 1;

                        var res = new Result
                        {
                            ID = runnerID,
                            RunnerName = name,
                            RunnerClub = club,
                            Class = classN,
                            StartTime = iStartTime,
                            Time = time,
                            Status = rstatus,
                            SplitTimes = SplitTimes
                        };

                        FireOnResult(res);
                    }
                }
                reader.Close();
            }

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

        private void handleUnknowns(Dictionary<int, List<SplitRawStruct>> splitList, ref List<int> unknownRunnersLast)
        {
            // Loop through the remaining entries in the splitList (those linked to runners are removed)
            List<int> unknownRunners = new List<int>();
            List<SplitRawStruct> splits;
            foreach (int ecard in splitList.Keys)
            {
                var split = new SplitRawStruct();
                splits = splitList[ecard];
                splits = splits.OrderByDescending(s => s.passTime).ToList();
                split = splits.Find(s => s.controlCode == 0); // Find last pass at start

                if (split.passTime > 0)
                {
                    unknownRunners.Add(ecard);
                    unknownRunnersLast.Remove(ecard);
                    var res = new Result
                    {
                        ID = -ecard,
                        RunnerName = ecard + " UKJENT BRIKKE START",
                        RunnerClub = "NOTEAM",
                        Class = "NOCLAS",
                        StartTime = split.passTime,
                        Time = -3,
                        Status = 9
                    };
                    FireOnResult(res);
                }
            }
            foreach (int ecard in unknownRunnersLast) // Delete those that are still in last arrray
                FireOnDeleteID(-ecard);

            unknownRunnersLast = unknownRunners;  // Set back new list of unknown runners
        }

    private static int GetStatusFromCode(ref int time, string status)
        {
            int rstatus = 10; //  Default: Entered
            switch (status)
            {
                case "S": // Started
                    rstatus = 9;
                    time = -3;
                    break;
                case "I": // Entered 
                    rstatus = 10;
                    time = -3;
                    break;
                case "A": // OK
                    rstatus = 0;
                    break;
                case "N": // Ikke startet
                    rstatus = 1;
                    time = -3;
                    break;
                case "B": // Brutt
                    rstatus = 2;
                    time = -3;
                    break;
                case "D": // Disk / mp
                    rstatus = 3;
                    time = -3;
                    break;
                case "NC": // Not classified
                    rstatus = 6;
                    time = -3;
                    break;
                case "F": // Finished (Fullført). For not ranked classes
                    rstatus = 13;
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
                    int ecard = 0, code = 0, station = 0, passTime = -2, netTime = -2, changedTime = 0;
                    double changedTimeD = 0.0;
                    try
                    {
                        if (reader["mecard"] != null && reader["mecard"] != DBNull.Value)
                        {
                            ecard = Convert.ToInt32(reader["mecard"].ToString());
                            lastRunner = "Ecard no:" + reader["mecard"].ToString();
                        }
                        else
                            continue;

                        if (reader["stasjon"] != null && reader["stasjon"] != DBNull.Value)
                            station = Convert.ToInt32(reader["stasjon"].ToString());
                        if (station == 0)
                            code = 0;
                        else
                        {
                            if (reader["iplace"] != null && reader["iplace"] != DBNull.Value)
                                code = Convert.ToInt32(reader["iplace"].ToString());
                            if (code > 1000)
                                code = code / 100; // Take away last to digits if code 1000+
                        }

                        if (reader["mintime"] != null && reader["mintime"] != DBNull.Value)
                            passTime = ConvertFromDay2cs(Convert.ToDouble(reader["mintime"]));

                        if (reader["nettotid"] != null && reader["nettotid"] != DBNull.Value)
                            netTime = ConvertFromDay2cs(Convert.ToDouble(reader["nettotid"]));

                        if (reader["nettotid"] != null && reader["nettotid"] != DBNull.Value)
                            netTime = ConvertFromDay2cs(Convert.ToDouble(reader["nettotid"]));

                        if (reader["timechanged"] != null && reader["timechanged"] != DBNull.Value)
                        {
                            changedTimeD = Convert.ToDouble(reader["timechanged"].ToString());
                            changedTime = ConvertFromDay2cs(changedTimeD % 1);
                        }

                        var res = new SplitRawStruct
                        {
                            controlCode = code,
                            station = station,
                            passTime = passTime,
                            netTime = netTime,
                            changedTime = changedTime
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
            if (!DateTime.TryParseExact(runTime, "mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                if (!DateTime.TryParseExact(runTime, "H:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if (!DateTime.TryParseExact(runTime, "mm:ss,f", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    {
                        if (!DateTime.TryParseExact(runTime, "H:mm:ss,f", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            if (!DateTime.TryParseExact(runTime, "mm:ss,ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                            {
                                if (!DateTime.TryParseExact(runTime, "H:mm:ss,ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                {
                                    if (!DateTime.TryParseExact(runTime, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
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
            }
            
            return (int)Math.Round(dt.TimeOfDay.TotalSeconds * 100 * factor);
        }

        
        private static int ConvertFromDay2cs(double timeD)
        {
            int timecs;
            timecs = Convert.ToInt32(100.0 * 86400.0 * timeD);
            return timecs;
        }


    public event RadioControlDelegate OnRadioControl;
    }
}
