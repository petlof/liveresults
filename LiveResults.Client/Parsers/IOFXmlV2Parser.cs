using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace LiveResults.Client.Parsers
{
    /// <summary>
    /// Simple parset for IOFv2XmlFiles
    /// </summary>
    public class IOFXmlV2Parser
    {
        public static Runner[] ParseFile(string filename, LogMessageDelegate logit, GetIdDelegate getIdFunc)
        {
            return ParseFile(filename, logit, true, getIdFunc);
        }

        static readonly Dictionary<string,string> m_suppressedIDCalculationErrors = new Dictionary<string, string>(); 

        public static Runner[] ParseFile(string filename, LogMessageDelegate logit, bool deleteFile, GetIdDelegate getIdFunc)
        {
            byte[] fileContents;
            if (!File.Exists(filename))
            {
                return null;
            }

            fileContents = File.ReadAllBytes(filename);

            if (deleteFile)
                File.Delete(filename);

            return ParseXmlData(fileContents, logit, deleteFile, getIdFunc);

        }

        public delegate int GetIdDelegate(string sourceid, string sicard, out string storeAlias);

        public class IDCalculator
        {
            private readonly int m_compid;
            public IDCalculator(int compId)
            {
                m_compid = compId;
            }

            public int CalculateID(string sourceId, string si, out string storeAlias)
            {
                long id;
                storeAlias = null;
                if (long.TryParse(sourceId, NumberStyles.Any, CultureInfo.InvariantCulture, out id))
                {
                    if (id < Int32.MaxValue && id > 0)
                    {
                        return (int) id;
                    }
                }
                if (!string.IsNullOrEmpty(sourceId))
                {
                    storeAlias = sourceId;
                    return EmmaMysqlClient.GetIdForSourceIdInCompetition(m_compid, sourceId);
                }
                if (!string.IsNullOrEmpty(si))
                {
                    storeAlias = "SI:" + si;
                    return EmmaMysqlClient.GetIdForSourceIdInCompetition(m_compid, storeAlias);
                }
                throw new FormatException("Could not calculate ID");
            }
        }

        public static Runner[] ParseXmlData(byte[] xml, LogMessageDelegate logit, bool deleteFile, GetIdDelegate getIdFunc)
        {

            var runners = new List<Runner>();

            var xmlDoc = new XmlDocument();
            using (var ms = new MemoryStream(xml))
            {
                var setts = new XmlReaderSettings();
                setts.XmlResolver = null;
                setts.ProhibitDtd = false;
                using (XmlReader xr = XmlReader.Create(ms, setts))
                {
                    xmlDoc.Load(xr);
                }
            }

            foreach (XmlNode classNode in xmlDoc.GetElementsByTagName("ClassStart"))
            {
                XmlNode classNameNode = classNode.SelectSingleNode("ClassShortName");
                if (classNameNode == null)
                    continue;

                string className = classNameNode.InnerText;
                var personNodes = classNode.SelectNodes("PersonStart");
                if (personNodes != null)
                {
                    foreach (XmlNode personNode in personNodes)
                    {
                        string familyname;
                        string givenname;
                        string club;
                        string sourceId;
                        if (!ParseNameClubAndId(personNode, out familyname, out givenname, out club, out sourceId)) continue;

                        var startTimeNode = personNode.SelectSingleNode("Start/StartTime/Clock");
                        var ccCardNode = personNode.SelectSingleNode("Start/CCard/CCardId");

                        if (startTimeNode == null || ccCardNode == null)
                            continue;
                        string starttime = startTimeNode.InnerText;
                        string si = ccCardNode.InnerText;
                        string storeAlias;

                        if (string.IsNullOrEmpty(sourceId) && string.IsNullOrEmpty(si))
                        {
                            string name = givenname + " " + familyname + ", " + club;
                            if (!m_suppressedIDCalculationErrors.ContainsKey(name))
                            {
                                logit("Cannot calculculate ID for runner: " + name + ", skipping [supressing further output for this name]");
                                m_suppressedIDCalculationErrors.Add(name, name);
                            }
                            continue;
                        }
                    

                        int dbId = getIdFunc(sourceId, si, out storeAlias);

                        var runner = new Runner(dbId, givenname + " " + familyname, club, className, storeAlias);

                        if (!string.IsNullOrEmpty(starttime))
                        {
                            int istarttime = ParseTime(starttime);
                            runner.SetStartTime(istarttime);
                        }

                        runners.Add(runner);
                    }
                }
            }

            foreach (XmlNode classNode in xmlDoc.GetElementsByTagName("ClassResult"))
            {
                XmlNode classNameNode = classNode.SelectSingleNode("ClassShortName");
                if (classNameNode == null)
                    continue;

                string className = classNameNode.InnerText;

                var personNodes = classNode.SelectNodes("PersonResult");
                if (personNodes != null)
                {
                    foreach (XmlNode personNode in personNodes)
                    {
                        string familyname;
                        string givenname;
                        string club;
                        string sourceId;
                        if (!ParseNameClubAndId(personNode, out familyname, out givenname, out club, out sourceId)) continue;

                        var competitorStatusNode = personNode.SelectSingleNode("Result/CompetitorStatus");
                        var resultTimeNode = personNode.SelectSingleNode("Result/Time");
                        var startTimeNode = personNode.SelectSingleNode("Result/StartTime/Clock");
                        var ccCardNode = personNode.SelectSingleNode("Result/CCard/CCardId");
                        if (competitorStatusNode == null || competitorStatusNode.Attributes == null || competitorStatusNode.Attributes["value"] == null ||
                            resultTimeNode == null || ccCardNode == null)
                            continue;

                        string status = competitorStatusNode.Attributes["value"].Value;
                        string time = resultTimeNode.InnerText;
                        string starttime = "";
                        if (startTimeNode != null)
                            starttime = startTimeNode.InnerText;
                        string si = ccCardNode.InnerText;
                        string storeAlias;

                        if (string.IsNullOrEmpty(sourceId) && string.IsNullOrEmpty(si))
                        {
                            string name = givenname + " " + familyname + ", " + club;
                            if (!m_suppressedIDCalculationErrors.ContainsKey(name))
                            {
                                logit("Cannot calculculate ID for runner: " + name + ", skipping [supressing further output for this name]");
                                m_suppressedIDCalculationErrors.Add(name, name);
                            }
                            continue;
                        }

                        int dbId = getIdFunc(sourceId, si, out storeAlias);
                        
                        var runner = new Runner(dbId, givenname + " " + familyname, club, className, storeAlias);

                        if (!string.IsNullOrEmpty(starttime))
                        {
                            int istarttime = ParseTime(starttime);
                            runner.SetStartTime(istarttime);
                        }

                        int itime = ParseTime(time);
                        int istatus = 10;

                        if (status.ToLower() == "notcompeting")
                        {
                            //Does not compete, exclude
                            continue;
                        }

                        //runners without starttimenode have not started yet
                        if (startTimeNode != null)
                        {
                            switch (status.ToLower())
                            {
                                case "mispunch":
                                    istatus = 3;
                                    break;

                                case "disqualified":
                                    istatus = 4;
                                    break;
                                case "didnotfinish":
                                    istatus = 3;
                                    itime = -3;
                                    break;
                                case "didnotstart":
                                    istatus = 1;
                                    itime = -3;
                                    break;
                                case "overtime":
                                    istatus = 5;
                                    break;
                                case "ok":
                                    istatus = 0;
                                    break;
                            }
                        }

                        runner.SetResult(itime, istatus);

                        var lsplitCodes = new List<int>();
                        var lsplitTimes = new List<int>();

                        XmlNodeList splittimes = personNode.SelectNodes("Result/SplitTime");
                        if (splittimes != null)
                        {
                            foreach (XmlNode splitNode in splittimes)
                            {
                                XmlNode splitcode = splitNode.SelectSingleNode("ControlCode");
                                XmlNode splittime = splitNode.SelectSingleNode("Time");
                                if (splittime == null || splitcode == null)
                                    continue;

                                int iSplitcode;
                                string sSplittime = splittime.InnerText;
                                
                                bool parseOK = int.TryParse(splitcode.InnerText, out iSplitcode);
                                bool isFinishPunch = splitcode.InnerText.StartsWith("F", StringComparison.InvariantCultureIgnoreCase) || iSplitcode == 999;
                                if ((parseOK || isFinishPunch) && sSplittime.Length > 0)
                                {
                                    if (isFinishPunch)
                                    {
                                        if ((istatus == 0 && itime == -1) || (istatus == 10 && itime == -9))
                                        {
                                            //Målstämpling
                                            itime = ParseTime(sSplittime);
                                            runner.SetResult(itime, 0);
                                        }
                                    }
                                    else
                                    {
                                        iSplitcode += 1000;
                                        while (lsplitCodes.Contains(iSplitcode))
                                        {
                                            iSplitcode += 1000;
                                        }

                                        int iSplittime = ParseTime(sSplittime);
                                        lsplitCodes.Add(iSplitcode);
                                        lsplitTimes.Add(iSplittime);

                                        runner.SetSplitTime(iSplitcode, iSplittime);
                                    }
                                }
                            }
                        }

                        runners.Add(runner);
                    }
                }
            }

            return runners.ToArray();
        }

        private static int CalculateIDFromSiCard(LogMessageDelegate logit, string si, string familyname, string givenname, long pid)
        {
            int iSi;
            if (!Int32.TryParse(si, out iSi))
            {
                //NO SICARD!
                logit("No SICard for Runner: " + familyname + " " + givenname);
            }
            int dbid = 0;
            if (pid < Int32.MaxValue && pid > 0)
            {
                dbid = (int) pid;
            }
            else if (iSi > 0)
            {
                dbid = -1*iSi;
            }
            else
            {
                logit("Cant generate DBID for runner: " + givenname + " " + familyname);
            }
            return dbid;
        }

        private static bool ParseNameClubAndId(XmlNode personNode, out string familyname, out string givenname, 
           out string club, out string sourceId)
        {
            familyname = null;
            givenname = null;
            club = null;
            sourceId = null;

            XmlNode personNameNode = personNode.SelectSingleNode("Person/PersonName");
            if (personNameNode == null)
            {
                return false;
            }

            var familyNameNode = personNameNode.SelectSingleNode("Family");
            var giveNameNode = personNameNode.SelectSingleNode("Given");
            var personIdNode = personNode.SelectSingleNode("Person/PersonId");
            if (familyNameNode == null || giveNameNode == null || personIdNode == null)
            {
                return false;
            }

            familyname = familyNameNode.InnerText;
            givenname = giveNameNode.InnerText;
            sourceId = personIdNode.InnerText;
            //if (sourceId.Trim().Length > 0)
            //{
            //    long tPid;
            //    if (!long.TryParse(sourceId, NumberStyles.Any, CultureInfo.InvariantCulture, out tPid))
            //    {
            //        //Could be textual, try combining charcodes to number
            //    }
            //    else
            //    {
            //        pid = tPid;
            //    }
            //}
            var clubNode = personNode.SelectSingleNode("Club/ShortName");
            club = "";
            if (clubNode != null)
            {
                club = clubNode.InnerText;
            }
            return true;
        }

        private static int ParseTime(string time)
        {
            int itime = -9;
            if (time != "--:--:--") //MeOS Hack
            {
                string[] timeParts = time.Split(':');
                if (timeParts.Length == 3)
                {
                        
                        if (timeParts[2].IndexOf(".", StringComparison.Ordinal) > 0)
                        {
                            //HH:MM:SS[.FFF]
                            itime = Convert.ToInt32(timeParts[0]) * 360000 + Convert.ToInt32(timeParts[1]) * 6000 + (int)(Convert.ToDouble(timeParts[2]) * 100);
                        }
                        else
                        {
                            itime = Convert.ToInt32(timeParts[0])*360000 + Convert.ToInt32(timeParts[1])*6000 + Convert.ToInt32(timeParts[2])*100;
                        }
                        
                }
                else if (timeParts.Length == 2)
                {
                    if (timeParts[1].IndexOf(".", StringComparison.Ordinal) > 0)
                    {
                        //MM:SS[.FFF]
                        itime = Convert.ToInt32(timeParts[0]) * 6000 + (int)Math.Round(Convert.ToDouble(timeParts[1]) * 100, 3);
                    }
                    else
                    {
                        //MM:SS
                        itime = Convert.ToInt32(timeParts[0]) * 6000 + Convert.ToInt32(timeParts[1]) * 100;
                    }
                }
            }
            return itime;
        }
    }
}
