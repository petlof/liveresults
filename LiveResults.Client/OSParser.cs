using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace LiveResults.Client
{
    
    public class OSParser
    {
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;
        public static char[] SplitChars ={ ';', '\t' };
        private readonly string m_directory;
        
        public OSParser()
        {
        }

        public static int CreateID(int leg, int stNo)
        {
            return leg*1000000 + stNo;
        }

        public static int StNoFromID(int leg, int id)
        {
            return id - leg * 1000000;
        }

        public OSParser(string directory)
        {
            m_directory = directory;
            var fsWatcher = new FileSystemWatcher(directory){
                EnableRaisingEvents = true
            };
            fsWatcher.Renamed += fsWatcher_Renamed;
        }

        public void Start()
        {
            string[] files = Directory.GetFiles(m_directory, "*.csv");
            foreach (string f in files)
            {
                AnalyzeFile(Path.Combine(m_directory, f));
                File.Delete(Path.Combine(m_directory, f));
            }

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

        void fsWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine("File Renamed: " + e.OldName + " to " + e.Name);
            AnalyzeFile(e.FullPath);
            Debug.WriteLine("File analysed");
            File.Delete(e.FullPath);
        }

        public void AnalyzeFile(string filename)
        {
            StreamReader sr =null;
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        sr = new StreamReader(filename, Encoding.Default);
                        break;
                    }
                    catch (Exception ee)
                    {
                        Debug.WriteLine(ee.Message);
                        Thread.Sleep(1000);
                    }
                }
                if (sr == null)
                    throw new ApplicationException("Could not open input file, copy error?");
                string header = sr.ReadLine();

                if (header == null)
                {
                    return;
                }

                string[] fields = header.Split(SplitChars);
                bool isOs2010Files = fields[0].StartsWith("OS", StringComparison.Ordinal);
                if (fields[0] == "OS0001")
                {
                    //OS Startlist, read..
                    ParseOSStartList(fields, sr, isOs2010Files);
                }
                else
                {
                    ParseOSResultFilt(fields, sr, isOs2010Files);
                }
            }
            catch (Exception ee)
            {
                FireLogMsg("ERROR in OSParser: " + ee.Message);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }
            
        }

        private void ParseOSStartList(string[] fields, StreamReader sr,  bool isOs2010Files)
        {
            string[] stoNoFieldNames = OxTools.GetOEStringsForKey("Stnr", OxTools.SourceProgram.OS);
            string[] numLegsField = OxTools.GetOEStringsForKey("OS_Strecken", OxTools.SourceProgram.OS);
            string[] clubFieldNames = OxTools.GetOEStringsForKey("Ort", OxTools.SourceProgram.OS);
            string[] clnameFieldNames = OxTools.GetOEStringsForKey("Abk", OxTools.SourceProgram.OS);
            string[] descFieldNames = OxTools.GetOEStringsForKey("Bez", OxTools.SourceProgram.OS);
            string[] classFieldNames = OxTools.GetOEStringsForKey("Kurz", OxTools.SourceProgram.OS);
            string[] leg1FieldNames = OxTools.GetOEStringsForKey("Lnr", OxTools.SourceProgram.OS);
            string[] start1FieldNames = OxTools.GetOEStringsForKey("Start", OxTools.SourceProgram.OS);
            string[] firstName1FieldNames = OxTools.GetOEStringsForKey("Vorname", OxTools.SourceProgram.OS);
            string[] lastName1FieldNames = OxTools.GetOEStringsForKey("Nachname", OxTools.SourceProgram.OS);
            var leg2FieldNames = new string[leg1FieldNames.Length];
            for (int i = 0; i < leg1FieldNames.Length; i++)
            {
                leg2FieldNames[i] = leg1FieldNames[i] + "2";
                leg1FieldNames[i] = leg1FieldNames[i] + "1";
            }
            for (int i = 0; i < start1FieldNames.Length; i++)
            {
                start1FieldNames[i] = start1FieldNames[i] + "1";
            }
            for (int i = 0; i < firstName1FieldNames.Length; i++)
            {
                firstName1FieldNames[i] = firstName1FieldNames[i] + "1";
            }
            for (int i = 0; i < lastName1FieldNames.Length; i++)
            {
                lastName1FieldNames[i] = lastName1FieldNames[i] + "1";
            }

            int fldID = OxTools.GetFieldFromHeader(fields, stoNoFieldNames);
            int fldNumLegs = OxTools.GetFieldFromHeader(fields, numLegsField);
            int fldClub = OxTools.GetFieldFromHeader(fields, clubFieldNames);
            int fldClass = OxTools.GetFieldFromHeader(fields, classFieldNames);
            int fldLeg1 = OxTools.GetFieldFromHeader(fields, leg1FieldNames);
            int fldLeg2 = OxTools.GetFieldFromHeader(fields, leg2FieldNames);
            int fldStart1 = OxTools.GetFieldFromHeader(fields, start1FieldNames);
            int fldFirstName1 = OxTools.GetFieldFromHeader(fields, firstName1FieldNames);
            int fldLastName1 = OxTools.GetFieldFromHeader(fields, lastName1FieldNames);
            int fldClName = OxTools.GetFieldFromHeader(fields, clnameFieldNames);
            int fldDesc = OxTools.GetFieldFromHeader(fields, descFieldNames);

            if (fldID == -1 || fldNumLegs == -1 || fldClub == -1 || fldClass == -1 || fldLeg1 == -1 ||
                fldLeg2 == -1 || fldStart1 == -1 || fldFirstName1 == -1 || fldLastName1 == -1 || fldClName == -1 || fldDesc == -1)
            {
                    throw new IOException("Cannot detect startlist format!");   
            }

            int fieldsPerLeg = fldLeg2 - fldLeg1;

            string temp;
            int row = 0;
            while ((temp = sr.ReadLine()) != null)
            {
                
                string[] parts = temp.Split(SplitChars);
                int numLegs = Convert.ToInt32(parts[fldNumLegs]);

                string club = parts[fldClName].Trim('\"') + " " + parts[fldClub].Trim('\"') + " " + parts[fldDesc].Trim('\"');
                club = club.Trim();
                for (int i = 1; i <= numLegs; i++)
                {
                    int leg = Convert.ToInt32(parts[fldLeg1 + (i - 1)*fieldsPerLeg].Trim('\"'));
                    int stNo = Convert.ToInt32(parts[fldID]);
                    int id = CreateID(leg, stNo);
                    string Class = parts[fldClass].Trim('\"') + "-" + leg;
                    string sstart = parts[fldStart1 + (i - 1)*fieldsPerLeg].Trim('\"');
                    string firstName = parts[fldFirstName1 + (i - 1)*fieldsPerLeg].Trim('\"');
                    string lastName = parts[fldLastName1 + (i - 1) * fieldsPerLeg].Trim('\"');

                    FireOnResult(new RelayResult{
                        ID = id,
                        LegNumber = leg,
                        RunnerName = firstName + " " + lastName,
                        RunnerClub = club,
                        Class = Class,
                        StartTime = string.IsNullOrEmpty(sstart) ? -1 : strTimeToInt(sstart),
                        Time = -1,
                        Status = 9,
                        SplitTimes = null,
                        OverallTime = -1,
                        OverallStatus = 9
                    });
                }
                row++;
            }
        }

        private void ParseOSResultFilt(string[] fields, StreamReader sr, bool isOs2010Files)
        {
            /*Detect OS format*/
            int fldID;
            int fldSI;
            int fldFName;
            int fldEName;
            int fldClub;
            int fldClass;
            int fldStart;
            int fldTime;
            int fldStatus;
            int fldFirstPost;
            int fldLeg;
            int fldFinish;
            int fldTxt1, fldTxt2, fldTxt3;
            int fldTotalTime;
            OxTools.DetectOxCSVFormat(OxTools.SourceProgram.OS, fields, out fldID, out fldSI, out fldFName, out fldEName, out fldClub, out fldClass, out fldStart,
                out fldTime, out fldStatus, out fldFirstPost, out fldLeg, out fldFinish, out fldTxt1, out fldTxt2, out fldTxt3, out fldTotalTime);

            if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
                || fldStart == -1 || fldTime == -1
                || fldStart == -1 || fldFirstPost == -1 || fldLeg == -1)
            {
                throw new IOException("Not OS-formatted file!");
            }

            string tmp;
            var teamStartTimes = new Dictionary<string, int>();
            var teamStatuses = new Dictionary<string, int>();
            while ((tmp = sr.ReadLine()) != null)
            {
                string[] parts = tmp.Split(SplitChars);

                /* check so that the line is not incomplete*/
                int id = CreateID(Convert.ToInt32(parts[fldLeg]), Convert.ToInt32(parts[fldID]));

                string name = parts[fldFName].Trim('\"') + " " + parts[fldEName].Trim('\"');
                string club = parts[fldClub].Trim('\"');
                string Class = parts[fldClass].Trim('\"') + "-" + parts[fldLeg].Trim('\"');
                int leg = Convert.ToInt32(parts[fldLeg].Trim('\"'));
                int start = strTimeToInt(parts[fldStart]);
                int time = strTimeToInt(parts[fldTime]);

                int status = 9;
                try
                {
                    status = Convert.ToInt32(parts[fldStatus]);
                    if (status == 0 && time < 0)
                    {
                        status = 9;
                    }
                }
                catch
                {
                }

                int totalTime = -1;
                int totalStatus = status;

                if (isOs2010Files)
                {
                    if (fldTotalTime != -1)
                    {
                        //OK We have a totaltimefield..
                        //If totaltime set, use it as time (and status is status)
                        //Else, check if runner on course (status == 0 and FinishTime is empty => Status = 9)
                        //Else, something is not right, set status if <> 0, else set mp

                        if (!string.IsNullOrEmpty(parts[fldTotalTime]))
                        {
                            totalTime = strTimeToInt(parts[fldTotalTime]);
                        }
                        else if (status == 9)
                        {
                            //Runner still on course
                        }
                        else
                        {
                            totalStatus = status != 0 ? status : 3;
                            totalTime = -3;
                        }
                    }
                    else
                    {
                        string key = parts[fldClass].Trim('\"') + ";" + club;
                        if (!teamStartTimes.ContainsKey(key))
                        {
                            teamStartTimes.Add(key, start);
                        }
                        else if (teamStartTimes[key] > start)
                        {
                            teamStartTimes[key] = start;
                        }

                        if (teamStatuses.ContainsKey(key))
                        {
                            int earlierStatus = teamStatuses[key];
                            if (status == 0 && earlierStatus != 0)
                            {
                                totalStatus = earlierStatus;
                            }
                        }
                        else if (status != 0)
                        {
                            teamStatuses.Add(key, status);
                        }

                        if (time >= 0)
                        {
                            totalTime = strTimeToInt(parts[fldFinish]) - teamStartTimes[key];
                        }
                    }
                }


                var splittimes = new List<ResultStruct>();
                /*parse splittimes*/
                var codes = new List<int>();
                for (int i = fldFirstPost; i < parts.Length - 4; i++)
                {
                    if (parts[i + 1].Length == 0
                        || parts[i + 2].Length == 0)
                    {
                        i += 3;
                        continue;
                    }
                    var s = new ResultStruct();
                    try
                    {
                        i++;
                        s.ControlCode = Convert.ToInt32(parts[i]);

                        if (s.ControlCode == 999 && (status == 0 || status == 9))
                        {
                            i++;
                            if (totalTime == -1 || totalTime== -9)
                            {
                                totalTime = strTimeToInt(parts[i]);
                            }
                            i++;
                        }
                        else
                        {
                            s.ControlCode += 1000;
                            while (codes.Contains(s.ControlCode))
                            {
                                s.ControlCode += 1000;
                            }
                            codes.Add(s.ControlCode);
                            i++;
                            s.Time = strTimeToInt(parts[i]);
                            i++;
                            s.Place = 0;
                            try
                            {
                                s.Place = Convert.ToInt32(parts[i]);
                            }
                            catch
                            {
                            }

                            splittimes.Add(s);
                        }
                    }
                    catch
                    {
                    }
                }
                FireOnResult(new RelayResult{
                    ID = id,
                    LegNumber = leg,
                    RunnerName = name,
                    RunnerClub = club,
                    Class = Class,
                    StartTime = start,
                    Time = time,
                    Status = status,
                    SplitTimes = splittimes,
                    OverallTime = totalTime,
                    OverallStatus = totalStatus
                });
            }
        }


        public void AnalyzeTeamFile(string filename)
        {
            StreamReader sr = null;
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        sr = new StreamReader(filename, Encoding.Default);
                        break;
                    }
                    catch (Exception ee)
                    {
                        Debug.WriteLine(ee.Message);
                        Thread.Sleep(1000);
                    }
                }
                if (sr == null)
                    throw new ApplicationException("Could not open input file, copy error?");
                string header = sr.ReadLine();
                if (header == null)
                    return;
                string[] fields = header.Split(SplitChars);

                /*Detect OS format*/
                int fldID, fldClub, fldClass, fldStart, fldName;
                
                fldID = Array.IndexOf(fields, "Stno");
                fldName = Array.IndexOf(fields, "Name");
                int fldFirstRunner = Array.IndexOf(fields, "Nachname");
                
                fldClub = Array.IndexOf(fields, "Club");
                fldClass = Array.IndexOf(fields, "Short");
                fldStart = Array.IndexOf(fields, "Start");
                

                if (fldID == -1 || fldClub == -1 || fldClass == -1 || fldName == -1 || fldFirstRunner == -1)
                {
                    /*Try detect swedish format??*/
                    fldID = Array.IndexOf(fields, "Startnr");
                    fldName = Array.IndexOf(fields, "Namn");
                    fldFirstRunner = Array.IndexOf(fields, "Efternamn");
                    
                    fldClub = Array.IndexOf(fields, "Ort");
                    fldClass = Array.IndexOf(fields, "Kort");
                    fldStart = Array.IndexOf(fields, "Start");

                    if (fldID == -1 || fldClub == -1 || fldClass == -1 || fldName == -1)
                    {
                        throw new IOException("Not OS-formatted file!");
                    }
                }

                string tmp;
                while ((tmp = sr.ReadLine()) != null)
                {
                    string[] parts = tmp.Split(SplitChars);

                    /* check so that the line is not incomplete*/
                    /*int id = Convert.ToInt32(parts[fldLeg]) * 1000000 + Convert.ToInt32(parts[fldID]);
                    int si = 0;
                    try
                    {
                        si = Convert.ToInt32(parts[fldSI]);
                    }
                    catch (Exception ee)
                    { }*/
                    //string name = parts[fldFName].Trim('\"') + " " + parts[fldEName].Trim('\"');
                    string club = parts[fldClub].Trim('\"');
                    string nameExtra = parts[fldName].Trim('\"');
                    if (nameExtra.Length > 0)
                        club += " " + nameExtra;
                    string Class = parts[fldClass].Trim('\"');// +"-" + parts[fldLeg].Trim('\"');
                    int start = strTimeToInt(parts[fldStart]);
                    //int time = strTimeToInt(parts[fldTime]);
                    int fldNextRunner = fldFirstRunner;
                    int leg = 1;
                    int teamPrevStatus = 0;
                    while (fldNextRunner >= 0)
                    {
                        string name = parts[fldNextRunner + 1].Trim('\"') + " " + parts[fldNextRunner].Trim('\"');
                        int time = strTimeToInt(parts[fldNextRunner + 5]);
                        if (time >= 0)
                        {
                            time = time - start;
                        }

                       

                        int status = 9;
                        try
                        {
                            status = Convert.ToInt32(parts[fldNextRunner + 7]);

                            if (status == 0 && time < 0)
                                status = 9;
                        }
                        catch
                        {
                        }

                        if (status == 0 && teamPrevStatus > 0)
                        {
                            status = teamPrevStatus;
                        }
                        else if (status > 0 && teamPrevStatus == 0)
                        {
                            teamPrevStatus = status;
                        }

                        int id = CreateID(leg, Convert.ToInt32(parts[fldID]));
                        FireOnResult(new Result
                        {
                            ID = id,
                            RunnerName = name,
                            RunnerClub = club,
                            Class = Class + "-" + leg,
                            StartTime = 0,
                            Time = time,
                            Status = status
                        });

                        fldNextRunner += 11;
                        if (fldNextRunner + 8 > parts.Length || parts[fldNextRunner].Trim('\"').Trim().Length == 0)
                        {
                            fldNextRunner = -1;
                            break;
                        }
                        leg++;

                    }
                }
            }
            catch (Exception ee)
            {
                FireLogMsg("ERROR in OEPArser: " + ee.Message);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

        }

        private int strTimeToInt(string time)
        {
            try
            {
                /* format is: 18:15,0*/
                string[] parts = time.Split(':');
                int totalTime = 0;
                totalTime += (int)(Convert.ToDouble(parts[parts.Length - 1]) * 100);

                int mod = 6000;
                for (int i = parts.Length - 2; i >= 0; i--)
                {
                    totalTime += Convert.ToInt32(parts[i]) * mod;
                    mod *= 60;
                }
                return totalTime;
            }
            catch (Exception ee)
            {
                return -9;
            }
            
        }
    }
}
