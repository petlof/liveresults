using System;
using System.Collections.Generic;
using System.Text;

namespace WOCEmmaClient
{
    
    public class OSParser
    {
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;
        public static char[] SplitChars = new char[] { ';', '\t' };
        private string m_Directory;
        
        public OSParser()
        {
        }
        public OSParser(string directory)
        {
            m_Directory = directory;
            System.IO.FileSystemWatcher fsWatcher = new System.IO.FileSystemWatcher(directory);
            fsWatcher.EnableRaisingEvents = true;
            fsWatcher.Renamed += new System.IO.RenamedEventHandler(fsWatcher_Renamed);
        }

        public void Start()
        {
            string[] files = System.IO.Directory.GetFiles(m_Directory, "*.csv");
            foreach (string f in files)
            {
                AnalyzeFile(System.IO.Path.Combine(m_Directory, f));
                System.IO.File.Delete(System.IO.Path.Combine(m_Directory, f));
            }

        }

        private void FireOnResult(int id, int SI, string name, string club, string Class, int start, int time, int status, List<ResultStruct> results)
        {
            if (OnResult != null)
            {
                OnResult(id, SI, name, club, Class, start,time,status, results);
            }
        }
        private void FireLogMsg(string msg)
        {
            if (OnLogMessage != null)
                OnLogMessage(msg);
        }

        void fsWatcher_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("File Renamed: " + e.OldName + " to " + e.Name);
            AnalyzeFile(e.FullPath);
            System.Diagnostics.Debug.WriteLine("File analysed");
            System.IO.File.Delete(e.FullPath);
        }

        public void AnalyzeFile(string filename)
        {
            System.IO.StreamReader sr =null;
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        sr = new System.IO.StreamReader(filename, Encoding.Default);
                        break;
                    }
                    catch (Exception ee)
                    {
                        System.Diagnostics.Debug.WriteLine(ee.Message);
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                if (sr == null)
                    throw new ApplicationException("Could not open input file, copy error?");
                string header = sr.ReadLine();

                string[] fields = header.Split(SplitChars);
                bool isOs2010Files = fields[0].StartsWith("OS");

                /*Detect OS format*/
                int fldID, fldSI, fldFName, fldEName, fldClub, fldClass, fldStart, fldTime, fldStatus, fldFirstPost, fldLeg, fldFinish;

                string[] stoNoFieldNames = { "Stno", "Lnro", "Stnr", "Startnr" };
                string[] legFieldNames = {"Leg","Osuus","Lnr","Sträcka"};


                string[] chipNoFieldNames = {
                                            "SI card",
                                            "SI bricka",
                                            "Bricka",
                                            "Chipno", //OS2010 - ENG
                                            "Korttinro", //OS2010 - FIN
                                            "Chipnr", //OS2010 - GER
                                            "Bricknr" //OS2010 - SWE
                                        };
                string[] firstNameFieldNames = {"Vorname","First name", "Etunimi","Förnamn"};
                string[] lastNameFieldNames = { "Nachname", "Surname", "Sukunimi", "Efternamn" };
                string[] clubFieldNames = { "Club", "Team", "Joukkue", "Staffel", "Lag", "Klubb" };
                string[] classFieldNames = { "Short", "Lyhyt", "Kurz", "Kort" };
                string[] startFieldNames = { "Start", "Lähtö" };
                string[] finishFieldNames = { "Finish", "Maali", "Ziel", "Mål" };
                string[] timeFieldNames = { "Time", "Aika", "Zeit", "Tid" };
                string[] statusFieldNames = { "Classifier", "Tila", "Wertung", "Status" };
                string[] no1FieldNames = { "No1", "Nro1", "Nr1" };

                fldID = GetFieldFromHeader(fields, stoNoFieldNames);
                fldLeg = GetFieldFromHeader(fields, legFieldNames);
                fldSI = GetFieldFromHeader(fields, chipNoFieldNames);
                fldFName = GetFieldFromHeader(fields, firstNameFieldNames);
                fldEName = GetFieldFromHeader(fields, lastNameFieldNames);
                fldClub = GetFieldFromHeader(fields, clubFieldNames);

                fldClass = GetFieldFromHeader(fields, classFieldNames);
                fldStart = GetFieldFromHeader(fields, startFieldNames);
                fldFinish = GetFieldFromHeader(fields, finishFieldNames);
                fldTime = GetFieldFromHeader(fields, timeFieldNames);
                fldStatus = GetFieldFromHeader(fields, statusFieldNames);

                fldFirstPost = GetFieldFromHeader(fields, no1FieldNames);

                if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
                    || fldStart == -1 || fldTime == -1
                    || fldStart == -1 || fldFirstPost == -1 || fldLeg == -1)
                {
                    /*Try detect fixedFormat*/
                    if (fields[0] == "OS0016")
                    {
                        fldID = 1;
                        fldLeg = 3;
                        fldSI = 13;
                        fldFName = 6;
                        fldEName = 5;
                        fldClub = 17;
                        fldClass = 19;
                        fldStart = 9;
                        fldFinish = 10;
                        fldTime = 11;
                        fldStatus = 12;
                        fldFirstPost = 27;
                    }
                    
                    if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
                   || fldStart == -1 || fldTime == -1
                   || fldStart == -1 || fldFirstPost == -1 || fldLeg == -1)
                    {
                        throw new System.IO.IOException("Not OS-formatted file!");
                    }
                }

                string tmp;
                Dictionary<string, int> teamStartTimes = new Dictionary<string, int>();
                Dictionary<string, int> teamStatuses = new Dictionary<string, int>();
                while ((tmp = sr.ReadLine()) != null)
                {
                    string[] parts = tmp.Split(SplitChars);

                    /* check so that the line is not incomplete*/
                    int id = Convert.ToInt32(parts[fldLeg])*1000000 + Convert.ToInt32(parts[fldID]);                    
                    int si = 0;
                    try
                    {
                        si = Convert.ToInt32(parts[fldSI]);
                    }
                    catch (Exception ee)
                    { }
                    string name = parts[fldFName].Trim('\"') + " " + parts[fldEName].Trim('\"');
                    string club = parts[fldClub].Trim('\"');
                    string Class = parts[fldClass].Trim('\"') + "-" + parts[fldLeg].Trim('\"');
                    int start = strTimeToInt(parts[fldStart]);
                    int time = strTimeToInt(parts[fldTime]);

                    int status = 0;
                    try
                    {
                        status = Convert.ToInt32(parts[fldStatus]);
                        if (status != 0)
                        {
                            bool test = true;
                        }
                    }
                    catch
                    {
                    }

                    if (isOs2010Files)
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
                                status = earlierStatus;
                        }
                        else if (status != 0)
                        {
                            teamStatuses.Add(key, status);
                        }

                        if (time >= 0)
                        {
                            time = strTimeToInt(parts[fldFinish]) - teamStartTimes[key];
                        }
                    }

                    

                    List<ResultStruct> splittimes = new List<ResultStruct>();
                    /*parse splittimes*/
                    List<int> codes = new List<int>();
                    for (int i = fldFirstPost; i < parts.Length - 4; i++)
                    {
                        if (parts[i + 1].Length == 0
                            || parts[i + 2].Length == 0)
                        {
                            i += 3;
                            continue;
                        }
                        ResultStruct s = new ResultStruct();
                        try
                        {
                            //s.ControlNo = Convert.ToInt32(parts[i]);
                            i++;
                            s.ControlCode = Convert.ToInt32(parts[i]);

                            if (s.ControlCode == 999 && status == 0)
                            {
                                i++;
                                if (time == -1)
                                    time = strTimeToInt(parts[i]);
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
                                { }

                                splittimes.Add(s);
                            }
                        }
                        catch
                        {
                        }
                        
                    }
                    FireOnResult(id,si,name,club,Class,start,time,status,splittimes);
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

        private static int GetFieldFromHeader(string[] fields, string[] fieldNames)
        {
            int fld = -1;
            for (int i = 0; i < fieldNames.Length; i++)
            {

                fld = Array.IndexOf(fields, fieldNames[i]);
                if (fld >= 0)
                    break;
            }
            return fld;
        }

        public void AnalyzeTeamFile(string filename)
        {
            System.IO.StreamReader sr = null;
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        sr = new System.IO.StreamReader(filename, Encoding.Default);
                        break;
                    }
                    catch (Exception ee)
                    {
                        System.Diagnostics.Debug.WriteLine(ee.Message);
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                if (sr == null)
                    throw new ApplicationException("Could not open input file, copy error?");
                string header = sr.ReadLine();

                string[] fields = header.Split(SplitChars);

                /*Detect OS format*/
                int fldID, fldSI, fldFName, fldEName, fldClub, fldClass, fldStart, fldTime, fldStatus, fldFirstPost, fldName;
                int fldFirstRunner = -1;
                fldID = Array.IndexOf(fields, "Stno");
                fldName = Array.IndexOf(fields, "Name");
                fldFirstRunner = Array.IndexOf(fields, "Nachname");
                /*fldSI = Array.IndexOf(fields, "SI card");
                if (fldSI == -1)
                {
                    fldSI = Array.IndexOf(fields, "Chip");
                }
                fldFName = Array.IndexOf(fields, "Vorname");
                fldEName = Array.IndexOf(fields, "Nachname");*/
                fldClub = Array.IndexOf(fields, "Club");
                fldClass = Array.IndexOf(fields, "Short");
                fldStart = Array.IndexOf(fields, "Start");
                /*fldTime = Array.IndexOf(fields, "Time");
                fldStatus = Array.IndexOf(fields, "Wertung");
                fldFirstPost = Array.IndexOf(fields, "No1");*/

                if (fldID == -1 || fldClub == -1 || fldClass == -1 || fldName == -1 || fldFirstRunner == -1)
                {
                    /*Try detect swedish format??*/
                    fldID = Array.IndexOf(fields, "Startnr");
                    fldName = Array.IndexOf(fields, "Namn");
                    fldFirstRunner = Array.IndexOf(fields, "Efternamn");
                    /*fldSI = Array.IndexOf(fields, "SI bricka");
                    if (fldSI == -1)
                    {
                        fldSI = Array.IndexOf(fields, "Bricka");
                    }
                    fldFName = Array.IndexOf(fields, "Vorname");
                    fldEName = Array.IndexOf(fields, "Nachname");*/
                    fldClub = Array.IndexOf(fields, "Ort");
                    fldClass = Array.IndexOf(fields, "Kort");
                    fldStart = Array.IndexOf(fields, "Start");
                    /*fldTime = Array.IndexOf(fields, "Tid");
                    fldStatus = Array.IndexOf(fields, "Wertung");
                    fldFirstPost = Array.IndexOf(fields, "Nr1");*/

                    if (fldID == -1 || fldClub == -1 || fldClass == -1 || fldName == -1)
                    {
                        throw new System.IO.IOException("Not OS-formatted file!");
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

                        int si = 0;
                        try
                        {
                            si = Convert.ToInt32(parts[fldNextRunner + 8]);
                        }
                        catch (Exception ee)
                        {
                        }

                        int status = 0;
                        try
                        {
                            status = Convert.ToInt32(parts[fldNextRunner + 7]);
                            if (status != 0)
                            {
                                bool test = true;
                            }
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

                        int id = leg * 1000000 + Convert.ToInt32(parts[fldID]);
                        FireOnResult(id, si, name, club, Class + "-" + leg, 0, time, status, null);

                        fldNextRunner += 11;
                        if (fldNextRunner + 8 > parts.Length || parts[fldNextRunner].Trim('\"').Trim().Length == 0)
                        {
                            fldNextRunner = -1;
                            break;
                        }
                        leg++;

                    }

                    /*List<ResultStruct> splittimes = new List<ResultStruct>();*/
                    /*parse splittimes*/
                    /*List<int> codes = new List<int>();
                    for (int i = fldFirstPost; i < parts.Length - 4; i++)
                    {
                        if (parts[i + 1].Length == 0
                            || parts[i + 2].Length == 0)
                        {
                            i += 3;
                            continue;
                        }
                        ResultStruct s = new ResultStruct();
                        try
                        {
                            //s.ControlNo = Convert.ToInt32(parts[i]);
                            i++;
                            s.ControlCode = Convert.ToInt32(parts[i]);
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
                            { }

                            splittimes.Add(s);
                        }
                        catch
                        {
                        }

                    }
                    FireOnResult(id, si, name, club, Class, start, time, status, splittimes);*/
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
                return -1;
            }
            
        }
    }
}
