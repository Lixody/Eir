using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PHPAnalysis.Parsing;

namespace PHPAnalysis.Tests
{
    public static class TestFileUtils
    {
        private const string Folder = "TempTestFile";
        public static string CreateTempFile(string content)
        {
            if ( !Directory.Exists(Folder) )
            {
                Directory.CreateDirectory(Folder);
            }

            string tempFile = Path.Combine(Folder, Path.GetRandomFileName());
            File.WriteAllText(tempFile, content);
            return tempFile;
        }

        public static void ClearTempFiles()
        {
            Directory.Delete(Folder, true);
        }
    }
}
