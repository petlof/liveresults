using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LiveResults.Client.Parsers
{
    public class IofXmlParser
    {
        public static Runner[] ParseFile(string filename, LogMessageDelegate logit, GetIdDelegate getIdFunc, bool readRadioControls, out RadioControl[] radioControls)
        {
            return ParseFile(filename, logit, true, getIdFunc, readRadioControls,out radioControls);
        }

        public static Runner[] ParseFile(string filename, LogMessageDelegate logit, bool deleteFile, GetIdDelegate getIdFunc, bool readRadioControls, out RadioControl[] radioControls)
        {
            byte[] fileContents;
            radioControls = null;
            if (!File.Exists(filename))
            {
                return null;
            }

            fileContents = File.ReadAllBytes(filename);

            if (deleteFile)
                File.Delete(filename);

            return ParseXmlData(fileContents, logit, deleteFile, getIdFunc, readRadioControls, out radioControls);

        }

        public static Runner[] ParseXmlData(byte[] xml, LogMessageDelegate logit, bool deleteFile, GetIdDelegate getIdFunc, bool readRadioControls, out RadioControl[] radioControls)
        {
            Runner[] runners;

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

            //Detect IOF-XML version..
            if (xmlDoc.DocumentElement.Attributes["iofVersion"] != null && xmlDoc.DocumentElement.Attributes["iofVersion"].Value != null && xmlDoc.DocumentElement.Attributes["iofVersion"].Value.StartsWith("3."))
            {
                runners = IofXmlV3Parser.ParseXmlData(xmlDoc, logit, deleteFile, getIdFunc, readRadioControls, out radioControls);
            }
            else
            {
                radioControls = null;
                //Fallback to 2.0
                runners = IOFXmlV2Parser.ParseXmlData(xmlDoc, logit, deleteFile, getIdFunc);
            }

            return runners;
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
                        return (int)id;
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
            return dbid;
        }
    }
}
