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
    }
}
