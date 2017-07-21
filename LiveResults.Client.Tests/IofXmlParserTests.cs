using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using LiveResults.Client.Parsers;
using NUnit.Framework;
using LiveResults.Model;

namespace LiveResults.Client.Tests
{
    [TestFixture]
    public class IofXmlParserTests
    {
        [Test]
        public void ParseIofV2XmlFile()
        {
            RadioControl[] radioControls;
            var runners = Parsers.IofXmlParser.ParseFile(TestHelpers.GetPathToTestFile("20130508_200904_emma.xml"),
                delegate(string msg)
                {
                }, false,
               new IofXmlParser.IDCalculator(0).CalculateID, true, out radioControls);
                                                                           

            Assert.AreEqual(377, runners.Length);

            Assert.AreEqual("Agnë Juodagalvytë", runners[0].Name);
            Assert.AreEqual("Àþuolas OK", runners[0].Club);
            Assert.AreEqual("S14", runners[0].Class);
            Assert.AreEqual(100400, runners[0].Time);
            Assert.AreEqual(0, runners[0].Status);
            Assert.AreEqual(1, runners[0].SplitTimes.Length);
            Assert.AreEqual(1043, runners[0].SplitTimes[0].Control);
            Assert.AreEqual(49100, runners[0].SplitTimes[0].Time);
        }

        [Test]
        public void ParseIofXmlFileTenthOfSecond()
        {
            RadioControl[] radioControls;
            var runners = Parsers.IofXmlParser.ParseFile(TestHelpers.GetPathToTestFile("oe_speaker_result_0.1sec.xml"),
                delegate (string msg)
                {
                }, false,
               new IofXmlParser.IDCalculator(0).CalculateID, true, out radioControls);


            Assert.AreEqual(82, runners.Length);

            Assert.AreEqual("Vladislav Kiselev", runners[0].Name);
            Assert.AreEqual("Russian Federation", runners[0].Club);
            Assert.AreEqual("Men", runners[0].Class);
            Assert.AreEqual(97830, runners[0].Time);
            Assert.AreEqual(0, runners[0].Status);
            Assert.AreEqual(3, runners[0].SplitTimes.Length);
            Assert.AreEqual(1069, runners[0].SplitTimes[0].Control);
            Assert.AreEqual(51920, runners[0].SplitTimes[0].Time);
            Assert.AreEqual(1049, runners[0].SplitTimes[1].Control);
            Assert.AreEqual(91920, runners[0].SplitTimes[1].Time);
            Assert.AreEqual(1100, runners[0].SplitTimes[2].Control);
            Assert.AreEqual(96520, runners[0].SplitTimes[2].Time);
        }

        [Test]
        public void TestFinishPunchDetected()
        {
            RadioControl[] radioControls;
            var runners = IofXmlParser.ParseFile(TestHelpers.GetPathToTestFile("splitsResult_Kugler_Johann_in_Finish.xml"),
                delegate(string msg)
                {
                }, false,
               new IofXmlParser.IDCalculator(0).CalculateID, true, out radioControls);


            var runner = runners.First(x => x.Name == "Johann Kugler");
            Assert.AreEqual(0, runner.Status);
            Assert.AreEqual(107600, runner.Time);
            
        }

        [Test]
        public void VerifyIofV2XmlFileNotCompetingDoesNotExis()
        {
            RadioControl[] radioControls;
            var runners = Parsers.IofXmlParser.ParseFile(TestHelpers.GetPathToTestFile("iof_xml_notcompeting.xml"),
                new LogMessageDelegate(delegate(string msg)
                {
                }), false,
               new IofXmlParser.IDCalculator(0).CalculateID, true, out radioControls);

            Assert.IsNull(runners.FirstOrDefault(x => x.Name == "Stepan Malinovskii"));
        }
    }
}
