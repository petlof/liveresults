using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using LiveResults.Client;
using System.IO;
namespace LiveResults.Client.Tests
{
    [TestClass]
    public class EncodingTests
    {
        [TestMethod]
        public void TestConvertBaltic()
        {
            string tmp = System.IO.File.ReadAllText(TestHelpers.GetPathToTestFile("20130508_200904_emma.xml"), Encoding.GetEncoding("ISO-8859-1"));
            tmp = tmp.Replace("<!DOCTYPE ResultList SYSTEM \"IOFdata.dtd\">", "");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(tmp);

            var personResult = xmlDoc.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[0];
            var name = personResult.ChildNodes[1].InnerText + " " + personResult.ChildNodes[0].InnerText;

            Assert.AreEqual("Agnë Juodagalvytë", name);

        }

        [TestMethod]
        public void TestConvertBaltic2()
        {
            string[] tmp = System.IO.File.ReadAllLines(TestHelpers.GetPathToTestFile("madonas.csv"), Encoding.GetEncoding(1257));
            string line = tmp[7];

            string[] parts = line.Split(';');
            string name = parts[6].Trim('"') + " " + parts[5].Trim('"');

            Assert.AreEqual("Sandra Paužaitė", name);
        }
    }
}
