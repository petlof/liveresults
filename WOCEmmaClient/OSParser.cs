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

                /*Detect OS format*/
                int fldID, fldSI, fldFName, fldEName, fldClub, fldClass, fldStart, fldTime, fldStatus, fldFirstPost, fldLeg;
                fldID = Array.IndexOf(fields, "Stno");
                fldLeg = Array.IndexOf(fields, "Leg");
                fldSI = Array.IndexOf(fields, "SI card");
                if (fldSI == -1)
                {
                    fldSI = Array.IndexOf(fields, "Chip");
                }
                fldFName = Array.IndexOf(fields, "Vorname");
                fldEName = Array.IndexOf(fields, "Nachname");
                fldClub = Array.IndexOf(fields, "Club");
                fldClass = Array.IndexOf(fields, "Short");
                fldStart = Array.IndexOf(fields, "Start");
                fldTime = Array.IndexOf(fields, "Time");
                fldStatus = Array.IndexOf(fields, "Wertung");
                fldFirstPost = Array.IndexOf(fields, "No1");

                if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
                    || fldStart == -1 || fldTime == -1
                    || fldStart == -1 || fldFirstPost == -1 || fldLeg == -1)
                {
                    /*Try detect swedish format??*/
                    fldID = Array.IndexOf(fields, "Startnr");
                    fldLeg = Array.IndexOf(fields, "Sträcka");
                    fldSI = Array.IndexOf(fields, "SI bricka");
                    if (fldSI == -1)
                    {
                        fldSI = Array.IndexOf(fields, "Bricka");
                    }
                    fldFName = Array.IndexOf(fields, "Vorname");
                    fldEName = Array.IndexOf(fields, "Nachname");
                    fldClub = Array.IndexOf(fields, "Klubb");
                    fldClass = Array.IndexOf(fields, "Kort");
                    fldStart = Array.IndexOf(fields, "Start");
                    fldTime = Array.IndexOf(fields, "Tid");
                    fldStatus = Array.IndexOf(fields, "Wertung");
                    fldFirstPost = Array.IndexOf(fields, "Nr1");
                    
                    if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
                   || fldStart == -1 || fldTime == -1
                   || fldStart == -1 || fldFirstPost == -1 || fldLeg == -1)
                    {
                        throw new System.IO.IOException("Not OS-formatted file!");
                    }
                }

                string tmp;
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
                    FireOnResult(id,si,name,club,Class,start,time,status,splittimes);
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
