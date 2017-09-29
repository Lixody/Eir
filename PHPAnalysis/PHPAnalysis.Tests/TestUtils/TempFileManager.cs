using System;
using System.IO;

namespace PHPAnalysis.Tests
{
    public sealed class TempFileManager : IDisposable
    {
        private readonly string _folder = Path.GetTempPath();
        private readonly string _file = Path.GetTempFileName();

        private string FilePath => Path.Combine(_folder, _file);

        private bool _createdFolder = false;

        public string WriteContent(string content)
        {
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
                _createdFolder = true;
            }

            File.WriteAllText(FilePath, content);
            return FilePath;
        }

        public void Dispose()
        {
            if (!File.Exists(FilePath)) { return; }

            if (_createdFolder)
            {
                Directory.Delete(_folder, true);
            }
            else
            {
                File.Delete(FilePath);
            }
        }
    }
}
