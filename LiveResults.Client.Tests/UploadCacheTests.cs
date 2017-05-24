using LiveResults.Model;
using NUnit.Framework;

namespace LiveResults.Client.Tests
{
    [TestFixture]
    public class UploadCacheTests
    {
        [Test]
        public void TestMergeRunnerUpdatesChangedNames()
        {
            var cli = new EmmaMysqlClient("", 0, "", "", "", 0);
            cli.AddRunner(new Runner(10, "Test", "Club", "Class"));
            Assert.IsTrue(cli.IsRunnerAdded(10));
            Assert.AreEqual("Test", cli.GetRunner(10).Name);
            Assert.AreEqual("Club", cli.GetRunner(10).Club);
            Assert.AreEqual("Class", cli.GetRunner(10).Class);

            cli.MergeRunners(new Runner[]
            {
                new Runner(10, "Updated", "UpdatedClub", "UpdatedClass")
            });

            Assert.IsTrue(cli.IsRunnerAdded(10));
            Assert.AreEqual("Updated", cli.GetRunner(10).Name);
            Assert.AreEqual("UpdatedClub", cli.GetRunner(10).Club);
            Assert.AreEqual("UpdatedClass", cli.GetRunner(10).Class);

        }
    }
}
