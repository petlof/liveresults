using System;
using System.Collections.Generic;
using System.Text;

namespace LiveResults.Client
{
    /// <summary>
    /// Common tools for parsing OS/OE files
    /// </summary>
    public class OXTools
    {
        internal static void DetectOXCSVFormat(string[] fields, out int fldID, out int fldSI, out int fldFName, out int fldEName, out int fldClub, out int fldClass, out int fldStart, out int fldTime, out int fldStatus, out int fldFirstPost, out int fldLeg, out int fldFinish, out int fldTxt1, out int fldTxt2, out int fldTxt3, out int fldTotalTime)
        {
            string[] stoNoFieldNames = { "Stno", "Lnro", "Stnr", "Startnr" };
            string[] legFieldNames = { "Leg", "Osuus", "Lnr", "Sträcka" };
            string[] chipNoFieldNames = {
                                            "SI card",
                                            "SI bricka",
                                            "Bricka",
                                            "Chipno", //OS2010 - ENG
                                            "Korttinro", //OS2010 - FIN
                                            "Chipnr", //OS2010 - GER
                                            "Bricknr", //OS2010 - SWE
                                            "Chip"
                                        };
            string[] firstNameFieldNames = { "Vorname", "First name", "Etunimi", "Förnamn" };
            string[] lastNameFieldNames = { "Nachname", "Surname", "Sukunimi", "Efternamn" };
            string[] clubFieldNames = { "Club", "Team", "Joukkue", "Staffel", "Lag", "Klubb", "Ort", "City" };
            string[] classFieldNames = { "Short", "Lyhyt", "Kurz", "Kort" };
            string[] startFieldNames = { "Start", "Lähtö" };
            string[] finishFieldNames = { "Finish", "Maali", "Ziel", "Mål" };
            string[] timeFieldNames = { "Time", "Aika", "Zeit", "Tid" };
            string[] statusFieldNames = { "Classifier", "Tila", "Wertung", "Status" };
            string[] no1FieldNames = { "No1", "Nro1", "Nr1" };
            string[] totalTimeFieldNames = { "Total tid", "Kokonaisaika", "Overall time" };
            string[] txt1Fields = { "Text1" };
            string[] txt2Fields = { "Text2" };
            string[] txt3Fields = { "Text3" };

            fldTotalTime = -1;

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
                || fldStart == -1 || fldFirstPost == -1 || fldLeg == -1)
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
                    fldFirstPost = 58;
                }
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

    }
}
