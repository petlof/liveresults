using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace LiveResults.Client.Tests
{
    public class TestHelpers
    {
        internal static string GetPathToTestFile(string fileName)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestFiles", fileName);
        }
    }
}
