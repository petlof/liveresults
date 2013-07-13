using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            List<Runner> runners = new List<Runner>();
            byte[] fileContents;
            if (!File.Exists(filename))
            {
                return null;
            }
            else
            {
                fileContents = File.ReadAllBytes(filename);
            }

            if (deleteFile)
                File.Delete(filename);

            XmlDocument xmlDoc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(fileContents))
            {
                XmlReaderSettings setts = new XmlReaderSettings();
                setts.XmlResolver = null;
                setts.ProhibitDtd = false;
                using (XmlReader xr = XmlTextReader.Create(ms, setts))
                {
                    xmlDoc.Load(xr);
                }
            }

            foreach (XmlNode classNode in xmlDoc.GetElementsByTagName("ClassResult"))
            {
                XmlNode classNameNode = classNode.SelectSingleNode("ClassShortName");
                string className = classNameNode.InnerText;

                foreach (XmlNode personNode in classNode.SelectNodes("PersonResult"))
                {
                    XmlNode personNameNode = personNode.SelectSingleNode("Person/PersonName");
                    string familyname = personNameNode.SelectSingleNode("Family").InnerText;
                    string givenname = personNameNode.SelectSingleNode("Given").InnerText;
                    string id = personNode.SelectSingleNode("Person/PersonId").InnerText;
                    long pid = 0;
                    if (id.Trim().Length > 0)
                    {
                        pid = Convert.ToInt64(id);
                    }
                    var clubNode = personNode.SelectSingleNode("Club/ShortName");
                    string club = "";
                    if (clubNode != null)
                        club = clubNode.InnerText;
                    string status = personNode.SelectSingleNode("Result/CompetitorStatus").Attributes["value"].Value;
                    string time = personNode.SelectSingleNode("Result/Time").InnerText;
                    string starttime = personNode.SelectSingleNode("Result/StartTime/Clock").InnerText;
                    string si = personNode.SelectSingleNode("Result/CCard/CCardId").InnerText;
                    int iSi;
                    if (!Int32.TryParse(si, out iSi))
                    {
                        //NO SICARD!
                        logit("No SICard for Runner: " + familyname + " " + givenname);
                    }
                    int dbid = 0;
                    if (pid < Int32.MaxValue && pid > 0)
                    {
                        dbid = (int)pid;
                    }
                    else if (iSi > 0)
                    {
                        dbid = -1 * iSi;
                    }
                    else
                    {
                        logit("Cant generate DBID for runner: " + givenname + " " + familyname);
                    }


                    Runner runner = new Runner(dbid, givenname + " " + familyname, club, className);

                    int istarttime = -1;
                    if (!string.IsNullOrEmpty(starttime))
                    {
                        istarttime = ParseTime(starttime);
                        runner.SetStartTime(istarttime);
                    }

                    int itime = -9;
                    itime = ParseTime(time);

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
                    }

                    runner.SetResult(itime, istatus);

                    List<int> lsplitCodes = new List<int>();
                    List<int> lsplitTimes = new List<int>();

                    XmlNodeList splittimes = personNode.SelectNodes("Result/SplitTime");
                    if (splittimes != null)
                    {
                        foreach (XmlNode splitNode in splittimes)
                        {
                            XmlNode splitcode = splitNode.SelectSingleNode("ControlCode");
                            XmlNode splittime = splitNode.SelectSingleNode("Time");
                            int i_splitcode;
                            string s_splittime = splittime.InnerText;
                            if (int.TryParse(splitcode.InnerText, out i_splitcode) && s_splittime.Length > 0)
                            {
                                if (i_splitcode == 999)
                                {
                                    if (istatus == 0 && itime == -1)
                                    {
                                        //Målstämpling
                                        itime = ParseTime(s_splittime);
                                        runner.SetResult(itime, 0);
                                    }
                                }
                                else
                                {
                                    i_splitcode += 1000;
                                    while (lsplitCodes.Contains(i_splitcode))
                                    {
                                        i_splitcode += 1000;
                                    }

                                    int i_splittime = ParseTime(s_splittime);
                                    lsplitCodes.Add(i_splitcode);
                                    lsplitTimes.Add(i_splittime);

                                    runner.SetSplitTime(i_splitcode, i_splittime);
                                }
                            }
                        }
                    }

                    runners.Add(runner);
                }
            }

            return runners.ToArray();
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
