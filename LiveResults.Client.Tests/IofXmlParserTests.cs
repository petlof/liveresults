using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LiveResults.Client.Tests
{
    [TestClass]
    public class IofXmlParserTests
    {
        [TestMethod]
        public void ParseIofV2XmlFile()
        {
            var runners = Parsers.IOFXmlV2Parser.ParseFile(TestHelpers.GetPathToTestFile("20130508_200904_emma.xml"), new LogMessageDelegate(delegate(string msg) { }), false);

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
    }
}
