using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace LiveResults.Client.Parsers
{
    /// <summary>
    /// Simple parset for IOFv2XmlFiles
    /// </summary>
    public class IOFXmlV2Parser
    {
        public static Runner[] ParseFile(string filename, LogMessageDelegate logit)
        {
            return ParseFile(filename, logit, true);
        }

        public static Runner[] ParseFile(string filename, LogMessageDelegate logit, bool deleteFile)
        {
            byte[] fileContents;
            if (!File.Exists(filename))
            {
                return null;
            }

            fileContents = File.ReadAllBytes(filename);

            if (deleteFile)
                File.Delete(filename);
            RadioControl[] ctrls;
            return ParseXmlData(fileContents, logit, deleteFile, out ctrls);

        }


        public static Runner[] ParseXmlData(byte[] xml, LogMessageDelegate logit, bool deleteFile, out RadioControl[] definedRadioControls)
        {

            var runners = new List<Runner>();
            definedRadioControls = null;
            var defRadios = new List<RadioControl>();

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
                        long pid;
                        string club;
                        if (!ParseNameClubAndId(personNode, out familyname, out givenname, out pid, out club)) continue;

                        var startTimeNode = personNode.SelectSingleNode("Start/StartTime/Clock");
                        var ccCardNode = personNode.SelectSingleNode("Start/CCard/CCardId");

                        if (startTimeNode == null || ccCardNode == null)
                            continue;
                        string starttime = startTimeNode.InnerText;
                        string si = ccCardNode.InnerText;
                        var dbid = CalculateIDFromSiCard(logit, si, familyname, givenname, pid);

                        var runner = new Runner(dbid, givenname + " " + familyname, club, className);

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
                        long pid;
                        string club;
                        if (!ParseNameClubAndId(personNode, out familyname, out givenname, out pid, out club)) continue;

                        var competitorStatusNode = personNode.SelectSingleNode("Result/CompetitorStatus");
                        var resultTimeNode = personNode.SelectSingleNode("Result/Time");
                        var startTimeNode = personNode.SelectSingleNode("Result/StartTime/Clock");
                        var ccCardNode = personNode.SelectSingleNode("Result/CCard/CCardId");
                        if (competitorStatusNode == null || competitorStatusNode.Attributes == null || competitorStatusNode.Attributes["value"] == null ||
                            resultTimeNode == null || startTimeNode == null || ccCardNode == null)
                            continue;
                        if (familyname == "* Radio controls definition *")
                        {
                            //Special handling of SportSoftware way of telling what RadioControls will appear for this class
                            XmlNodeList pSplittimes = personNode.SelectNodes("Result/SplitTime");
                            if (pSplittimes != null)
                            {
                                
                            }

                            continue;
                        }

                        string status = competitorStatusNode.Attributes["value"].Value;
                        string time = resultTimeNode.InnerText;
                        string starttime = startTimeNode.InnerText;
                        string si = ccCardNode.InnerText;
                        var dbid = CalculateIDFromSiCard(logit, si, familyname, givenname, pid);


                        var runner = new Runner(dbid, givenname + " " + familyname, club, className);

                        if (!string.IsNullOrEmpty(starttime))
                        {
                            int istarttime = ParseTime(starttime);
                            runner.SetStartTime(istarttime);
                        }

                        int itime = ParseTime(time);
                        int istatus = 10;

                        switch (status)
                        {
                            case "MisPunch":
                                istatus = 3;
                                break;

                            case "Disqualified":
                                istatus = 4;
                                break;
                            case "DidNotFinish":
                                istatus = 3;
                                itime = -3;
                                break;
                            case "DidNotStart":
                                istatus = 1;
                                itime = -3;
                                break;
                            case "Overtime":
                                istatus = 5;
                                break;
                            case "OK":
                                istatus = 0;
                                break;
                            case "NotCompeting":
                                //Does not compete, exclude
                                continue;
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
                                if (int.TryParse(splitcode.InnerText, out iSplitcode) && sSplittime.Length > 0)
                                {
                                    if (iSplitcode == 999)
                                    {
                                        if (istatus == 0 && itime == -1)
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

        private static bool ParseNameClubAndId(XmlNode personNode, out string familyname, out string givenname, out long pid, out string club)
        {
            familyname = null;
            givenname = null;
            club = null;
            pid = 0;

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
            string id = personIdNode.InnerText;
            pid = 0;
            if (id.Trim().Length > 0)
            {
                pid = Convert.ToInt64(id);
            }
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
            string[] timeParts = time.Split(':');
            if (timeParts.Length == 3)
            {
                //HH:MM:SS
                itime = Convert.ToInt32(timeParts[0]) * 360000 + Convert.ToInt32(timeParts[1]) * 6000 + Convert.ToInt32(timeParts[2]) * 100;
            }
            else if (timeParts.Length == 2)
            {
                //MM:SS
                itime = Convert.ToInt32(timeParts[0]) * 6000 + Convert.ToInt32(timeParts[1]) * 100;
            }
            return itime;
        }
    }
}
