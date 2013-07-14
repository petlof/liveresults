using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveResults.Client.Tests
{
    [TestClass]
    public class OEParserTests
    {
        [TestMethod]
        public void TestLookupOEStringsStNo()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Stnr", OXTools.SourceProgram.OE);
            Assert.IsTrue(res.Contains("Stno"));
            Assert.IsTrue(res.Contains("Lnro"));
            Assert.IsTrue(res.Contains("Stnr"));
            Assert.IsTrue(res.Contains("Startnr"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Stnr", OXTools.SourceProgram.OS);
            Assert.IsTrue(res.Contains("Stno"));
            Assert.IsTrue(res.Contains("Lnro"));
            Assert.IsTrue(res.Contains("Stnr"));
            Assert.IsTrue(res.Contains("Startnr"));
        }


        [TestMethod]
        public void TestLookupOEStringsLeg()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Lnr", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Lnr"));
            Assert.IsTrue(res.Contains("Leg"));
            Assert.IsTrue(res.Contains("Osuus"));
            Assert.IsTrue(res.Contains("Sträcka"));
        }

        [TestMethod]
        public void TestLookupOEStringsChipNo()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Chipnr", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Chipnr"));
            Assert.IsTrue(res.Contains("Chipno"));
            Assert.IsTrue(res.Contains("Korttinro"));
            Assert.IsTrue(res.Contains("Bricknr"));
            Assert.IsTrue(res.Contains("Chip"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("ChipNr", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("ChipNr"));
            Assert.IsTrue(res.Contains("Chipno"));
            Assert.IsTrue(res.Contains("Korttinro"));
            Assert.IsTrue(res.Contains("Bricknr"));
            Assert.IsTrue(res.Contains("Chip"));
        }

        [TestMethod]
        public void TestLookupOEStringsClub()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Ort", OXTools.SourceProgram.OE);
            Assert.IsTrue(res.Contains("Ort"));
            Assert.IsTrue(res.Contains("City"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Staffel", OXTools.SourceProgram.OS);
            Assert.IsTrue(res.Contains("Staffel"));
            Assert.IsTrue(res.Contains("Team"));
            Assert.IsTrue(res.Contains("Lag"));
            Assert.IsTrue(res.Contains("Joukkue"));
        }
        
        [TestMethod]
        public void TestLookupOEStringsFirstName()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Vorname", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Vorname"));
            Assert.IsTrue(res.Contains("First name"));
            Assert.IsTrue(res.Contains("Etunimi"));
            Assert.IsTrue(res.Contains("Förnamn"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Vorname", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Vorname"));
            Assert.IsTrue(res.Contains("First name"));
            Assert.IsTrue(res.Contains("Etunimi"));
            Assert.IsTrue(res.Contains("Förnamn"));
        }
        [TestMethod]
        public void TestLookupOEStringsLastName()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Nachname", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Nachname"));
            Assert.IsTrue(res.Contains("Surname"));
            Assert.IsTrue(res.Contains("Sukunimi"));
            Assert.IsTrue(res.Contains("Efternamn"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Nachname", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Nachname"));
            Assert.IsTrue(res.Contains("Surname"));
            Assert.IsTrue(res.Contains("Sukunimi"));
            Assert.IsTrue(res.Contains("Efternamn"));
        }
        [TestMethod]
        public void TestLookupOEStringsCLass()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Kurz", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Kurz"));
            Assert.IsTrue(res.Contains("Short"));
            Assert.IsTrue(res.Contains("Lyhyt"));
            Assert.IsTrue(res.Contains("Kort"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Kurz", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Kurz"));
            Assert.IsTrue(res.Contains("Short"));
            Assert.IsTrue(res.Contains("Lyhyt"));
            Assert.IsTrue(res.Contains("Kort"));
        }
        [TestMethod]
        public void TestLookupOEStringsStart()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Start", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Start"));
            Assert.IsTrue(res.Contains("Lähtö"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Start", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Start"));
            Assert.IsTrue(res.Contains("Lähtö"));
        }
        [TestMethod]
        public void TestLookupOEStringsFinish()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Ziel", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Ziel"));
            Assert.IsTrue(res.Contains("Finish"));
            Assert.IsTrue(res.Contains("Maali"));
            Assert.IsTrue(res.Contains("Mål"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Ziel", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Ziel"));
            Assert.IsTrue(res.Contains("Finish"));
            Assert.IsTrue(res.Contains("Maali"));
            Assert.IsTrue(res.Contains("Mål"));
        }
        [TestMethod]
        public void TestLookupOEStringsTime()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Zeit", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Zeit"));
            Assert.IsTrue(res.Contains("Time"));
            Assert.IsTrue(res.Contains("Aika"));
            Assert.IsTrue(res.Contains("Tid"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Zeit", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Zeit"));
            Assert.IsTrue(res.Contains("Time"));
            Assert.IsTrue(res.Contains("Aika"));
            Assert.IsTrue(res.Contains("Tid"));
        }
        [TestMethod]
        public void TestLookupOEStringsStatus()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Wertung", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Wertung"));
            Assert.IsTrue(res.Contains("Classifier"));
            Assert.IsTrue(res.Contains("Tila"));
            Assert.IsTrue(res.Contains("Status"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Wertung", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Wertung"));
            Assert.IsTrue(res.Contains("Classifier"));
            Assert.IsTrue(res.Contains("Tila"));
            Assert.IsTrue(res.Contains("Status"));
        }
        [TestMethod]
        public void TestLookupOEStringsNr()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Nr", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Nr"));
            Assert.IsTrue(res.Contains("No"));
            Assert.IsTrue(res.Contains("Nro"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Nr", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Nr"));
            Assert.IsTrue(res.Contains("No"));
            Assert.IsTrue(res.Contains("Nro"));
        }
        [TestMethod]
        public void TestLookupOEStringsTotalTime()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Gesamtzeit", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Gesamtzeit"));
            Assert.IsTrue(res.Contains("Total tid"));
            Assert.IsTrue(res.Contains("Kokonaisaika"));
            Assert.IsTrue(res.Contains("Overall time"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Gesamtzeit", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Gesamtzeit"));
            Assert.IsTrue(res.Contains("Total tid"));
            Assert.IsTrue(res.Contains("Kokonaisaika"));
            Assert.IsTrue(res.Contains("Overall time"));
        }
        [TestMethod]
        public void TestLookupOEStringsText()
        {
            var res = LiveResults.Client.OXTools.GetOEStringsForKey("Text", OXTools.SourceProgram.OE);

            Assert.IsTrue(res.Contains("Text"));

            res = LiveResults.Client.OXTools.GetOEStringsForKey("Text", OXTools.SourceProgram.OS);

            Assert.IsTrue(res.Contains("Text"));
        }

        [TestMethod]
        public void TestParseOE2010NewFile()
        {
            LiveResults.Client.OEParser pars = new OEParser();

            List<Result> results = new List<Result>();

            pars.OnResult += new ResultDelegate(delegate(Result newRes)
                {
                    results.Add(newRes);
                });

            pars.AnalyzeFile(TestHelpers.GetPathToTestFile("oe2010_splits20130311.csv"));

            Assert.AreEqual(85, results.Count);

            var res = results.Where(x => x.RunnerName == "Sami Vähänen").FirstOrDefault();
            Assert.IsNotNull(res);
            Assert.AreEqual("Rajamäen Rykmentti", res.RunnerClub);
            Assert.AreEqual("H21A", res.Class);
            Assert.AreEqual(14, res.ID);
            Assert.AreEqual(438000, res.StartTime);
            Assert.AreEqual(0, res.SplitTimes.Count);
            Assert.AreEqual(-1, res.Time);
            Assert.AreEqual(1, res.Status);

            res = results.Where(x => x.RunnerName == "Janne Mänkärlä").FirstOrDefault();
            Assert.IsNotNull(res);
            Assert.AreEqual("Lynx", res.RunnerClub);
            Assert.AreEqual("H21A", res.Class);
            Assert.AreEqual(6, res.ID);
            Assert.AreEqual(390000, res.StartTime);
            Assert.AreEqual(2, res.SplitTimes.Count);
            Assert.AreEqual(176000, res.Time);
            Assert.AreEqual(0, res.Status);

            Assert.AreEqual(154500,res.SplitTimes[0].Time);
            Assert.AreEqual(1121, res.SplitTimes[0].ControlCode);
            Assert.AreEqual(173700, res.SplitTimes[1].Time);
            Assert.AreEqual(1125, res.SplitTimes[1].ControlCode);
        }

        [TestMethod]
        public void TestParseOS2010EngFile()
        {
            LiveResults.Client.OSParser pars = new OSParser();

            List<Result> results = new List<Result>();

            pars.OnResult += new ResultDelegate(delegate(Result newRes)
            {
                results.Add(newRes);
            });

            pars.AnalyzeFile(TestHelpers.GetPathToTestFile("os2010_splits_eng.csv"));
            VerifyOSFile1(results);
        }

        [TestMethod]
        public void TestParseOS2010FinFile()
        {
            LiveResults.Client.OSParser pars = new OSParser();

            List<Result> results = new List<Result>();

            pars.OnResult += new ResultDelegate(delegate(Result newRes)
            {
                results.Add(newRes);
            });

            pars.AnalyzeFile(TestHelpers.GetPathToTestFile("os2010_splits_fin.csv"));
            VerifyOSFile1(results);
        }
        [TestMethod]
        public void TestParseOS2010SweFile()
        {
            LiveResults.Client.OSParser pars = new OSParser();

            List<Result> results = new List<Result>();

            pars.OnResult += new ResultDelegate(delegate(Result newRes)
            {
                results.Add(newRes);
            });

            pars.AnalyzeFile(TestHelpers.GetPathToTestFile("os2010_splits_sve.csv"));
            VerifyOSFile1(results);
        }
        private static void VerifyOSFile1(List<Result> results)
        {
            Assert.AreEqual(1006, results.Count);


            var res = results.Where(x => x.RunnerName == "Heini Saarimäki").FirstOrDefault();
            Assert.IsNotNull(res);

            Assert.IsInstanceOfType(res, typeof(RelayResult));
            Assert.AreEqual(1, (res as RelayResult).LegNumber);
            Assert.AreEqual("Angelniemen Ankkuri 2", res.RunnerClub);
            Assert.AreEqual("D21-1", res.Class);
            Assert.AreEqual(1000503, res.ID);
            Assert.AreEqual(360000, res.StartTime);
            Assert.AreEqual(2, res.SplitTimes.Count);
            Assert.AreEqual((12 * 60 + 30) * 100, res.Time);
            Assert.AreEqual((12 * 60 + 30) * 100, (res as RelayResult).OverallTime);
            Assert.AreEqual(0, (res as RelayResult).OverallStatus);
            Assert.AreEqual(0, res.Status);

            Assert.AreEqual(1123, res.SplitTimes[0].ControlCode);
            Assert.AreEqual(1126, res.SplitTimes[1].ControlCode);

            Assert.AreEqual((9 * 60 + 58) * 100, res.SplitTimes[0].Time);
            Assert.AreEqual((12 * 60 + 19) * 100, res.SplitTimes[1].Time);



            res = results.Where(x => x.RunnerName == "Veli Kangas").FirstOrDefault();
            Assert.IsNotNull(res);

            Assert.IsInstanceOfType(res, typeof(RelayResult));
            Assert.AreEqual(2, (res as RelayResult).LegNumber);
            Assert.AreEqual("Helsingin Suunnistajat 1", res.RunnerClub);
            Assert.AreEqual("H21-2", res.Class);
            Assert.AreEqual(2000405, res.ID);
            Assert.AreEqual((3600+22*60+24)*100, res.StartTime);
            Assert.AreEqual(2, res.SplitTimes.Count);
            Assert.AreEqual((11 * 60 + 35) * 100, res.Time);
            Assert.AreEqual((23 * 60 + 59) * 100, (res as RelayResult).OverallTime);
            Assert.AreEqual(0, (res as RelayResult).OverallStatus);
            Assert.AreEqual(0, res.Status);

            Assert.AreEqual(1123, res.SplitTimes[0].ControlCode);
            Assert.AreEqual(1126, res.SplitTimes[1].ControlCode);

            Assert.AreEqual((21 * 60 + 33) * 100, res.SplitTimes[0].Time);
            Assert.AreEqual((23 * 60 + 48) * 100, res.SplitTimes[1].Time);

            res = results.Where(x => x.RunnerName == "Lasse Suonpää").FirstOrDefault();
            Assert.IsNotNull(res);

            Assert.IsInstanceOfType(res, typeof(RelayResult));
            Assert.AreEqual(1, (res as RelayResult).LegNumber);
            Assert.AreEqual("MS Parma 1", res.RunnerClub);
            Assert.AreEqual("H21-1", res.Class);
            Assert.AreEqual(1000402, res.ID);
            Assert.AreEqual((3600 + 10 * 60) * 100, res.StartTime);
            Assert.AreEqual(2, res.SplitTimes.Count);
            Assert.AreEqual((10 * 60 + 51) * 100, res.Time);
            Assert.AreEqual((10 * 60 + 51) * 100, (res as RelayResult).OverallTime);
            Assert.AreEqual(0, (res as RelayResult).OverallStatus);
            Assert.AreEqual(0, res.Status);

            Assert.AreEqual(1123, res.SplitTimes[0].ControlCode);
            Assert.AreEqual(1126, res.SplitTimes[1].ControlCode);

            Assert.AreEqual((8 * 60 + 34) * 100, res.SplitTimes[0].Time);
            Assert.AreEqual((10 * 60 + 42) * 100, res.SplitTimes[1].Time);

            res = results.Where(x => x.RunnerName == "Peeter Pihl").FirstOrDefault();
            Assert.IsNotNull(res);

            Assert.IsInstanceOfType(res, typeof(RelayResult));
            Assert.AreEqual(2, (res as RelayResult).LegNumber);
            Assert.AreEqual("MS Parma 1", res.RunnerClub);
            Assert.AreEqual("H21-2", res.Class);
            Assert.AreEqual(2000402, res.ID);
            Assert.AreEqual((3600 + 20 * 60+51) * 100, res.StartTime);
            Assert.AreEqual(2, res.SplitTimes.Count);
            Assert.AreEqual((10 * 60 + 29) * 100, res.Time);
            Assert.AreEqual(-3, (res as RelayResult).OverallTime);
            Assert.AreEqual(3, (res as RelayResult).OverallStatus);
            Assert.AreEqual(3, res.Status);

            Assert.AreEqual(1123, res.SplitTimes[0].ControlCode);
            Assert.AreEqual(1126, res.SplitTimes[1].ControlCode);

            Assert.AreEqual((19 * 60 + 12) * 100, res.SplitTimes[0].Time);
            Assert.AreEqual((21 * 60 + 10) * 100, res.SplitTimes[1].Time);

            res = results.Where(x => x.ID == 3000402).FirstOrDefault();
            Assert.IsNotNull(res);

            Assert.IsInstanceOfType(res, typeof(RelayResult));
            Assert.AreEqual(3, (res as RelayResult).LegNumber);
            Assert.AreEqual("MS Parma 1", res.RunnerClub);
            Assert.AreEqual("H21-3", res.Class);
            Assert.AreEqual("Lasse Suonpää", res.RunnerName);
            Assert.AreEqual(3000402, res.ID);
            Assert.AreEqual((3600 + 31 * 60 + 20) * 100, res.StartTime);
            Assert.AreEqual(3, res.SplitTimes.Count);
            Assert.AreEqual((15 * 60 + 17) * 100, res.Time);
            Assert.AreEqual(-3, (res as RelayResult).OverallTime);
            Assert.AreEqual(3, (res as RelayResult).OverallStatus);
            Assert.AreEqual(0, res.Status);

            Assert.AreEqual(1122, res.SplitTimes[0].ControlCode);
            Assert.AreEqual(1123, res.SplitTimes[1].ControlCode);
            Assert.AreEqual(1126, res.SplitTimes[2].ControlCode);

            Assert.AreEqual((35 * 60 + 47) * 100, res.SplitTimes[0].Time);
            Assert.AreEqual((26 * 60 + 37) * 100, res.SplitTimes[1].Time);
            Assert.AreEqual((36 * 60 + 26) * 100, res.SplitTimes[2].Time);
        }
    }
}
