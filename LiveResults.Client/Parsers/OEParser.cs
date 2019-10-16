﻿using LiveResults.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LiveResults.Client
{
    //public delegate void ResultDelegate(int id, int SI, string name, string club, string Class, int start, int time, int status, List<ResultStruct> splits);
    
    public class OEParser
    {
        public event ResultDelegate OnResult;
        public event LogMessageDelegate OnLogMessage;
        public static char[] SplitChars = new char[] { ';', '\t' };
        private string m_Directory;
        public OEParser()
        {
        }

        public OEParser(string directory)
        {
            m_Directory = directory;
            System.IO.FileSystemWatcher fsWatcher = new System.IO.FileSystemWatcher(directory);
            fsWatcher.EnableRaisingEvents = true;
            fsWatcher.Renamed += new System.IO.RenamedEventHandler(fsWatcher_Renamed);
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

        void fsWatcher_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("File Renamed: " + e.OldName + " to " + e.Name);
            AnalyzeFile(e.FullPath);
            System.Diagnostics.Debug.WriteLine("File analysed");
            System.IO.File.Delete(e.FullPath);
        }


        public void AnalyzeFile(string filename, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.Default;

            System.IO.StreamReader sr =null;
            try
            {
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        sr = new System.IO.StreamReader(filename, encoding);
                        break;
                    }
                    catch (Exception ee)
                    {
                        System.Diagnostics.Debug.WriteLine(ee.Message);
                    }
                }
                if (sr == null)
                    throw new ApplicationException("Could not open input file, copy error?");
                string header = sr.ReadLine();

                string[] fields = header.Split(SplitChars);

                /*Detect OE format*/
                int fldID, fldSI, fldFName, fldEName, fldClub, fldClass, fldStart, fldTime, fldStatus, fldFirstPost, fldText1, fldText2, fldText3, fldFinish, fldLeg;
                int fldTotalTime;
                OxTools.DetectOxCSVFormat(OxTools.SourceProgram.OE, fields, out fldID, out fldSI, out fldFName, out fldEName, out fldClub, out fldClass, out fldStart, out fldTime, out fldStatus, out fldFirstPost, out fldLeg, out fldFinish, out fldText1, out fldText2, out fldText3, out fldTotalTime);

                if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
           || fldStart == -1 || fldTime == -1
           || fldStart == -1 || fldFirstPost == -1)
                {
                    throw new System.IO.IOException("Not OE-formatted file!");
                }

                fldText1 = Array.IndexOf(fields, "Text1");
                fldText2 = Array.IndexOf(fields, "Text2");
                fldText3 = Array.IndexOf(fields, "Text3");




                if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
                    || fldStart == -1 || fldTime == -1)
                    throw new System.IO.IOException("Not OE-formatted file!");

                string tmp;
                while ((tmp = sr.ReadLine()) != null)
                {
                    string[] parts = tmp.Split(SplitChars);

                    /* check so that the line is not incomplete*/

                    int id;
                    if (!int.TryParse(parts[fldID], NumberStyles.Any, CultureInfo.InvariantCulture,out id))
                    {
                        FireLogMsg("Error: Could not use \"" + parts[fldID] + "\" as ID for runner " + parts[fldFName] + " " + parts[fldEName] +
                                           " - Skipping!, value taken from field: " + fields[fldID]);
                        continue;
                    }
                    string name = parts[fldFName].Trim('\"') + " " + parts[fldEName].Trim('\"');
                    string club = parts[fldClub].Trim('\"');
                    string Class = parts[fldClass].Trim('\"');
                    int start = strTimeToInt(parts[fldStart]);
                    int time = strTimeToInt(parts[fldTime]);


                    int status = 9;
                    try
                    {
                        status = Convert.ToInt32(parts[fldStatus]);
                        if (status == 0 && time < 0)
                            status = 9;
                    }
                    catch (Exception ee)
                    {
                        FireLogMsg("Could not use status " + parts[fldStatus] + " for runner " + name + ", assuming status 0");
                    }

                    List<ResultStruct> splittimes = new List<ResultStruct>();
                    /*parse splittimes*/
                    List<int> codes = new List<int>();
                    if (fldFirstPost >= 0)
                    {
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

                                if (s.ControlCode == 999)
                                {                                    
                                    i++;
                                    if (time == -1 && status == 0)
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

                                    if (s.Time > 0)
                                    {
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
                            }
                            catch
                            {
                            }

                        }
                    }
                    FireOnResult(new Result()
                    {
                        ID = id,
                        RunnerName = name,
                        RunnerClub = club,
                        Class = Class,
                        StartTime = start,
                        Time = time,
                        Status = status,
                        SplitTimes = splittimes
                    });
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
                /*CHANGED TO DECIMAL*/
                totalTime += (int)(Convert.ToDecimal(parts[parts.Length - 1]) * 100);

                int mod = 6000;
                for (int i = parts.Length - 2; i >= 0; i--)
                {
                    totalTime += Int32.Parse(parts[i]) * mod;
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
