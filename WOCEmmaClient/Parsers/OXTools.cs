using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LiveResults.Client.Parsers
{
    /// <summary>
    /// Common tools for parsing OS/OE files
    /// </summary>
    public class OxTools
    {
        public enum SourceProgram { OE, OS };
        internal static void DetectOxCSVFormat(SourceProgram source, string[] fields, out int fldID, out int fldSI, out int fldFName, out int fldEName, out int fldClub, 
            out int fldClass, out int fldStart, out int fldTime, out int fldStatus, out int fldFirstPost, out int fldLeg, out int fldFinish, 
            out int fldTxt1, out int fldTxt2, out int fldTxt3, out int fldTotalTime)
        {

            string[] stoNoFieldNames = GetOEStringsForKey("Stnr", source);
            string[] legFieldNames = GetOEStringsForKey("Lnr", source); 
            string[] chipNoFieldNames = GetOEStringsForKey(source == SourceProgram.OE ? "Chipnr" : "ChipNr", source);
            string[] firstNameFieldNames = GetOEStringsForKey("Vorname", source);
            string[] lastNameFieldNames = GetOEStringsForKey("Nachname", source);
            string[] clubFieldNames = GetOEStringsForKey(source == SourceProgram.OE ? "Ort" : "Staffel", source); 
            string[] classFieldNames = GetOEStringsForKey("Kurz", source);
            string[] startFieldNames = GetOEStringsForKey("Start", source); 
            string[] finishFieldNames = GetOEStringsForKey("Ziel", source); 
            string[] timeFieldNames =  GetOEStringsForKey("Zeit", source); 
            string[] statusFieldNames = GetOEStringsForKey("Wertung", source);

            string[] noFieldNames = GetOEStringsForKey("Nr", source);
            var no1FieldNames = new string[noFieldNames.Length];
            for (int i = 0; i < no1FieldNames.Length; i++)
                no1FieldNames[i] = noFieldNames[i] + "1";
            string[] totalTimeFieldNames = GetOEStringsForKey("Gesamtzeit", source); 

            string[] txtFields = GetOEStringsForKey("Text", source);

            var txt1Fields = new string[txtFields.Length];
            var txt2Fields = new string[txtFields.Length];
            var txt3Fields = new string[txtFields.Length];
            for (int i = 0; i < txtFields.Length; i++)
            {
                txt1Fields[i] = txtFields[i] + "1";
                txt2Fields[i] = txtFields[i] + "2";
                txt3Fields[i] = txtFields[i] + "3";
            }

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
            fldTxt1 = GetFieldFromHeader(fields, txt1Fields);
            fldTxt2 = GetFieldFromHeader(fields, txt2Fields);
            fldTxt3 = GetFieldFromHeader(fields, txt3Fields);
            fldTotalTime = GetFieldFromHeader(fields, totalTimeFieldNames);

            if (fldID == -1 || fldSI == -1 || fldFName == -1 || fldEName == -1 || fldClub == -1 || fldClass == -1
                || fldStart == -1 || fldTime == -1
                || fldStart == -1 || fldFirstPost == -1 || ( source == SourceProgram.OS || fldLeg == -1))
            {
                /*Try detect fixedFormat*/
                if (fields[0] == "OS0016")
                {
                    fldID = 1;
                    fldLeg = 4;
                    fldSI = 14;
                    fldFName = 7;
                    fldEName = 6;
                    fldClub = 18;
                    fldClass = 20;
                    fldStart = 10;
                    fldFinish = 11;
                    fldTime = 12;
                    fldStatus = 13;
                    fldFirstPost = 32;
                    fldTotalTime = 30;
                }
                else if (fields[0] == "OS0012")
                {
                    fldID = 1;
                    fldLeg = 4;
                    fldSI = 14;
                    fldFName = 7;
                    fldEName = 6;
                    fldClub = 18;
                    fldClass = 20;
                    fldStart = 10;
                    fldFinish = 11;
                    fldTime = 12;
                    fldStatus = 13;
                    fldFirstPost = 31;
                }
                else if (fields[0] == "OE0016")
                {
                    fldID = 1;
                    fldLeg = -1;
                    fldSI = 3;
                    fldFName = 6;
                    fldEName = 5;
                    fldClub = 20;
                    fldClass = 25;
                    fldStart = 11;
                    fldFinish = 12;
                    fldTime = 13;
                    fldStatus = 14;
                    fldFirstPost = 59;
                }
            }
        }

        private static int GetFieldFromHeader(string[] fields, IEnumerable<string> fieldNames)
        {
            int fld = -1;
            foreach (string t in fieldNames)
            {
                fld = Array.IndexOf(fields, t);
                if (fld >= 0)
                    break;
            }
            return fld;
        }

        static readonly Dictionary<string, string[]> m_lookupCacheOE = new Dictionary<string, string[]>();
        static readonly Dictionary<string, string[]> m_lookupCacheOS = new Dictionary<string, string[]>();
        public static string[] GetOEStringsForKey(string key, SourceProgram source)
        {
            var cache = source == SourceProgram.OE ? m_lookupCacheOE : m_lookupCacheOS;
            if (cache.ContainsKey(key))
                return cache[key];


            var texts = new List<string>();

            string file = source == SourceProgram.OE ? "LiveResults.Client.OLEinzel.mlf" : "LiveResults.Client.OLStaffel.mlf";

            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
            {
                if (s != null)
                {
                    using (var sr = new StreamReader(s, Encoding.GetEncoding("iso-8859-1")))
                    {
                        string temp;
                        while ((temp = sr.ReadLine()) != null)
                        {
                            string[] parts = temp.Split(new string[]
                            {
                                "¦"
                            }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts[0] == key)
                            {
                                texts.AddRange(parts);
                                break;
                            }
                        }
                    }
                }
            }

            cache.Add(key, texts.ToArray());
            return texts.ToArray();
        }

    }
}
