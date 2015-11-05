using System.Text;
using System.Xml;
using System.IO;
using NUnit.Framework;

namespace LiveResults.Client.Tests
{
    [TestFixture]
    public class EncodingTests
    {
        [Test]
        public void TestConvertBaltic()
        {
            string tmp = File.ReadAllText(TestHelpers.GetPathToTestFile("20130508_200904_emma.xml"), Encoding.GetEncoding("ISO-8859-1"));
            tmp = tmp.Replace("<!DOCTYPE ResultList SYSTEM \"IOFdata.dtd\">", "");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(tmp);

            var personResult = xmlDoc.ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[0].ChildNodes[0];
            var name = personResult.ChildNodes[1].InnerText + " " + personResult.ChildNodes[0].InnerText;

            Assert.AreEqual("Agnë Juodagalvytë", name);

        }

        [Test]
        public void TestConvertBaltic2()
        {
            string[] tmp = File.ReadAllLines(TestHelpers.GetPathToTestFile("madonas.csv"), Encoding.GetEncoding(1257));
            string line = tmp[7];

            string[] parts = line.Split(';');
            string name = parts[6].Trim('"') + " " + parts[5].Trim('"');

            Assert.AreEqual("Sandra Paužaitė", name);
        }
    }
}
