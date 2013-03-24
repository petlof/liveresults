using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace LiveResults.Client.Tests
{
    public class TestHelpers
    {
        internal static string GetPathToTestFile(string fileName)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestFiles", fileName);
        }
    }
}
