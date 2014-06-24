using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LiveResults.Client.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace LiveResults.Client.Tests
{
    [TestClass]
    public class TestRaComFiles
    {
        [TestMethod]
        public void TestReadRaComFiles()
        {
            var racomParser = new RacomFileSetParser();
            string curDIr = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var runners = racomParser.ParseFiles(new DateTime(2014, 01, 9, 9, 0, 0), curDIr + @"\TestFiles\Racom\Middle\w_test2\w_test2.startlist.txt",
                curDIr + @"\TestFiles\Racom\Middle\w_test2\w_test2.rawsplits.txt",
                curDIr + @"\TestFiles\Racom\Middle\race.txt",
                curDIr + @"\TestFiles\Racom\Middle\w_test2\w_test2.disks.txt");

            Assert.AreEqual(214, runners.Length);
            Assert.AreEqual("KAUPPI Minna", runners.Where(x => x.ID == 203).Select(x => x.Name).FirstOrDefault());
            Assert.AreEqual("JUUØENÍKOVÁ Eva", runners.Where(x => x.ID == 210).Select(x => x.Name).FirstOrDefault());
            Assert.AreEqual(4, runners.Where(x => x.Name == "LAKANEN Jani").Select(x => x.Status).FirstOrDefault());
            Assert.AreEqual(4, runners.Where(x => x.Name == "FINCKE Simo-Pekka").Select(x => x.Status).FirstOrDefault());
            Assert.AreEqual(4, runners.Where(x => x.Name == "DENT Julian").Select(x => x.Status).FirstOrDefault());
            Assert.AreEqual(4, runners.Where(x => x.Name == "O'SULLIVAN-HOURIHAN Jo").Select(x => x.Status).FirstOrDefault());

              
                 
                     
        }
    }
}
